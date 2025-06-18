using MDE_API.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Services
{
    public class FileSystem : IFileSystem
    {
        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

        public void WriteAllLines(string path, string[] lines) => File.WriteAllLines(path, lines);

        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);

        public void WriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path, contents);

        public bool FileExists(string path) => File.Exists(path);

        public void DeleteFile(string path) => File.Delete(path);

        public string GetTempFileName() => Path.GetTempFileName();
    }


}
