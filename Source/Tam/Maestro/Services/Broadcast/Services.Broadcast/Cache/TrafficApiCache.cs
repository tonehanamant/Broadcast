using BroadcastLogging;
using Common.Services.Extensions;
using log4net;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Cache
{
    public interface ITrafficApiCache
    {
        List<AgencyDto> GetAgencies();

        AgencyDto GetAgency(int agencyId);

        List<AdvertiserDto> GetAdvertisers();

        AdvertiserDto GetAdvertiser(int advertiserId);

        List<ProductDto> GetProductsByAdvertiserId(int advertiserId);

        ProductDto GetProduct(int productId);

        /// <summary>
        /// Clears the agencies cache.
        /// </summary>
        void ClearAgenciesCache();

        /// <summary>
        /// Clears the advertisers cache.
        /// </summary>
        void ClearAdvertisersCache();
    }

    public class TrafficApiCache : BroadcastBaseClass, ITrafficApiCache
    {
        private readonly int CACHE_ITEM_TTL_SECONDS;
        private readonly ITrafficApiClient _TrafficApiClient;

        private const string CACHE_NAME_AGENCIES = "Agencies";
        private const string CACHE_KEY_AGENCIES = "Agencies";
        private readonly MemoryCache _AgencyCache = new MemoryCache(CACHE_NAME_AGENCIES);
        private List<AgencyDto> _Agencies { get; set; }

        private const string CACHE_NAME_AGENCY_ADVERTISERS = "AgencyAdvertiserCache";
        private readonly BaseMemoryCache<List<AdvertiserDto>> _AgencyAdvertisersCache = new BaseMemoryCache<List<AdvertiserDto>>(CACHE_NAME_AGENCY_ADVERTISERS);

        private const string CACHE_NAME_ADVERTISERS = "AdvertiserCache";
        private readonly BaseMemoryCache<AdvertiserDto> _AdvertisersCache = new BaseMemoryCache<AdvertiserDto>(CACHE_NAME_ADVERTISERS);

        private const string CACHE_NAME_ADVERTISER_PRODUCTS = "AdvertiserProductsCache";
        private readonly BaseMemoryCache<List<ProductDto>> _AdvertiserProductsCache = new BaseMemoryCache<List<ProductDto>>(CACHE_NAME_ADVERTISER_PRODUCTS);

        private const string CACHE_NAME_PRODUCTS = "Products";
        private readonly BaseMemoryCache<ProductDto> _ProductsCache = new BaseMemoryCache<ProductDto>(CACHE_NAME_PRODUCTS);

        public TrafficApiCache(ITrafficApiClient trafficApiClient)
        {
            var log = LogManager.GetLogger(GetType());
            if (BroadcastServiceSystemParameter.AABCacheExpirationSeconds < 0)
            {
                var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage("Parameter AABCacheExpirationSeconds does not have a value"
                    , GetType(), string.Empty);
                log.Warn(logMessage.ToJson());
                CACHE_ITEM_TTL_SECONDS = 300; //default to 5 minutes
            }
            else
            {
                CACHE_ITEM_TTL_SECONDS = BroadcastServiceSystemParameter.AABCacheExpirationSeconds;
            }

            _TrafficApiClient = trafficApiClient;
        }

        public List<AgencyDto> GetAgencies()
        {
            if (_Agencies == null)
            {
                BuildAgencyCache(null);
            }

            if (_Agencies == null)
            {
                throw new InvalidOperationException("Failed to get Agencies from the source.");
            }
            return _Agencies;
        }

        public AgencyDto GetAgency(int agencyId)
        {
            return GetAgencies().Single(a => a.Id == agencyId, $"Agency with id '{agencyId}' not found.");
        }

        public List<AdvertiserDto> GetAdvertisers()
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _AgencyAdvertisersCache.GetOrCreate(CACHE_NAME_ADVERTISERS, () => _TrafficApiClient.GetAdvertisers(), policy);
        }

        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _AdvertisersCache.GetOrCreate(advertiserId.ToString(), () => _TrafficApiClient.GetAdvertiser(advertiserId), policy);
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _AdvertiserProductsCache.GetOrCreate(advertiserId.ToString(), () => _TrafficApiClient.GetProductsByAdvertiserId(advertiserId), policy);
        }

        public ProductDto GetProduct(int productId)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _ProductsCache.GetOrCreate(productId.ToString(), () => _TrafficApiClient.GetProduct(productId), policy);
        }

        /// <inheritdoc />
        public void ClearAgenciesCache()
        {
            _Agencies = null;
            _AgencyCache.Remove(CACHE_NAME_AGENCIES);
        }

        /// <inheritdoc />
        public void ClearAdvertisersCache()
        {
            _AgencyAdvertisersCache.GetItemCount(true);
            _AdvertisersCache.GetItemCount(true);
            _AdvertiserProductsCache.GetItemCount(true);
            _ProductsCache.GetItemCount(true);
        }

        private void BuildAgencyCache(CacheEntryRemovedArguments args)
        {
            List<AgencyDto> agencies = null;
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS),
                RemovedCallback = BuildAgencyCache
            };

            try
            {
                agencies = _TrafficApiClient.GetAgencies();
                _Agencies = agencies;
                
            }
            catch (Exception e)
            {
                _LogError("Error attempting to refresh the Agency cache from the AAB Traffic Api.", e);
                agencies = new List<AgencyDto>();
                // set this to null so we pick up the error downstream
                _Agencies = null;
            }
            _AgencyCache.Add(CACHE_KEY_AGENCIES, agencies, policy);
        }
    }
}