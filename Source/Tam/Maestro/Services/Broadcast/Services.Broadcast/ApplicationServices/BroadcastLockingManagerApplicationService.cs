using Common.Services.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.ApplicationServices
{
    public class BroadcastLockingManagerApplicationService : ILockingManagerApplicationService
    {
        private readonly ISMSClient _SmsClient;

        public BroadcastLockingManagerApplicationService(ISMSClient smsClient)
        {
            _SmsClient = smsClient;
            System.Diagnostics.Debug.WriteLine("Initializing BroadcastLockingManagerApplicationService");
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
            if (_SmsClient.GetType().Name == "BOMSClient_InProcess")
            {
                return WindowsIdentity.GetCurrent().User.ToString();
            }

            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.LogonUserIdentity.User.Value;
            }

            return WindowsIdentity.GetCurrent().User.ToString();

        }

    }
}
