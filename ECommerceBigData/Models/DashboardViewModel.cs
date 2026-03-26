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
        public MonthlyTrendDto MonthlyTrend { get; set; }
    }
}

