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


namespace MDE_API.Controllers
{
    [ApiController]
    [Route("api/vpn")]
    public class VPNController : ControllerBase
    {
        private readonly IVPNService _vpnService;
        private readonly ILogger<VPNController> _logger;
        private readonly IOpenSslHelper _helper;
        private readonly IFileSystem _fileSystem;
        private readonly IProcessRunner _processRunner;

        public VPNController(IVPNService vpnService, ILogger<VPNController> logger, IOpenSslHelper helper, IFileSystem fileSystem, IProcessRunner processRunner)
        {
            _vpnService = vpnService;
            _logger = logger;
            _helper = helper;
            _fileSystem = fileSystem;
            _processRunner = processRunner;
        }

        [HttpPost("client-connected")]
        public IActionResult ClientConnected([FromBody] VpnClientConnectModel model)
        {
            _logger.LogInformation("🔗 Received VPN client connection:");
            _logger.LogInformation("📛 ClientName: {ClientName}", model.ClientName);
            _logger.LogInformation("🌐 AssignedIp: {AssignedIp}", model.AssignedIp);
            _logger.LogInformation("📝 Description: {Description}", model.Description);
            _logger.LogInformation("👤 UserID: {}", model.CompanyID);

            if (model.UiBuilderUrls?.Count > 0)
            {
                _logger.LogInformation("📄 Received uibuilder URLs: ", model.UiBuilderUrls);
                _logger.LogInformation("📄 Received uibuilder URLs: {Urls}", string.Join(", ", model.UiBuilderUrls));
            }

            if (string.IsNullOrWhiteSpace(model.ClientName) ||
                string.IsNullOrWhiteSpace(model.AssignedIp) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                model.CompanyID <= 0)
            {
                _logger.LogWarning("⚠️ Invalid data received from VPN client.");
                return BadRequest("Invalid client data.");
            }

            _vpnService.SaveClientConnection(model.ClientName, model.Description, model.CompanyID, model.AssignedIp, model.UiBuilderUrls);
            _logger.LogInformation("✅ Client info saved successfully.");

            _logger.LogInformation("📄 Received uibuilder URLs: ", model.UiBuilderUrls);

            if (model.UiBuilderUrls?.Count > 0)
            {
                _logger.LogInformation("📄 Received uibuilder URLs: ", model.UiBuilderUrls);
                _logger.LogInformation("📄 Received uibuilder URLs: {Urls}", string.Join(", ", model.UiBuilderUrls));
            }
            else
            {
                _logger.LogInformation("📄 No uibuilder URLs provided.");
            }

            return Ok();
        }

        [HttpGet("generate-machine/{clientName}/{companyName}/{subnet}")]
        public IActionResult GenerateMachine(string clientName, string companyName, string subnet)
        {
            string certName = $"{companyName}_machines_{clientName}";
            var opensslPath = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe";
            var caCertPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\ca.crt";
            var caKeyPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\private\ca.key";
            var certsRootFolder = @"C:\Program Files\OpenVPN\clients";

            if (!_helper.GenerateClientCert(certName, out var clientFolder, out var error))
                return BadRequest(new { error = "Failed to generate cert: " + error });

            string companyPath = $"{companyName}_machines_{clientName}";
            string companySubnet = subnet;

            string base64Company = _helper.EncryptToBase64(companyPath, opensslPath, caKeyPath);
            string base64Subnet = _helper.EncryptToBase64(companySubnet, opensslPath, caKeyPath);

            // Write both Base64-encoded lines into auth.txt
            string finalAuthPath = Path.Combine(clientFolder, "auth.txt");
            _fileSystem.WriteAllLines(finalAuthPath, new[] { base64Company, base64Subnet });

            // Read remaining files
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

        [HttpGet("generate-user/{clientName}/{companyName}/{subnet}")]
        public IActionResult GenerateUser(string clientName, string companyName, string subnet)
        {
            var opensslPath = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe";
            var caCertPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\ca.crt";
            var caKeyPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\private\ca.key";
            var certsRootFolder = @"C:\Program Files\OpenVPN\clients";

            var helper = new OpenSslHelper(opensslPath, caCertPath, caKeyPath, certsRootFolder);
            string certName = $"{companyName}_users_{clientName}";

            if (!helper.GenerateClientCert(certName, out var clientFolder, out var error))
                return BadRequest(new { error = "Failed to generate cert: " + error });

            string companyPath = $"{companyName}_users_{clientName}";
            string companySubnet = subnet;

           


            string base64Company = _helper.EncryptToBase64(companyPath, opensslPath, caKeyPath);
            string base64Subnet = _helper.EncryptToBase64(companySubnet, opensslPath, caKeyPath);

            // Write both Base64-encoded lines into auth.txt
            string finalAuthPath = Path.Combine(clientFolder, "auth.txt");
            System.IO.File.WriteAllLines(finalAuthPath, new[] { base64Company, base64Subnet });

            // Read remaining files
            var clientKey = System.IO.File.ReadAllBytes(Path.Combine(clientFolder, $"{certName}.key"));
            var clientCrt = System.IO.File.ReadAllBytes(Path.Combine(clientFolder, $"{certName}.crt"));
            var caCrt = System.IO.File.ReadAllBytes(caCertPath);
            var taKey = System.IO.File.ReadAllBytes(Path.Combine(@"C:\Program Files\OpenVPN\config-auto", "ta.key"));
            var authTxt = System.IO.File.ReadAllBytes(finalAuthPath);

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

        public class VpnClientConnectModel
    {
        public string ClientName { get; set; }
        public string Description { get; set; }
        public int CompanyID { get; set; }
        public string AssignedIp { get; set; }
        public List<string> UiBuilderUrls { get; set; }
    }
}
