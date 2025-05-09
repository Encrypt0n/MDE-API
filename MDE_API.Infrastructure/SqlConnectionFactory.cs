using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MDE_API.Application.Interfaces;
using System.Data;


namespace MDE_API.Infrastructure
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
