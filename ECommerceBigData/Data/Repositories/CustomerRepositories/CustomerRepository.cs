using Dapper;
using ECommerceBigData.Data.Context;

namespace ECommerceBigData.Data.Repositories.CustomerRepositories
{
    public class CustomerRepository:ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCustomerCountAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Customers");
        }

    }
}
