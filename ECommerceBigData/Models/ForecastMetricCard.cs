namespace ECommerceBigData.Models
{
    public class ForecastMetricCard
    {
        public string HorizonLabel { get; set; } = string.Empty;
        public double Mae { get; set; }
        public double Rmse { get; set; }
    }
}
