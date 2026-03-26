using ECommerceBigData.Data.Repositories.OrderRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBigData.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orders;
        public OrdersController(IOrderRepository orders) => _orders = orders;

        // GET /Orders?page=1&status=Delivered&from=2024-01-01
        public async Task<IActionResult> Index(
            int page = 1, int pageSize = 20,
            string? status = null,
            DateTime? from = null, DateTime? to = null)
        {
            var result = await _orders.GetPagedOrdersAsync(page, pageSize, status, from, to);
            ViewBag.Status = status;
            ViewBag.From = from;
            ViewBag.To = to;
            return View(result);
        }
    }
}