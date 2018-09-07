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
    }
    public class FileServiceSingleFileStubb : FileServiceStubb
    {
        private string _SingleFileName = "file1.txt";
        private string _BasePath = "c:\\temp";
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
}