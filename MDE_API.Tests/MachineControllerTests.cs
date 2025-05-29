using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using MDE_API.Controllers;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;

namespace MDE_API.Tests.Controllers
{
    public class MachineControllerTests
    {
        private readonly Mock<IMachineService> _machineServiceMock;
        private readonly Mock<ILogger<MachineController>> _loggerMock;
        private readonly MachineController _controller;

        public MachineControllerTests()
        {
            _machineServiceMock = new Mock<IMachineService>();
            _loggerMock = new Mock<ILogger<MachineController>>();
            _controller = new MachineController(_machineServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetMachinesForUser_ValidCompanyId_ReturnsOkWithMachines()
        {
            // Arrange
            int companyId = 1;
            var machines = new ObservableCollection<Machine>
    {
        new Machine { MachineID = 1, Name = "Machine 1" },
        new Machine { MachineID = 2, Name = "Machine 2" }
    };

            _machineServiceMock.Setup(s => s.GetMachinesForCompany(companyId)).Returns(machines);

            // Act
            var result = _controller.GetMachinesForUser(companyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(machines, okResult.Value);
            _machineServiceMock.Verify(s => s.GetMachinesForCompany(companyId), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("👤 Name:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(machines.Count));
        }


        [Fact]
        public void UpdateDashboardUrl_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            int machineId = 1;
            var model = new MachineController.DashboardUrlUpdateModel { DashboardUrl = " " }; // whitespace is invalid

            // Act
            var result = _controller.UpdateDashboardUrl(machineId, model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Dashboard URL is required.", badRequestResult.Value);
        }

        [Fact]
        public void UpdateDashboardUrl_ValidModel_ReturnsOk()
        {
            // Arrange
            int machineId = 1;
            var model = new MachineController.DashboardUrlUpdateModel { DashboardUrl = "http://dashboard.url" };

            // Act
            var result = _controller.UpdateDashboardUrl(machineId, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Dashboard URL updated successfully.", okResult.Value);
            _machineServiceMock.Verify(s => s.UpdateDashboardUrl(machineId, model.DashboardUrl), Times.Once);
        }

        [Fact]
        public void GetMachineById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            int invalidId = 0;

            // Act
            var result = _controller.GetMachineById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid machine ID.", badRequestResult.Value);
        }

        [Fact]
        public void GetMachineById_NotFound_ReturnsNotFound()
        {
            // Arrange
            int machineId = 10;
            _machineServiceMock.Setup(s => s.GetMachineById(machineId)).Returns((Machine?)null);

            // Act
            var result = _controller.GetMachineById(machineId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Machine with ID {machineId} not found.", notFoundResult.Value);
        }

        [Fact]
        public void GetMachineById_Found_ReturnsOkWithMachine()
        {
            // Arrange
            int machineId = 5;
            var machine = new Machine { MachineID = machineId, Name = "TestMachine", Description = "description" };
            _machineServiceMock.Setup(s => s.GetMachineById(machineId)).Returns(machine);

            // Act
            var result = _controller.GetMachineById(machineId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(machine, okResult.Value);
            _machineServiceMock.Verify(s => s.GetMachineById(machineId), Times.Once);
        }
    }
}
