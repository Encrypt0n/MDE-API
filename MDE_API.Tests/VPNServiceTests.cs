using Xunit;
using Moq;
using MDE_API.Application.Interfaces;
using MDE_API.Application.Services;

namespace MDE_API.Tests
{
    public class VPNServiceTests
    {
        private readonly Mock<IVPNRepository> _mockRepository;
        private readonly VPNService _vpnService;

        public VPNServiceTests()
        {
            _mockRepository = new Mock<IVPNRepository>();
            _vpnService = new VPNService(_mockRepository.Object);
        }

        [Fact]
        public void SaveClientConnection_DelegatesToRepository_WithCorrectParameters()
        {
            // Arrange
            string clientName = "TestClient";
            string description = "TestDescription";
            int companyId = 1;
            string assignedIp = "10.0.0.1";
            var uibuilderUrls = new List<string> { "dashboard", "settings" };

            // Act
            _vpnService.SaveClientConnection(clientName, description, companyId, assignedIp, uibuilderUrls);

            // Assert
            _mockRepository.Verify(r => r.SaveClientConnection(
                clientName,
                description,
                companyId,
                assignedIp,
                uibuilderUrls
            ), Times.Once);
        }
    }
}
