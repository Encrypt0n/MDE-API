using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;

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
            cmd.CommandText = @"INSERT INTO UserActivityLog (UserId, MachineId, Action, Target, Timestamp, IpAddress, UserAgent)
                                 VALUES (@UserId, @MachineId, @Action, @Target, @Timestamp, @IpAddress, @UserAgent)";

            AddParam(cmd.Parameters, "@UserId", activity.UserId);
            AddParam(cmd.Parameters, "@MachineId", activity.MachineId);
            AddParam(cmd.Parameters, "@Action", activity.Action);
            AddParam(cmd.Parameters, "@Target", activity.Target);
            AddParam(cmd.Parameters, "@Timestamp", activity.Timestamp);
            AddParam(cmd.Parameters, "@IpAddress", activity.IpAddress);
            AddParam(cmd.Parameters, "@UserAgent", activity.UserAgent);

            cmd.ExecuteNonQuery();
        }

        public ObservableCollection<UserActivityLog> GetActivitiesForMachine(int machineId)
        {
            var result = new ObservableCollection<UserActivityLog>();

            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM UserActivityLog WHERE MachineId = @MachineId ORDER BY Timestamp DESC";
            AddParam(cmd.Parameters, "@MachineId", machineId);

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

        private void AddParam(IDataParameterCollection parameters, string name, object? value)
        {
            var param = new SqlParameter(name, value ?? DBNull.Value);
            parameters.Add(param);
        }
    }
}