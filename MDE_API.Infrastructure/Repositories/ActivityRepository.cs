using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace MDE_API.Infrastructure.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly string _connectionString;

        public ActivityRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public void LogActivity(UserActivityLog activity)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("INSERT INTO UserActivityLog (UserId, MachineId, Action, Target, Timestamp, IpAddress, UserAgent) VALUES (@UserId, @MachineId, @Action, @Target, @Timestamp, @IpAddress, @UserAgent)", connection);

            command.Parameters.AddWithValue("@UserId", activity.UserId);
            command.Parameters.AddWithValue("@MachineId", activity.MachineId);
            command.Parameters.AddWithValue("@Action", activity.Action);
            command.Parameters.AddWithValue("@Target", (object?)activity.Target ?? DBNull.Value);
            command.Parameters.AddWithValue("@Timestamp", activity.Timestamp);
            command.Parameters.AddWithValue("@IpAddress", (object?)activity.IpAddress ?? DBNull.Value);
            command.Parameters.AddWithValue("@UserAgent", (object?)activity.UserAgent ?? DBNull.Value);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public ObservableCollection<UserActivityLog> GetActivitiesForMachine(int machineId)
        {
            var result = new ObservableCollection<UserActivityLog>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM UserActivityLog WHERE MachineId = @MachineId ORDER BY Timestamp DESC", connection);
            command.Parameters.AddWithValue("@MachineId", machineId);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new UserActivityLog
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    MachineId = reader.GetInt32(reader.GetOrdinal("MachineId")),
                    Action = reader.GetString(reader.GetOrdinal("Action")),
                    Target = reader.IsDBNull(reader.GetOrdinal("Target")) ? null : reader.GetString(reader.GetOrdinal("Target")),
                    Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                    IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString(reader.GetOrdinal("IpAddress")),
                    UserAgent = reader.IsDBNull(reader.GetOrdinal("UserAgent")) ? null : reader.GetString(reader.GetOrdinal("UserAgent"))
                });
            }

            return result;
        }
    }
}
