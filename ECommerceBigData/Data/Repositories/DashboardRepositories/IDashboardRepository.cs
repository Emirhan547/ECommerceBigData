using System.Data;
using ECommerceBigData.Dtos;

namespace ECommerceBigData.Data.Repositories.DashboardRepositories
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<List<DailySalesDto>> GetDailySalesAsync();
        Task<List<CountrySalesDto>> GetCountrySalesAsync();
        Task<List<CitySalesDto>> GetCitySalesAsync();
        Task<List<CategorySalesDto>> GetCategorySalesAsync();
        IDbConnection GetConnection();

        // Yeni
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync();
        Task<List<HourlyOrderDto>> GetHourlyOrderHeatmapAsync();
        Task<List<TopCustomerDto>> GetTopCustomersAsync(int count = 5);
        Task<KpiMetricsDto> GetKpiMetricsAsync();
        Task<List<SegmentDistributionDto>> GetSegmentDistributionAsync();
    }
}