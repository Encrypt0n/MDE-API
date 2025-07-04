using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MDE_API.Domain;
using MDE_API.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using MDE_API.Application.Services;
using System.IO.Compression;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;
using MDE_API.Domain.Models;
using System.Security.Claims;
using System.Net;


namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/vpn")]
    public class VPNController : ControllerBase
    {
        private readonly IVPNService _vpnService;
        private readonly IMachineService _machineService;
        private readonly ILogger<VPNController> _logger;
        private readonly IOpenSslHelper _helper;
        private readonly IFileSystem _fileSystem;
       
        private readonly IVPNClientNotifier _notifier;

        public VPNController(IVPNService vpnService, ILogger<VPNController> logger, IOpenSslHelper helper, IFileSystem fileSystem, IVPNClientNotifier notifier, IMachineService machineService)
        {
            _vpnService = vpnService;
            _machineService = machineService;
            _logger = logger;
            _helper = helper;
            _fileSystem = fileSystem;
            
            _notifier = notifier;
        }

        [HttpPost("client-connected")]
        public IActionResult ClientConnected([FromBody] VpnClientConnectModel model)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;

            // Allow only localhost (IPv4 or IPv6 loopback)
            if (!IPAddress.IsLoopback(remoteIp))
            {
                _logger.LogWarning($"❌ Access denied: request from {remoteIp}");
                return Forbid("Only localhost is allowed to access this endpoint.");
            }
            _logger.LogInformation("🔗 Received VPN client connection:");
            _logger.LogInformation("📛 ClientName: {ClientName}", model.ClientName);
            _logger.LogInformation("🌐 AssignedIp: {AssignedIp}", model.AssignedIp);
            _logger.LogInformation("📝 Description: {Description}", model.Description);
            _logger.LogInformation("👤 CompanyID: {CompanyID}", model.CompanyID);

            if (string.IsNullOrWhiteSpace(model.ClientName) ||
                string.IsNullOrWhiteSpace(model.AssignedIp) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                model.CompanyID <= 0)
            {
                _logger.LogWarning("⚠️ Invalid data received from VPN client.");
                return BadRequest("Invalid client data.");
            }

            string clientName = model.ClientName;
            string[] parts = clientName.Split("machines_");
            string baseName = parts.Length > 1 ? parts[1] : clientName;

            _logger.LogInformation("👤 baseName: {baseName}", baseName);


            int machineId = _vpnService.SaveClientConnection(baseName, model.Description, model.CompanyID, model.AssignedIp, model.UiBuilderUrls);
            _logger.LogInformation("✅ Client info saved successfully.");
            // 🔔 Notify observers
            string domain = $"https://{baseName}.mde-portal.site:444";
            _machineService.UpdateDashboardUrl(machineId, domain);
            _notifier.NotifyAsync(model, baseName, machineId);

            return Ok();
        }


        [HttpGet("generate-cert/{clientName}/{companyName}/{subnet}/{user}")]
        public IActionResult GenerateCert(string clientName, string companyName, string subnet, bool user)
        {
            companyName = companyName.Replace(" ", "-");
            string certName;
            _logger.LogInformation("User: {User.Claims} ", User.Claims);
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("userRole")?.Value;
            _logger.LogInformation("Role: {role} ", role);
            if (!user && role == "1")
            {
                certName = $"{companyName}_machines_{clientName}";
            } else
            {
                certName = $"{companyName}_users_{clientName}";
            }
            var opensslPath = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe";
            var caCertPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\ca.crt";
            var caKeyPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\private\ca.key";
            var certsRootFolder = @"C:\Program Files\OpenVPN\clients";
            if (!_helper.GenerateClientCert(certName, out var clientFolder, out var error))
                return BadRequest(new { error = "Failed to generate cert: " + error });
            string companyPath = certName;
            string companySubnet = subnet;
            string base64Company = _helper.EncryptToBase64(companyPath, opensslPath, caKeyPath);
            string base64Subnet = _helper.EncryptToBase64(companySubnet, opensslPath, caKeyPath);
            // Write both Base64-encoded lines into auth.txt
            string finalAuthPath = Path.Combine(clientFolder, "auth.txt");
            _fileSystem.WriteAllLines(finalAuthPath, new[] { base64Company, base64Subnet });
            var clientKey = _fileSystem.ReadAllBytes(Path.Combine(clientFolder, $"{certName}.key"));
            var clientCrt = _fileSystem.ReadAllBytes(Path.Combine(clientFolder, $"{certName}.crt"));
            var caCrt = _fileSystem.ReadAllBytes(caCertPath);
            var taKey = _fileSystem.ReadAllBytes(Path.Combine(@"C:\Program Files\OpenVPN\config-auto", "ta.key"));
            var authTxt = _fileSystem.ReadAllBytes(finalAuthPath);
            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                void AddFile(string fileName, byte[] content)
                {
                    var entry = archive.CreateEntry(fileName);
                    using var stream = entry.Open();
                    stream.Write(content, 0, content.Length);
                }
                AddFile("client.crt", clientCrt);
                AddFile("client.key", clientKey);
                AddFile("ca.crt", caCrt);
                AddFile("ta.key", taKey);
                AddFile("auth.txt", authTxt);
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return File(zipStream.ToArray(), "application/zip", $"openvpn_client_{clientName}.zip");
        }






    }

    
}
