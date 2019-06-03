using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces.Common;

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
    public class DaypartCache : IDaypartCache
    {
        public static IDaypartCache DaypartCacheInstance;

        private static readonly Lazy<IDaypartCache> _Lazy = new Lazy<IDaypartCache>(
            () =>
            {
                if (DaypartCacheInstance == null)
                {
                    DaypartCacheInstance = new DaypartCache(new BroadcastDataDataRepositoryFactory().GetDataRepository<IDisplayDaypartRepository>());
                }
                return DaypartCacheInstance;
            });

        private MemoryCache _Cache = new MemoryCache("DaypartCache");
        private int _CacheTimeoutInSeconds;

        [ThreadStatic]
        private static Dictionary<DisplayDaypart, int> _CachedDisplayDayparts;

        private readonly IDisplayDaypartRepository _DisplayDaypartRepository;

        //This is only public so that the class can be tested.
        public DaypartCache(IDisplayDaypartRepository displayDaypartRepository)
        {
            _DisplayDaypartRepository = displayDaypartRepository;

            try
            {
                _CacheTimeoutInSeconds = Convert.ToInt32(Releases2ServiceSystemParameter.DaypartCacheSlidingExpirationSeconds);
                if (_CacheTimeoutInSeconds <= 0)
                    throw new ApplicationException(string.Format("The value of the DaypartCacheSlidingExpirationSeconds is {0}.  It must be greater than zero.", _CacheTimeoutInSeconds));
            }
            catch (Exception ex)
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
            if (_CachedDisplayDayparts == null)
            {
                _CachedDisplayDayparts = new Dictionary<DisplayDaypart, int>();
            }

            if (!_CachedDisplayDayparts.ContainsKey(displayDaypart))
            {
                var id = _DisplayDaypartRepository.SaveDaypart(displayDaypart);
                _CachedDisplayDayparts[displayDaypart] = id;
                return id;
            }
            else
            {
                return _CachedDisplayDayparts[displayDaypart];
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
