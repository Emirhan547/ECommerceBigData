using ECommerceBigData.Dtos;

namespace ECommerceBigData.Models;

public class DashboardViewModel
{

    public DashboardSummaryDto Summary { get; set; } = new();
    public List<DailySalesDto> DailySales { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<CountrySalesDto> CountrySales { get; set; } = new();
    public List<CitySalesDto> CitySales { get; set; } = new();
    public List<LastOrderDto> LastOrders { get; set; } = new();
    public List<CategorySalesDto> CategorySales { get; set; } = new();
    public EntityOverviewDto EntityOverview { get; set; } = new();
    public decimal RevenueGrowthRate { get; set; }
    public decimal OrdersGrowthRate { get; set; }
    public decimal CustomersGrowthRate { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    public List<HourlyOrderDto> HourlyHeatmap { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
    public KpiMetricsDto KpiMetrics { get; set; } = new();
    public List<SegmentDistributionDto> SegmentDistribution { get; set; } = new();


    public DashboardFilters Filters { get; set; } = new();
    public List<string> AvailableCountries { get; set; } = new();
    public List<string> AvailableCities { get; set; } = new();
    public List<string> AvailableCategories { get; set; } = new();
    public List<string> AvailableSegments { get; set; } = new();


    public string CurrentPage { get; set; } = "Genel Bakış";
    public SalesForecastResult? ForecastResult { get; set; }
    public ExecutiveInsightResult? ExecutiveInsight { get; set; }
}
