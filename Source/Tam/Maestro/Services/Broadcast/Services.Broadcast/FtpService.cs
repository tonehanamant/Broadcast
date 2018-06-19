using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;
using Common.Services.ApplicationServices;
using Services.Broadcast;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Logging;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Common.Services
{
    public interface IFtpService 
    {
        NetworkCredential GetCredentials(string ftpUserName, string ftpPassword);
        void UploadFile(NetworkCredential credentials, string sourceFilePath, string destPath);
        List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath);
        void DeleteFile(NetworkCredential credentials, string remoteFTPPath);

        void DownloadFileFromWebClient(WebClient webClient, string path, string localPath);
    }

    public class FtpService : IFtpService
    {
        public NetworkCredential GetCredentials(string ftpUserName, string ftpPassword)
        {
            return new NetworkCredential(ftpUserName, ftpPassword);
        }

        public void DeleteFile(NetworkCredential credentials, string remoteFTPPath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFTPPath);

            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = credentials;
            request.Proxy = null;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();

        }

        public void DownloadFileFromWebClient(WebClient webClient, string path, string localPath)
        {
            if (webClient == null)
                throw new InvalidEnumArgumentException("webClient must not be null");

            webClient.DownloadFile(path,localPath);
        }

        public void UploadFile(NetworkCredential credentials, string sourceFilePath, string destPath)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = credentials;
                ftpClient.UploadFile(destPath, "STOR", sourceFilePath);
            }
        }
        public List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            var request = (FtpWebRequest)WebRequest.Create(remoteFTPPath);

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = credentials;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            List<string> files = new List<string>();

            using (StreamReader reader = new StreamReader(responseStream))
            {
                var line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    files.Add(line);
                    line = reader.ReadLine();
                }
            }
            return files;
        }

        public void Dispose()
        {
        }
    }
}