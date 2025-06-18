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

        public int SaveClientConnection(string clientName, string description, int companyId, string assignedIp, List<string> uibuilderUrls)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            int machineId;

            // First try to get the existing machine
            using (var checkCmd = con.CreateCommand())
            {
                checkCmd.CommandText = "SELECT MachineID FROM Machines WHERE Name = @Name";
                checkCmd.Parameters.Add(new SqlParameter("@Name", clientName ?? (object)DBNull.Value));

                object result = checkCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    machineId = Convert.ToInt32(result);

                    // Update the machine info
                    using var updateCmd = con.CreateCommand();
                    updateCmd.CommandText = @"
                UPDATE Machines 
                SET Description = @Description, IP = @IP 
                WHERE MachineID = @MachineID";

                    updateCmd.Parameters.Add(new SqlParameter("@Description", description ?? (object)DBNull.Value));
                    updateCmd.Parameters.Add(new SqlParameter("@IP", assignedIp ?? (object)DBNull.Value));
                    updateCmd.Parameters.Add(new SqlParameter("@MachineID", machineId));

                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    // Insert new machine and get the ID
                    using var insertCmd = con.CreateCommand();
                    insertCmd.CommandText = @"
                INSERT INTO Machines (Name, Description, IP) 
                OUTPUT INSERTED.MachineID 
                VALUES (@Name, @Description, @IP)";

                    insertCmd.Parameters.Add(new SqlParameter("@Name", clientName ?? (object)DBNull.Value));
                    insertCmd.Parameters.Add(new SqlParameter("@Description", description ?? (object)DBNull.Value));
                    insertCmd.Parameters.Add(new SqlParameter("@IP", assignedIp ?? (object)DBNull.Value));

                    machineId = Convert.ToInt32(insertCmd.ExecuteScalar());
                }
            }

            // Step 2: Link machine to company
            using (var linkCmd = con.CreateCommand())
            {
                linkCmd.CommandText = @"
            MERGE INTO Companies_Machines AS Target
            USING (VALUES (@CompanyID, @MachineID)) AS Source (CompanyID, MachineID)
            ON Target.CompanyID = Source.CompanyID AND Target.MachineID = Source.MachineID
            WHEN NOT MATCHED THEN
                INSERT (CompanyID, MachineID)
                VALUES (Source.CompanyID, Source.MachineID);";

                linkCmd.Parameters.Add(new SqlParameter("@CompanyID", companyId));
                linkCmd.Parameters.Add(new SqlParameter("@MachineID", machineId));
                linkCmd.ExecuteNonQuery();
            }

            // Step 3: Add DashboardPages
            foreach (var url in uibuilderUrls ?? Enumerable.Empty<string>())
            {
                using var pageCmd = con.CreateCommand();
                pageCmd.CommandText = @"
            MERGE INTO DashboardPages AS Target
            USING (VALUES (@MachineID, @PageName, @PageURL)) AS Source (MachineID, PageName, PageURL)
            ON Target.MachineID = Source.MachineID AND Target.PageName = Source.PageName AND Target.PageURL = Source.PageURL
            WHEN NOT MATCHED THEN
                INSERT (MachineID, PageName, PageURL)
                VALUES (Source.MachineID, Source.PageName, Source.PageURL);";

                pageCmd.Parameters.Add(new SqlParameter("@MachineID", machineId));
                pageCmd.Parameters.Add(new SqlParameter("@PageName", url ?? (object)DBNull.Value));
                pageCmd.Parameters.Add(new SqlParameter("@PageURL", $"https://{clientName}.mde-portal.site:444/{url}"));

                pageCmd.ExecuteNonQuery();
            }

            return machineId;
        }
    }

    }
