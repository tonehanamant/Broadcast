using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Concurrent;
using System.Security.Principal;
using System.Web;
using Tam.Maestro.Services.Clients;
namespace Services.Broadcast.ApplicationServices
{

    public interface IBroadcastLockingService 
    {
        object GetNotUserBasedLockObjectForKey(string key);

        BroadcastLockResponse GetLockObject(string key);

        bool IsObjectLocked(string key);
        BroadcastLockResponse LockObject(string key);
        BroadcastReleaseLockResponse ReleaseObject(string key);
    }

    public class BroadcastLockingService : IBroadcastLockingService
    {
        private readonly ISMSClient _SmsClient;
        private readonly ConcurrentDictionary<string, object> _NotUserBasedLockObjects;

        public BroadcastLockingService(ISMSClient smsClient)
        {
            _SmsClient = smsClient;
            _NotUserBasedLockObjects = new ConcurrentDictionary<string, object>();
            System.Diagnostics.Debug.WriteLine("Initializing BroadcastLockingManagerApplicationService");
        }

        public BroadcastLockResponse LockObject(string key)
        {
            var lockResponse = _SmsClient.LockObject(key, GetUserSID());
            BroadcastLockResponse broadcastLockResponse = new BroadcastLockResponse
            {
                Error = lockResponse.Error,
                Key = lockResponse.Key,
                LockedUserId = lockResponse.LockedUserId,
                LockTimeoutInSeconds = lockResponse.LockTimeoutInSeconds,
                Success = lockResponse.Success
            };
            return broadcastLockResponse;
        }

        public BroadcastReleaseLockResponse ReleaseObject(string key)
        {           
            var releaseLockResponse = _SmsClient.ReleaseObject(key, GetUserSID());
            BroadcastReleaseLockResponse broadcastReleaseLockResponse = new BroadcastReleaseLockResponse
            {
                Error = releaseLockResponse.Error,
                Key = releaseLockResponse.Key,              
                Success = releaseLockResponse.Success
            };
            return broadcastReleaseLockResponse;
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

        public object GetNotUserBasedLockObjectForKey(string key)
        {
            return _NotUserBasedLockObjects.GetOrAdd(key, new object());
        }

        public BroadcastLockResponse GetLockObject(string key)
        {
            if (IsObjectLocked(key))
            {
                // if the key is locked you won`t be able to lock it but you can get the person`s name who has locked it
                return LockObject(key);
            }
            else
            {
                // this is a usual Success = true response
                return new BroadcastLockResponse
                {
                    Key = key,
                    Success = true,
                    LockTimeoutInSeconds = 900
                };
            }
        }

        

    }
}

