using System.Data;
using ECommerceBigData.Dtos;
using ECommerceBigData.Models;

namespace ECommerceBigData.Data.Repositories.DashboardRepositories
{
    public interface IDashboardRepository
    {

        Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilters filters);
        Task<List<DailySalesDto>> GetDailySalesAsync(DashboardFilters filters);
        Task<List<CountrySalesDto>> GetCountrySalesAsync(DashboardFilters filters);
        Task<List<CitySalesDto>> GetCitySalesAsync(DashboardFilters filters);
        Task<List<CategorySalesDto>> GetCategorySalesAsync(DashboardFilters filters);
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(DashboardFilters filters);
        Task<List<MonthlyDataDto>> GetMonthlyDataAsync(DashboardFilters filters, int monthCount = 36);
        Task<List<HourlyOrderDto>> GetHourlyOrderHeatmapAsync(DashboardFilters filters);
        Task<List<TopCustomerDto>> GetTopCustomersAsync(DashboardFilters filters, int count = 5);
        Task<KpiMetricsDto> GetKpiMetricsAsync(DashboardFilters filters);
        Task<EntityOverviewDto> GetEntityOverviewAsync(DashboardFilters filters);
        Task<List<SegmentDistributionDto>> GetSegmentDistributionAsync(DashboardFilters filters);
        Task<List<string>> GetCountryFilterOptionsAsync(DashboardFilters filters);
        Task<List<string>> GetCityFilterOptionsAsync(DashboardFilters filters);
        Task<List<string>> GetCategoryFilterOptionsAsync(DashboardFilters filters);
        Task<List<string>> GetSegmentFilterOptionsAsync(DashboardFilters filters);
        IDbConnection GetConnection();

    }

}