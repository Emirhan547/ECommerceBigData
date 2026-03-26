using Dapper;
using ECommerceBigData.Data.Context;
using ECommerceBigData.Dtos;

namespace ECommerceBigData.Data.Repositories.CustomerRepositories
{
    public class CustomerRepository:ICustomerRepository
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context) => _context = context;

        public async Task<int> GetTotalCustomerCountAsync()
        {
            using var conn = _context.CreateConnection();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Customers WITH(NOLOCK)");
        }

        public async Task<PagedResult<CustomerListDto>> GetPagedCustomersAsync(
             int page, int pageSize,
             string? segment = null, string? country = null)
        {
            var whereClauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(segment)) whereClauses.Add("cs.Segment = @segment");
            if (!string.IsNullOrWhiteSpace(country)) whereClauses.Add("c.Country = @country");

            var where = whereClauses.Count > 0
                ? "WHERE " + string.Join(" AND ", whereClauses)
                : string.Empty;

            var countSql = $@"
                SELECT COUNT(*)
                FROM Customers c WITH(NOLOCK)
                LEFT JOIN CustomerSegments cs WITH(NOLOCK) ON cs.CustomerId = c.Id
                {where}";

            var dataSql = $@"
                SELECT
                    c.Id,
                    c.FullName,
                    c.Email,
                    c.Country,
                    c.City,
                    ISNULL(cs.Segment, 'Regular')             AS Segment,
                    ISNULL(stats.order_count, 0)               AS TotalOrders,
                    ISNULL(stats.total_spend, 0)               AS TotalSpend,
                    stats.last_order_date                      AS LastOrderDate
                FROM Customers c WITH(NOLOCK)
                LEFT JOIN CustomerSegments cs WITH(NOLOCK) ON cs.CustomerId = c.Id
                LEFT JOIN (
                    SELECT CustomerId,
                           COUNT(*) AS order_count,
                           SUM(TotalAmount) AS total_spend,
                           MAX(OrderDate) AS last_order_date
                    FROM Orders WITH(NOLOCK)
                    GROUP BY CustomerId
                ) stats ON stats.CustomerId = c.Id
                {where}
                ORDER BY stats.total_spend DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var prms = new { segment, country, offset = (page - 1) * pageSize, pageSize };

            using var conn = _context.CreateConnection();
            var total = await conn.ExecuteScalarAsync<int>(countSql, prms);
            var items = (await conn.QueryAsync<CustomerListDto>(dataSql, prms)).ToList();

            return new PagedResult<CustomerListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

    }
}
