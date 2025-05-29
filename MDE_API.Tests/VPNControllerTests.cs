using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MDE_API.Controllers;
using MDE_API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace MDE_API.Tests
{
    public class VPNControllerTests
    {
        private readonly Mock<IVPNService> _vpnServiceMock;
        private readonly Mock<ILogger<VPNController>> _loggerMock;
        private readonly VPNController _controller;

        public VPNControllerTests()
        {
            _vpnServiceMock = new Mock<IVPNService>();
            _loggerMock = new Mock<ILogger<VPNController>>();
            _controller = new VPNController(_vpnServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void ClientConnected_ValidModel_CallsSaveAndReturnsOk()
        {
            // Arrange
            var model = new VpnClientConnectModel
            {
                ClientName = "client1",
                Description = "desc",
                CompanyID = 123,
                AssignedIp = "10.0.0.1",
                UiBuilderUrls = new List<string> { "page1", "page2" }
            };

            // Act
            var result = _controller.ClientConnected(model);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _vpnServiceMock.Verify(v => v.SaveClientConnection(
                model.ClientName,
                model.Description,
                model.CompanyID,
                model.AssignedIp,
                model.UiBuilderUrls
            ), Times.Once);
        }

        [Theory]
        [InlineData(null, "desc", 1, "10.0.0.1")]
        [InlineData("client", null, 1, "10.0.0.1")]
        [InlineData("client", "desc", 0, "10.0.0.1")]
        [InlineData("client", "desc", 1, null)]
        [InlineData("", "desc", 1, "10.0.0.1")]
        public void ClientConnected_InvalidModel_ReturnsBadRequest(
            string clientName, string description, int companyId, string assignedIp)
        {
            // Arrange
            var model = new VpnClientConnectModel
            {
                ClientName = clientName,
                Description = description,
                CompanyID = companyId,
                AssignedIp = assignedIp,
                UiBuilderUrls = null
            };

            // Act
            var result = _controller.ClientConnected(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            _vpnServiceMock.Verify(v => v.SaveClientConnection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
        }
    }
}
