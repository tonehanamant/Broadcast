using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        public override string Copy(string filePath, string destinationPath, bool overwriteExisting = false)
        {
            if (Exists(destinationPath))
            {
                if (overwriteExisting)
                {
                    Delete(destinationPath);
                }
                else
                {
                    throw new Exception($"Cannot overwrite {destinationPath}");
                }
            }

            File.Copy(filePath, destinationPath);

            return destinationPath;
        }

        public override string Copy(Stream inputStream, string destinationPath, bool overwriteExisting = false)
        {
            if (Exists(destinationPath))
            {
                if (overwriteExisting)
                {
                    Delete(destinationPath);
                }
                else
                {
                    throw new Exception($"Cannot overwrite {destinationPath}");
                }
            }

            using (var fileStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write))
            {
                inputStream.CopyTo(fileStream);
            }

            return destinationPath;
        }

        public override void Delete(params string[] path)
        {
            foreach(string file in path)
            {
                if (Exists(file))
                    File.Delete(file);
            }
        }
    }
}