using MDE_API.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Infrastructure.Repositories
{
    public class VPNRepository : IVPNRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public VPNRepository(IDbConnectionFactory connectionFactory)
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

        private void AddParam(IDataParameterCollection parameters, string name, object value)
        {
            var param = new SqlParameter(name, value ?? DBNull.Value);
            parameters.Add(param);
        }
    }

}
