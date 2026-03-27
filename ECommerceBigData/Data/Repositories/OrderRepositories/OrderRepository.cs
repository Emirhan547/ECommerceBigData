using ECommerceBigData.Data.Context;
using ECommerceBigData.Dtos;
using Dapper;

namespace ECommerceBigData.Data.Repositories.OrderRepositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        public OrderRepository(AppDbContext context) => _context = context;

        public async Task<int> GetTotalOrderCountAsync()
        {
            using var conn = _context.CreateConnection();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Orders");
        }

        public async Task<List<LastOrderDto>> GetLastOrdersAsync()
        {
            var sql = @"
                SELECT TOP 10
                    o.Id           AS OrderId,
                    c.FullName     AS CustomerName,
                    o.TotalAmount,
                    o.OrderDate,
                    o.Status
               FROM Orders o
                JOIN Customers c ON c.Id = o.CustomerId
                ORDER BY o.OrderDate DESC";

            using var conn = _context.CreateConnection();
            return (await conn.QueryAsync<LastOrderDto>(sql)).ToList();
        }

        /// <summary>
        /// Sayfalı sipariş listesi — durum ve tarih filtreli
        /// </summary>
        public async Task<PagedOrderDto> GetPagedOrdersAsync(
            int page, int pageSize,
            string? status = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var whereClauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(status)) whereClauses.Add("o.Status = @status");
            if (from.HasValue) whereClauses.Add("o.OrderDate >= @from");
            if (to.HasValue) whereClauses.Add("o.OrderDate <= @to");

            var where = whereClauses.Count > 0
                ? "WHERE " + string.Join(" AND ", whereClauses)
                : string.Empty;

            var countSql = $"SELECT COUNT(*) FROM Orders o {where}";

            var dataSql = $@"
                SELECT
                    o.Id           AS OrderId,
                    c.FullName     AS CustomerName,
                    o.Country,
                    o.City,
                    o.TotalAmount,
                    o.OrderDate,
                    o.Status
               FROM Orders o
                JOIN Customers c ON c.Id = o.CustomerId
                {where}
                ORDER BY o.OrderDate DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY";

            var prms = new
            {
                status,
                from,
                to,
                offset = (page - 1) * pageSize,
                pageSize
            };

            using var conn = _context.CreateConnection();
            var total = await conn.ExecuteScalarAsync<int>(countSql, prms);
            var items = (await conn.QueryAsync<OrderSearchDto>(dataSql, prms)).ToList();

            return new PagedOrderDto
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Sipariş no veya müşteri adına göre hızlı arama
        /// </summary>
        public async Task<List<OrderSearchDto>> SearchOrdersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();

            // Sayısal ise ID araması, değilse isim araması
            var isNumeric = int.TryParse(query.Trim(), out int orderId);

            var sql = isNumeric
                ? @"SELECT TOP 10 o.Id AS OrderId, c.FullName AS CustomerName,
                           o.Country, o.City, o.TotalAmount, o.OrderDate, o.Status
                   FROM Orders o JOIN Customers c ON c.Id = o.CustomerId
                    WHERE o.Id = @orderId"
                : @"SELECT TOP 10 o.Id AS OrderId, c.FullName AS CustomerName,
                           o.Country, o.City, o.TotalAmount, o.OrderDate, o.Status
                    FROM Orders o JOIN Customers c ON c.Id = o.CustomerId
                    WHERE c.FullName LIKE @like
                    ORDER BY o.OrderDate DESC";

            using var conn = _context.CreateConnection();
            return (await conn.QueryAsync<OrderSearchDto>(sql,
                isNumeric
                    ? (object)new { orderId }
                    : new { like = $"%{query}%" }
            )).ToList();
        }
    }
}
