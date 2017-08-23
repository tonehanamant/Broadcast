using System;
using System.Security.Principal;
using System.Web;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Common.Services.ApplicationServices
{
    public interface ILockingManagerApplicationService : IApplicationService
    {
        LockResponse LockObject(string key);
        ReleaseLockResponse ReleaseObject(string key);
        bool IsObjectLocked(string key);
    }

    public class LockingManagerApplicationService : ILockingManagerApplicationService
    {
        private readonly ISMSClient _SmsClient;

        public LockingManagerApplicationService(ISMSClient smsClient)
        {
            _SmsClient = smsClient;
            System.Diagnostics.Debug.WriteLine("Initializing LockingManagerApplicationService");
        }

        public LockResponse LockObject(string key)
        {
            return _SmsClient.LockObject(key, GetUserSID());
        }

        public ReleaseLockResponse ReleaseObject(string key)
        {
            return _SmsClient.ReleaseObject(key, GetUserSID());
        }

        public bool IsObjectLocked(string key)
        {
            return _SmsClient.IsObjectLocked(key, GetUserSID());
        }

        private String GetUserSID()
        {
            string result;
            if (_SmsClient.GetType().Name == "BOMSClient_InProcess")
            {
                result = WindowsIdentity.GetCurrent().User.ToString();
            }
            else
            {
                result = HttpContext.Current == null ? null : HttpContext.Current.Request.LogonUserIdentity.User.Value;
            }
            return result;
        }
    }
}
