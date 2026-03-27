using Dapper;
using ECommerceBigData.Data.Context;
using ECommerceBigData.Dtos;
using ECommerceBigData.Models;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mono.TextTemplating;
using System.Data;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static NuGet.Packaging.PackagingConstants;

namespace ECommerceBigData.Data.Repositories.DashboardRepositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public DashboardRepository(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

        
    public async Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilters filters)
    {
        const string query = @"
            SELECT
                ISNULL(SUM(o.TotalAmount), 0) AS TotalRevenue,
                COUNT(*) AS TotalOrders,
                COUNT(DISTINCT o.CustomerId) AS TotalCustomers,
                ISNULL(AVG(o.TotalAmount), 0) AS AvgOrderValue
            FROM Orders o WITH(NOLOCK)
            WHERE o.OrderDate >= @from AND o.OrderDate < @to
              AND (@country IS NULL OR o.Country = @country)
              AND (@city IS NULL OR o.City = @city)
              AND (@segment IS NULL OR EXISTS(
                  SELECT 1 FROM CustomerSegments cs WITH(NOLOCK)
                  WHERE cs.CustomerId = o.CustomerId AND cs.Segment = @segment
              ))";

        using var conn = _context.CreateConnection();
        return await conn.QueryFirstAsync<DashboardSummaryDto>(query, filters.ToSqlParams());
    }

    public async Task<List<DailySalesDto>> GetDailySalesAsync(DashboardFilters filters)
    {
        const string query = @"
            SELECT
                CAST(o.OrderDate AS DATE) AS Date,
                ISNULL(SUM(o.TotalAmount), 0) AS TotalSales
            FROM Orders o WITH(NOLOCK)
            WHERE o.OrderDate >= @from AND o.OrderDate < @to
              AND (@country IS NULL OR o.Country = @country)
              AND (@city IS NULL OR o.City = @city)
            GROUP BY CAST(o.OrderDate AS DATE)
            ORDER BY Date";

        
        using var conn = _context.CreateConnection();
        var result = (await conn.QueryAsync<DailySalesDto>(query, filters.ToSqlParams())).ToList();

       
    var allDates = Enumerable.Range(0, (filters.EffectiveTo - filters.EffectiveFrom).Days + 1)
        .Select(i => filters.EffectiveFrom.AddDays(i));

    var indexed = result.ToDictionary(x => x.Date.Date, x => x.TotalSales);
        return allDates.Select(date => new DailySalesDto
        {
            Date = date,
            TotalSales = indexed.TryGetValue(date.Date, out var val) ? val : 0
        }).ToList();
    }

    public async Task<List<CountrySalesDto>> GetCountrySalesAsync(DashboardFilters filters)
{
    const string query = @"
            SELECT TOP 10
                o.Country,
                ISNULL(SUM(o.TotalAmount), 0) AS TotalSales
            FROM Orders o WITH(NOLOCK)
            WHERE o.OrderDate >= @from AND o.OrderDate < @to
              AND o.Country IS NOT NULL
              AND (@country IS NULL OR o.Country = @country)
              AND (@city IS NULL OR o.City = @city)
            GROUP BY o.Country
            ORDER BY TotalSales DESC";

    using var conn = _context.CreateConnection();
    return (await conn.QueryAsync<CountrySalesDto>(query, filters.ToSqlParams())).ToList();
}

public async Task<List<CitySalesDto>> GetCitySalesAsync(DashboardFilters filters)
{
    const string query = @"
            SELECT TOP 10
                o.City,
                ISNULL(SUM(o.TotalAmount), 0) AS TotalSales
            FROM Orders o WITH(NOLOCK)
            WHERE o.OrderDate >= @from AND o.OrderDate < @to
              AND o.City IS NOT NULL
              AND (@country IS NULL OR o.Country = @country)
              AND (@city IS NULL OR o.City = @city)
            GROUP BY o.City
            ORDER BY TotalSales DESC";

    using var conn = _context.CreateConnection();
    return (await conn.QueryAsync<CitySalesDto>(query, filters.ToSqlParams())).ToList();
}

public async Task<List<CategorySalesDto>> GetCategorySalesAsync(DashboardFilters filters)
{
    const string query = @"
            SELECT
                c.Name AS CategoryName,
                ISNULL(SUM(od.TotalPrice), 0) AS TotalSales
            FROM Categories c WITH(NOLOCK)
            LEFT JOIN Products p WITH(NOLOCK) ON p.CategoryId = c.Id
            LEFT JOIN OrderDetails od WITH(NOLOCK) ON od.ProductId = p.Id
            LEFT JOIN Orders o WITH(NOLOCK) ON o.Id = od.OrderId
            WHERE (o.Id IS NULL OR (
                o.OrderDate >= @from AND o.OrderDate < @to
                AND (@country IS NULL OR o.Country = @country)
                AND (@city IS NULL OR o.City = @city)
            ))
            AND (@category IS NULL OR c.Name = @category)
            GROUP BY c.Name
            ORDER BY TotalSales DESC";

    using var conn = _context.CreateConnection();
    var result = (await conn.QueryAsync<CategorySalesDto>(query, filters.ToSqlParams())).ToList();

    var total = result.Sum(x => x.TotalSales);
    foreach (var item in result)
    {
        item.Percentage = total > 0 ? (double)(item.TotalSales / total * 100) : 0;
        item.Icon = GetCategoryIcon(item.CategoryName);
    }

    return result;
}


public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(DashboardFilters filters)
{
    var monthlyData = await GetMonthlyDataAsync(filters, 12);
    return monthlyData.Select(x => new MonthlyRevenueDto
    {
        MonthLabel = x.MonthStart.ToString("MMM yy"),
        Revenue = x.TotalRevenue,
        OrderCount = x.TotalOrders
    }).ToList();
}

    public async Task<List<MonthlyDataDto>> GetMonthlyDataAsync(DashboardFilters filters, int monthCount = 36)
    {
        const string query = @"
        SELECT
            DATEFROMPARTS(YEAR(o.OrderDate), MONTH(o.OrderDate), 1) AS MonthStart,
            ISNULL(SUM(o.TotalAmount), 0) AS TotalRevenue,
            COUNT(*) AS TotalOrders
        FROM Orders o WITH(NOLOCK)
        WHERE o.OrderDate >= DATEADD(MONTH, -@monthCount, @to)
          AND o.OrderDate < @to
          AND (@country IS NULL OR o.Country = @country)
          AND (@city IS NULL OR o.City = @city)
        GROUP BY DATEFROMPARTS(YEAR(o.OrderDate), MONTH(o.OrderDate), 1)
        ORDER BY MonthStart";

        var sqlParams = new DynamicParameters(filters.ToSqlParams()); // ✅
        sqlParams.Add("monthCount", monthCount); // ✅ KRİTİK SATIR

        using var conn = _context.CreateConnection();
        return (await conn.QueryAsync<MonthlyDataDto>(query, sqlParams)).ToList();
    }

    public async Task<List<HourlyOrderDto>> GetHourlyOrderHeatmapAsync(DashboardFilters filters)
{
    const string query = @"
            SELECT
                DATEPART(WEEKDAY, o.OrderDate) AS DayOfWeek,
                DATEPART(HOUR, o.OrderDate) AS [Hour],
                COUNT(*) AS OrderCount
            FROM Orders o WITH(NOLOCK)
            WHERE o.OrderDate >= DATEADD(MONTH, -3, @to)
              AND o.OrderDate < @to
              AND (@country IS NULL OR o.Country = @country)
              AND (@city IS NULL OR o.City = @city)
            GROUP BY DATEPART(WEEKDAY, o.OrderDate), DATEPART(HOUR, o.OrderDate)
            ORDER BY DayOfWeek, [Hour]";

    using var conn = _context.CreateConnection();
    return (await conn.QueryAsync<HourlyOrderDto>(query, filters.ToSqlParams())).ToList();
}

public async Task<List<TopCustomerDto>> GetTopCustomersAsync(DashboardFilters filters, int count = 5)
{
    const string query = @"
            SELECT TOP (@count)
                c.Id AS CustomerId,
                c.FullName,
                c.Country,
                c.City,
                COUNT(o.Id) AS TotalOrders,
                ISNULL(SUM(o.TotalAmount), 0) AS LifetimeValue,
                ISNULL(cs.Segment, 'Regular') AS Segment
            FROM Customers c WITH(NOLOCK)
            JOIN Orders o WITH(NOLOCK) ON o.CustomerId = c.Id
            LEFT JOIN CustomerSegments cs WITH(NOLOCK) ON cs.CustomerId = c.Id
            WHERE o.OrderDate >= @from AND o.OrderDate < @to
              AND (@country IS NULL OR c.Country = @country)
              AND (@city IS NULL OR c.City = @city)
              AND (@segment IS NULL OR cs.Segment = @segment)
            GROUP BY c.Id, c.FullName, c.Country, c.City, cs.Segment
            ORDER BY LifetimeValue DESC";

    var prms = new DynamicParameters(filters.ToSqlParams());
    prms.Add("count", count);

    using var conn = _context.CreateConnection();
    return (await conn.QueryAsync<TopCustomerDto>(query, prms)).ToList();
}

public async Task<KpiMetricsDto> GetKpiMetricsAsync(DashboardFilters filters)
{
    const string conversionQuery = @"
            SELECT CAST(SUM(CAST(pv.Purchases AS FLOAT)) AS FLOAT)
                 / NULLIF(SUM(CAST(pv.ViewCount AS FLOAT)), 0) * 100
            FROM ProductViews pv WITH(NOLOCK)
            WHERE pv.ViewDate >= @from AND pv.ViewDate < @to
              AND (@country IS NULL OR pv.Country = @country)
              AND (@city IS NULL OR pv.City = @city)";

    const string refundQuery = @"
            SELECT CAST(SUM(CASE WHEN st.Status = 'Returned' THEN 1 ELSE 0 END) * 100.0
                 / NULLIF(COUNT(*), 0) AS FLOAT)
            FROM ShipmentTracking st WITH(NOLOCK)
            JOIN Orders o WITH(NOLOCK) ON o.Id = st.OrderId
            WHERE o.OrderDate >= @from AND o.OrderDate < @to
              AND (@country IS NULL OR o.Country = @country)
              AND (@city IS NULL OR o.City = @city)";

       
        const string deliveryQuery = @"
            SELECT AVG(CAST(st.EstimatedDays AS FLOAT))
            FROM ShipmentTracking st WITH(NOLOCK)
            JOIN Orders o WITH(NOLOCK) ON o.Id = st.OrderId
            WHERE o.OrderDate >= @from AND o.OrderDate<@to
              AND st.EstimatedDays IS NOT NULL
              AND(@country IS NULL OR o.Country = @country)
              AND(@city IS NULL OR o.City = @city)";

        const string clvQuery = @"
            SELECT AVG(total_spend)
            FROM (
                SELECT o.CustomerId, SUM(o.TotalAmount) AS total_spend
                FROM Orders o WITH(NOLOCK)
                WHERE o.OrderDate >= @from AND o.OrderDate < @to
                  AND (@country IS NULL OR o.Country = @country)
                  AND (@city IS NULL OR o.City = @city)
               
    GROUP BY o.CustomerId
            ) t";

        var sqlParams = filters.ToSqlParams();

   
var convTask = ExecuteScalarAsync<double?>(conversionQuery, sqlParams);
var refundTask = ExecuteScalarAsync<double?>(refundQuery, sqlParams);
var deliveryTask = ExecuteScalarAsync<double?>(deliveryQuery, sqlParams);
var clvTask = ExecuteScalarAsync<decimal?>(clvQuery, sqlParams);

await Task.WhenAll(convTask, refundTask, deliveryTask, clvTask);

        return new KpiMetricsDto
        {
            
            ConversionRate = Math.Round(convTask.Result ?? 0, 2),
            RefundRate = Math.Round(refundTask.Result ?? 0, 2),
            AvgDeliveryDays = Math.Round(deliveryTask.Result ?? 0, 1),
            AvgCLV = Math.Round(clvTask.Result ?? 0m, 2)
        };
    }

    public async Task<List<SegmentDistributionDto>> GetSegmentDistributionAsync(DashboardFilters filters)
    {
        const string query = @"
            WITH scoped AS (
                SELECT ISNULL(cs.Segment, 'Regular') AS Segment
                FROM Orders o WITH(NOLOCK)
                LEFT JOIN CustomerSegments cs WITH(NOLOCK) ON cs.CustomerId = o.CustomerId
                WHERE o.OrderDate >= @from AND o.OrderDate < @to
                  AND (@country IS NULL OR o.Country = @country)
                  AND (@city IS NULL OR o.City = @city)
               
            ), totals AS(
                SELECT COUNT(*) AS grand_total FROM scoped
            )
            SELECT
                s.Segment,
                COUNT(*) AS Count,
                CAST(COUNT(*) * 100.0 / NULLIF(t.grand_total, 0) AS FLOAT) AS Percentage
            FROM scoped s
            CROSS JOIN totals t
            GROUP BY s.Segment, t.grand_total
            ORDER BY Count DESC";

        using var conn = _context.CreateConnection();
return (await conn.QueryAsync<SegmentDistributionDto>(query, filters.ToSqlParams())).ToList();
    }

    public Task<List<string>> GetCountryFilterOptionsAsync(DashboardFilters filters)
        => GetCachedFilterOptionsAsync($"dashboard:filters:country:{ScopeKey(filters)}", TimeSpan.FromMinutes(15), async () =>
        {
            const string query = @"
                SELECT DISTINCT Country
                FROM Orders WITH(NOLOCK)
                WHERE Country IS NOT NULL
                  AND OrderDate >= @from AND OrderDate < @to
                  AND (@country IS NULL OR Country = @country)
                  AND (@city IS NULL OR City = @city)
                ORDER BY Country";

            using var conn = _context.CreateConnection();
        
            return (await conn.QueryAsync<string>(query, filters.ToSqlParams())).ToList();
        });

    public Task<List<string>> GetCityFilterOptionsAsync(DashboardFilters filters)
        => GetCachedFilterOptionsAsync($"dashboard:filters:city:{ScopeKey(filters)}", TimeSpan.FromMinutes(10), async () =>
        {
            const string query = @"
                SELECT DISTINCT City
                FROM Orders WITH(NOLOCK)
                WHERE City IS NOT NULL
                  AND OrderDate >= @from AND OrderDate < @to
                  AND (@country IS NULL OR Country = @country)
                  AND (@city IS NULL OR City = @city)
                ORDER BY City";

            using var conn = _context.CreateConnection();
           
            return (await conn.QueryAsync<string>(query, filters.ToSqlParams())).ToList();
        });

    public Task<List<string>> GetCategoryFilterOptionsAsync(DashboardFilters filters)
        => GetCachedFilterOptionsAsync($"dashboard:filters:category:{ScopeKey(filters)}", TimeSpan.FromMinutes(5), async () =>
        {
            const string query = @"
              
            SELECT DISTINCT c.Name
            FROM Categories c WITH(NOLOCK)
                JOIN Products p WITH(NOLOCK) ON p.CategoryId = c.Id
                JOIN OrderDetails od WITH(NOLOCK) ON od.ProductId = p.Id
                JOIN Orders o WITH(NOLOCK) ON o.Id = od.OrderId
                WHERE o.OrderDate >= @from AND o.OrderDate<@to
                  AND(@country IS NULL OR o.Country = @country)
                  AND(@city IS NULL OR o.City = @city)
                ORDER BY c.Name";

            using var conn = _context.CreateConnection();
           
            return (await conn.QueryAsync<string>(query, filters.ToSqlParams())).ToList();
        });


    public Task<List<string>> GetSegmentFilterOptionsAsync(DashboardFilters filters)
        => GetCachedFilterOptionsAsync($"dashboard:filters:segment:{ScopeKey(filters)}", TimeSpan.FromMinutes(10), async () =>
        {
            const string query = @"
               
            SELECT DISTINCT ISNULL(cs.Segment, 'Regular')
                FROM Orders o WITH(NOLOCK)
                LEFT JOIN CustomerSegments cs WITH(NOLOCK) ON cs.CustomerId = o.CustomerId
                WHERE o.OrderDate >= @from AND o.OrderDate<@to
                  AND(@country IS NULL OR o.Country = @country)
                  AND(@city IS NULL OR o.City = @city)
                ORDER BY ISNULL(cs.Segment, 'Regular')";

            using var conn = _context.CreateConnection();
           
            return (await conn.QueryAsync<string>(query, filters.ToSqlParams())).ToList();
        });

public IDbConnection GetConnection() => _context.CreateConnection();

private async Task<T> ExecuteScalarAsync<T>(string query, object parameters)
{
    using var conn = _context.CreateConnection();
    return await conn.ExecuteScalarAsync<T>(query, parameters);
}

private async Task<List<string>> GetCachedFilterOptionsAsync(
    string cacheKey,
    TimeSpan ttl,
    Func<Task<List<string>>> factory)
{
    if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached is not null)
    {
        return cached;
    }

    var data = await factory();
    _cache.Set(cacheKey, data, ttl);
    return data;
}

private static string ScopeKey(DashboardFilters filters)
    => $"{filters.EffectiveFrom:yyyyMMdd}-{filters.EffectiveTo:yyyyMMdd}-{filters.Country ?? "*"}-{filters.City ?? "*"}";


private static string GetCategoryIcon(string? categoryName) =>
    categoryName?.ToLowerInvariant() switch
    {
        "electronics" => "devices",
        "clothing" => "checkroom",
        "home & garden" => "home",
        "sports & outdoors" => "sports_soccer",
        "books & media" => "menu_book",
        "beauty & health" => "spa",
        "toys & games" => "toys",
        "automotive" => "directions_car",
        "food & beverage" => "restaurant",
        "office supplies" => "work",
        _ => "category"
    };
}

