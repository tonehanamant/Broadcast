
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Services
{
    public interface IFileService
    {
        List<string> GetFiles(string path);
        bool Exists(string path);
        void Delete(string path);
    }

    public class FileService : IFileService
    {
        public List<string> GetFiles(string path)
        {
            return Directory.GetFiles(path).ToList();
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }
    }
}