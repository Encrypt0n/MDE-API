using System.Data;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using Microsoft.Data.SqlClient;

public class DatabaseService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void SaveClientConnection(string clientName, string description, int userId, string assignedIp)
    {
        using var con = _connectionFactory.CreateConnection();
        con.Open();

        string query = @"MERGE INTO Machines AS Target
                         USING (VALUES (@Name, @Description, @UserID, @IP)) AS Source (Name, Description, UserID, IP)
                         ON Target.Name = Source.Name AND Target.UserID = Source.UserID
                         WHEN MATCHED THEN
                             UPDATE SET Target.IP = Source.IP, Target.Description = Source.Description
                         WHEN NOT MATCHED THEN
                             INSERT (Name, Description, UserID, IP)
                             VALUES (Source.Name, Source.Description, Source.UserID, Source.IP);";

        using var cmd = con.CreateCommand();
        cmd.CommandText = query;

        var p = cmd.Parameters;
        AddParam(p, "@Name", clientName);
        AddParam(p, "@Description", description);
        AddParam(p, "@UserID", userId);
        AddParam(p, "@IP", assignedIp);

        cmd.ExecuteNonQuery();
    }

    public bool RegisterUser(string username, string password)
    {
        using var con = _connectionFactory.CreateConnection();
        con.Open();

        using var checkCmd = con.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
        AddParam(checkCmd.Parameters, "@Username", username);

        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0) return false;

        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        using var insertCmd = con.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
        AddParam(insertCmd.Parameters, "@Username", username);
        AddParam(insertCmd.Parameters, "@PasswordHash", hash);

        insertCmd.ExecuteNonQuery();
        return true;
    }

    public User ValidateUser(string username, string password)
    {
        using var con = _connectionFactory.CreateConnection();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT UserID, PasswordHash FROM Users WHERE Username = @Username";
        AddParam(cmd.Parameters, "@Username", username);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            string hash = reader.GetString(1);
            if (BCrypt.Net.BCrypt.Verify(password, hash))
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

    private void AddParam(IDataParameterCollection parameters, string name, object value)
    {
        var param = new SqlParameter(name, value ?? DBNull.Value);
        parameters.Add(param);
    }
}
