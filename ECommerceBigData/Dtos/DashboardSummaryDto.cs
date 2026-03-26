namespace ECommerceBigData.Dtos
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AvgOrderValue { get; set; }
    }
}
