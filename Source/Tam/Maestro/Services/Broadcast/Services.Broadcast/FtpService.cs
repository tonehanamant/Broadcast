using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace Common.Services
{
    public interface IFtpService 
    {
        NetworkCredential GetCredentials(string ftpUserName, string ftpPassword);
        void UploadFile(NetworkCredential credentials, string sourceFilePath, string destPath);
        List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath);
        void DeleteFile(NetworkCredential credentials, string remoteFTPPath);
        void DownloadFile(WebClient webClient, string path, string localPath);
        Stream DownloadFileToStream(WebClient webClient, string path);
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

        public void DownloadFile(WebClient webClient, string path, string localPath)
        {
            if (webClient == null)
                throw new InvalidEnumArgumentException("webClient must not be null");

            webClient.DownloadFile(path,localPath);
        }

        public Stream DownloadFileToStream(WebClient webClient, string path)
        {
            if (webClient == null)
                throw new InvalidEnumArgumentException("webClient must not be null");

            return webClient.OpenRead(path);
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
            List<string> files = new List<string>();

            using (Stream responseStream = response.GetResponseStream())
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