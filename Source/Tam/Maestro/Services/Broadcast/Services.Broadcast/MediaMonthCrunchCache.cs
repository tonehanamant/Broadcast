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
        private const string MediaMonthCacheInstanceName = "MediaMonthCacheKey";

        public static IMediaMonthCrunchCache MediaMonthCrunchCacheInstance;

        private MemoryCache _Cache = new MemoryCache(MediaMonthCacheInstanceName);
        private int _CacheTimeoutInSeconds;

        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregate;



        private static readonly Lazy<IMediaMonthCrunchCache> _Lazy = new Lazy<IMediaMonthCrunchCache>(
            () =>
            {
                if (MediaMonthCrunchCacheInstance == null)
                {
                    MediaMonthCrunchCacheInstance = new MediaMonthCrunchCache(new BroadcastDataDataRepositoryFactory(), BroadcastApplicationServiceFactory.Instance.Resolve<IMediaMonthAndWeekAggregateCache>());
                }
                return MediaMonthCrunchCacheInstance;
            });

        //This is only public so that the class can be tested.
        public MediaMonthCrunchCache(IDataRepositoryFactory dataRepositoryFactory, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _MediaMonthAndWeekAggregate = mediaMonthAndWeekAggregateCache;

            try
            {
                _CacheTimeoutInSeconds = Convert.ToInt32(BroadcastServiceSystemParameter.MediaMonthCruchCacheSlidingExpirationSeconds);
                if (_CacheTimeoutInSeconds <= 0)
                    throw new ApplicationException(string.Format("The value of the DaypartCacheSlidingExpirationSeconds is {0}.  It must be greater than zero.", _CacheTimeoutInSeconds));
            }
            catch (Exception ex)
            {   //use a default
                _CacheTimeoutInSeconds = 24*60*60; //24 hours;
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

            var sweepsMonths = _MediaMonthAndWeekAggregate.GetAllSweepsMonthsBeforeCurrentMonth();

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var nielsonMarkets = externalRatingRepository.GetNielsonMarkets(sweepsMonths);
                var forecastDetails = ratingForecastRepository.GetForecastDetails(sweepsMonths);
                _Cache[MediaMonthCacheKey] = forecastDetails.Select(s =>
                        new MediaMonthCrunchStatus(s, nielsonMarkets.First(m => m.Item1 == s.MediaMonth).Item2))
                    .OrderByDescending(d => d.MediaMonth.Id)
                    .ToList();
                return (List<MediaMonthCrunchStatus>)_Cache.Get(MediaMonthCacheKey);
            }
        }

        public void ClearMediaMonthCache()
        {
            _Cache.Remove(MediaMonthCacheKey);
        }
    }
}
