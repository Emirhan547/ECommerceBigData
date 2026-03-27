using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace ECommerceBigData.ML;

public class SalesForecastTrainer
{
    private readonly MLContext _mlContext = new(seed: 7);

    public (TimeSeriesPredictionEngine<SalesForecastInput, SalesForecastOutput> Engine, double Mae, double Rmse) Train(
        IReadOnlyList<float> series,
        int horizon)
    {
        if (series.Count < 12)
        {
            throw new InvalidOperationException("Tahmin üretmek için en az 12 aylık veri gereklidir.");
        }

        var observations = series.Select(value => new SalesForecastInput { Value = value });
        var dataView = _mlContext.Data.LoadFromEnumerable(observations);

        const int windowSize = 6;
        const int seriesLength = 12;
        const int trainSize = 24;

        IEstimator<ITransformer> pipeline = _mlContext.Forecasting.ForecastBySsa(
            outputColumnName: nameof(SalesForecastOutput.ForecastedValues),
            inputColumnName: nameof(SalesForecastInput.Value),
            windowSize: Math.Min(windowSize, series.Count - 1),
            seriesLength: Math.Min(seriesLength, series.Count),
            trainSize: Math.Min(trainSize, series.Count),
            horizon: horizon,
            confidenceLevel: 0.95f,
            confidenceLowerBoundColumn: nameof(SalesForecastOutput.LowerBoundValues),
            confidenceUpperBoundColumn: nameof(SalesForecastOutput.UpperBoundValues));

        var model = pipeline.Fit(dataView);

        var engine = model.CreateTimeSeriesEngine<SalesForecastInput, SalesForecastOutput>(_mlContext);
        var (mae, rmse) = Evaluate(series, horizon);

        return (engine, mae, rmse);
    }

    private static (double Mae, double Rmse) Evaluate(IReadOnlyList<float> series, int horizon)
    {
        var lastWindow = series.TakeLast(Math.Max(horizon, 1)).ToArray();
        if (lastWindow.Length == 0)
        {
            return (0, 0);
        }

        var mean = series.Average();
        var errors = lastWindow.Select(v => Math.Abs(v - mean)).ToArray();
        var mae = errors.Average();
        var rmse = Math.Sqrt(errors.Select(e => e * e).Average());
        return (mae, rmse);
    }
}