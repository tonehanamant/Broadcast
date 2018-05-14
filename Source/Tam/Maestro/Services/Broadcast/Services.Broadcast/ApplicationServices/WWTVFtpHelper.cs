using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using OfficeOpenXml.FormulaParsing.Exceptions;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public static class WWTVFtpHelper
    {
        private const string FTP_SCHEME = "ftp://";

        public static NetworkCredential GetFtpClientCredentials()
        {
            return new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                BroadcastServiceSystemParameter.WWTV_FtpPassword);
        }

        
        #region Remote path getters 

        public static string GetOutboundPath()
        {
            return $"ftp://{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpOutboundFolder}";
        }
        
        public static string GetErrorPath()
        {
            return $"ftp://{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpErrorFolder}";
        }

        public static string GetInboundPath()
        {
            return
                $"{FTP_SCHEME}{BroadcastServiceSystemParameter.WWTV_FtpHost}/{BroadcastServiceSystemParameter.WWTV_FtpInboundFolder}";
        }

        #endregion


        #region Basic FTP operations
        public static void UploadFile(string sourceFilePath, string destPath, Action<string> OnSuccessfulUpload)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = GetFtpClientCredentials();
                ftpClient.UploadFile(sourceFilePath, destPath);

                OnSuccessfulUpload.Invoke(sourceFilePath);
            }
        }


        public static List<string> GetFileList(string remoteFTPPath, Func<string, bool> isValidFile = null)
        {
            var request = (FtpWebRequest)WebRequest.Create(remoteFTPPath);

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = WWTVFtpHelper.GetFtpClientCredentials();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            List<string> files = new List<string>();

            using (StreamReader reader = new StreamReader(responseStream))
            {
                var line = reader.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    if (isValidFile == null || isValidFile(line))
                        files.Add(line);

                    line = reader.ReadLine();
                }
            }
            return files;
        }


        public static void DeleteFiles(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                DeleteFile(fileName);
            }
        }

        public static void DeleteFile(string remoteFTPPath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFTPPath);

            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = GetFtpClientCredentials();
            request.Proxy = null;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }

#endregion
    }
}
