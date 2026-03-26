using ECommerceBigData.Dtos;
using System.Linq.Dynamic.Core;

namespace ECommerceBigData.Data.Repositories.CustomerRepositories
{
    public interface ICustomerRepository
    {
        Task<int> GetTotalCustomerCountAsync();
        Task<PagedResult<CustomerListDto>> GetPagedCustomersAsync(int page, int pageSize, string? segment = null, string? country = null);
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}