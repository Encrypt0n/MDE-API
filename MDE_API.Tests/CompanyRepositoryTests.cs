using Xunit;
using Moq;
using System;
using System.Data;
using System.Collections.Generic;
using MDE_API.Domain.Models;
using MDE_API.Application.Interfaces;

namespace MDE_API.Tests.Repositories
{
    public class CompanyRepositoryTests
    {
        private readonly Mock<IDbConnectionFactory> _mockConnectionFactory;
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbCommand> _mockCommand;
        private readonly Mock<IDataReader> _mockReader;
        private readonly CompanyRepository _repository;

        public CompanyRepositoryTests()
        {
            _mockConnectionFactory = new Mock<IDbConnectionFactory>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();

            _mockConnectionFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
            _mockCommand.SetupAllProperties();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            _mockCommand.Setup(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);

            _repository = new CompanyRepository(_mockConnectionFactory.Object);
        }

        [Fact]
        public void GetAllCompanies_ReturnsList()
        {
            // Arrange
            var callIndex = -1;
            _mockReader.Setup(r => r.Read()).Returns(() => ++callIndex < 1);
            _mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            _mockReader.Setup(r => r.GetString(1)).Returns("Test Company");
            _mockReader.Setup(r => r.GetString(2)).Returns("10.8.10.0");

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = _repository.GetAllCompanies();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Company", result[0].Name);
            Assert.Equal("10.8.10.0", result[0].Subnet);
        }

        [Fact]
        public void GetCompanyById_Exists_ReturnsCompany()
        {
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            _mockReader.Setup(r => r.GetInt32(0)).Returns(2);
            _mockReader.Setup(r => r.GetString(1)).Returns("Company X");
            _mockReader.Setup(r => r.GetString(2)).Returns("10.8.11.0");

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            var result = _repository.GetCompanyById(2);

            Assert.NotNull(result);
            Assert.Equal("Company X", result?.Name);
            Assert.Equal(2, result?.CompanyID);
        }

        [Fact]
        public void GetCompanyById_NotFound_ReturnsNull()
        {
            _mockReader.Setup(r => r.Read()).Returns(false);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            var result = _repository.GetCompanyById(999);

            Assert.Null(result);
        }

        [Fact]
        public void CreateCompany_SetsId()
        {
            var company = new Company { Name = "New Co", Description = "Desc", Subnet = "10.8.12.0" };

            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(123);

            _repository.CreateCompany(company);

            Assert.Equal(123, company.CompanyID);
        }

        [Fact]
        public void UpdateCompany_ReturnsTrue_WhenRowAffected()
        {
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            var model = new CompanyModel { CompanyID = 1, Name = "Updated Name" };

            var result = _repository.UpdateCompany(model);

            Assert.True(result);
        }

        [Fact]
        public void UpdateCompany_ReturnsFalse_WhenNoRowAffected()
        {
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(0);

            var model = new CompanyModel { CompanyID = 1, Name = "Updated Name" };

            var result = _repository.UpdateCompany(model);

            Assert.False(result);
        }

        [Fact]
        public void DeleteCompany_ReturnsTrue_WhenRowDeleted()
        {
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            var result = _repository.DeleteCompany(1);

            Assert.True(result);
        }

        [Fact]
        public void DeleteCompany_ReturnsFalse_WhenNoRowDeleted()
        {
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(0);

            var result = _repository.DeleteCompany(1);

            Assert.False(result);
        }
    }
}
