
using ECommerceBigData.Data.Context;
using ECommerceBigData.Data.Repositories.CustomerRepositories;
using ECommerceBigData.Data.Repositories.DashboardRepositories;
using ECommerceBigData.Data.Repositories.OrderRepositories;
using ECommerceBigData.Data.Repositories.ProductRepositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<AppDbContext>();

// Repository DI kayýtlarý
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

// Cache (2M satýr sorgularýný hafifletmek için)
builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// AJAX arama endpoint'i
app.MapControllerRoute(
    name: "search",
    pattern: "Dashboard/Search",
    defaults: new { controller = "Dashboard", action = "Search" });

app.Run();