using Dapper;
using ECommerceBigData.Data.Context;
using ECommerceBigData.Data.Repositories.CustomerRepositories;
using ECommerceBigData.Dtos;
using ECommerceBigData.Models;

namespace ECommerceBigData.Data.Repositories.ProductRepositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context) => _context = context;

        public async Task<List<TopProductDto>> GetTopProductsAsync(DashboardFilters? filters = null)
        {
            filters ??= new DashboardFilters();
            var sql = @"
                SELECT TOP 10
                    p.Name AS ProductName,
                    ISNULL(SUM(od.Quantity), 0) AS TotalQuantity
                FROM Products p WITH(NOLOCK)
                LEFT JOIN OrderDetails od WITH(NOLOCK) ON od.ProductId = p.Id
 LEFT JOIN Orders o WITH(NOLOCK) ON o.Id = od.OrderId
                LEFT JOIN Categories c WITH(NOLOCK) ON c.Id = p.CategoryId
                WHERE (o.Id IS NULL OR (
                    o.OrderDate >= @from AND o.OrderDate < @to
                    AND (@country IS NULL OR o.Country = @country)
                    AND (@city IS NULL OR o.City = @city)
                ))
                  AND (@category IS NULL OR c.Name = @category)
                GROUP BY p.Name
                HAVING ISNULL(SUM(od.Quantity), 0) > 0
                ORDER BY TotalQuantity DESC";

            using var conn = _context.CreateConnection();
            var result = (await conn.QueryAsync<TopProductDto>(sql, filters.ToSqlParams())).ToList();

            if (!result.Any())
            {
                result = Enumerable.Range(1, 5)
                    .Select(i => new TopProductDto { ProductName = $"Ürün {i}", TotalQuantity = 50 - i * 5 })
                    .ToList();
            }
            return result;
        }

        public async Task<PagedResult<ProductListDto>> GetPagedProductsAsync(
            int page, int pageSize,
            int? categoryId = null,
            string? sortBy = null)
        {
            var where = categoryId.HasValue ? "WHERE p.CategoryId = @categoryId" : string.Empty;

            var orderBy = sortBy?.ToLower() switch
            {
                "price_asc" => "p.Price ASC",
                "price_desc" => "p.Price DESC",
                "rating" => "avg_rating DESC",
                "bestseller" => "total_sold DESC",
                _ => "total_sold DESC"
            };

            var countSql = $"SELECT COUNT(*) FROM Products p WITH(NOLOCK) {where}";

            var dataSql = $@"
                SELECT
                    p.Id,
                    p.Name,
                    cat.Name                       AS CategoryName,
                    p.Price,
                    p.Stock,
                    ISNULL(SUM(od.Quantity), 0)    AS total_sold,
                    ISNULL(AVG(CAST(r.Rating AS FLOAT)), 0) AS avg_rating,
                    COUNT(DISTINCT r.Id)            AS ReviewCount
                FROM Products p WITH(NOLOCK)
                JOIN Categories cat WITH(NOLOCK) ON cat.Id = p.CategoryId
                LEFT JOIN OrderDetails od WITH(NOLOCK) ON od.ProductId = p.Id
                LEFT JOIN Reviews r WITH(NOLOCK) ON r.ProductId = p.Id
                {where}
                GROUP BY p.Id, p.Name, cat.Name, p.Price, p.Stock
                ORDER BY {orderBy}
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var prms = new { categoryId, offset = (page - 1) * pageSize, pageSize };

            using var conn = _context.CreateConnection();
            var total = await conn.ExecuteScalarAsync<int>(countSql, prms);
            var items = (await conn.QueryAsync<ProductListDto>(dataSql, prms)).ToList();

            return new PagedResult<ProductListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}