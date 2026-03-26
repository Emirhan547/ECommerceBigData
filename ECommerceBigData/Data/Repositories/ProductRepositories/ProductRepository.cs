using Dapper;
using ECommerceBigData.Data.Context;
using ECommerceBigData.Dtos;

namespace ECommerceBigData.Data.Repositories.ProductRepositories
{
    public class ProductRepository:IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync()
        {
            var query = @"
        SELECT TOP 10 
            p.Name AS ProductName,
            SUM(od.Quantity) AS TotalQuantity
        FROM OrderDetails od
        JOIN Products p ON p.Id = od.ProductId
        GROUP BY p.Name
        ORDER BY TotalQuantity DESC";

            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<TopProductDto>(query)).ToList();
        }
    }
}
