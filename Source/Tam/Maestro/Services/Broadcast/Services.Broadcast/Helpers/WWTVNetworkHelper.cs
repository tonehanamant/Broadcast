using System;
using System.IO;
using System.Net;
using Services.Broadcast.ApplicationServices.Security;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices.Helpers
{
    public interface IWWTVSharedNetworkHelper
    {
        void Impersonate(Action actionToExecute);
    }

    public class WWTVSharedNetworkHelper : IWWTVSharedNetworkHelper
    {
        private readonly IImpersonateUser _ImpersonateUser;

        public WWTVSharedNetworkHelper(IImpersonateUser impersonateUser)
        {
            _ImpersonateUser = impersonateUser;
        }

        public void Impersonate(Action actionToExecute)
        {
            var userName = BroadcastServiceSystemParameter.WWTV_SharedFolder_UserName;
            var password = BroadcastServiceSystemParameter.WWTV_SharedFolder_Password;

            _ImpersonateUser.Impersonate("", userName, password, actionToExecute);
        }
                
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
            return GetConnection(GetLocalDropFolder());
        }
        public static SharedNetworkConnection GetLocalErrorFolderConnection()
        {
            return GetConnection(GetLocalErrorFolder());
        }

        #endregion

        #region Impersonation

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
