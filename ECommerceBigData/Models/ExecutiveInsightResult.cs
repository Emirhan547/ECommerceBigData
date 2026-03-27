namespace ECommerceBigData.Models
{
    public class ExecutiveInsightResult
    {
        public string YoneticiOzeti { get; set; } = string.Empty;
        public string RiskDegerlendirmesi { get; set; } = string.Empty;
        public string AksiyonOnerileri { get; set; } = string.Empty;
        public DateTime UretimZamani { get; set; } = DateTime.UtcNow;
    }
}
