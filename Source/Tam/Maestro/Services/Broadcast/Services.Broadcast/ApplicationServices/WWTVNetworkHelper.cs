using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using OfficeOpenXml.FormulaParsing.Exceptions;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public static class WWTVSharedNetworkHelper
    {
        #region Local Paths and network shared connections
        public static string GetLocalDropFolder()
        {
            return BroadcastServiceSystemParameter.WWTV_SharedFolder;
        }

        public static string GetLocalErrorFolder()
        {
            return BroadcastServiceSystemParameter.WWTV_LocalFtpErrorFolder;
        }

        public static string BuildLocalErrorPath(string invalidFilePath)
        {
            var combinedFilePath = Path.Combine(GetLocalErrorFolder(), invalidFilePath);
            return combinedFilePath;
        }

        public static SharedNetworkConnection GetLocalDropfolderConnection()
        {
            return WWTVSharedNetworkHelper.GetConnection(WWTVSharedNetworkHelper.GetLocalDropFolder());
        }
        public static SharedNetworkConnection GetLocalErrorFolderConnection()
        {
            return WWTVSharedNetworkHelper.GetConnection(WWTVSharedNetworkHelper.GetLocalErrorFolder());
        }

        #endregion

        #region Impersonation

        public static void Impersonate(Action actionToExecute)
        {
            var userName = BroadcastServiceSystemParameter.WWTV_SharedFolder_UserName;
            var password = BroadcastServiceSystemParameter.WWTV_SharedFolder_Password;

            ImpersonateUser.Impersonate("", userName, password, actionToExecute);
        }


        #endregion


        #region Shared Network Resource
        public static SharedNetworkConnection GetConnection(string sharedFolder)
        {
            var credentials = GetNetworkCredentials();
            return new SharedNetworkConnection(sharedFolder,credentials);
        }

        public static NetworkCredential GetNetworkCredentials()
        {
            var userName = BroadcastServiceSystemParameter.WWTV_SharedFolder_UserName;
            var password = BroadcastServiceSystemParameter.WWTV_SharedFolder_Password;
            //var userName = "svc_wwtvdata@crossmw.com";
            //var password = "78!ttwG&Dc$4fB2xZ94x";

            /*
                \\Cadfs10\tbn\WWTVData
                \\Cadfs10\tbn\WWTVErrors
            */
            if (userName == null)
                return null;

            var credentials = new NetworkCredential(userName, password);

            return credentials;
        }
        #endregion
    }
}
