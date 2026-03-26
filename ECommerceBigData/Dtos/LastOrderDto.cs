namespace ECommerceBigData.Dtos
{
    public class LastOrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
