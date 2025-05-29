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
        public void GetDashboardPages_ReturnsPages_WhenRecordsExist()
        {
            // Arrange
            var connMock = new Mock<IDbConnection>();
            var cmdMock = new Mock<IDbCommand>();
            var factoryMock = new Mock<IDbConnectionFactory>();

            var paramCollection = new SqlCommand().Parameters;

            factoryMock.Setup(f => f.CreateConnection()).Returns(connMock.Object);
            connMock.Setup(c => c.CreateCommand()).Returns(cmdMock.Object);

            cmdMock.SetupAllProperties();
            cmdMock.SetupGet(c => c.Parameters).Returns(paramCollection);

            // Mock data reader
            var readerMock = new Mock<IDataReader>();

            // Setup sequence for Read() calls: true, true, false (two records)
            var readCallCount = 0;
            readerMock.Setup(r => r.Read()).Returns(() => readCallCount++ < 2);

            // Setup GetInt32, GetString to return data for two rows
            readerMock.Setup(r => r.GetInt32(0)).Returns(() => readCallCount == 1 ? 1 : 2);
            readerMock.Setup(r => r.GetString(1)).Returns(() => readCallCount == 1 ? "Page 1" : "Page 2");
            readerMock.Setup(r => r.GetString(2)).Returns(() => readCallCount == 1 ? "url1" : "url2");

            cmdMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);

            var repo = new DashboardRepository(factoryMock.Object);

            // Act
            var result = repo.GetDashboardPages(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Equal(1, result[0].PageID);
            Assert.Equal("Page 1", result[0].PageName);
            Assert.Equal("url1", result[0].PageURL);

            Assert.Equal(2, result[1].PageID);
            Assert.Equal("Page 2", result[1].PageName);
            Assert.Equal("url2", result[1].PageURL);

            Assert.Equal(@"SELECT PageID, PageName, PageURL FROM DashboardPages WHERE MachineID = @MachineID ORDER BY PageID ASC", cmdMock.Object.CommandText);
            Assert.Single(paramCollection);
            Assert.Equal("@MachineID", paramCollection[0].ParameterName);
            Assert.Equal(123, paramCollection[0].Value);

            cmdMock.Verify(c => c.ExecuteReader(), Times.Once);
            readerMock.Verify(r => r.Read(), Times.Exactly(3)); // 2 true + 1 false
        }


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

        [Fact]
        public void GetFirstDashboardPageUrl_ReturnsUrl_WhenRecordExists()
        {
            // Arrange
            var connMock = new Mock<IDbConnection>();
            var cmdMock = new Mock<IDbCommand>();
            var factoryMock = new Mock<IDbConnectionFactory>();
            var paramCollection = new SqlCommand().Parameters;

            factoryMock.Setup(f => f.CreateConnection()).Returns(connMock.Object);
            connMock.Setup(c => c.CreateCommand()).Returns(cmdMock.Object);

            // Add this line to track CommandText and other properties
            cmdMock.SetupAllProperties();

            cmdMock.SetupGet(c => c.Parameters).Returns(paramCollection);

            object expectedUrl = "http://dashboard/page1";
            cmdMock.Setup(c => c.ExecuteScalar()).Returns(expectedUrl);

            var repo = new DashboardRepository(factoryMock.Object);

            // Act
            var url = repo.GetFirstDashboardPageUrl(123);

            // Assert
            Assert.Equal(expectedUrl.ToString(), url);
            Assert.Equal("SELECT TOP 1 PageURL FROM DashboardPages WHERE MachineID = @MachineID ORDER BY PageID ASC", cmdMock.Object.CommandText);
            cmdMock.Verify(c => c.ExecuteScalar(), Times.Once);
            Assert.Single(paramCollection);
            Assert.Equal("@MachineID", paramCollection[0].ParameterName);
            Assert.Equal(123, paramCollection[0].Value);
        }


        [Fact]
        public void GetFirstDashboardPageUrl_ReturnsEmptyString_WhenNoRecord()
        {
            // Arrange
            var connMock = new Mock<IDbConnection>();
            var cmdMock = new Mock<IDbCommand>();
            var factoryMock = new Mock<IDbConnectionFactory>();
            var paramCollection = new SqlCommand().Parameters;

            factoryMock.Setup(f => f.CreateConnection()).Returns(connMock.Object);
            connMock.Setup(c => c.CreateCommand()).Returns(cmdMock.Object);
            cmdMock.SetupGet(c => c.Parameters).Returns(paramCollection);

            cmdMock.Setup(c => c.ExecuteScalar()).Returns(null);

            var repo = new DashboardRepository(factoryMock.Object);

            // Act
            var url = repo.GetFirstDashboardPageUrl(123);

            // Assert
            Assert.Equal(string.Empty, url);
        }

    }
}
