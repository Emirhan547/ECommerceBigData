using System.Globalization;
using ECommerceBigData.Data.Context;
using ECommerceBigData.Data.Repositories.CustomerRepositories;
using ECommerceBigData.Data.Repositories.DashboardRepositories;
using ECommerceBigData.Data.Repositories.OrderRepositories;
using ECommerceBigData.Data.Repositories.ProductRepositories;
using ECommerceBigData.ML;
using ECommerceBigData.Services.Insights;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddLocalization(options => options.ResourcesPath = "Resources")
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddScoped<AppDbContext>();

// Repository DI kaytlar
// Repository DI kayýtlarý
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

// Cache (2M satr sorgularn hafifletmek iin)
// Servis kayýtlarý
builder.Services.AddScoped<SalesForecastService>();
builder.Services.AddScoped<IExecutiveInsightService, ExecutiveInsightService>();

// Cache (aðýr dashboard sorgularý için)
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var supportedCultures = new[]
{
    new CultureInfo("tr-TR"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

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