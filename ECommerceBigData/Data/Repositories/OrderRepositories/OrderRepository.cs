using ECommerceBigData.Data.Context;
using ECommerceBigData.Dtos;
using Dapper;

namespace ECommerceBigData.Data.Repositories.OrderRepositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalOrderCountAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Orders");
        }

        public async Task<List<LastOrderDto>> GetLastOrdersAsync()
        {
            var query = @"
        SELECT TOP 10 
            o.Id AS OrderId,
            c.FullName AS CustomerName,
            o.TotalAmount,
            o.OrderDate
        FROM Orders o
        JOIN Customers c ON c.Id = o.CustomerId
        ORDER BY o.OrderDate DESC";

            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<LastOrderDto>(query)).ToList();
        }
    }
}
