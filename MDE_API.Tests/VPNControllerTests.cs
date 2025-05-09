using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using MDE_API.Controllers;
using MDE_API.Domain;
using MDE_API.Application;
using MDE_API.Application.Interfaces; // if DatabaseService is here

public class VPNControllerTests
{
    private readonly Mock<IDatabaseService> _mockDb;
    private readonly Mock<ILogger<VPNController>> _mockLogger;
    private readonly VPNController _controller;

    public VPNControllerTests()
    {
        _mockDb = new Mock<IDatabaseService>(); // Or mock the interface if you refactor
        _mockLogger = new Mock<ILogger<VPNController>>();
        _controller = new VPNController(_mockDb.Object, _mockLogger.Object);
    }

    [Fact]
    public void ClientConnected_ValidInput_ReturnsOk()
    {
        // Arrange
        var model = new VpnClientConnectModel
        {
            ClientName = "IPC-01",
            Description = "Some machine",
            AssignedIp = "10.8.0.101",
            UserID = 1
        };

        // Act
        var result = _controller.ClientConnected(model);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        _mockDb.Verify(db => db.SaveClientConnection(
            model.ClientName, model.Description, model.UserID, model.AssignedIp), Times.Once);
    }

    [Theory]
    [InlineData(null, "desc", "10.8.0.1", 1)]
    [InlineData("client", null, "10.8.0.1", 1)]
    [InlineData("client", "desc", null, 1)]
    [InlineData("client", "desc", "10.8.0.1", 0)]
    public void ClientConnected_InvalidInput_ReturnsBadRequest(string name, string desc, string ip, int userId)
    {
        // Arrange
        var model = new VpnClientConnectModel
        {
            ClientName = name,
            Description = desc,
            AssignedIp = ip,
            UserID = userId
        };

        // Act
        var result = _controller.ClientConnected(model);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        _mockDb.Verify(db => db.SaveClientConnection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }
}
