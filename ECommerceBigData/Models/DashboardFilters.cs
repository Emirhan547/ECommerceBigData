namespace ECommerceBigData.Models
{
    public class DashboardFilters
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Category { get; set; }
        public string? Segment { get; set; }

        public DateTime EffectiveFrom => From?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        public DateTime EffectiveTo => To?.Date ?? DateTime.UtcNow.Date;

        public object ToSqlParams() => new
        {
            from = EffectiveFrom,
            to = EffectiveTo.AddDays(1), // inclusive end-of-day
            country = string.IsNullOrWhiteSpace(Country) ? null : Country,
            city = string.IsNullOrWhiteSpace(City) ? null : City,
            category = string.IsNullOrWhiteSpace(Category) ? null : Category,
            segment = string.IsNullOrWhiteSpace(Segment) ? null : Segment
        };
    }
}