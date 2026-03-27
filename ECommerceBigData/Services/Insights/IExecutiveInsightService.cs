using ECommerceBigData.Models;

namespace ECommerceBigData.Services.Insights
{
    public interface IExecutiveInsightService
    {
        Task<ExecutiveInsightResult> GenerateAsync(DashboardViewModel model);
    }
}
