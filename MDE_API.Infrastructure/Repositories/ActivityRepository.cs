using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System;

namespace MDE_API.Infrastructure.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ActivityRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void LogActivity(UserActivityLog activity)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO UserActivityLog (UserId, MachineId, Action, Target, Timestamp, IpAddress, UserAgent)
                VALUES (@UserId, @MachineId, @Action, @Target, @Timestamp, @IpAddress, @UserAgent)";

            cmd.Parameters.Add(new SqlParameter("@UserId", activity.UserId));
            cmd.Parameters.Add(new SqlParameter("@MachineId", activity.MachineId));
            cmd.Parameters.Add(new SqlParameter("@Action", activity.Action));
            cmd.Parameters.Add(new SqlParameter("@Target", (object?)activity.Target ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Timestamp", activity.Timestamp));
            cmd.Parameters.Add(new SqlParameter("@IpAddress", (object?)activity.IpAddress ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@UserAgent", (object?)activity.UserAgent ?? DBNull.Value));

            cmd.ExecuteNonQuery();
        }

        public ObservableCollection<UserActivityLog> GetActivitiesForMachine(int machineId)
        {
            var result = new ObservableCollection<UserActivityLog>();

            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM UserActivityLog WHERE MachineId = @MachineId ORDER BY Timestamp DESC";
            cmd.Parameters.Add(new SqlParameter("@MachineId", machineId));

            using var reader = cmd.ExecuteReader();
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
