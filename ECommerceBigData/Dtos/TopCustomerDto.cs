namespace ECommerceBigData.Dtos
{
    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int TotalOrders { get; set; }
        public decimal LifetimeValue { get; set; }
        public string Segment { get; set; }
    }
}
