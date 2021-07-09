using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Cache
{
    /// <summary>
    /// Caches Aab data
    /// </summary>
    public interface IAabCache
    {
        /// <summary>
        /// Gets the agencies.
        /// </summary>
        List<AgencyDto> GetAgencies();

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        List<AdvertiserDto> GetAdvertisers();

        /// <summary>
        /// Gets the advertiser products.
        /// </summary>
        /// <param name="advertiserMasterId">The advertiser master identifier.</param>
        List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId);

        /// <summary>
        /// Clears the agencies cache.
        /// </summary>
        void ClearAgenciesCache();

        /// <summary>
        /// Clears the advertisers cache.
        /// </summary>
        void ClearAdvertisersCache();
    }

    /// <summary>
    /// Caches Aab data
    /// </summary>
    /// <seealso cref="Services.Broadcast.Cache.IAabCache" />
    public class AabCache : IAabCache
    {
        private const string CACHE_NAME_AGENCIES = "Agencies";
        private readonly BaseMemoryCache<List<AgencyDto>> _AgenciesCache = new BaseMemoryCache<List<AgencyDto>>(CACHE_NAME_AGENCIES);

        private const string CACHE_NAME_ADVERTISERS = "Advertisers";
        private readonly BaseMemoryCache<List<AdvertiserDto>> _AdvertisersCache = new BaseMemoryCache<List<AdvertiserDto>>(CACHE_NAME_ADVERTISERS);
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        internal static Lazy<bool> _IsPipelineVariablesEnabled;
        private IAgencyAdvertiserBrandApiClient _AabApiClient;
        private Lazy<int> _CacheItemTtlSeconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="AabCache"/> class.
        /// </summary>
        /// <param name="aabApiClient">The aab API client.</param>
        /// <param name="featureToggleHelper"> Feature Toggle Helper.</param>
        /// <param name="configurationSettingsHelper">Configuration Settings Helper.</param>
        public AabCache(IAgencyAdvertiserBrandApiClient aabApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _AabApiClient = aabApiClient;
            _CacheItemTtlSeconds = new Lazy<int>(_GetCacheItemTtlSeconds);
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
        }      

        /// <inheritdoc />
        public List<AgencyDto> GetAgencies()
        {
            var policy = _GetCacheItemPolicy();
            var result = _AgenciesCache.GetOrCreate(CACHE_NAME_AGENCIES, () =>
                _AabApiClient.GetAgencies().OrderBy(a => a.Name).ToList(), policy);
            return result;
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisers()
        {
            var policy = _GetCacheItemPolicy();
            var result = _AdvertisersCache.GetOrCreate(CACHE_NAME_ADVERTISERS, () => 
                _AabApiClient.GetAdvertisers().OrderBy(a => a.Name).ToList(), policy);
            return result;
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId)
        {
            var result = _AabApiClient.GetAdvertiserProducts(advertiserMasterId).OrderBy(a => a.Name).ToList();
            return result;
        }

        /// <inheritdoc />
        public void ClearAgenciesCache()
        {
            _AgenciesCache.Remove(CACHE_NAME_AGENCIES);
        }

        /// <inheritdoc />
        public void ClearAdvertisersCache()
        {
            _AdvertisersCache.Remove(CACHE_NAME_ADVERTISERS);
        }

        private int _GetCacheItemTtlSeconds()
        {        
           if (_IsPipelineVariablesEnabled.Value)
            {
                var AABCacheExpirationSeconds = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.AABCACHEEXPIRATIONSECONDS_KEY,300);
                var result =AABCacheExpirationSeconds < 0
               ? 300 //default to 5 minutes
               : AABCacheExpirationSeconds;
                return result;
            }
            else
            {
                var result = BroadcastServiceSystemParameter.AABCacheExpirationSeconds < 0
                ? 300 //default to 5 minutes
                : BroadcastServiceSystemParameter.AABCacheExpirationSeconds;
                return result;
            }
           
            
        }

        private CacheItemPolicy _GetCacheItemPolicy()
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(_CacheItemTtlSeconds.Value) };
            return policy;
        }
    }
}