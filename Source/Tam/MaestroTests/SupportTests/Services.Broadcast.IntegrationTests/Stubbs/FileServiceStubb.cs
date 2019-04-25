using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common.Services;


namespace Services.Broadcast.IntegrationTests
{
    public class FileServiceStubb : IFileService
    {
        public virtual List<string> GetFiles(string path)
        {
            return new List<string>();
        }

        public virtual bool Exists(string path)
        {
            return true;
        }

        public virtual void Delete(params string[] path)
        {
        }

        public virtual string Move(string filePath, string destinationFolderPath)
        {
            return string.Empty;
        }

        public void CreateZipArchive(List<string> filePaths, string zipFileName)
        {
            throw new NotImplementedException();
        }

        public virtual string Copy(string filePath, string destinationPath, bool overwriteExisting = false)
        {
            throw new NotImplementedException();
        }

        public virtual string Copy(Stream inputStream, string destinationPath, bool overwriteExisting = false)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string filePath)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string filePath)
        {
            throw new NotImplementedException();
        }
    }

    public class FileServiceSingleFileStubb : FileServiceStubb
    {
        private readonly string _SingleFileName = "file1.txt";
        private readonly string _BasePath = "c:\\temp";
        public override List<string> GetFiles(string path)
        {
            return new List<string>() {_SingleFileName};
        }

        public override bool Exists(string path)
        {
            return string.Compare(Path.Combine(_BasePath, _SingleFileName), path,
                       StringComparison.CurrentCultureIgnoreCase) == 0;
        }
    }

    public class FileServiceDataLakeStubb : FileServiceStubb
    {
        private List<string> _Files = new List<string>();
        
        public override bool Exists(string path)
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            Debug.WriteLine($"===> Called Exists({path}) from {method.Name} {method.DeclaringType}");
            Debug.WriteLine("===> Existing files:");
            foreach(var file in _Files)
            {
                Debug.WriteLine(file);
            }
            Debug.WriteLine("<===");
            return _Files.Where(x=> x.Equals(path)).Count() == 1;
        }

        public override string Copy(string filePath, string destinationPath, bool overwriteExisting = false)
        {
            _Files.Add(destinationPath);
            return destinationPath;
        }

        public override string Copy(Stream inputStream, string destinationPath, bool overwriteExisting = false)
        {
            _Files.Add(destinationPath);
            return destinationPath;
        }

        public override void Delete(params string[] paths)
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            Debug.WriteLine($"===> Called Delete({paths}) from {method.Name} {method.DeclaringType}");
            Debug.WriteLine("===> Existing files:");
            foreach (string path in paths)
            {
                _Files.RemoveAll(x => x.Equals(path));
            }
            foreach (var file in _Files)
            {
                Debug.WriteLine(file);
            }
            Debug.WriteLine("<===");
        }
    }
}