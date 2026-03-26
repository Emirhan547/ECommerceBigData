using ECommerceBigData.Data.Repositories.CustomerRepositories;
using ECommerceBigData.Dtos;


namespace ECommerceBigData.Data.Repositories.ProductRepositories
{
    public interface IProductRepository
    {
        Task<List<TopProductDto>> GetTopProductsAsync();
        Task<PagedResult<ProductListDto>> GetPagedProductsAsync(int page, int pageSize, int? categoryId = null, string? sortBy = null);
    }
}
