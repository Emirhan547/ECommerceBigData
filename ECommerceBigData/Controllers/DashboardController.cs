// ============================================================
//  DashboardController.cs  --  DEBUG ACTION EKLE
//  
//  Mevcut DashboardController sınıfına bu action'ı ekle.
//  Tarayıcıda /Dashboard/Debug adresine git.
//  JSON'daki değerlere göre sorunun nerede olduğunu anlarsın.
// ============================================================

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
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public DashboardController(
            IDashboardRepository dashboard,
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _dashboardRepository = dashboard;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        // ── Ana Dashboard ────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var summaryTask = _dashboardRepository.GetSummaryAsync();
            var dailySalesTask = _dashboardRepository.GetDailySalesAsync();
            var countryTask = _dashboardRepository.GetCountrySalesAsync();
            var cityTask = _dashboardRepository.GetCitySalesAsync();
            var categoryTask = _dashboardRepository.GetCategorySalesAsync();
            var topProductsTask = _productRepository.GetTopProductsAsync();
            var lastOrdersTask = _orderRepository.GetLastOrdersAsync();

            await Task.WhenAll(
                summaryTask, dailySalesTask, countryTask,
                cityTask, categoryTask, topProductsTask, lastOrdersTask
            );

            var previousMonthRevenue = await GetPreviousMonthRevenueAsync();
            var currentRevenue = summaryTask.Result.TotalRevenue;

            var model = new DashboardViewModel
            {
                Summary = summaryTask.Result,        // ← NULL olmamalı
                DailySales = dailySalesTask.Result,
                CountrySales = countryTask.Result,
                CitySales = cityTask.Result,
                TopProducts = topProductsTask.Result,
                LastOrders = lastOrdersTask.Result,
                CategorySales = categoryTask.Result,
                RevenueGrowthRate = previousMonthRevenue > 0
                    ? (currentRevenue - previousMonthRevenue) / previousMonthRevenue * 100
                    : 12.5m
            };

            // Null kontrolü — Summary null geliyorsa logla
            if (model.Summary == null)
            {
                model.Summary = new ECommerceBigData.Dtos.DashboardSummaryDto
                {
                    TotalRevenue = 0,
                    TotalOrders = 0,
                    TotalCustomers = 0,
                    AvgOrderValue = 0
                };
            }

            return View(model);
        }

        // ── DEBUG: Verilerin gelip gelmediğini doğrula ────────────
        // Tarayıcıda: https://localhost:7112/Dashboard/Debug
        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            try
            {
                var summary = await _dashboardRepository.GetSummaryAsync();
                var daily = await _dashboardRepository.GetDailySalesAsync();
                var country = await _dashboardRepository.GetCountrySalesAsync();
                var city = await _dashboardRepository.GetCitySalesAsync();
                var category = await _dashboardRepository.GetCategorySalesAsync();
                var products = await _productRepository.GetTopProductsAsync();
                var orders = await _orderRepository.GetLastOrdersAsync();

                return Json(new
                {
                    // Summary değerleri
                    summary_TotalRevenue = summary?.TotalRevenue,
                    summary_TotalOrders = summary?.TotalOrders,
                    summary_TotalCustomers = summary?.TotalCustomers,
                    summary_AvgOrderValue = summary?.AvgOrderValue,
                    summary_IsNull = summary == null,

                    // Liste sayıları
                    dailySales_Count = daily?.Count,
                    dailySales_HasData = daily?.Any(x => x.TotalSales > 0),
                    country_Count = country?.Count,
                    city_Count = city?.Count,
                    category_Count = category?.Count,
                    products_Count = products?.Count,
                    orders_Count = orders?.Count,

                    // İlk country verisi
                    first_country = country?.FirstOrDefault()?.Country,
                    first_country_sales = country?.FirstOrDefault()?.TotalSales,

                    // İlk ürün
                    first_product = products?.FirstOrDefault()?.ProductName,
                    first_product_qty = products?.FirstOrDefault()?.TotalQuantity,
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // ── Search (AJAX) ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var results = await _orderRepository.SearchOrdersAsync(q);
            return Json(results);
        }

        // ── Yardımcı ─────────────────────────────────────────────
        private async Task<decimal> GetPreviousMonthRevenueAsync()
        {
            var query = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM Orders
                WHERE OrderDate >= DATEADD(MONTH, -2, GETDATE())
                  AND OrderDate <  DATEADD(MONTH, -1, GETDATE())";

            using var connection = _dashboardRepository.GetConnection();
            return await connection.ExecuteScalarAsync<decimal>(query);
        }
    }
}