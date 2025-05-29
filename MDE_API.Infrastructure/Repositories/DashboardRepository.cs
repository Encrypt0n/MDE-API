using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace MDE_API.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DashboardRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public ObservableCollection<DashboardPage> GetDashboardPages(int machineId)
        {
            var pages = new ObservableCollection<DashboardPage>();

            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT PageID, PageName, PageURL
                FROM DashboardPages
                WHERE MachineID = @MachineID
                ORDER BY PageID ASC";

            cmd.Parameters.Add(new SqlParameter("@MachineID", machineId));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pages.Add(new DashboardPage
                {
                    PageID = reader.GetInt32(0),
                    PageName = reader.GetString(1),
                    PageURL = reader.GetString(2)
                });
            }

            return pages;
        }

        public void AddDashboardPage(int machineId, string pageName, string pageUrl)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO DashboardPages (MachineID, PageName, PageURL) VALUES (@MachineID, @PageName, @PageURL)";

            cmd.Parameters.Add(new SqlParameter("@MachineID", machineId));
            cmd.Parameters.Add(new SqlParameter("@PageName", pageName ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@PageURL", pageUrl ?? (object)DBNull.Value));

            cmd.ExecuteNonQuery();
        }

        public void DeleteDashboardPage(int pageId)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM DashboardPages WHERE PageID = @PageID";

            cmd.Parameters.Add(new SqlParameter("@PageID", pageId));

            cmd.ExecuteNonQuery();
        }

        public string GetFirstDashboardPageUrl(int machineId)
        {
            using var con = _connectionFactory.CreateConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT TOP 1 PageURL
                FROM DashboardPages
                WHERE MachineID = @MachineID
                ORDER BY PageID ASC";

            cmd.Parameters.Add(new SqlParameter("@MachineID", machineId));

            object result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : string.Empty;
        }
    }
}
