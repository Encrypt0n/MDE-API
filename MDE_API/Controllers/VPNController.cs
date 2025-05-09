using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MDE_API.Services;

namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/vpn")]
    public class VPNController : ControllerBase
    {
        private readonly DatabaseService _db;
        private readonly ILogger<VPNController> _logger;

        public VPNController(DatabaseService db, ILogger<VPNController> logger)
        {
            _db = db;
            _logger = logger;
        }


        [HttpPost("client-connected")]
        public IActionResult ClientConnected([FromBody] VpnClientConnectModel model)
        {
            _logger.LogInformation("🔗 Received VPN client connection:");
            _logger.LogInformation("📛 ClientName: {ClientName}", model.ClientName);
            _logger.LogInformation("🌐 AssignedIp: {AssignedIp}", model.AssignedIp);
            _logger.LogInformation("📝 Description: {Description}", model.Description);
            _logger.LogInformation("👤 UserID: {UserID}", model.UserID);

            if (string.IsNullOrWhiteSpace(model.ClientName) ||
                string.IsNullOrWhiteSpace(model.AssignedIp) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                model.UserID <= 0)
            {
                _logger.LogWarning("⚠️ Invalid data received from VPN client.");
                return BadRequest("Invalid client data.");
            }

            _db.SaveClientConnection(model.ClientName, model.Description, model.UserID, model.AssignedIp);
            _logger.LogInformation("✅ Client info saved successfully.");

            return Ok();
        }
    }

    public class VpnClientConnectModel
    {
        public string ClientName { get; set; }

        public string Description { get; set; }

        public int UserID { get; set; }
        public string AssignedIp { get; set; }
    }

}
