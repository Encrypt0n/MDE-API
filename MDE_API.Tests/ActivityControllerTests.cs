using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MDE_API.Controllers;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System.Collections.Generic;
using MDE_API.Application.Interfaces.MDE_API.Services;

namespace MDE_API.Tests.Controllers
{
    public class ActivityControllerTests
    {
        private readonly Mock<IActivityService> _activityServiceMock;
        private readonly ActivityController _controller;

        public ActivityControllerTests()
        {
            _activityServiceMock = new Mock<IActivityService>();
            _controller = new ActivityController(_activityServiceMock.Object);
        }

        [Fact]
        public void LogActivity_ValidActivity_CallsLogAndReturnsOk()
        {
            // Arrange
            var activity = new UserActivityLog
            {
                MachineId = 1,
                UserId = 123,
                Action = "TestAction",
                Target = "TestTarget",
                IpAddress = "127.0.0.1",
                UserAgent = "UnitTestAgent"
            };

            // Act
            var result = _controller.LogActivity(activity);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _activityServiceMock.Verify(a => a.LogActivity(activity), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void LogActivity_InvalidActivity_ReturnsBadRequest(int machineId)
        {
            // Arrange
            var activity = new UserActivityLog
            {
                MachineId = machineId,
                UserId = 123,
                Action = "TestAction"
            };

            // Act
            var result = _controller.LogActivity(activity);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            _activityServiceMock.Verify(a => a.LogActivity(It.IsAny<UserActivityLog>()), Times.Never);
        }

        [Fact]
        public void LogActivity_NullActivity_ReturnsBadRequest()
        {
            // Act
            var result = _controller.LogActivity(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            _activityServiceMock.Verify(a => a.LogActivity(It.IsAny<UserActivityLog>()), Times.Never);
        }

        [Fact]
        public void GetActivityForMachine_ValidMachineId_ReturnsOkWithActivities()
        {
            // Arrange
            int machineId = 1;
            var activities = new System.Collections.ObjectModel.ObservableCollection<UserActivityLog>
            {
                new UserActivityLog { MachineId = machineId, UserId = 1, Action = "Action1" },
                new UserActivityLog { MachineId = machineId, UserId = 2, Action = "Action2" }
            };

            _activityServiceMock.Setup(a => a.GetActivitiesForMachine(machineId)).Returns(activities);

            // Act
            var result = _controller.GetActivityForMachine(machineId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(activities, okResult.Value);
            _activityServiceMock.Verify(a => a.GetActivitiesForMachine(machineId), Times.Once);
        }
    }
}
