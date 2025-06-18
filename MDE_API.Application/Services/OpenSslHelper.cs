using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Services
{
    using MDE_API.Application.Interfaces;
    using System.Diagnostics;
    using System.IO;

    public class OpenSslHelper: IOpenSslHelper
    {
       

        public OpenSslHelper()
        {
            
        }

        // Helper function to encrypt a string and return Base64-encoded result
        public string EncryptToBase64(string input, string openssl, string keyPath)
        {
            string tempInput = Path.GetTempFileName();
            string tempOutput = Path.GetTempFileName();

            try
            {
                System.IO.File.WriteAllText(tempInput, input);
                string cmd = $"rsautl -sign -inkey \"{keyPath}\" -in \"{tempInput}\" -out \"{tempOutput}\"";
                if (!RunOpenSSL(openssl, cmd, out var err))
                    throw new Exception($"Encryption failed: {err}");

                byte[] encryptedBytes = System.IO.File.ReadAllBytes(tempOutput);
                return Convert.ToBase64String(encryptedBytes);
            }
            finally
            {
                if (System.IO.File.Exists(tempInput)) System.IO.File.Delete(tempInput);
                if (System.IO.File.Exists(tempOutput)) System.IO.File.Delete(tempOutput);
            }
        }

        // Helper to run OpenSSL commands
        public bool RunOpenSSL(string opensslPath, string arguments, out string error)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = opensslPath,
                        Arguments = arguments,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool GenerateClientCert(string clientName, out string clientFolder, out string error)
        {
            error = null;
            clientFolder = Path.Combine(@"C:\Program Files\OpenVPN\clients", clientName);

            try
            {
                Directory.CreateDirectory(clientFolder);

                string keyPath = Path.Combine(clientFolder, $"{clientName}.key");
                string csrPath = Path.Combine(clientFolder, $"{clientName}.csr");
                string crtPath = Path.Combine(clientFolder, $"{clientName}.crt");
                string caPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\ca.crt";
                string caKeyPath = @"C:\Program Files\OpenVPN\easy-rsa\pki\private\ca.key";

                // 1. Generate client key
                if (!RunCommand($"genrsa -out \"{keyPath}\" 2048", clientFolder, out error)) return false;

                // 2. Generate CSR
                if (!RunCommand($"req -new -key \"{keyPath}\" -out \"{csrPath}\" -subj \"/CN={clientName}\"", clientFolder, out error)) return false;

                // 3. Sign with CA to produce client cert
                if (!RunCommand($"x509 -req -in \"{csrPath}\" -CA \"{caPath}\" -CAkey \"{caKeyPath}\" -CAcreateserial -out \"{crtPath}\" -days 365 -sha256", clientFolder, out error)) return false;

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
                FileName = @"C:\Program Files\OpenSSL-Win64\bin\openssl.exe",
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
