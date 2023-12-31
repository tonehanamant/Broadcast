﻿using Services.Broadcast.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Helpers;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Unity;

namespace Common.Services
{
    public interface IDaypartCache
    {
        int GetIdByDaypart(DisplayDaypart displayDaypart);
        DisplayDaypart GetDisplayDaypart(int daypartId);
        Dictionary<int, DisplayDaypart> GetDisplayDayparts(IEnumerable<int> daypartIds);
        LookupDto GetLookupDto(int daypartId);
        void SyncDaypartsToIds(IEnumerable<DisplayDaypart> displayDayparts);
    }

    //this is a singleton implementation
    public class DaypartCache : BroadcastBaseClass, IDaypartCache
    {
        public static IDaypartCache DaypartCacheInstance;

        private static readonly Lazy<IDaypartCache> _Lazy = new Lazy<IDaypartCache>(
            () =>
            {
                if (DaypartCacheInstance == null)
                {
                    var daypartRepo = BroadcastDataDataRepositoryFactory.Instance.Resolve<IDisplayDaypartRepository>();
                    var featureToggleHelper = BroadcastApplicationServiceFactory.Instance.Resolve<IFeatureToggleHelper>();
                    var configHelper = BroadcastApplicationServiceFactory.Instance.Resolve<IConfigurationSettingsHelper>();

                    DaypartCacheInstance = new DaypartCache(daypartRepo, featureToggleHelper, configHelper);
                }
                return DaypartCacheInstance;
            });

        private MemoryCache _Cache = new MemoryCache("DaypartCache");
        private int _CacheTimeoutInSeconds;

        // Dictionary<>.Insert() will throw a NullReferenceException internally 
        // if the dictionary instance is modified from another thread during the insert operation
        // that`s why ConcurrentDictionary must be used here
        private static ConcurrentDictionary<DisplayDaypart, int> _CachedDisplayDayparts = new ConcurrentDictionary<DisplayDaypart, int>();

        // key locks dictionary is needed to prevent multiple calls _DisplayDaypartRepository.SaveDaypart(displayDaypart) in GetIdByDaypart
        private static ConcurrentDictionary<DisplayDaypart, object> _KeyLocks = new ConcurrentDictionary<DisplayDaypart, object>();

        private readonly IDisplayDaypartRepository _DisplayDaypartRepository;

        //This is only public so that the class can be tested.
        public DaypartCache(IDisplayDaypartRepository displayDaypartRepository, 
            IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(featureToggleHelper, configurationSettingsHelper)
        {
            _DisplayDaypartRepository = displayDaypartRepository;
            try
            {
                _CacheTimeoutInSeconds = _ConfigurationSettingsHelper.GetConfigValueWithDefault<int>(ConfigKeys.DaypartCacheSlidingExpirationSeconds, 300);
                if (_CacheTimeoutInSeconds <= 0)
                    throw new ApplicationException(string.Format("The value of the DaypartCacheSlidingExpirationSeconds is {0}.  It must be greater than zero.", _CacheTimeoutInSeconds));
            }
            catch
            {   //use a default
                _CacheTimeoutInSeconds = 300; //5 minutes;
            }
        }

        public void SetCacheTimeout(int timeoutSeconds)
        {
            _CacheTimeoutInSeconds = timeoutSeconds;
        }

        public static IDaypartCache Instance
        {
            get { return _Lazy.Value; }
        }

        public void SyncDaypartsToIds(IEnumerable<DisplayDaypart> displayDayparts)
        {
            foreach (var daypart in displayDayparts)
            {
                daypart.Id = GetIdByDaypart(daypart);
            }
        }

        public int GetIdByDaypart(DisplayDaypart displayDaypart)
        {
            if (_CachedDisplayDayparts.ContainsKey(displayDaypart))
            {
                return _CachedDisplayDayparts[displayDaypart];
            }
            else
            {
                var keyLock = _KeyLocks.GetOrAdd(displayDaypart, k => new object());

                // lock all the other requests with the same daypart
                // so that SaveDaypart is called only once for several parallel requests with the same daypart
                lock (keyLock)
                {
                    if (_CachedDisplayDayparts.ContainsKey(displayDaypart))
                    {
                        return _CachedDisplayDayparts[displayDaypart];
                    }
                    else
                    {
                        // if it is still not in the cache, let's get it
                        var id = _DisplayDaypartRepository.SaveDaypart(displayDaypart);
                        return _CachedDisplayDayparts.AddOrUpdate(displayDaypart, key => id, (key, oldValue) => id);
                    }
                }
            }
        }

        public LookupDto GetLookupDto(int daypartId)
        {
            var daypart = GetDisplayDaypart(daypartId);
            return new LookupDto(daypart.Id, daypart.ToString());
        }

        /// <summary>
        /// Attempts to find the daypart in the cache, and if not found obtains it from the database then adds it to the cache.  
        /// </summary>
        /// <param name="daypartId">The id of the daypart</param>
        /// <returns>A valid DisplayDaypart or null if the daypartId is 0</returns>
        public DisplayDaypart GetDisplayDaypart(int daypartId)
        {
            return (DisplayDaypart)Find(daypartId).Clone();
        }

        /// <summary>
        /// Attempts to find the daypart in the cache, and if not found obtains it from the database then adds it to the cache.  
        /// </summary>
        /// <param name="daypartIds">The id of the daypart</param>
        /// <returns>A valid DisplayDaypart or null if the daypartId is 0</returns>
        public Dictionary<int, DisplayDaypart> GetDisplayDayparts(IEnumerable<int> daypartIds)
        {
            var daypartDictionary = Find(daypartIds);
            var clonedDayparts = daypartDictionary.ToList().Select(x => (DisplayDaypart)x.Value.Clone()).ToList();
            var newDictionaryWithClones = clonedDayparts.ToDictionary(x => x.Id, x => x);
            return newDictionaryWithClones;
        }

        /// <summary>
        /// Attempts to find the daypart in the cache, and if not found obtains it from the database then adds it to the cache
        /// </summary>
        /// <param name="daypartId">The id of the daypart</param>
        /// <returns>A valid DisplayDaypart or null if the daypartId is 0</returns>
        private DisplayDaypart Find(int daypartId)
        {
            if (daypartId == 0)
            {
                throw new ApplicationException("Could not find a daypart with id 0");
            }

            if (daypartId < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format("daypartId must be greater than -1.  The daypartId holds a value of {0}", daypartId));
            }

            var lCacheKey = daypartId.ToString();
            if (_Cache.Contains(lCacheKey))
            {
                return (DisplayDaypart)_Cache.Get(lCacheKey);
            }

            //if not in _cache, get from database
            DisplayDaypart daypart = _DisplayDaypartRepository.GetDisplayDaypart(daypartId);
            if (daypart == null)
            {
                throw new ApplicationException("Could not find a daypart with id " + daypartId); //call ToString() to avoid boxing
            }

            _Cache.Add(
                lCacheKey,
                daypart,
                new CacheItemPolicy
                {
                    SlidingExpiration = new TimeSpan(0, 0, _CacheTimeoutInSeconds),
                    RemovedCallback = (arg) =>
                    {
                        //_Log.Info(string.Format("The DaypartCache sliding expiration limit of {0} seconds has been reached. The daypart of id {1} has been removed.",
                        //    _CacheTimeoutInSeconds, arg.CacheItem.Key));
                    }
                }
            );

            return daypart;
        }

        /// <summary>
        /// Attempts to find the daypart in the cache, and if not found obtains it from the database then adds it to the cache
        /// </summary>
        /// <param name="daypartIds">The id of the daypart</param>
        /// <returns>A valid DisplayDaypart or null if the daypartId is 0</returns>
        private Dictionary<int, DisplayDaypart> Find(IEnumerable<int> daypartIds)
        {
            daypartIds = daypartIds.Distinct();

            var dict = new Dictionary<int, DisplayDaypart>();

            var idsToGet = new List<int>();
            foreach (var daypartId in daypartIds)
            {
                if (daypartId == 0)
                {
                    throw new ApplicationException("Could not find a daypart with id 0");
                }

                if (daypartId < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format("daypartId must be greater than -1.  The daypartId holds a value of {0}",
                            daypartId));
                }

                var lCacheKey = daypartId.ToString();
                if (_Cache.Contains(lCacheKey))
                {
                    dict.Add(daypartId, (DisplayDaypart)_Cache.Get(lCacheKey));
                }
                else
                {
                    idsToGet.Add(daypartId);
                }
            }

            // If all ids are in cache then return the dictionary; no need to talk to db
            if (idsToGet.Count == 0) return dict;

            //if not in _cache, get from database
            var dayparts = _DisplayDaypartRepository.GetDisplayDayparts(idsToGet);
            foreach (var displayDaypart in dayparts)
            {
                _Cache.Add(
                    displayDaypart.Key.ToString(),
                    displayDaypart.Value,
                    new CacheItemPolicy
                    {
                        SlidingExpiration = new TimeSpan(0, 0, _CacheTimeoutInSeconds),
                        RemovedCallback = (arg) =>
                        {
                            //_Log.Info(string.Format("The DaypartCache sliding expiration limit of {0} seconds has been reached. The daypart of id {1} has been removed.",
                            //    _CacheTimeoutInSeconds, arg.CacheItem.Key));
                        }
                    }
                );
                dict.Add(displayDaypart.Key, displayDaypart.Value);
            }
            return dict;
        }
    }
}
