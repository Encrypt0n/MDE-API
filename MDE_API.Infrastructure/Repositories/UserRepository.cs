using MDE_API.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MDE_API.Domain.Models;

namespace MDE_API.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public bool UserExists(string username)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

            var paramUsername = cmd.CreateParameter();
            paramUsername.ParameterName = "@Username";
            paramUsername.Value = username ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramUsername);

            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public void CreateUser(string username, string passwordHash, int companyId)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, CompanyID) VALUES (@Username, @PasswordHash, @CompanyID)";

            var paramUsername = cmd.CreateParameter();
            paramUsername.ParameterName = "@Username";
            paramUsername.Value = username ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramUsername);

            var paramPasswordHash = cmd.CreateParameter();
            paramPasswordHash.ParameterName = "@PasswordHash";
            paramPasswordHash.Value = passwordHash ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramPasswordHash);

            var paramCompanyId = cmd.CreateParameter();
            paramCompanyId.ParameterName = "@CompanyID";
            paramCompanyId.Value = companyId;
            cmd.Parameters.Add(paramCompanyId);

            cmd.ExecuteNonQuery();
        }

        public UserWithPasswordHash GetUserWithPasswordHash(string username)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT UserID, PasswordHash, Role, CompanyID FROM Users WHERE Username = @Username";

            var paramUsername = cmd.CreateParameter();
            paramUsername.ParameterName = "@Username";
            paramUsername.Value = username ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramUsername);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserWithPasswordHash
                {
                    UserID = reader.GetInt32(0),
                    PasswordHash = reader.GetString(1),
                    Role = reader.GetInt32(2),
                    CompanyID = reader.GetInt32(3)
                };
            }

            return null;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = new List<User>();

            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT UserID, Username, CompanyID FROM Users";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    CompanyID = reader.GetInt32(2)
                });
            }

            return users;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT UserID, Username, Role, CompanyID FROM Users WHERE UserID = @UserID";

            var paramUserId = cmd.CreateParameter();
            paramUserId.ParameterName = "@UserID";
            paramUserId.Value = id;
            cmd.Parameters.Add(paramUserId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Role = reader.GetInt32(2),
                    CompanyID = reader.GetInt32(3)
                };
            }

            return null;
        }
    }
}
