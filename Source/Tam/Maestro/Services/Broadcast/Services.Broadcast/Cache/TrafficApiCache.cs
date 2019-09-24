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

        List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId);

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

        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _AgencyAdvertisersCache.GetOrCreate(agencyId.ToString(), () => _TrafficApiClient.GetAdvertisersByAgencyId(agencyId), policy);
        }

        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _AdvertisersCache.GetOrCreate(advertiserId.ToString(),() => _TrafficApiClient.GetAdvertiser(advertiserId), policy);
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _AdvertiserProductsCache.GetOrCreate(advertiserId.ToString(),() => _TrafficApiClient.GetProductsByAdvertiserId(advertiserId), policy);
        }

        public ProductDto GetProduct(int productId)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS) };
            return _ProductsCache.GetOrCreate(productId.ToString(), () => _TrafficApiClient.GetProduct(productId), policy);
        }

        private void BuildAgencyCache(CacheEntryRemovedArguments args)
        {
            var agencies = GetAgenciesFromClient();
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(CACHE_ITEM_TTL_SECONDS),
                RemovedCallback = BuildAgencyCache
            };

            _AgencyCache.Add(CACHE_KEY_AGENCIES, agencies, policy);
            _Agencies = agencies;
        }

        // the API is not case sensitive
        private static readonly string[] _AgencyAllowedChars = { "!", "\"", "#", "$", "%", "&amp;", "'", "(", ")", "*", "+", ",", "-", ".", "/", ":", ";", "&lt;", "=", "&gt;", "?", "@", "[", "\\", "]", "^", "_", "`", "{", "|", "}", "~", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        private List<AgencyDto> GetAgenciesFromClient()
        {
            const int callMaxReturn = 50;

            var agencies = new List<AgencyDto>();
            foreach (var filter in _AgencyAllowedChars)
            {
                List<AgencyDto> found = _TrafficApiClient.GetFilteredAgencies(filter);
                if (found.Count == callMaxReturn)
                {
                    // according to the data set this should not happen.
                    // at time of this writing (9/2019) :
                    //      Max Call Count = 50
                    //      Max Agencies per Letter = 31
                    // 
                    //  This solution is temporary until late PI-5
                    //  Between now and then no Agency list per first letter will go beyond 50... right?
                    throw new InvalidOperationException(
                        $"The return count for filter '{filter}' returned more than the max count of {callMaxReturn}.");
                }

                if (found.Count > 0)
                {
                    agencies.AddRange(found);
                }
            }

            return agencies;
        }
    }
}