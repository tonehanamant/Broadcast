using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Transactions;
using Common.Services.Repositories;
using Microsoft.Practices.Unity;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Common.Services
{
    public interface IMediaMonthCrunchCache
    {
        List<MediaMonthCrunchStatus> GetMediaMonthCrunchStatuses();
        void ClearMediaMonthCache();
    }

    //this is a singleton implementation
    public class MediaMonthCrunchCache : IMediaMonthCrunchCache
    {
        private const string MediaMonthCacheKey = "MediaMonthCacheKey";
        private const string MediaMonthCacheInstanceName = "MediaMonthCacheInstance";

        public static IMediaMonthCrunchCache MediaMonthCrunchCacheInstance;

        private MemoryCache _Cache = new MemoryCache(MediaMonthCacheInstanceName);
        private int _CacheTimeoutInSeconds;

        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;


        private static readonly Lazy<IMediaMonthCrunchCache> _Lazy = new Lazy<IMediaMonthCrunchCache>(
            () =>
            {
                if (MediaMonthCrunchCacheInstance == null)
                {
                    throw new Exception("Do not access instance before accessing BroadcastApplicationServiceFactory");
                }
                return MediaMonthCrunchCacheInstance;
            });

        //This is only public so that the class can be tested.
        public MediaMonthCrunchCache(IDataRepositoryFactory dataRepositoryFactory,IMediaMonthAndWeekAggregateCache cache)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _MediaMonthAndWeekAggregateCache = cache;

            try
            {
                _CacheTimeoutInSeconds =
                    Convert.ToInt32(BroadcastServiceSystemParameter.MediaMonthCruchCacheSlidingExpirationSeconds);
                if (_CacheTimeoutInSeconds <= 0)
                    _CacheTimeoutInSeconds = 24 * 60 * 60; //24 hours;
            }
            catch (Exception ex)
            {
                //use a default
                _CacheTimeoutInSeconds = 24 * 60 * 60; //24 hours;
            }
        }

        public void SetCacheTimeout(int timeoutSeconds)
        {
            _CacheTimeoutInSeconds = timeoutSeconds;
        }

        public static IMediaMonthCrunchCache Instance
        {
            get { return _Lazy.Value; }
        }

        public List<MediaMonthCrunchStatus> GetMediaMonthCrunchStatuses()
        {
            if (_Cache.Contains(MediaMonthCacheKey))
            {
                return (List<MediaMonthCrunchStatus>)_Cache.Get(MediaMonthCacheKey);
            }
            var ratingForecastRepository = _DataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var externalRatingRepository = _DataRepositoryFactory.GetDataRepository<IRatingsRepository>();

            var sweepsMonths = _MediaMonthAndWeekAggregateCache.GetAllSweepsMonthsBeforeCurrentMonth();

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var nielsonMarkets = externalRatingRepository.GetNielsonMarkets(sweepsMonths);
                var forecastDetails = ratingForecastRepository.GetForecastDetails(sweepsMonths);
                var results = forecastDetails.Select(s =>
                        new MediaMonthCrunchStatus(s, nielsonMarkets.First(m => m.Item1 == s.MediaMonth).Item2))
                    .OrderByDescending(d => d.MediaMonth.Id)
                    .ToList();

                _Cache.Add(
                    MediaMonthCacheKey,
                    results,
                    new CacheItemPolicy
                    {
                        SlidingExpiration = new TimeSpan(0, 0, _CacheTimeoutInSeconds)
                    });

                return results;
            }
        }

        public void ClearMediaMonthCache()
        {
            _Cache.Remove(MediaMonthCacheKey);
        }
    }
}
