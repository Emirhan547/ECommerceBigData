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
        Task<MonthlyTrendDto> GetMonthlyTrendAsync();
    }
}
