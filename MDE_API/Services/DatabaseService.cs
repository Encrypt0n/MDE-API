using MDE_API.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace MDE_API.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public void SaveClientConnection(string clientName, string description, int userId, string assignedIp)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            string query = @"
        MERGE INTO Machines AS Target
        USING (VALUES (@Name, @Description, @UserID, @IP)) AS Source (Name, Description, UserID, IP)
            ON Target.Name = Source.Name AND Target.UserID = Source.UserID
        WHEN MATCHED THEN
            UPDATE SET Target.IP = Source.IP, Target.Description = Source.Description
        WHEN NOT MATCHED THEN
            INSERT (Name, Description, UserID, IP)
            VALUES (Source.Name, Source.Description, Source.UserID, Source.IP);";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Name", clientName);
            cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@IP", assignedIp);

            con.Open();
            cmd.ExecuteNonQuery();
        }


        public bool RegisterUser(string username, string password)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            con.Open();

            // Check if user already exists
            using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", con))
            {
                checkCmd.Parameters.AddWithValue("@Username", username);
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                    return false;
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Insert new user
            using (var insertCmd = new SqlCommand("INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)", con))
            {
                insertCmd.Parameters.AddWithValue("@Username", username);
                insertCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                insertCmd.ExecuteNonQuery();
            }

            return true;
        }

        public User ValidateUser(string username, string password)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            con.Open();

            using var cmd = new SqlCommand("SELECT UserID, PasswordHash FROM Users WHERE Username = @Username", con);
            cmd.Parameters.AddWithValue("@Username", username);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string storedHash = reader.GetString(1);
                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                {
                    return new User
                    {
                        UserID = reader.GetInt32(0),
                        Username = username
                    };
                }
            }

            return null;
        }
    }
}
