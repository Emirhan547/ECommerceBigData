using Dapper;
using ECommerceBigData.Data.Context;
using ECommerceBigData.Dtos;

namespace ECommerceBigData.Data.Repositories.DashboardRepositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var query = @"
                SELECT 
                    ISNULL(SUM(TotalAmount), 0) AS TotalRevenue,
                    COUNT(*) AS TotalOrders,
                    (SELECT COUNT(*) FROM Customers) AS TotalCustomers,
                    ISNULL(AVG(TotalAmount), 0) AS AvgOrderValue
                FROM Orders";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstAsync<DashboardSummaryDto>(query);
        }

        public async Task<List<DailySalesDto>> GetDailySalesAsync()
        {
            var query = @"
                SELECT 
                    CAST(OrderDate AS DATE) AS Date,
                    SUM(TotalAmount) AS TotalSales
                FROM Orders
                WHERE OrderDate >= DATEADD(DAY, -30, GETDATE())
                GROUP BY CAST(OrderDate AS DATE)
                ORDER BY Date";

            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<DailySalesDto>(query)).ToList();
        }

        public async Task<List<CountrySalesDto>> GetCountrySalesAsync()
        {
            var query = @"
                SELECT 
                    Country, 
                    SUM(TotalAmount) AS TotalSales
                FROM Orders
                WHERE Country IS NOT NULL
                GROUP BY Country
                ORDER BY TotalSales DESC";

            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<CountrySalesDto>(query)).ToList();
        }

        public async Task<List<CitySalesDto>> GetCitySalesAsync()
        {
            var query = @"
                SELECT 
                    City, 
                    SUM(TotalAmount) AS TotalSales
                FROM Orders
                WHERE City IS NOT NULL
                GROUP BY City
                ORDER BY TotalSales DESC";

            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<CitySalesDto>(query)).ToList();
        }

        // Yeni metod: Kategori bazlı satışlar için
        public async Task<List<CategorySalesDto>> GetCategorySalesAsync()
        {
            var query = @"
                SELECT 
                    c.Name AS CategoryName,
                    SUM(od.TotalPrice) AS TotalSales
                FROM OrderDetails od
                JOIN Products p ON p.Id = od.ProductId
                JOIN Categories c ON c.Id = p.CategoryId
                GROUP BY c.Name
                ORDER BY TotalSales DESC";

            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<CategorySalesDto>(query)).ToList();
        }

        // Yeni metod: Aylık trend verileri için (growth rate hesaplama)
        public async Task<MonthlyTrendDto> GetMonthlyTrendAsync()
        {
            var query = @"
                SELECT 
                    DATEADD(MONTH, DATEDIFF(MONTH, 0, OrderDate), 0) AS MonthStart,
                    SUM(TotalAmount) AS TotalRevenue,
                    COUNT(*) AS TotalOrders
                FROM Orders
                WHERE OrderDate >= DATEADD(MONTH, -2, GETDATE())
                GROUP BY DATEADD(MONTH, DATEDIFF(MONTH, 0, OrderDate), 0)
                ORDER BY MonthStart";

            using var connection = _context.CreateConnection();
            var monthlyData = (await connection.QueryAsync<MonthlyDataDto>(query)).ToList();

            var result = new MonthlyTrendDto();
            if (monthlyData.Count >= 2)
            {
                var currentMonth = monthlyData[monthlyData.Count - 1];
                var previousMonth = monthlyData[monthlyData.Count - 2];

                result.RevenueGrowthRate = previousMonth.TotalRevenue > 0
                    ? ((currentMonth.TotalRevenue - previousMonth.TotalRevenue) / previousMonth.TotalRevenue) * 100
                    : 0;

                result.OrdersGrowthRate = previousMonth.TotalOrders > 0
                    ? ((currentMonth.TotalOrders - previousMonth.TotalOrders) / (decimal)previousMonth.TotalOrders) * 100
                    : 0;
            }

            return result;
        }
    }
}