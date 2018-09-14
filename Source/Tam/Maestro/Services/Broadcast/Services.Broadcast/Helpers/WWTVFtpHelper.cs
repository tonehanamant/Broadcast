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

        /// <summary>
        /// Gets the newtwork credentials for WWTV FTP
        /// </summary>
        /// <returns>NetworkCredential object</returns>
        NetworkCredential GetClientCredentials();

        /// <summary>
        /// Builds the complete path with scheme, host and a specified path
        /// </summary>
        /// <param name="path">Specific path inside FTP directory structure</param>
        /// <returns>Full path</returns>
        string GetRemoteFullPath(string path);

        #region Client Operations (WebClient style ftp)

        /// <summary>
        /// Creates new WebClient with proper credentials to be used accross multiple client operations.
        /// </summary>
        WebClient EnsureFtpClient();
        void DownloadFileFromClient(WebClient client, string path, string localPath);

        #endregion

        void UploadFile(string sourceFilePath, string destPath, Action<string> OnSuccessfulUpload);

        /// <summary>
        /// Downloads a ftp file
        /// </summary>
        /// <param name="fileName">File name to download</param>
        /// <param name="success">True or false based on download success</param>
        /// <param name="errorMessage">Empty or not based on download success</param>
        /// <returns>File content as string</returns>
        string DownloadFTPFileContent(string fileName, out bool success, out string errorMessage);

        List<string> GetFtpErrorFileList(string path, Func<string, bool> isValidFile = null);

        /// <summary>
        /// Get a list of files from the remote path.
        /// </summary>
        /// <param name="path">Remote path</param>
        /// <param name="isValidFile">Function that will filter the remote files</param>
        /// <returns>List of remote file paths</returns>
        List<string> GetInboundFileList(string path, Func<string, bool> isValidFile = null);

        /// <summary>
        /// Delete files from the ftp path
        /// </summary>
        /// <param name="filePaths">List of filepaths to delete</param>
        void DeleteFiles(params string[] fileNames);
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

        /// <summary>
        /// Gets the newtwork credentials for WWTV FTP
        /// </summary>
        /// <returns>NetworkCredential object</returns>
        public NetworkCredential GetClientCredentials()
        {
            return new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                BroadcastServiceSystemParameter.WWTV_FtpPassword);
        }

        
        #region Remote path getter

        /// <summary>
        /// Builds the complete path with scheme, host and a specified path
        /// </summary>
        /// <param name="path">Specific path inside FTP directory structure</param>
        /// <returns>Full path</returns>
        public string GetRemoteFullPath(string path)
        {
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

        /// <summary>
        /// Downloads a ftp file
        /// </summary>
        /// <param name="fileName">File name to download</param>
        /// <param name="success">True or false based on download success</param>
        /// <param name="errorMessage">Empty or not based on download success</param>
        /// <returns>File content as string</returns>
        public string DownloadFTPFileContent(string fileName, out bool success, out string errorMessage)
        {
            try
            {
                success = true;
                errorMessage = string.Empty;

                var shareFolder = GetRemoteFullPath(BroadcastServiceSystemParameter.WWTV_FtpInboundFolder);
                using (var ftpClient = new WebClient())
                {
                    ftpClient.Credentials = GetClientCredentials();
                    using (StreamReader reader = new StreamReader(_FtpService.DownloadFileToStream(ftpClient, $"{shareFolder}/{fileName}")))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
                errorMessage = ex.ToString();
                return string.Empty;
            }
        }

        public List<string> GetFtpErrorFileList(string path, Func<string, bool> isValidFile = null)
        {
            var remoteFTPPath = GetRemoteFullPath(path);

            var credentials = GetClientCredentials();
            var list = _FtpService.GetFileList(credentials, remoteFTPPath);

            var validList = list;
            if (isValidFile != null)
                validList = list.Where(f => isValidFile(f)).ToList();

            return validList;
        }

        /// <summary>
        /// Get a list of files from the remote path.
        /// </summary>
        /// <param name="path">Remote path</param>
        /// <param name="isValidFile">Function that will filter the remote files</param>
        /// <returns>List of remote file paths</returns>
        public List<string> GetInboundFileList(string path, Func<string, bool> isValidFile = null)
        {
            string remoteFTPPath = GetRemoteFullPath(path);

            var credentials = GetClientCredentials();
            var list = _FtpService.GetFileList(credentials, remoteFTPPath);

            var validList = list;
            if (isValidFile != null)
                validList = list.Where(f => isValidFile(f)).ToList();

            return validList;
        }

        /// <summary>
        /// Delete files from the ftp path
        /// </summary>
        /// <param name="filePaths">List of filepaths to delete</param>
        public void DeleteFiles(params string[] filePaths)
        {
            foreach (var fileName in filePaths)
            {
                _FtpService.DeleteFile(GetClientCredentials(), fileName);
            }
        }
        
        #endregion
    }
}
