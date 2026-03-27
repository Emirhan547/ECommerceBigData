using ECommerceBigData.Data.Repositories.OrderRepositories;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ECommerceBigData.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orders;
        public OrdersController(IOrderRepository orders) => _orders = orders;

        // GET /Orders?page=1&status=Delivered&from=2024-01-01
        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 20,
            string? status = null,
            DateTime? from = null,
            DateTime? to = null,
            string? query = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 10, 100);

            var result = await _orders.GetPagedOrdersAsync(page, pageSize, status, from, to);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var matched = await _orders.SearchOrdersAsync(query);
                if (matched.Count > 0)
                {
                    result.Items = matched;
                    result.TotalCount = matched.Count;
                    result.Page = 1;
                    result.PageSize = Math.Max(matched.Count, 1);
                }
}

ViewBag.Status = status;
ViewBag.From = from;
ViewBag.To = to;
ViewBag.Query = query;
return View(result);
        }
    }
}
