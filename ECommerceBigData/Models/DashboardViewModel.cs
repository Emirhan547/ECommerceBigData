using ECommerceBigData.Dtos;

namespace ECommerceBigData.Models
{
    public class DashboardViewModel
    {
        public DashboardSummaryDto Summary { get; set; }
        public List<DailySalesDto> DailySales { get; set; }
        public List<TopProductDto> TopProducts { get; set; }
        public List<CountrySalesDto> CountrySales { get; set; }
        public List<CitySalesDto> CitySales { get; set; }
        public List<LastOrderDto> LastOrders { get; set; }
        public List<CategorySalesDto> CategorySales { get; set; }
        public decimal RevenueGrowthRate { get; set; } = 12.5m;
        public decimal OrdersGrowthRate { get; set; } = 4.2m;
        public decimal CustomersGrowthRate { get; set; } = 5.2m;

        // Yeni
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; }
        public List<HourlyOrderDto> HourlyHeatmap { get; set; }
        public List<TopCustomerDto> TopCustomers { get; set; }
        public KpiMetricsDto KpiMetrics { get; set; }
        public List<SegmentDistributionDto> SegmentDistribution { get; set; }
    }
}