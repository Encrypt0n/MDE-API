using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IOpenSslHelper
    {
        bool GenerateClientCert(string certName, out string clientFolder, out string error);
        string EncryptToBase64(string input, string openssl, string keyPath);
        bool RunOpenSSL(string opensslPath, string arguments, out string error);

       
    }

    public interface IFileSystem
    {
        void WriteAllText(string path, string contents);
        void WriteAllLines(string path, IEnumerable<string> contents);
        byte[] ReadAllBytes(string path);
        bool FileExists(string path);
        void DeleteFile(string path);
        string GetTempFileName();

    }

    

}
