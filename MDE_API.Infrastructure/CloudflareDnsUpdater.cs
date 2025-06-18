using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDE_API.Infrastructure
{
    public class CloudflareDnsUpdater : IVPNClientConnectedObserver
    {
        private readonly ILogger<CloudflareDnsUpdater> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiToken = "6WYx0f-LkSgujUSxbn0EUbXSSd8DI0B75ptvsxwI"; // Move to config!
        private readonly string _zoneId = "8eb734374b1ef138b41104b31620f7ea";       // Move to config!

        public CloudflareDnsUpdater(HttpClient httpClient, ILogger<CloudflareDnsUpdater> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
        }

        public async Task OnClientConnectedAsync(VpnClientConnectModel model, string baseName, int machineId)
        {
            var records = new[]
            {
                new {
                    type = "A",
                    name = $"{baseName}.mde-portal.site",
                    content = "217.63.76.110",
                    ttl = 120,
                    proxied = false
                },
                new {
                    type = "A",
                    name = $"{baseName}-camera.mde-portal.site",
                    content = "217.63.76.110",
                    ttl = 120,
                    proxied = false
                },
                new {
                    type = "A",
                    name = $"{baseName}-vnc.mde-portal.site",
                    content = "217.63.76.110",
                    ttl = 120,
                    proxied = false
                }
            };

            var payload = new
            {
                posts = records
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://api.cloudflare.com/client/v4/zones/{_zoneId}/dns_records/batch",
                content
            );

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Batch DNS records created for {BaseName}", baseName);
            }
            else
            {
                _logger.LogError("❌ Failed to create DNS records for {BaseName}. Response: {Response}", baseName, result);
            }
        }
    }
}
