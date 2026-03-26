using Dapper;
using ECommerceBigData.Data.Repositories.DashboardRepositories;
using ECommerceBigData.Data.Repositories.OrderRepositories;
using ECommerceBigData.Data.Repositories.ProductRepositories;
using ECommerceBigData.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBigData.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboard;
        private readonly IProductRepository _products;
        private readonly IOrderRepository _orders;

        public DashboardController(
            IDashboardRepository dashboard,
            IProductRepository products,
            IOrderRepository orders)
        {
            _dashboard = dashboard;
            _products = products;
            _orders = orders;
        }

        // GET /Dashboard
        public async Task<IActionResult> Index()
        {
            var tasks = await Task.WhenAll(
                _dashboard.GetSummaryAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetDailySalesAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetCountrySalesAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetCitySalesAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetCategorySalesAsync().ContinueWith(t => (object)t.Result),
                _products.GetTopProductsAsync().ContinueWith(t => (object)t.Result),
                _orders.GetLastOrdersAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetMonthlyRevenueAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetHourlyOrderHeatmapAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetTopCustomersAsync(5).ContinueWith(t => (object)t.Result),
                _dashboard.GetKpiMetricsAsync().ContinueWith(t => (object)t.Result),
                _dashboard.GetSegmentDistributionAsync().ContinueWith(t => (object)t.Result)
            );

            // Growth rate hesapla
            var prevRevenue = await GetPreviousMonthRevenueAsync();
            var currRevenue = ((ECommerceBigData.Dtos.DashboardSummaryDto)tasks[0]).TotalRevenue;

            var model = new DashboardViewModel
            {
                Summary = (ECommerceBigData.Dtos.DashboardSummaryDto)tasks[0],
                DailySales = (List<ECommerceBigData.Dtos.DailySalesDto>)tasks[1],
                CountrySales = (List<ECommerceBigData.Dtos.CountrySalesDto>)tasks[2],
                CitySales = (List<ECommerceBigData.Dtos.CitySalesDto>)tasks[3],
                CategorySales = (List<ECommerceBigData.Dtos.CategorySalesDto>)tasks[4],
                TopProducts = (List<ECommerceBigData.Dtos.TopProductDto>)tasks[5],
                LastOrders = (List<ECommerceBigData.Dtos.LastOrderDto>)tasks[6],
                MonthlyRevenue = (List<ECommerceBigData.Dtos.MonthlyRevenueDto>)tasks[7],
                HourlyHeatmap = (List<ECommerceBigData.Dtos.HourlyOrderDto>)tasks[8],
                TopCustomers = (List<ECommerceBigData.Dtos.TopCustomerDto>)tasks[9],
                KpiMetrics = (ECommerceBigData.Dtos.KpiMetricsDto)tasks[10],
                SegmentDistribution = (List<ECommerceBigData.Dtos.SegmentDistributionDto>)tasks[11],
                RevenueGrowthRate = prevRevenue > 0
                    ? Math.Round((currRevenue - prevRevenue) / prevRevenue * 100, 1)
                    : 12.5m
            };

            return View(model);
        }

        // GET /Dashboard/Search?q=...   (AJAX)
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var results = await _orders.SearchOrdersAsync(q);
            return Json(results);
        }

        private async Task<decimal> GetPreviousMonthRevenueAsync()
        {
            var sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM Orders WITH(NOLOCK)
                WHERE OrderDate >= DATEADD(MONTH, -2, GETDATE())
                  AND OrderDate <  DATEADD(MONTH, -1, GETDATE())";

            using var conn = _dashboard.GetConnection();
            return await conn.ExecuteScalarAsync<decimal>(sql);
        }
    }
}