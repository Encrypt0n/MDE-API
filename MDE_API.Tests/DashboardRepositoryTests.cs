using Xunit;
using Moq;
using System.Data;
using MDE_API.Infrastructure.Repositories;
using MDE_API.Application.Interfaces;
using Microsoft.Data.SqlClient;
using MDE_API.Domain.Models;
using System.Collections.ObjectModel;

namespace MDE_API.Tests.Repositories
{
    public class DashboardRepositoryTests
    {
        [Fact]
        public void AddDashboardPage_InsertsCorrectData()
        {
            // Arrange
            var connMock = new Mock<IDbConnection>();
            var cmdMock = new Mock<IDbCommand>();

            var factoryMock = new Mock<IDbConnectionFactory>();
            factoryMock.Setup(f => f.CreateConnection()).Returns(connMock.Object);
            connMock.Setup(c => c.CreateCommand()).Returns(cmdMock.Object);

            var repo = new DashboardRepository(factoryMock.Object);

            cmdMock.SetupAllProperties();
            cmdMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
            cmdMock.SetupGet(c => c.Parameters).Returns(new SqlCommand().Parameters);
  
            // Act
            repo.AddDashboardPage(1, "Page", "url");

            // Assert
            Assert.Equal("INSERT INTO DashboardPages (MachineID, PageName, PageURL) VALUES (@MachineID, @PageName, @PageURL)", cmdMock.Object.CommandText);
            cmdMock.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [Fact]
        public void DeleteDashboardPage_DeletesCorrectPage()
        {
            // Arrange
            var connMock = new Mock<IDbConnection>();
            var cmdMock = new Mock<IDbCommand>();

            var factoryMock = new Mock<IDbConnectionFactory>();
            factoryMock.Setup(f => f.CreateConnection()).Returns(connMock.Object);
            connMock.Setup(c => c.CreateCommand()).Returns(cmdMock.Object);

            var repo = new DashboardRepository(factoryMock.Object);

            cmdMock.SetupAllProperties();
            cmdMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
            cmdMock.SetupGet(c => c.Parameters).Returns(new SqlCommand().Parameters);

            // Act
            repo.DeleteDashboardPage(5);

            // Assert
            Assert.Equal("DELETE FROM DashboardPages WHERE PageID = @PageID", cmdMock.Object.CommandText);
            cmdMock.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }
    }
}
