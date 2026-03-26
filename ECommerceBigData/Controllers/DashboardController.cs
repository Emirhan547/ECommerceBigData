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

        public async Task<IActionResult> Index()
        {
            // Tüm task'ları paralel çalıştır
            var summaryTask = _dashboardRepository.GetSummaryAsync();
            var dailySalesTask = _dashboardRepository.GetDailySalesAsync();
            var countryTask = _dashboardRepository.GetCountrySalesAsync();
            var cityTask = _dashboardRepository.GetCitySalesAsync();
            var categoryTask = _dashboardRepository.GetCategorySalesAsync();
            var monthlyTrendTask = _dashboardRepository.GetMonthlyTrendAsync();
            var topProductsTask = _productRepository.GetTopProductsAsync();
            var lastOrdersTask = _orderRepository.GetLastOrdersAsync();

            await Task.WhenAll(
                summaryTask,
                dailySalesTask,
                countryTask,
                cityTask,
                categoryTask,
                monthlyTrendTask,
                topProductsTask,
                lastOrdersTask
            );

            // Kategori satışlarına yüzde hesapla
            var categorySales = categoryTask.Result;
            var totalCategorySales = categorySales.Sum(x => x.TotalSales);
            foreach (var category in categorySales)
            {
                category.Percentage = totalCategorySales > 0
                    ? (double)(category.TotalSales / totalCategorySales * 100)
                    : 0;

                // İkonları kategorilere göre ayarla
                category.Icon = GetCategoryIcon(category.CategoryName);
            }

            var model = new DashboardViewModel
            {
                Summary = summaryTask.Result,
                DailySales = dailySalesTask.Result,
                CountrySales = countryTask.Result,
                CitySales = cityTask.Result,
                TopProducts = topProductsTask.Result,
                LastOrders = lastOrdersTask.Result,
                CategorySales = categorySales,
                MonthlyTrend = monthlyTrendTask.Result
            };

            return View(model);
        }

        private string GetCategoryIcon(string categoryName)
        {
            return categoryName?.ToLower() switch
            {
                "electronics" => "devices",
                "clothing" => "checkroom",
                "home" => "home",
                "sports" => "sports_soccer",
                "books" => "menu_book",
                _ => "category"
            };
        }
    }
}