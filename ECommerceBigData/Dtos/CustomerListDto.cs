namespace ECommerceBigData.Dtos
{
    public class CustomerListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Segment { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpend { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }
}
