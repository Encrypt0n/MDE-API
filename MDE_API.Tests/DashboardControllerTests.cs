using Xunit;
using Moq;

using MDE_API.Controllers;
using MDE_API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MDE_API.Tests
{
    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _dashboardServiceMock;
        private readonly Mock<ILogger<DashboardController>> _loggerMock;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _dashboardServiceMock = new Mock<IDashboardService>();
            _loggerMock = new Mock<ILogger<DashboardController>>();
            _controller = new DashboardController(_dashboardServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetDashboardPages_ValidMachineId_ReturnsOkWithPages()
        {
            // Arrange
            var machineId = 1;
            var expectedPages = new System.Collections.ObjectModel.ObservableCollection<Domain.Models.DashboardPage>
{
    new Domain.Models.DashboardPage
    {
        PageID = 1,
        PageName = "page1",
        PageOrder = 1,
        PageURL = "testurl.com"
    },
    new Domain.Models.DashboardPage
    {
        PageID = 2,
        PageName = "page2",
        PageOrder = 1,
        PageURL = "testurl.com"
    }
};

            _dashboardServiceMock.Setup(x => x.GetDashboardPages(machineId)).Returns(expectedPages);

            // Act
            var result = _controller.GetDashboardPages(machineId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedPages, okResult.Value);
        }

        [Fact]
        public void AddDashboardPage_ValidModel_CallsServiceAndReturnsOk()
        {
            // Arrange
            var model = new DashboardController.AddDashboardPageModel
            {
                MachineId = 1,
                PageName = "Home",
                PageUrl = "http://example.com"
            };

            // Act
            var result = _controller.AddDashboardPage(model);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _dashboardServiceMock.Verify(s => s.AddDashboardPage(model.MachineId, model.PageName, model.PageUrl), Times.Once);
        }

        [Theory]
        [InlineData(0, "Home", "http://example.com")]
        [InlineData(1, "", "http://example.com")]
        [InlineData(1, "Home", "")]
        [InlineData(-5, null, null)]
        public void AddDashboardPage_InvalidModel_ReturnsBadRequest(int machineId, string pageName, string pageUrl)
        {
            // Arrange
            var model = new DashboardController.AddDashboardPageModel
            {
                MachineId = machineId,
                PageName = pageName,
                PageUrl = pageUrl
            };

            // Act
            var result = _controller.AddDashboardPage(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            _dashboardServiceMock.Verify(s => s.AddDashboardPage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void DeleteDashboardPage_ValidPageId_CallsServiceAndReturnsOk()
        {
            // Arrange
            var pageId = 42;

            // Act
            var result = _controller.DeleteDashboardPage(pageId);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _dashboardServiceMock.Verify(s => s.DeleteDashboardPage(pageId), Times.Once);
        }

        [Fact]
        public void GetFirstDashboardPageUrl_ValidMachineId_ReturnsOkWithUrl()
        {
            // Arrange
            var machineId = 99;
            var expectedUrl = "http://first-dashboard.com";
            _dashboardServiceMock.Setup(x => x.GetFirstDashboardPageUrl(machineId)).Returns(expectedUrl);

            // Act
            var result = _controller.GetFirstDashboardPageUrl(machineId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedUrl, okResult.Value);
        }
    }
}
