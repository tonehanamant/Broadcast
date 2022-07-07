using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Locking;
using Services.Broadcast.Helpers;
using System;
using System.Runtime.Caching;

namespace Services.Broadcast.Cache
{
    /// <summary>
    /// This interface contains the methods which supports the unit test cases for General locking microservice
    /// </summary>
    public interface ILockingCache
    {
        /// <summary>
        /// Supports the unit test case for the locking an object
        /// </summary>
        /// <param name="lockingRequest">lockingRequest passes as argument</param>
        /// <returns>Response with lock result</returns>
        LockingResultResponse LockObject(LockingApiRequest lockingRequest);
        /// <summary>
        /// Suports the unit test case for release an locked object
        /// </summary>
        /// <param name="objectType">objectType is passes as parameter</param>
        /// <param name="objectId">objectId is passes as argument</param>
        /// <returns>Returns the release response</returns>
        ReleaseLockResponse ReleaseObject(string objectType, string objectId);
        /// <summary>
        /// Supports the unit test cases for the checking the locking status of an object
        /// </summary>
        /// <param name="objectType">objectType is passes as parameter</param>
        /// <param name="objectId">objectId is passes as argument</param>
        /// <returns>Returns True if object is locked otherwise false</returns>
        bool IsObjectLocked(string objectType, string objectId);
    }
    /// <summary>
    /// This class contains the methods which supports the unit test cases for General locking microservice
    /// </summary>
    public class LockingCache : BroadcastBaseClass, ILockingCache
    {
        private const string CACHE_NAME_Lock = "Lock";
        private const string CACHE_NAME_Release = "Release";
        private const string CACHE_NAME_ObjectLocked = "ViewLock";
        private readonly BaseMemoryCache<LockingResultResponse> _LockCache = new BaseMemoryCache<LockingResultResponse>(CACHE_NAME_Lock);
        private readonly BaseMemoryCache<ReleaseLockResponse> _ReleaseCache = new BaseMemoryCache<ReleaseLockResponse>(CACHE_NAME_Release);
        private readonly BaseMemoryCache<bool> _ViewLockedCache = new BaseMemoryCache<bool>(CACHE_NAME_ObjectLocked);
        private readonly IGeneralLockingApiClient _GeneralLockingApiClient;
        private readonly Lazy<int> _CacheItemTtlSeconds;
        public LockingCache(IGeneralLockingApiClient generalLockingApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
           : base(featureToggleHelper, configurationSettingsHelper)
        {
            _GeneralLockingApiClient = generalLockingApiClient;
            _CacheItemTtlSeconds = new Lazy<int>(_GetCacheItemTtlSeconds);
        }

        /// <summary>
        /// Supports the unit test case for the locking an object
        /// </summary>
        /// <param name="lockingRequest">lockingRequest passes as argument</param>
        /// <returns>Response with lock result</returns>
        public LockingResultResponse LockObject(LockingApiRequest lockingRequest)
        {
            var policy = _GetCacheItemPolicy();
            var result = _LockCache.GetOrCreate(CACHE_NAME_Lock, () =>
                _GeneralLockingApiClient.LockObject(lockingRequest), policy);
            return result;
        }
        /// <summary>
        /// Suports the unit test case for release an locked object
        /// </summary>
        /// <param name="objectType">objectType is passes as parameter</param>
        /// <param name="objectId">objectId is passes as argument</param>
        /// <returns>Returns the release response</returns>
        public ReleaseLockResponse ReleaseObject(string objectType, string objectId)
        {
            var policy = _GetCacheItemPolicy();
            var result = _ReleaseCache.GetOrCreate(CACHE_NAME_Release, () =>
            _GeneralLockingApiClient.ReleaseObject(objectType, objectId), policy);
            return result;
        }
        /// <summary>
        /// Supports the unit test cases for the checking the locking status of an object
        /// </summary>
        /// <param name="objectType">objectType is passes as parameter</param>
        /// <param name="objectId">objectId is passes as argument</param>
        /// <returns>Returns True if object is locked otherwise false</returns>
        public bool IsObjectLocked(string objectType, string objectId)
        {
            var policy = _GetCacheItemPolicy();
            var result = _ViewLockedCache.GetOrCreate(CACHE_NAME_ObjectLocked, () =>
            _GeneralLockingApiClient.IsObjectLocked(objectType, objectId), policy);            
            return result;
        }
        private int _GetCacheItemTtlSeconds()
        {
            var AABCacheExpirationSeconds = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.AABCacheExpirationSeconds, 300);
            var result = AABCacheExpirationSeconds < 0
                ? 300 //default to 5 minutes
                : AABCacheExpirationSeconds;
            return result;
        }
        private CacheItemPolicy _GetCacheItemPolicy()
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(_CacheItemTtlSeconds.Value) };
            return policy;
        }
    }
}
