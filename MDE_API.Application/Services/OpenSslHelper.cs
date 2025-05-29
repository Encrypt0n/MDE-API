using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Services
{
    using System.Diagnostics;

    public class OpenSslHelper
    {
        private readonly string _opensslPath;
        private readonly string _caCertPath;
        private readonly string _caKeyPath;
        private readonly string _certsRootFolder;

        public OpenSslHelper(string opensslPath, string caCertPath, string caKeyPath, string certsRootFolder)
        {
            _opensslPath = opensslPath;
            _caCertPath = caCertPath;
            _caKeyPath = caKeyPath;
            _certsRootFolder = certsRootFolder;
        }

        public bool GenerateClientCert(string clientName, out string clientFolder, out string error)
        {
            error = null;
            clientFolder = Path.Combine(_certsRootFolder, clientName);

            try
            {
                Directory.CreateDirectory(clientFolder);

                string keyPath = Path.Combine(clientFolder, $"{clientName}.key");
                string csrPath = Path.Combine(clientFolder, $"{clientName}.csr");
                string crtPath = Path.Combine(clientFolder, $"{clientName}.crt");

                // 1. Generate client key
                if (!RunCommand($"genrsa -out \"{keyPath}\" 2048", clientFolder, out error)) return false;

                // 2. Generate CSR
                if (!RunCommand($"req -new -key \"{keyPath}\" -out \"{csrPath}\" -subj \"/CN={clientName}\"", clientFolder, out error)) return false;

                // 3. Sign with CA to produce client cert
                if (!RunCommand($"x509 -req -in \"{csrPath}\" -CA \"{_caCertPath}\" -CAkey \"{_caKeyPath}\" -CAcreateserial -out \"{crtPath}\" -days 365 -sha256", clientFolder, out error)) return false;

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private bool RunCommand(string args, string workingDir, out string error)
        {
            error = null;
            var psi = new ProcessStartInfo
            {
                FileName = _opensslPath,
                Arguments = args,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                error = stderr;
                return false;
            }

            return true;
        }
    }

}
