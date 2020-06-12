using Common.Services.Extensions;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Services.Broadcast.Entities.DTO;

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

    public class TrafficApiCache : ITrafficApiCache
    {
        private const int CACHE_ITEM_TTL_SECONDS = 3600;
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
            _TrafficApiClient = trafficApiClient;
            BuildAgencyCache(null);
        }

        public List<AgencyDto> GetAgencies()
        {
            return _Agencies;
        }

        public AgencyDto GetAgency(int agencyId)
        {
            return _Agencies.Single(a => a.Id == agencyId, $"Agency with id '{agencyId}' not found.");
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
            var agencies = _TrafficApiClient.GetAgencies();
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS),
                RemovedCallback = BuildAgencyCache
            };

            _AgencyCache.Add(CACHE_KEY_AGENCIES, agencies, policy);
            _Agencies = agencies;
        }
    }
}