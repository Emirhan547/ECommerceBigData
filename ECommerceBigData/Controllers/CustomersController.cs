using ECommerceBigData.Data.Repositories.CustomerRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBigData.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerRepository _customers;
        public CustomersController(ICustomerRepository customers) => _customers = customers;

        public async Task<IActionResult> Index(
            int page = 1, int pageSize = 20,
            string? segment = null, string? country = null)
        {
            var result = await _customers.GetPagedCustomersAsync(page, pageSize, segment, country);
            ViewBag.Segment = segment;
            ViewBag.Country = country;
            return View(result);
        }
    }
}