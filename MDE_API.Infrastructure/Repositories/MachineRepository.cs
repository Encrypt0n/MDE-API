using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data;

namespace MDE_API.Infrastructure.Repositories
{
    public class MachineRepository : IMachineRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MachineRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public ObservableCollection<Machine> GetMachinesForCompany(int companyId)
        {
            var machines = new ObservableCollection<Machine>();

            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
        SELECT m.MachineID, m.Name, m.Description, m.IP, m.DashboardUrl
        FROM Machines m
        INNER JOIN Companies_Machines cm ON m.MachineID = cm.MachineID
        WHERE cm.CompanyID = @CompanyID";

            var paramCompanyId = cmd.CreateParameter();
            paramCompanyId.ParameterName = "@CompanyID";
            paramCompanyId.Value = companyId;
            cmd.Parameters.Add(paramCompanyId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                machines.Add(new Machine
                {
                    MachineID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    IP = reader.GetString(3),
                    DashboardUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return machines;
        }


        public Machine GetMachineById(int machineId)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT MachineID, Name, Description, IP, DashboardUrl FROM Machines WHERE MachineID = @MachineID";

            var paramMachineId = cmd.CreateParameter();
            paramMachineId.ParameterName = "@MachineID";
            paramMachineId.Value = machineId;
            cmd.Parameters.Add(paramMachineId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Machine
                {
                    MachineID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    IP = reader.GetString(3),
                    DashboardUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
            }

            return null;
        }

        public void UpdateDashboardUrl(int machineId, string dashboardUrl)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "UPDATE Machines SET DashboardUrl = @DashboardUrl WHERE MachineID = @MachineID";

            var paramDashboardUrl = cmd.CreateParameter();
            paramDashboardUrl.ParameterName = "@DashboardUrl";
            paramDashboardUrl.Value = dashboardUrl ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramDashboardUrl);

            var paramMachineId = cmd.CreateParameter();
            paramMachineId.ParameterName = "@MachineID";
            paramMachineId.Value = machineId;
            cmd.Parameters.Add(paramMachineId);

            cmd.ExecuteNonQuery();
        }
    }
}
