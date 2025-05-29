using Xunit;
using Moq;
using System.Collections.Generic;
using MDE_API.Application.Services;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System;

namespace MDE_API.Tests.Services
{
    public class CompanyServiceTests
    {
        private readonly Mock<ICompanyRepository> _mockRepo;
        private readonly CompanyService _service;

        public CompanyServiceTests()
        {
            _mockRepo = new Mock<ICompanyRepository>();
            _service = new CompanyService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllCompanies_ReturnsCompanies()
        {
            var companies = new List<Company>
            {
                new Company { CompanyID = 1, Name = "A", Subnet = "10.8.10.0" }
            };
            _mockRepo.Setup(r => r.GetAllCompanies()).Returns(companies);

            var result = _service.GetAllCompanies();

            Assert.Single(result);
            Assert.Equal("A", result[0].Name);
        }

        [Fact]
        public void GetCompanyById_ExistingId_ReturnsCompany()
        {
            var company = new Company { CompanyID = 1, Name = "B" };
            _mockRepo.Setup(r => r.GetCompanyById(1)).Returns(company);

            var result = _service.GetCompanyById(1);

            Assert.NotNull(result);
            Assert.Equal("B", result?.Name);
        }

        [Fact]
        public void GetCompanyById_NonExistingId_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetCompanyById(99)).Returns((Company?)null);

            var result = _service.GetCompanyById(99);

            Assert.Null(result);
        }

        [Fact]
        public void CreateCompany_AssignsNextAvailableSubnet_AndCallsRepo()
        {
            // Arrange
            var used = new List<Company>
            {
                new Company { Subnet = "10.8.10.0" },
                new Company { Subnet = "10.8.11.0" }
            };
            _mockRepo.Setup(r => r.GetAllCompanies()).Returns(used);

            Company? createdCompany = null;
            _mockRepo.Setup(r => r.CreateCompany(It.IsAny<Company>()))
                .Callback<Company>(c => createdCompany = c);

            var model = new CompanyModel { Name = "New", Description = "Test" };

            // Act
            _service.CreateCompany(model);

            // Assert
            Assert.NotNull(createdCompany);
            Assert.Equal("New", createdCompany.Name);
            Assert.Equal("10.8.12.0", createdCompany.Subnet);
        }

        [Fact]
        public void CreateCompany_WhenAllSubnetsUsed_ThrowsException()
        {
            // Arrange: simulate all subnets 10.8.10.0 to 10.8.200.0 are used
            var used = new List<Company>();
            for (int i = 10; i <= 200; i++)
                used.Add(new Company { Subnet = $"10.8.{i}.0" });

            _mockRepo.Setup(r => r.GetAllCompanies()).Returns(used);

            var model = new CompanyModel { Name = "Overflow", Description = "Fail" };

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _service.CreateCompany(model));
            Assert.Contains("No available subnet", ex.Message);
        }

        [Fact]
        public void UpdateCompany_DelegatesToRepository()
        {
            var model = new CompanyModel { CompanyID = 1, Name = "Updated" };
            _mockRepo.Setup(r => r.UpdateCompany(model)).Returns(true);

            var result = _service.UpdateCompany(model);

            Assert.True(result);
        }

        [Fact]
        public void DeleteCompany_DelegatesToRepository()
        {
            _mockRepo.Setup(r => r.DeleteCompany(1)).Returns(true);

            var result = _service.DeleteCompany(1);

            Assert.True(result);
        }
    }
}
