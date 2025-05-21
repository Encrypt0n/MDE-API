using MDE_API.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
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
            AddParam(cmd.Parameters, "@Username", username);

            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public void CreateUser(string username, string passwordHash)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
            AddParam(cmd.Parameters, "@Username", username);
            AddParam(cmd.Parameters, "@PasswordHash", passwordHash);

            cmd.ExecuteNonQuery();
        }

        public UserWithPasswordHash GetUserWithPasswordHash(string username)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT UserID, PasswordHash FROM Users WHERE Username = @Username";
            AddParam(cmd.Parameters, "@Username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserWithPasswordHash
                {
                    UserID = reader.GetInt32(0),
                    PasswordHash = reader.GetString(1)
                };
            }

            return null;
        }

        private void AddParam(IDataParameterCollection parameters, string name, object value)
        {
            var param = new SqlParameter(name, value ?? DBNull.Value);
            parameters.Add(param);
        }
    }

}
