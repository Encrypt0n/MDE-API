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
        private readonly Mock<IOpenSslHelper> _helper;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IProcessRunner> _mockProcessRunner;

        public VPNControllerTests()
        {
            _vpnServiceMock = new Mock<IVPNService>();
            _loggerMock = new Mock<ILogger<VPNController>>();
            _helper = new Mock<IOpenSslHelper>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockProcessRunner = new Mock<IProcessRunner>();
            _controller = new VPNController(_vpnServiceMock.Object, _loggerMock.Object, _helper.Object, _mockFileSystem.Object, _mockProcessRunner.Object);
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

        [Fact]
        public void GenerateMachine_ReturnsFile_WhenCertGenerated()
        {
            // Arrange
            var mockHelper = new Mock<IOpenSslHelper>();
            var mockFileSystem = new Mock<IFileSystem>();
            var clientName = "client1";
            var companyName = "company1";
            var subnet = "10.0.0.0/24";
            var certName = $"{companyName}_machines_{clientName}";
            var expectedClientFolder = @"C:\fake-client-folder";
            var dummyBytes = new byte[] { 0x01, 0x02, 0x03 };
            var opensslPath = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe";
            var caKeyPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\private\ca.key";

            string ignoredError = null!;
            mockHelper.Setup(h => h.GenerateClientCert(certName, out expectedClientFolder, out ignoredError))
                      .Returns(true);

            mockHelper.Setup(h => h.EncryptToBase64("fake-base64", opensslPath, caKeyPath));

            mockFileSystem.Setup(fs => fs.ReadAllBytes(It.IsAny<string>())).Returns(dummyBytes);

            // ✅ Mock WriteAllLines to avoid touching disk
            mockFileSystem.Setup(fs => fs.WriteAllLines(It.IsAny<string>(), It.IsAny<string[]>()))
                          .Verifiable();

            var controller = new VPNController(
                _vpnServiceMock.Object,
                _loggerMock.Object,
                mockHelper.Object,
                mockFileSystem.Object,
                _mockProcessRunner.Object);

            // Act
            var result = controller.GenerateMachine(clientName, companyName, subnet);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/zip", fileResult.ContentType);
            Assert.StartsWith("openvpn_client_", fileResult.FileDownloadName);
            Assert.EndsWith(".zip", fileResult.FileDownloadName);
            Assert.NotNull(fileResult.FileContents);
            Assert.True(fileResult.FileContents.Length > 0);
        }



    }
}
