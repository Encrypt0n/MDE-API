using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Infrastructure.Repositories
{
    public class MachineRepository : IMachineRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MachineRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public ObservableCollection<Machine> GetMachinesForUser(int userId)
        {
            var machines = new ObservableCollection<Machine>();

            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT MachineID, Name, Description, IP FROM Machines WHERE UserID = @UserID";
            AddParam(cmd.Parameters, "@UserID", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                machines.Add(new Machine
                {
                    MachineID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    IP = reader.GetString(3)
                });
            }

            return machines;
        }

        public Machine GetMachineById(int machineId)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM Machines WHERE MachineID = @MachineID";
            AddParam(cmd.Parameters, "@MachineID", machineId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Machine
                {
                    MachineID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    IP = reader.GetString(4)  // Assuming index 4 is IP
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
