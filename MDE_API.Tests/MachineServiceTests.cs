using Xunit;
using Moq;
using System.Collections.ObjectModel;
using MDE_API.Application.Services;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;

namespace MDE_API.Tests.Services
{
    public class MachineServiceTests
    {
        private readonly Mock<IMachineRepository> _machineRepositoryMock;
        private readonly MachineService _machineService;

        public MachineServiceTests()
        {
            _machineRepositoryMock = new Mock<IMachineRepository>();
            _machineService = new MachineService(_machineRepositoryMock.Object);
        }

        [Fact]
        public void GetMachinesForCompany_ReturnsExpectedMachines()
        {
            // Arrange
            int companyId = 1;
            var expectedMachines = new ObservableCollection<Machine>
            {
                new Machine { MachineID = 1, Name = "Machine1", Description = "description1" },
                new Machine { MachineID = 2, Name = "Machine2", Description = "description2" }
            };

            _machineRepositoryMock
                .Setup(repo => repo.GetMachinesForCompany(companyId))
                .Returns(expectedMachines);

            // Act
            var result = _machineService.GetMachinesForCompany(companyId);

            // Assert
            Assert.Equal(expectedMachines, result);
            _machineRepositoryMock.Verify(repo => repo.GetMachinesForCompany(companyId), Times.Once);
        }

        [Fact]
        public void GetMachineById_ReturnsExpectedMachine()
        {
            // Arrange
            int machineId = 10;
            var expectedMachine = new Machine { MachineID = machineId, Name = "TestMachine", Description = "test description" };

            _machineRepositoryMock
                .Setup(repo => repo.GetMachineById(machineId))
                .Returns(expectedMachine);

            // Act
            var result = _machineService.GetMachineById(machineId);

            // Assert
            Assert.Equal(expectedMachine, result);
            _machineRepositoryMock.Verify(repo => repo.GetMachineById(machineId), Times.Once);
        }

        [Fact]
        public void UpdateDashboardUrl_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            int machineId = 5;
            string dashboardUrl = "http://dashboard.url";

            // Act
            _machineService.UpdateDashboardUrl(machineId, dashboardUrl);

            // Assert
            _machineRepositoryMock.Verify(repo => repo.UpdateDashboardUrl(machineId, dashboardUrl), Times.Once);
        }
    }
}
