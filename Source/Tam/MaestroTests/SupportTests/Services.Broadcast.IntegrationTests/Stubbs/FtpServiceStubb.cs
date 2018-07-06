using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Common.Services;


namespace Services.Broadcast.IntegrationTests
{
    public class FtpServiceStubb: IFtpService 
    {
        public static List<string> ResponseFromGetFileList { get; set; }

        public NetworkCredential GetCredentials(string ftpUserName, string ftpPassword)
        {
            return new NetworkCredential(ftpUserName,ftpPassword);
        }

        public void UploadFile(NetworkCredential credentials, string sourceFilePath, string destPath)
        {
            // do nothing, for now
        }

        public List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            return ResponseFromGetFileList;
        }

        public void DeleteFile(NetworkCredential credentials, string remoteFTPPath)
        {
            // do nothing for now
        }


        public void DownloadFileFromWebClient(WebClient webClient, string path, string localPath)
        {
            if (!File.Exists(localPath))
            {
                File.Create(localPath).Close();
                _FilesCreated.Add(localPath);
            }
        }

        private static readonly List<string> _FilesCreated = new List<string>();
        public static void CleanUpCreatedFiles()
        {
            _FilesCreated.ForEach(f =>
            {
                if (File.Exists(f))
                {
                    File.Delete(f);
                }
            });
            _FilesCreated.Clear();
        }}

}