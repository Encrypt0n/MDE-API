using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MDE_API.Controllers;
using MDE_API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MDE_API.Domain.Models;
using MDE_API.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net;

namespace MDE_API.Tests
{
    public class VPNControllerTests
    {
        private readonly Mock<IVPNService> _vpnServiceMock;
        private readonly Mock<IMachineService> _machineServiceMock;
        private readonly Mock<ILogger<VPNController>> _loggerMock;
        private readonly VPNController _controller;
        private readonly Mock<IOpenSslHelper> _helper;
        private readonly Mock<IFileSystem> _mockFileSystem;
        
        private readonly Mock<IVPNClientNotifier> _vpnNotifier;

        public VPNControllerTests()
        {
            _vpnServiceMock = new Mock<IVPNService>();
            _machineServiceMock = new Mock<IMachineService>();
            _loggerMock = new Mock<ILogger<VPNController>>();
            _helper = new Mock<IOpenSslHelper>();
            _mockFileSystem = new Mock<IFileSystem>();
         
            _vpnNotifier = new Mock<IVPNClientNotifier>();
            _controller = new VPNController(_vpnServiceMock.Object, _loggerMock.Object, _helper.Object, _mockFileSystem.Object, _vpnNotifier.Object, _machineServiceMock.Object);
        }

        [Fact]
        public void ClientConnected_ValidModel_CallsSaveAndReturnsOk()
        {

            // Arrange
            var mockHelper = new Mock<IOpenSslHelper>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockVpnService = new Mock<IVPNService>();
            var mockMachineService = new Mock<IMachineService>();
            var mockLogger = new Mock<ILogger<VPNController>>();
            var mockNotifier = new Mock<IVPNClientNotifier>(); // use interface version of VPNClientNotifier

            var controller = new VPNController(
                mockVpnService.Object,
                mockLogger.Object,
                mockHelper.Object,
                mockFileSystem.Object,
                mockNotifier.Object,
                mockMachineService.Object
            );

            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Role, "1") // or "typ" = "1", depending on your logic
};
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    
                    User = claimsPrincipal,
                    Connection =
                    {
                        RemoteIpAddress = IPAddress.Loopback // 127.0.0.1

                    }
                }
            };
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
            var result = controller.ClientConnected(model);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            mockVpnService.Verify(v => v.SaveClientConnection(
                model.ClientName,
                model.Description,
                model.CompanyID,
                model.AssignedIp,
                model.UiBuilderUrls
            ), Times.Once);
        }



        [Fact]
        public void GenerateCert_ReturnsFile_WhenCertGenerated()
        {
            // Arrange
            var mockHelper = new Mock<IOpenSslHelper>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockVpnService = new Mock<IVPNService>();
            var mockMachineService = new Mock<IMachineService>();   
            var mockLogger = new Mock<ILogger<VPNController>>();
            var mockNotifier = new Mock<IVPNClientNotifier>(); // use interface version of VPNClientNotifier

            var controller = new VPNController(
                mockVpnService.Object,
                mockLogger.Object,
                mockHelper.Object,
                mockFileSystem.Object,
                mockNotifier.Object,
                mockMachineService.Object
            );

            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Role, "1") // or "typ" = "1", depending on your logic
};
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };


            var clientName = "client1";
            var companyName = "company1";
            var subnet = "10.0.0.0/24";
            bool user = false;
            var certName = $"{companyName}_machines_{clientName}";
            var expectedClientFolder = @"C:\fake-machine";
            var dummyBytes = new byte[] { 0x01, 0x02, 0x03 };
            var opensslPath = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe";
            var caKeyPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\private\ca.key";

            string ignoredError = null!;

            mockHelper.Setup(h => h.GenerateClientCert(certName, out expectedClientFolder, out ignoredError))
                      .Returns(true);

            mockHelper.Setup(h => h.EncryptToBase64(It.IsAny<string>(), opensslPath, caKeyPath));

            mockFileSystem.Setup(fs => fs.ReadAllBytes(It.IsAny<string>())).Returns(dummyBytes);

            mockFileSystem.Setup(fs => fs.WriteAllLines(It.IsAny<string>(), It.IsAny<string[]>())).Verifiable();

            // Act
            var result = controller.GenerateCert(clientName, companyName, subnet, user);

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
