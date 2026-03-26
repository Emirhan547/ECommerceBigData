using ECommerceBigData.Dtos;

namespace ECommerceBigData.Data.Repositories.OrderRepositories
{
    public interface IOrderRepository
    {
        Task<List<LastOrderDto>> GetLastOrdersAsync();
        Task<int> GetTotalOrderCountAsync();
    }
}
