using ECommerceBigData.Data.Repositories.CustomerRepositories;
using ECommerceBigData.Dtos;
using ECommerceBigData.Models;


namespace ECommerceBigData.Data.Repositories.ProductRepositories
{
    public interface IProductRepository
    {
        Task<List<TopProductDto>> GetTopProductsAsync(DashboardFilters? filters = null);
        Task<PagedResult<ProductListDto>> GetPagedProductsAsync(int page, int pageSize, int? categoryId = null, string? sortBy = null);
    }
}
