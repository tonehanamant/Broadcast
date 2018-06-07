﻿using System;
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

        public static string Host
        {
            get
            {
                // uncomment if debugging locally (don't forget to recomment when checking in)
                //return "localhost";
                return BroadcastServiceSystemParameter.WWTV_FtpHost;
            }
        }
        public static NetworkCredential GetFtpClientCredentials()
        {
            return new NetworkCredential(BroadcastServiceSystemParameter.WWTV_FtpUsername,
                BroadcastServiceSystemParameter.WWTV_FtpPassword);
        }

        
        #region Remote path getters 

        public static string GetFTPOutboundPath()
        {
            var path = BroadcastServiceSystemParameter.WWTV_FtpOutboundFolder;

            return $"{FTP_SCHEME}{Host}/{path}";
        }
        
        public static string GetFTPErrorPath()
        {
            var path = BroadcastServiceSystemParameter.WWTV_FtpErrorFolder;

            return $"{FTP_SCHEME}{Host}/{path}";
        }

        public static string GetFTPInboundPath()
        {
            var path = BroadcastServiceSystemParameter.WWTV_FtpInboundFolder;

            return $"{FTP_SCHEME}{Host}/{path}";
        }

        #endregion


        #region Basic FTP operations
        public static void UploadFile(string sourceFilePath, string destPath, Action<string> OnSuccessfulUpload)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = GetFtpClientCredentials();
                ftpClient.UploadFile(destPath, "STOR",sourceFilePath);

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
