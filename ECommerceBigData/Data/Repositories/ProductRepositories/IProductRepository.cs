using ECommerceBigData.Dtos;

namespace ECommerceBigData.Data.Repositories.ProductRepositories
{
    public interface IProductRepository
    {
        Task<List<TopProductDto>> GetTopProductsAsync();
    }
}
