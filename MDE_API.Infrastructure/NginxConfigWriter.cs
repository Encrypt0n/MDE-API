using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Infrastructure
{
    using System.Text;
    using MDE_API.Application.Interfaces;
    using MDE_API.Domain.Models;
    using Microsoft.Extensions.Logging;

    public class NginxConfigWriter : IVPNClientConnectedObserver
    {
        private readonly ILogger<NginxConfigWriter> _logger;
        private readonly string _confPath = @"C:\nginx\conf\nginx-clients.conf";
        private readonly string _nginxExePath = @"C:\nginx\nginx.exe";

        public NginxConfigWriter(ILogger<NginxConfigWriter> logger)
        {
            _logger = logger;
        }

        public async Task OnClientConnectedAsync(VpnClientConnectModel model,string baseName, int machineId)
        {
            string nodeRedDomain = $"{baseName}.mde-portal.site";
            string cameraDomain = $"{baseName}-camera.mde-portal.site";
            string vncDomain = $"{baseName}-vnc.mde-portal.site";

            string nodeRedBlock = $@"
# Auto-generated Node-RED for {baseName}
server {{
    listen 444 ssl;
    server_name {nodeRedDomain};

    ssl_certificate      certs/mde-portal_chain.pem;
    ssl_certificate_key  certs/mde-portal_key.pem;

    location ~ ^/([\w\-]+)$ {{
    proxy_pass_request_body off;
    proxy_set_header Content-Length """";
    set $machine_id {machineId};

    set $auth_token """";
    if ($arg_token != """") {{
        set $auth_token ""Bearer $arg_token"";
    }}
    proxy_set_header Authorization $auth_token;

    proxy_intercept_errors on;
    proxy_pass http://localhost:5000/api/auth/validate?machineId=$machine_id;

    error_page 418 = @auth_success;
    error_page 401 = @unauthorized;
}}

    location @auth_success {{
    add_header Set-Cookie ""mde_auth_token=$arg_token; Path=/; Max-Age=30; SameSite=Strict"";
    return 302 $uri/;
}}

    location @unauthorized {{
    return 401 ""Unauthorized"";
}}

    location ~ ^/([\w\-]+)/ {{
    if ($cookie_mde_auth_token = """") {{ return 403; }}

    proxy_pass http://{model.AssignedIp}:1880;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection ""upgrade"";
}}

  
}}";

            string cameraBlock = $@"
# Auto-generated Camera for {baseName}
server {{
    listen 444 ssl;
    server_name {cameraDomain};

    ssl_certificate      certs/mde-portal_chain.pem;
    ssl_certificate_key  certs/mde-portal_key.pem;


location ~ ^/([\w\-]+)$ {{
    proxy_pass_request_body off;
    proxy_set_header Content-Length """";
    set $machine_id {machineId};

    set $auth_token """";
    if ($arg_token != """") {{
        set $auth_token ""Bearer $arg_token"";
    }}
    proxy_set_header Authorization $auth_token;

    proxy_intercept_errors on;
    proxy_pass http://localhost:5000/api/auth/validate?machineId=$machine_id;

    error_page 418 = @auth_success;
    error_page 401 = @unauthorized;
}}

    location @auth_success {{
    add_header Set-Cookie ""mde_auth_token=$arg_token; Path=/; Max-Age=30; SameSite=Strict"";
    return 302 $uri/;
}}

    location @unauthorized {{
    return 401 ""Unauthorized"";
}}

    location ~ ^/([\w\-]+)/ {{
    if ($cookie_mde_auth_token = """") {{ return 403; }}

    proxy_pass http://192.168.0.30:88;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection ""upgrade"";
}}
}}";

            string vncBlock = $@"
# Auto-generated VNC for {baseName}
server {{
    listen 444 ssl;
    server_name {vncDomain};

    ssl_certificate      certs/mde-portal_chain.pem;
    ssl_certificate_key  certs/mde-portal_key.pem;

location ~ ^/([\w\-]+)$ {{
    proxy_pass_request_body off;
    proxy_set_header Content-Length """";
    set $machine_id {machineId};

    set $auth_token """";
    if ($arg_token != """") {{
        set $auth_token ""Bearer $arg_token"";
    }}
    proxy_set_header Authorization $auth_token;

    proxy_intercept_errors on;
    proxy_pass http://localhost:5000/api/auth/validate?machineId=$machine_id;

    error_page 418 = @auth_success;
    error_page 401 = @unauthorized;
}}

    location @auth_success {{
    add_header Set-Cookie ""mde_auth_token=$arg_token; Path=/; Max-Age=30; SameSite=Strict"";
    return 302 $uri/;
}}

    location @unauthorized {{
    return 401 ""Unauthorized"";
}}


    location ~ ^/([\w\-]+)/ {{
    if ($cookie_mde_auth_token = """") {{ return 403; }}
    

 

    proxy_pass http://{model.AssignedIp}:6080;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection ""upgrade"";
}}
}}";

            try
            {
                string existingConf = System.IO.File.Exists(_confPath)
                    ? await System.IO.File.ReadAllTextAsync(_confPath)
                    : string.Empty;

                List<string> addedDomains = new();
                StringBuilder newBlocks = new();

                if (!existingConf.Contains($"server_name {nodeRedDomain};"))
                {
                    newBlocks.AppendLine(nodeRedBlock);
                    addedDomains.Add(nodeRedDomain);
                }

                if (!existingConf.Contains($"server_name {cameraDomain};"))
                {
                    newBlocks.AppendLine(cameraBlock);
                    addedDomains.Add(cameraDomain);
                }

                if (!existingConf.Contains($"server_name {vncDomain};"))
                {
                    newBlocks.AppendLine(vncBlock);
                    addedDomains.Add(vncDomain);
                }

                if (newBlocks.Length > 0)
                {
                    await System.IO.File.AppendAllTextAsync(_confPath, newBlocks.ToString());
                    _logger.LogInformation("📄 Added new server blocks for: {Domains}", string.Join(", ", addedDomains));

                    ReloadNginx();
                }
                else
                {
                    _logger.LogInformation("ℹ️ All server blocks for {BaseName} already exist.", baseName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to update nginx config.");
            }
        }

        private void ReloadNginx()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c \"C:\\nginx\\nginx.exe -p C:\\nginx -c conf\\nginx.conf -s reload\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("✅ NGINX reloaded successfully.");
                }
                else
                {
                    _logger.LogError("❌ NGINX reload failed:\n{Error}", error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while reloading NGINX.");
            }
        }
    }

}
