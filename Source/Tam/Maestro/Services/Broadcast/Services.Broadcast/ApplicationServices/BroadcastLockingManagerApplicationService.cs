using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Concurrent;
using System.Security.Principal;
using System.Web;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.ApplicationServices
{
    public interface IBroadcastLockingManagerApplicationService : ILockingManagerApplicationService
    {
        object GetNotUserBasedLockObjectForKey(string key);

        LockResponse GetLockObject(string key);
    }
    /// <summary>
    /// Represents the BroadcastLockingManagerApplicationService
    /// </summary>
    public class BroadcastLockingManagerApplicationService : BroadcastBaseClass, IBroadcastLockingManagerApplicationService
    {
        private readonly ISMSClient _SmsClient;
        private readonly IGeneralLockingApiClient _GeneralLockingApiClient;
        private readonly ConcurrentDictionary<string, object> _NotUserBasedLockObjects;
        public BroadcastLockingManagerApplicationService(ISMSClient smsClient, IGeneralLockingApiClient generalLockingApiClient
             , IConfigurationSettingsHelper configurationSettingsHelper,
            IFeatureToggleHelper featureToggleHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SmsClient = smsClient;
            _GeneralLockingApiClient = generalLockingApiClient;
            _NotUserBasedLockObjects = new ConcurrentDictionary<string, object>();
            System.Diagnostics.Debug.WriteLine("Initializing BroadcastLockingManagerApplicationService");
        }

        public LockResponse LockObject(string key)
        {
            LockResponse broadcastLockResponse = null;
            LockingApiRequest lockingRequest = _GeneralLockingApiClient.GetLockingRequest(key);
            var lockResponse = _GeneralLockingApiClient.LockObject(lockingRequest);
            if (lockResponse != null)
            {
                broadcastLockResponse = new LockResponse
                {
                    Error = lockResponse.Error,
                    Key = lockResponse.Key,
                    LockedUserId = lockResponse.LockedUserId,
                    LockedUserName = lockResponse.LockedUserName,
                    LockTimeoutInSeconds = lockResponse.LockTimeoutInSeconds,
                    Success = lockResponse.Success
                };
            }
            return broadcastLockResponse;
        }

        public ReleaseLockResponse ReleaseObject(string key)
        {
            return _SmsClient.ReleaseObject(key, GetUserSID());
        }

        public bool IsObjectLocked(string key)
        {
            string[] lockObject = key.Split(':');
            string objectType = lockObject[0].ToString();
            string objectId = lockObject[1].ToString();
            return _GeneralLockingApiClient.IsObjectLocked(objectType, objectId);

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

        public LockResponse GetLockObject(string key)
        {
            if (IsObjectLocked(key))
            {
                // if the key is locked you won`t be able to lock it but you can get the person`s name who has locked it
                return LockObject(key);
            }
            else
            {
                // this is a usual Success = true response
                return new LockResponse
                {
                    Key = key,
                    Success = true,
                    LockTimeoutInSeconds = 900
                };
            }
        }
    }
}
