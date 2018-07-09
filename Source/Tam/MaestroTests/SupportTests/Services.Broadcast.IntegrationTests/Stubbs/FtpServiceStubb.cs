using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Common.Services;
using Tam.Maestro.Services.ContractInterfaces;


namespace Services.Broadcast.IntegrationTests
{
    public class FtpServiceStubb_Empty : IFtpService
    {
        public virtual NetworkCredential GetCredentials(string ftpUserName, string ftpPassword)
        {
            return new NetworkCredential(ftpUserName, ftpPassword);
        }

        public virtual void UploadFile(NetworkCredential credentials, string sourceFilePath, string destPath)
        {
        }

        public virtual List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            return new List<string>();
        }

        public virtual void DeleteFile(NetworkCredential credentials, string remoteFTPPath)
        {
        }


        public virtual void DownloadFile(WebClient webClient, string path, string localPath)
        {
        }

        public virtual Stream DownloadFileToStream(WebClient webClient, string path)
        {
            return new MemoryStream(); 
        }

    }

    public class FtpServiceStubb_SingleFile : FtpServiceStubb_Empty
    {
        public override List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            return new List<string>()
            {
                "Special_Ftp_Phantom_File.txt"
            };
        }

        protected virtual string GetFileContents()
        {
            return "Sample Content for Special_Ftp_Phantom_File.txt";
        }

        public override void DownloadFile(WebClient webClient, string path, string localPath)
        {
            if (!File.Exists(localPath))
            {
                using (var file = File.Create(localPath))
                {
                    var byteArray = GetFileContents().ToByteArray();
                    file.Write(byteArray,0,byteArray.Length);
                    _FilesCreated.Add(localPath);
                }
            }
        }

        public override Stream DownloadFileToStream(WebClient webClient, string path)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(GetFileContents() ?? ""));
        }

        private readonly List<string> _FilesCreated = new List<string>();
        public void CleanUpCreatedFiles()
        {
            _FilesCreated.ForEach(f =>
            {
                if (File.Exists(f))
                {
                    File.Delete(f);
                }
            });
            _FilesCreated.Clear();
        }
    }
}