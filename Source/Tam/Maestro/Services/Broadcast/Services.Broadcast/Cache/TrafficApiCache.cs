using Common.Services.Extensions;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using log4net;
using BroadcastLogging;
using Microsoft.EntityFrameworkCore.Internal;

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
    }

    public class TrafficApiCache : BroadcastBaseClass, ITrafficApiCache
    {
        private readonly int CACHE_ITEM_TTL_SECONDS;
        private readonly ITrafficApiClient _TrafficApiClient;

        private const string CACHE_NAME_AGENCIES = "Agencies";
        private const string CACHE_KEY_AGENCIES = "Agencies";
        private readonly MemoryCache _AgencyCache = new MemoryCache(CACHE_NAME_AGENCIES);
        private List<AgencyDto> _Agencies { get; set; } = new List<AgencyDto>();

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
            if (_Agencies?.Any() != true)
            {
                BuildAgencyCache(null);
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
            return _AgencyAdvertisersCache.GetOrCreate("advertisers", () => _TrafficApiClient.GetAdvertisers(), policy);
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

        private void BuildAgencyCache(CacheEntryRemovedArguments args)
        {
            try
            {
                var agencies = _TrafficApiClient.GetAgencies();
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS),
                    RemovedCallback = BuildAgencyCache
                };

                _AgencyCache.Add(CACHE_KEY_AGENCIES, agencies, policy);
                _Agencies = agencies;
            }
            catch (Exception e)
            {
                _LogError("Error attempting to refresh the Agency cache from the AAB Traffic Api.", e);
            }
        }
    }
}