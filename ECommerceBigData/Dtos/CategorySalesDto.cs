namespace ECommerceBigData.Dtos
{
    public class CategorySalesDto
    {
        public string CategoryName { get; set; }
        public decimal TotalSales { get; set; }
        public double Percentage { get; set; }
        public string Icon { get; set; } = "category";
    }
}
