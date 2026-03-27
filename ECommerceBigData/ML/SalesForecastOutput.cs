namespace ECommerceBigData.ML
{
    public class SalesForecastOutput
    {
        public float[] ForecastedValues { get; set; } = Array.Empty<float>();
        public float[] LowerBoundValues { get; set; } = Array.Empty<float>();
        public float[] UpperBoundValues { get; set; } = Array.Empty<float>();
    }
}
