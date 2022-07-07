﻿using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
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
    /// <summary>
    /// Represents the broadcast locking service with smsclient and general locking microserivce managed by feature toggle
    /// </summary>
    public class BroadcastLockingService : BroadcastBaseClass, IBroadcastLockingService
    {
        private readonly ISMSClient _SmsClient;
        private readonly IGeneralLockingApiClient _GeneralLockingApiClient;
        private readonly ConcurrentDictionary<string, object> _NotUserBasedLockObjects;
        private readonly Lazy<bool> _IsLockingMigrationEnabled;        
        public BroadcastLockingService(ISMSClient smsClient, IGeneralLockingApiClient generalLockingApiClient
            , IConfigurationSettingsHelper configurationSettingsHelper,
            IFeatureToggleHelper featureToggleHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SmsClient = smsClient;
            _GeneralLockingApiClient = generalLockingApiClient;
            _NotUserBasedLockObjects = new ConcurrentDictionary<string, object>();
            _IsLockingMigrationEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_LOCKING_MIGRATION));
            System.Diagnostics.Debug.WriteLine("Initializing BroadcastLockingManagerApplicationService");
        }

        public BroadcastLockResponse LockObject(string key)
        {
            BroadcastLockResponse broadcastLockResponse = null;
            if(_IsLockingMigrationEnabled.Value)
            {
                LockingApiRequest lockingRequest = KeyHelper.GetLokcingRequest(key);
                var lockResponse = _GeneralLockingApiClient.LockObject(lockingRequest);
                if (lockResponse != null)
                {
                    broadcastLockResponse = new BroadcastLockResponse
                    {
                        Error = lockResponse.Error,
                        Key = lockResponse.Key,
                        LockedUserId = lockResponse.LockedUserId,
                        LockedUserName = lockResponse.LockedUserName,
                        LockTimeoutInSeconds = lockResponse.LockTimeoutInSeconds,
                        Success = lockResponse.Success
                    };
                }
            }
            else
            {
                var lockResponse = _SmsClient.LockObject(key, GetUserSID());
                if (lockResponse != null)
                {
                    broadcastLockResponse = new BroadcastLockResponse
                    {
                        Error = lockResponse.Error,
                        Key = lockResponse.Key,
                        LockedUserId = lockResponse.LockedUserId,
                        LockedUserName = lockResponse.LockedUserName,
                        LockTimeoutInSeconds = lockResponse.LockTimeoutInSeconds,
                        Success = lockResponse.Success
                    };
                }
            }
            
            return broadcastLockResponse;
        }

        public BroadcastReleaseLockResponse ReleaseObject(string key)
        {
            BroadcastReleaseLockResponse broadcastReleaseLockResponse = null;
            if (_IsLockingMigrationEnabled.Value)
            {
                string[] lockObject = key.Split(':');
                if(lockObject.Length > 0)
                {
                    var releaseLockResponse = _GeneralLockingApiClient.ReleaseObject(lockObject[0].ToString(), lockObject[1].ToString());
                    if(releaseLockResponse != null)
                    {
                        broadcastReleaseLockResponse = new BroadcastReleaseLockResponse
                        {
                            Error = releaseLockResponse.Error,
                            Key = releaseLockResponse.Key,
                            Success = releaseLockResponse.Success
                        };
                    }
                }
                return broadcastReleaseLockResponse;
            }
            else
            {
                var releaseLockResponse = _SmsClient.ReleaseObject(key, GetUserSID());
                if (releaseLockResponse != null)
                {
                    broadcastReleaseLockResponse = new BroadcastReleaseLockResponse
                    {
                        Error = releaseLockResponse.Error,
                        Key = releaseLockResponse.Key,
                        Success = releaseLockResponse.Success
                    };
                }
                return broadcastReleaseLockResponse;
            }
        }

        public bool IsObjectLocked(string key)
        {
            if (_IsLockingMigrationEnabled.Value)
            {
                string[] lockObject = key.Split(':');
                string objectType = lockObject[0].ToString();
                string objectId = lockObject[1].ToString();
                return _GeneralLockingApiClient.IsObjectLocked(objectType, objectId);
            }
            else
            {
                return _SmsClient.IsObjectLocked(key, GetUserSID());
            }
                
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

