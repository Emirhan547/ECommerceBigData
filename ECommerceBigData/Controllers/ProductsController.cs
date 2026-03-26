using ECommerceBigData.Data.Repositories.ProductRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBigData.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _products;
        public ProductsController(IProductRepository products) => _products = products;

        public async Task<IActionResult> Index(
            int page = 1, int pageSize = 20,
            int? categoryId = null, string? sortBy = null)
        {
            var result = await _products.GetPagedProductsAsync(page, pageSize, categoryId, sortBy);
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            return View(result);
        }
    }
}