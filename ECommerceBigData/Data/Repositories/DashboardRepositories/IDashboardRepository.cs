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
        Task<List<HourlyOrderDto>> GetHourlyOrderHeatmapAsync(DashboardFilters filters);
        Task<List<TopCustomerDto>> GetTopCustomersAsync(DashboardFilters filters, int count = 5);
        Task<KpiMetricsDto> GetKpiMetricsAsync(DashboardFilters filters);
        Task<List<SegmentDistributionDto>> GetSegmentDistributionAsync(DashboardFilters filters);
        Task<List<string>> GetCountryFilterOptionsAsync();
        Task<List<string>> GetCityFilterOptionsAsync(string? country = null);
        Task<List<string>> GetCategoryFilterOptionsAsync();
        Task<List<string>> GetSegmentFilterOptionsAsync();
        IDbConnection GetConnection();

       
    }

}