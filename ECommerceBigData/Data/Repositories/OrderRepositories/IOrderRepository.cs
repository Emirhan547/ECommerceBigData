using ECommerceBigData.Dtos;

namespace ECommerceBigData.Data.Repositories.OrderRepositories
{
    public interface IOrderRepository
    {
        Task<List<LastOrderDto>> GetLastOrdersAsync();
        Task<int> GetTotalOrderCountAsync();
        Task<PagedOrderDto> GetPagedOrdersAsync(int page, int pageSize, string? status = null, DateTime? from = null, DateTime? to = null);
        Task<List<OrderSearchDto>> SearchOrdersAsync(string query);
    }
}
