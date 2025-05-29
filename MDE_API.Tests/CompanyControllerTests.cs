using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MDE_API.Controllers;
using MDE_API.Application.Interfaces;
using MDE_API.Domain;
using MDE_API.Domain.Models;

namespace MDE_API.Tests.Controllers
{
    public class CompanyControllerTests
    {
        private readonly Mock<ICompanyService> _mockService;
        private readonly CompanyController _controller;

        public CompanyControllerTests()
        {
            _mockService = new Mock<ICompanyService>();
            _controller = new CompanyController(_mockService.Object);
        }

        [Fact]
        public void GetAll_ReturnsListOfCompanies()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { CompanyID = 1, Name = "A Corp" },
                new Company { CompanyID = 2, Name = "B Inc" }
            };

            _mockService.Setup(s => s.GetAllCompanies()).Returns(companies);

            // Act
            var result = _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCompanies = Assert.IsAssignableFrom<List<Company>>(okResult.Value);
            Assert.Equal(2, returnedCompanies.Count);
        }

        [Fact]
        public void GetById_ExistingId_ReturnsCompany()
        {
            var company = new Company { CompanyID = 1, Name = "Test Co" };
            _mockService.Setup(s => s.GetCompanyById(1)).Returns(company);

            var result = _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<Company>(okResult.Value);
            Assert.Equal(company.CompanyID, returned.CompanyID);
        }

        [Fact]
        public void GetById_NonExistingId_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetCompanyById(It.IsAny<int>())).Returns<Company>(null!);

            var result = _controller.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_ValidCompany_ReturnsCreated()
        {
            var companyModel = new CompanyModel { CompanyID = 10, Name = "New Co" };

            var result = _controller.Create(companyModel);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedCompany = Assert.IsType<CompanyModel>(createdResult.Value);
            Assert.Equal(companyModel.CompanyID, returnedCompany.CompanyID);
        }

        [Fact]
        public void Update_ValidMatch_ReturnsNoContent()
        {
            var company = new CompanyModel { CompanyID = 1, Name = "Updated Co" };
            _mockService.Setup(s => s.UpdateCompany(company)).Returns(true);

            var result = _controller.Update(1, company);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Update_IdMismatch_ReturnsBadRequest()
        {
            var company = new CompanyModel { CompanyID = 1, Name = "Mismatch" };

            var result = _controller.Update(2, company);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch.", badRequest.Value);
        }

        [Fact]
        public void Update_NotFound_ReturnsNotFound()
        {
            var company = new CompanyModel { CompanyID = 1, Name = "Missing" };
            _mockService.Setup(s => s.UpdateCompany(company)).Returns(false);

            var result = _controller.Update(1, company);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_ExistingId_ReturnsNoContent()
        {
            _mockService.Setup(s => s.DeleteCompany(1)).Returns(true);

            var result = _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_NonExistingId_ReturnsNotFound()
        {
            _mockService.Setup(s => s.DeleteCompany(It.IsAny<int>())).Returns(false);

            var result = _controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
