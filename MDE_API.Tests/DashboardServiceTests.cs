using Xunit;
using Moq;
using System.Collections.ObjectModel;
using MDE_API.Application.Services;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;

namespace MDE_API.Tests
{
    public class DashboardServiceTests
    {
        private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
        private readonly DashboardService _service;

        public DashboardServiceTests()
        {
            _dashboardRepositoryMock = new Mock<IDashboardRepository>();
            _service = new DashboardService(_dashboardRepositoryMock.Object);
        }

        [Fact]
        public void GetDashboardPages_ReturnsExpectedPages()
        {
            // Arrange
            var expectedPages = new ObservableCollection<DashboardPage>
            {
                new DashboardPage { PageID = 1, PageName = "page1", PageOrder = 1, PageURL = "testurl.com" },
                new DashboardPage { PageID = 2, PageName = "page2", PageOrder = 2, PageURL = "testurl2.com" }
            };

            _dashboardRepositoryMock.Setup(repo => repo.GetDashboardPages(123))
                .Returns(expectedPages);

            // Act
            var result = _service.GetDashboardPages(123);

            // Assert
            Assert.Equal(expectedPages, result);
            _dashboardRepositoryMock.Verify(repo => repo.GetDashboardPages(123), Times.Once);
        }

        [Fact]
        public void AddDashboardPage_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            int machineId = 5;
            string pageName = "Test Page";
            string pageUrl = "http://test.com";

            // Act
            _service.AddDashboardPage(machineId, pageName, pageUrl);

            // Assert
            _dashboardRepositoryMock.Verify(repo => repo.AddDashboardPage(machineId, pageName, pageUrl), Times.Once);
        }

        [Fact]
        public void DeleteDashboardPage_CallsRepositoryWithCorrectPageId()
        {
            // Arrange
            int pageId = 7;

            // Act
            _service.DeleteDashboardPage(pageId);

            // Assert
            _dashboardRepositoryMock.Verify(repo => repo.DeleteDashboardPage(pageId), Times.Once);
        }

        [Fact]
        public void GetFirstDashboardPageUrl_ReturnsExpectedUrl()
        {
            // Arrange
            int machineId = 9;
            string expectedUrl = "http://dashboard.com/start";

            _dashboardRepositoryMock.Setup(repo => repo.GetFirstDashboardPageUrl(machineId))
                .Returns(expectedUrl);

            // Act
            var result = _service.GetFirstDashboardPageUrl(machineId);

            // Assert
            Assert.Equal(expectedUrl, result);
            _dashboardRepositoryMock.Verify(repo => repo.GetFirstDashboardPageUrl(machineId), Times.Once);
        }
    }
}
