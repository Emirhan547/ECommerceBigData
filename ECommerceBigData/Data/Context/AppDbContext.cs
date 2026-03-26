using Microsoft.Data.SqlClient;
using System.Data;

namespace ECommerceBigData.Data.Context
{
    public class AppDbContext
    {
        private readonly IConfiguration _configuration;

        public AppDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IDbConnection CreateConnection()
        => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }
}
