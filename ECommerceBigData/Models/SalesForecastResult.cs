namespace ECommerceBigData.Models
{
    public class SalesForecastResult
    {
        public List<ForecastPointViewModel> Points { get; set; } = new();
        public List<ForecastMetricCard> Metrics { get; set; } = new();
    }
}
