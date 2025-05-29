using System;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using Moq;
using MDE_API.Domain.Models;
using MDE_API.Infrastructure.Repositories;
using MDE_API.Application.Interfaces;
using Xunit;

namespace MDE_API.Tests.Repositories
{
    public class ActivityRepositoryTests
    {
        private readonly Mock<IDbConnectionFactory> _mockFactory;
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbCommand> _mockCommand;
        private readonly ActivityRepository _repository;

        public ActivityRepositoryTests()
        {
            _mockFactory = new Mock<IDbConnectionFactory>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();

            _mockFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
            _mockCommand.SetupAllProperties();

            _repository = new ActivityRepository(_mockFactory.Object);
        }

        [Fact]
        public void LogActivity_InsertsActivityCorrectly()
        {
            // Arrange
            var parameters = new SqlCommand().Parameters;
            _mockCommand.Setup(c => c.Parameters).Returns(parameters);
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            var activity = new UserActivityLog
            {
                UserId = 1,
                MachineId = 42,
                Action = "Clicked",
                Target = "Button1",
                Timestamp = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                UserAgent = "TestAgent"
            };

            // Act
            _repository.LogActivity(activity);

            // Assert
            Assert.Equal(7, parameters.Count);
            Assert.Equal(activity.UserId, parameters["@UserId"].Value);
            Assert.Equal(activity.MachineId, parameters["@MachineId"].Value);
            Assert.Equal(activity.Action, parameters["@Action"].Value);
            Assert.Equal(activity.Target, parameters["@Target"].Value);
            Assert.Equal(activity.Timestamp, parameters["@Timestamp"].Value);
            Assert.Equal(activity.IpAddress, parameters["@IpAddress"].Value);
            Assert.Equal(activity.UserAgent, parameters["@UserAgent"].Value);

            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [Fact]
        public void GetActivitiesForMachine_ReturnsCorrectResults()
        {
            // Arrange
            var data = new[]
            {
                new UserActivityLog
                {
                    Id = 1,
                    UserId = 10,
                    MachineId = 42,
                    Action = "Login",
                    Target = "Dashboard",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "127.0.0.1",
                    UserAgent = "TestBrowser"
                }
            };

            var mockReader = new Mock<IDataReader>();
            var callCount = -1;

            mockReader.Setup(r => r.Read()).Returns(() => ++callCount < data.Length);
            mockReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns<string>(name =>
                name switch
                {
                    "Id" => 0,
                    "UserId" => 1,
                    "MachineId" => 2,
                    "Action" => 3,
                    "Target" => 4,
                    "Timestamp" => 5,
                    "IpAddress" => 6,
                    "UserAgent" => 7,
                    _ => throw new IndexOutOfRangeException()
                });
            mockReader.Setup(r => r.GetInt32(0)).Returns(data[0].Id);
            mockReader.Setup(r => r.GetInt32(1)).Returns(data[0].UserId);
            mockReader.Setup(r => r.GetInt32(2)).Returns(data[0].MachineId);
            mockReader.Setup(r => r.GetString(3)).Returns(data[0].Action);
            mockReader.Setup(r => r.IsDBNull(4)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns(data[0].Target);
            mockReader.Setup(r => r.GetDateTime(5)).Returns(data[0].Timestamp);
            mockReader.Setup(r => r.IsDBNull(6)).Returns(false);
            mockReader.Setup(r => r.GetString(6)).Returns(data[0].IpAddress);
            mockReader.Setup(r => r.IsDBNull(7)).Returns(false);
            mockReader.Setup(r => r.GetString(7)).Returns(data[0].UserAgent);

            var parameters = new SqlCommand().Parameters;
            _mockCommand.Setup(c => c.Parameters).Returns(parameters);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(mockReader.Object);

            // Act
            var result = _repository.GetActivitiesForMachine(42);

            // Assert
            Assert.Single(result);
            Assert.Equal(data[0].Action, result[0].Action);
            Assert.Equal(data[0].UserId, result[0].UserId);
            Assert.Equal(data[0].Target, result[0].Target);
        }
    }
}
