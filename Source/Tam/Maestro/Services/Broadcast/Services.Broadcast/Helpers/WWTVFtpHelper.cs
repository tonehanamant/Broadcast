using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using Common.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices.Helpers
{
    public interface IWWTVFtpHelper
    {
        string Host { get; }
        NetworkCredential GetClientCredentials();
        string GetOutboundPath();
        string GetErrorPath();
        string GetInboundPath();

        #region Client Operations (WebClient style ftp)

        /// <summary>
        /// Creates new WebClient with proper credentials to be used accross multiple client operations.
        /// </summary>
        WebClient EnsureFtpClient();
        void DownloadFileFromClient(WebClient client, string path, string localPath);

        #endregion

        void UploadFile(string sourceFilePath, string destPath, Action<string> OnSuccessfulUpload);

        string DownloadFileFtpToString(string fileName);

        List<string> GetFtpErrorFileList(Func<string, bool> isValidFile = null);
        List<string> GetInboundFileList(Func<string, bool> isValidFile = null);
        void DeleteFiles(List<string> fileNames);
        void DeleteFile(string remoteFfpPath);
    }

    public class WWTVFtpHelper : IWWTVFtpHelper
    {
        private const string FTP_SCHEME = "ftp://";

        private IFtpService _FtpService;
        public string Host
        {
            get
            {
                // uncomment if debugging locally (don't forget to recomment when checking in)
                //return "localhost";
                return BroadcastServiceSystemParameter.WWTV_FtpHost;
            }
        }

        public WWTVFtpHelper(IFtpService ftpService)
        {
            _FtpService = ftpService;
        }
        public NetworkCredential GetClientCredentials()
        {
            return new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                BroadcastServiceSystemParameter.WWTV_FtpPassword);
        }

        
        #region Remote path getters 

        public string GetOutboundPath()
        {
            var path = BroadcastServiceSystemParameter.WWTV_FtpOutboundFolder;

            return $"{FTP_SCHEME}{Host}/{path}";
        }
        
        public string GetErrorPath()
        {
            var path = BroadcastServiceSystemParameter.WWTV_FtpErrorFolder;

            return $"{FTP_SCHEME}{Host}/{path}";
        }

        public string GetInboundPath()
        {
            var path = BroadcastServiceSystemParameter.WWTV_FtpInboundFolder;

            return $"{FTP_SCHEME}{Host}/{path}";
        }

        #endregion


        #region Basic FTP operations

        public WebClient EnsureFtpClient()
        {
            var webClient = new WebClient();
            webClient.Credentials = GetClientCredentials();

            return webClient;
        }

        public void DownloadFileFromClient(WebClient webClient, string path, string localPath)
        {
            if (webClient == null)
                throw new InvalidEnumArgumentException("WebClient parameter must not be null");

            _FtpService.DownloadFile(webClient,path, localPath);
        }

        public void UploadFile(string sourceFilePath, string destPath, Action<string> OnSuccessfulUpload = null)
        {
            var credentials = GetClientCredentials();
            _FtpService.UploadFile(credentials, sourceFilePath, destPath);

            OnSuccessfulUpload?.Invoke(sourceFilePath);
        }

        public string DownloadFileFtpToString(string fileName)
        {
            var shareFolder = GetInboundPath();
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = GetClientCredentials();
                using (StreamReader reader = new StreamReader(_FtpService.DownloadFileToStream(ftpClient, $"{shareFolder}/{fileName}")))
                {
                    return reader.ReadToEnd();
                }
            }

        }



        public List<string> GetFtpErrorFileList(Func<string, bool> isValidFile = null)
        {
            var remoteFTPPath = GetErrorPath();

            var credentials = GetClientCredentials();
            var list = _FtpService.GetFileList(credentials, remoteFTPPath);

            var validList = list;
            if (isValidFile != null)
                validList = list.Where(f => isValidFile(f)).ToList();

            return validList;
        }

        public List<string> GetInboundFileList(Func<string, bool> isValidFile = null)
        {
            string remoteFTPPath = GetInboundPath();

            var credentials = GetClientCredentials();
            var list = _FtpService.GetFileList(credentials, remoteFTPPath);

            var validList = list;
            if (isValidFile != null)
                validList = list.Where(f => isValidFile(f)).ToList();

            return validList;
        }


        public void DeleteFiles(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                DeleteFile(fileName);
            }
        }

        public void DeleteFile(string remoteFtpPath)
        {
            _FtpService.DeleteFile(GetClientCredentials(), remoteFtpPath);
        }

        #endregion
    }
}
