using Dapper;
using ECommerceBigData.Data.Repositories.DashboardRepositories;
using ECommerceBigData.Data.Repositories.OrderRepositories;
using ECommerceBigData.Data.Repositories.ProductRepositories;
using ECommerceBigData.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace ECommerceBigData.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IWebHostEnvironment _environment;

        public DashboardController(
            IDashboardRepository dashboard,
            IProductRepository productRepository,
          
            IOrderRepository orderRepository,
            IWebHostEnvironment environment)
        {
            _dashboardRepository = dashboard;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _environment = environment;
        }

       
        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? from,
            DateTime? to,
            string? country,
            string? city,
            string? category,
            string? segment)
        {
           
            var filters = new DashboardFilters
            {
                From = from,
                To = to,
                Country = country,
                City = city,
                Category = category,
                Segment = segment
            };

            var summaryTask = _dashboardRepository.GetSummaryAsync(filters);
            var dailySalesTask = _dashboardRepository.GetDailySalesAsync(filters);
            var countryTask = _dashboardRepository.GetCountrySalesAsync(filters);
            var cityTask = _dashboardRepository.GetCitySalesAsync(filters);
            var categoryTask = _dashboardRepository.GetCategorySalesAsync(filters);
            var topProductsTask = _productRepository.GetTopProductsAsync(filters);
            var lastOrdersTask = _orderRepository.GetLastOrdersAsync();
            var monthlyRevenueTask = _dashboardRepository.GetMonthlyRevenueAsync(filters);
            var hourlyHeatmapTask = _dashboardRepository.GetHourlyOrderHeatmapAsync(filters);
            var topCustomersTask = _dashboardRepository.GetTopCustomersAsync(filters, 8);
            var kpiTask = _dashboardRepository.GetKpiMetricsAsync(filters);
            var segmentTask = _dashboardRepository.GetSegmentDistributionAsync(filters);

           
            var countriesTask = _dashboardRepository.GetCountryFilterOptionsAsync();
            var citiesTask = _dashboardRepository.GetCityFilterOptionsAsync(country);
            var categoriesTask = _dashboardRepository.GetCategoryFilterOptionsAsync();
            var segmentsTask = _dashboardRepository.GetSegmentFilterOptionsAsync();

           
            await Task.WhenAll(
                summaryTask,
                dailySalesTask,
                countryTask,
                cityTask,
                categoryTask,
                topProductsTask,
                lastOrdersTask,
                monthlyRevenueTask,
                hourlyHeatmapTask,
                topCustomersTask,
                kpiTask,
                segmentTask,
                countriesTask,
                citiesTask,
                categoriesTask,
                segmentsTask);

            var previousPeriod = await GetPreviousPeriodMetricsAsync(filters);
            var summary = summaryTask.Result;

            var model = new DashboardViewModel
            {
              
                Summary = summary,
                DailySales = dailySalesTask.Result,
                CountrySales = countryTask.Result,
                CitySales = cityTask.Result,
                TopProducts = topProductsTask.Result,
                LastOrders = lastOrdersTask.Result,
                CategorySales = categoryTask.Result,
               
                MonthlyRevenue = monthlyRevenueTask.Result,
                HourlyHeatmap = hourlyHeatmapTask.Result,
                TopCustomers = topCustomersTask.Result,
                KpiMetrics = kpiTask.Result,
                SegmentDistribution = segmentTask.Result,
                RevenueGrowthRate = CalculateGrowthRate(summary.TotalRevenue, previousPeriod.revenue),
                OrdersGrowthRate = CalculateGrowthRate(summary.TotalOrders, previousPeriod.orders),
                CustomersGrowthRate = CalculateGrowthRate(summary.TotalCustomers, previousPeriod.customers),
                Filters = filters,
                AvailableCountries = countriesTask.Result,
                AvailableCities = citiesTask.Result,
                AvailableCategories = categoriesTask.Result,
                AvailableSegments = segmentsTask.Result
            };

           

            return View(model);
        }

        
        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            try
            {
               
                var filters = new DashboardFilters();
                var summary = await _dashboardRepository.GetSummaryAsync(filters);
                var daily = await _dashboardRepository.GetDailySalesAsync(filters);
                var country = await _dashboardRepository.GetCountrySalesAsync(filters);
                var city = await _dashboardRepository.GetCitySalesAsync(filters);
                var category = await _dashboardRepository.GetCategorySalesAsync(filters);
                var products = await _productRepository.GetTopProductsAsync(filters);
                var orders = await _orderRepository.GetLastOrdersAsync();

                return Json(new
                {
                   
                    summary_TotalRevenue = summary?.TotalRevenue,
                    summary_TotalOrders = summary?.TotalOrders,
                    summary_TotalCustomers = summary?.TotalCustomers,
                    summary_AvgOrderValue = summary?.AvgOrderValue,
                    summary_IsNull = summary == null,

                   
                    dailySales_Count = daily?.Count,
                    dailySales_HasData = daily?.Any(x => x.TotalSales > 0),
                    country_Count = country?.Count,
                    city_Count = city?.Count,
                    category_Count = category?.Count,
                    products_Count = products?.Count,
                   
                    orders_Count = orders?.Count
                });
            }
           
            catch (Exception)
            {
                return Problem("Debug endpoint failed. Check server logs for details.");
            }
        }

     
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var results = await _orderRepository.SearchOrdersAsync(q);
            return Json(results);
        }

      
        private async Task<(decimal revenue, int orders, int customers)> GetPreviousPeriodMetricsAsync(DashboardFilters filters)
        {
            var days = Math.Max(1, (filters.EffectiveTo - filters.EffectiveFrom).Days + 1);
            var prevFrom = filters.EffectiveFrom.AddDays(-days);
            var prevToExclusive = filters.EffectiveFrom;

            var query = @"
               
            SELECT
                    ISNULL(SUM(o.TotalAmount), 0) AS Revenue,
                    COUNT(*)                      AS Orders,
                    COUNT(DISTINCT o.CustomerId)  AS Customers
                FROM Orders o WITH(NOLOCK)
                WHERE o.OrderDate >= @prevFrom AND o.OrderDate<@prevTo
                  AND(@country IS NULL OR o.Country = @country)
                  AND(@city IS NULL OR o.City = @city)";

            using var connection = _dashboardRepository.GetConnection();
            var row = await connection.QueryFirstAsync<(decimal Revenue, int Orders, int Customers)>(query, new
            {
                prevFrom,
                prevTo = prevToExclusive,
                country = string.IsNullOrWhiteSpace(filters.Country) ? null : filters.Country,
                city = string.IsNullOrWhiteSpace(filters.City) ? null : filters.City
            });

            return (row.Revenue, row.Orders, row.Customers);
        }

        private static decimal CalculateGrowthRate(decimal current, decimal previous)
        {
            if (previous <= 0)
            {
                return current > 0 ? 100 : 0;
            }

            return (current - previous) / previous * 100;
        }

        private static decimal CalculateGrowthRate(int current, int previous)
            => CalculateGrowthRate((decimal)current, previous);
    }

}