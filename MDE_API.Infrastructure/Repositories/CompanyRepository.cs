﻿using MDE_API.Application.Interfaces;
using MDE_API.Domain;
using MDE_API.Domain.Models;
using MDE_API.Infrastructure.Factories;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;
using System.Data;

public class CompanyRepository : ICompanyRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CompanyRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<Company> GetAllCompanies()
    {
        var companies = new List<Company>();

        using var con = _connectionFactory.CreateConnection();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT CompanyID, Name, Subnet FROM Companies";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            companies.Add(new Company
            {
                CompanyID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Subnet = reader.GetString(2)
            });
        }

        return companies;
    }

    public Company? GetCompanyById(int id)
    {
        using var con = _connectionFactory.CreateConnection();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT CompanyID, Name, Subnet FROM Companies WHERE CompanyID = @CompanyID";
        cmd.Parameters.Add(new SqlParameter("@CompanyID", id));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Company
            {
                CompanyID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Subnet = reader.GetString(2)
            };
        }

        return null;
    }

    public void CreateCompany(Company company)
    {
        using var con = _connectionFactory.CreateConnection();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Companies (Name, Description, Subnet)
            VALUES (@Name, @Description, @Subnet);
            SELECT SCOPE_IDENTITY();";

        cmd.Parameters.Add(new SqlParameter("@Name", company.Name ?? (object)DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@Description", company.Description ?? (object)DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@Subnet", company.Subnet ?? (object)DBNull.Value));

        var id = Convert.ToInt32(cmd.ExecuteScalar());
        company.CompanyID = id;
    }

    public bool UpdateCompany(CompanyModel company)
    {
        using var con = _connectionFactory.CreateConnection();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE Companies SET Name = @Name WHERE CompanyID = @CompanyID";

        cmd.Parameters.Add(new SqlParameter("@Name", company.Name ?? (object)DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@CompanyID", company.CompanyID));

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool DeleteCompany(int id)
    {
        using var con = _connectionFactory.CreateConnection();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "DELETE FROM Companies WHERE CompanyID = @CompanyID";
        cmd.Parameters.Add(new SqlParameter("@CompanyID", id));

        return cmd.ExecuteNonQuery() > 0;
    }
}
