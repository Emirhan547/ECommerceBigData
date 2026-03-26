namespace ECommerceBigData.Dtos
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int TotalSold { get; set; }
        public double AvgRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
