using MDE_API.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Application.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MDE_API.Application.Services
{
    public class VPNService: IVPNService
    {
        
            private readonly IVPNRepository _vpnRepository;

            public VPNService(IVPNRepository vpnRepository)
            {
                _vpnRepository = vpnRepository;
            }

        public async Task AddCloudflareDnsRecordAsync(string baseName)
        {
            string apiToken = "baa728e93aa7b8d9266ca9cdf9fba78cf3d31";
            string zoneId = "8eb734374b1ef138b41104b31620f7ea"; // From Cloudflare dashboard

            var domains = new[]
            {
                   $"{baseName}.mde-portal.site",
                   $"{baseName}-camera.mde-portal.site",
                   $"{baseName}-vnc.mde-portal.site"
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            foreach (var domain in domains)
            {
                var payload = new
                {
                    type = "A",
                    name = domain,
                    content = "217.63.76.110",
                    ttl = 120,
                    proxied = true
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records",
                    content);

                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                   // _logger.LogInformation("✅ Created DNS record for {Domain}", domain);
                }
                else
                {
                    //_logger.LogError("❌ Failed to create DNS for {Domain}: {Response}", domain, result);
                }
            }
        }


        public int SaveClientConnection(string clientName, string description, int companyId, string assignedIp, List<string> uibuilderUrls)
            {
                return _vpnRepository.SaveClientConnection(clientName, description, companyId, assignedIp, uibuilderUrls);
            }
        }




       
    
}
