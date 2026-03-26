namespace ECommerceBigData.Dtos
{
    public class MonthlyTrendDto
    {
        public decimal RevenueGrowthRate { get; set; }
        public decimal OrdersGrowthRate { get; set; }
        public decimal CustomersGrowthRate { get; set; } = 5.2m;
    }
}
