namespace ECommerceBigData.Dtos
{
    public class OrderSearchDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}
