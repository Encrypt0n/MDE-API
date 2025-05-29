using Xunit;
using Moq;
using MDE_API.Application.Services;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System.Collections.ObjectModel;

namespace MDE_API.Tests.Services
{
    public class ActivityServiceTests
    {
        private readonly Mock<IActivityRepository> _activityRepositoryMock;
        private readonly ActivityService _service;

        public ActivityServiceTests()
        {
            _activityRepositoryMock = new Mock<IActivityRepository>();
            _service = new ActivityService(_activityRepositoryMock.Object);
        }

        [Fact]
        public void LogActivity_ValidActivity_CallsRepositoryOnce()
        {
            // Arrange
            var activity = new UserActivityLog
            {
                MachineId = 1,
                UserId = 123,
                Action = "Test",
                Target = "Target"
            };

            // Act
            _service.LogActivity(activity);

            // Assert
            _activityRepositoryMock.Verify(r => r.LogActivity(activity), Times.Once);
        }

        [Fact]
        public void GetActivitiesForMachine_ValidId_ReturnsActivities()
        {
            // Arrange
            int machineId = 1;
            var expectedActivities = new ObservableCollection<UserActivityLog>
            {
                new UserActivityLog { MachineId = machineId, UserId = 1, Action = "Start" },
                new UserActivityLog { MachineId = machineId, UserId = 2, Action = "Stop" }
            };

            _activityRepositoryMock.Setup(r => r.GetActivitiesForMachine(machineId)).Returns(expectedActivities);

            // Act
            var result = _service.GetActivitiesForMachine(machineId);

            // Assert
            Assert.Equal(expectedActivities, result);
            _activityRepositoryMock.Verify(r => r.GetActivitiesForMachine(machineId), Times.Once);
        }
    }
}
