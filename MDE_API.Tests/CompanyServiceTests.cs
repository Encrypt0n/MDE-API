using Xunit;
using Moq;
using System.Collections.Generic;
using MDE_API.Application.Services;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System;
using Microsoft.Extensions.Logging;

namespace MDE_API.Tests.Services
{
    public class CompanyServiceTests
    {
        private readonly Mock<ICompanyRepository> _mockRepo;
        private readonly Mock<ILogger<CompanyService>> _loggerMock;
        private readonly CompanyService _service;

        public CompanyServiceTests()
        {
            _mockRepo = new Mock<ICompanyRepository>();
            _loggerMock = new Mock<ILogger<CompanyService>>();
            _service = new CompanyService(_mockRepo.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetAllCompanies_ReturnsCompanies_AndCallsRepoOnce()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { CompanyID = 1, Name = "A", Subnet = "10.8.10.0" }
            };
            _mockRepo.Setup(r => r.GetAllCompanies()).Returns(companies);

            // Act
            var result = _service.GetAllCompanies();

            // Assert
            Assert.Single(result);
            Assert.Equal("A", result[0].Name);
            _mockRepo.Verify(r => r.GetAllCompanies(), Times.Once);
        }

        [Fact]
        public void GetCompanyById_ExistingId_ReturnsCompany_AndCallsRepo()
        {
            // Arrange
            var company = new Company { CompanyID = 1, Name = "B" };
            _mockRepo.Setup(r => r.GetCompanyById(1)).Returns(company);

            // Act
            var result = _service.GetCompanyById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("B", result?.Name);
            _mockRepo.Verify(r => r.GetCompanyById(1), Times.Once);
        }

        [Fact]
        public void GetCompanyById_NonExistingId_ReturnsNull_AndCallsRepo()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetCompanyById(99)).Returns((Company?)null);

            // Act
            var result = _service.GetCompanyById(99);

            // Assert
            Assert.Null(result);
            _mockRepo.Verify(r => r.GetCompanyById(99), Times.Once);
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
            _mockRepo.Verify(r => r.GetAllCompanies(), Times.Once);
            _mockRepo.Verify(r => r.CreateCompany(It.IsAny<Company>()), Times.Once);
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
            _mockRepo.Verify(r => r.GetAllCompanies(), Times.Once);
            _mockRepo.Verify(r => r.CreateCompany(It.IsAny<Company>()), Times.Never);
        }

        [Fact]
        public void UpdateCompany_DelegatesToRepository_AndReturnsTrue()
        {
            // Arrange
            var model = new CompanyModel { CompanyID = 1, Name = "Updated" };
            _mockRepo.Setup(r => r.UpdateCompany(model)).Returns(true);

            // Act
            var result = _service.UpdateCompany(model);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.UpdateCompany(model), Times.Once);
        }

        [Fact]
        public void DeleteCompany_DelegatesToRepository_AndReturnsTrue()
        {
            // Arrange
            _mockRepo.Setup(r => r.DeleteCompany(1)).Returns(true);

            // Act
            var result = _service.DeleteCompany(1);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.DeleteCompany(1), Times.Once);
        }

        [Fact]
        public void CreateCompany_AssignsSubnet_CallsRepository_And_LogsInformation()
        {
            // Arrange
            var mockRepo = new Mock<ICompanyRepository>();
            var mockLogger = new Mock<ILogger<CompanyService>>();

            var used = new List<Company>
    {
        new Company { Subnet = "10.8.10.0" },
        new Company { Subnet = "10.8.11.0" }
    };
            mockRepo.Setup(r => r.GetAllCompanies()).Returns(used);

            Company? createdCompany = null;
            mockRepo.Setup(r => r.CreateCompany(It.IsAny<Company>()))
                .Callback<Company>(c => createdCompany = c);

            var service = new CompanyService(mockRepo.Object, mockLogger.Object);
            var model = new CompanyModel { Name = "NewCompany", Description = "Description" };

            // Act
            service.CreateCompany(model);

            // Assert repository call
            Assert.NotNull(createdCompany);
            Assert.Equal("NewCompany", createdCompany.Name);
            Assert.Equal("Description", createdCompany.Description);
            Assert.Equal("10.8.12.0", createdCompany.Subnet);

            // Assert logger call
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("🔥 Logger is working inside CompanyService.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}
