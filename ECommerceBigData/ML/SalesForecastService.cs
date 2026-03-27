using ECommerceBigData.Data.Repositories.DashboardRepositories;
using ECommerceBigData.Models;

namespace ECommerceBigData.ML;

public class SalesForecastService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly SalesForecastTrainer _trainer;

    public SalesForecastService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
        _trainer = new SalesForecastTrainer();
    }

    public async Task<SalesForecastResult> BuildForecastAsync(DashboardFilters filters)
    {
        var monthlyData = await _dashboardRepository.GetMonthlyDataAsync(filters, 36);
        var ordered = monthlyData.OrderBy(x => x.MonthStart).ToList();

        if (!ordered.Any())
        {
            return new SalesForecastResult();
        }

        var result = new SalesForecastResult
        {
            Points = ordered.Select(x => new ForecastPointViewModel
            {
                MonthStart = x.MonthStart,
                Revenue = (float)x.TotalRevenue,
                Orders = x.TotalOrders,
                IsForecast = false
            }).ToList()
        };

        var horizons = new[] { 3, 6, 12 };
        var revenueSeries = ordered.Select(x => (float)x.TotalRevenue).ToList();
        var ordersSeries = ordered.Select(x => (float)x.TotalOrders).ToList();

        foreach (var horizon in horizons)
        {
            var (revenueEngine, revenueMae, revenueRmse) = _trainer.Train(revenueSeries, horizon);
            var (ordersEngine, _, _) = _trainer.Train(ordersSeries, horizon);
            var revenuePrediction = revenueEngine.Predict();
            var ordersPrediction = ordersEngine.Predict();

            var startMonth = ordered.Last().MonthStart;
            for (var i = 0; i < horizon; i++)
            {
                result.Points.Add(new ForecastPointViewModel
                {
                    MonthStart = startMonth.AddMonths(i + 1),
                    Revenue = revenuePrediction.ForecastedValues.ElementAtOrDefault(i),
                    Orders = ordersPrediction.ForecastedValues.ElementAtOrDefault(i),
                    IsForecast = true
                });
            }

            result.Metrics.Add(new ForecastMetricCard
            {
                HorizonLabel = $"{horizon} Ay",
                Mae = Math.Round(revenueMae, 2),
                Rmse = Math.Round(revenueRmse, 2)
            });
        }

        result.Points = result.Points
            .OrderBy(x => x.MonthStart)
            .GroupBy(x => new { x.MonthStart, x.IsForecast })
            .Select(x => x.Last())
            .ToList();

        return result;
    }
}