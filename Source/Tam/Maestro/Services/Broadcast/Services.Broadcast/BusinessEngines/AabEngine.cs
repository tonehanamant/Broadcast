using Common.Services.Extensions;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.BusinessEngines
{
    /// <summary>
    /// Gets Aab data from the source.
    /// </summary>
    public interface IAabEngine
    {
        /// <summary>
        /// Gets the agencies.
        /// </summary>
        List<AgencyDto> GetAgencies();

        /// <summary>
        /// Gets the agency.
        /// </summary>
        /// <param name="agencyId">The identifier used by the Aab Traffic Api.</param>
        AgencyDto GetAgency(int agencyId);

        /// <summary>
        /// Gets the agency.
        /// </summary>
        /// <param name="agencyMasterId">The identifier used by the Aab Api.</param>
        AgencyDto GetAgency(Guid agencyMasterId);

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        List<AdvertiserDto> GetAdvertisers();

        /// <summary>
        /// Gets the advertiser.
        /// </summary>
        /// <param name="advertiserId">The advertiser Id.</param>
        AdvertiserDto GetAdvertiser(int advertiserId);

        /// <summary>
        /// Gets the advertiser.
        /// </summary>
        /// <param name="advertiserMasterId">The identifier used by the Aab Api.</param>
        AdvertiserDto GetAdvertiser(Guid advertiserMasterId);

        /// <summary>
        /// Gets the advertiser products.
        /// </summary>
        /// <param name="advertiserId">The advertiser id.</param>
        List<ProductDto> GetAdvertiserProducts(int advertiserId);

        /// <summary>
        /// Gets the advertiser products.
        /// </summary>
        /// <param name="advertiserMasterId">The identifier used by the Aab Api.</param>
        List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId);

        /// <summary>
        /// Gets the advertiser product.
        /// </summary>
        /// <param name="advertiserMasterId">The advertiser master identifier used by the Aab Api.</param>
        /// <param name="productMasterId">The product master identifier used by the Aab Api.</param>
        ProductDto GetAdvertiserProduct(Guid advertiserMasterId, Guid productMasterId);

        /// <summary>
        /// Gets the product.
        /// </summary>
        /// <param name="productId">The identifier used by the Aab Traffic Api.</param>
        ProductDto GetProduct(int productId);

        /// <summary>
        /// Clears the agencies cache.
        /// </summary>
        void ClearAgenciesCache();

        /// <summary>
        /// Clears the advertisers cache.
        /// </summary>
        /// <returns></returns>
        void ClearAdvertisersCache();
    }

    /// <summary>
    /// Gets Aab data from the correct source based on the system configuration.
    /// </summary>
    /// <seealso cref="Services.Broadcast.BusinessEngines.IAabEngine" />
    public class AabEngine : IAabEngine
    {
        private IAabCache _AabApiCache;
        private ITrafficApiCache _TrafficApiCache;
        private IFeatureToggleHelper _FeatureToggleHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AabEngine"/> class.
        /// </summary>
        /// <param name="aabApiCache">The aab API cache.</param>
        /// <param name="trafficApiCache">The traffic API cache.</param>
        /// <param name="featureToggleHelper">The feature toggle helper.</param>
        public AabEngine(IAabCache aabApiCache, 
            ITrafficApiCache trafficApiCache,
            IFeatureToggleHelper featureToggleHelper)
        {
            _AabApiCache = aabApiCache;
            _TrafficApiCache = trafficApiCache;
            _FeatureToggleHelper = featureToggleHelper;
        }

        /// <inheritdoc />
        public List<AgencyDto> GetAgencies()
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            var result = isAabEnabled 
                ? _AabApiCache.GetAgencies() 
                : _TrafficApiCache.GetAgencies();
            return result;
        }

        /// <inheritdoc />
        public AgencyDto GetAgency(int agencyId)
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            AgencyDto result;
            if (isAabEnabled)
            {
                var items = _AabApiCache.GetAgencies();
                result = items.Single(i => i.Id == agencyId, $"Agency with id {agencyId} not found.");
            }
            else
            {
                result = _TrafficApiCache.GetAgency(agencyId);
            }
            return result;
        }

        /// <inheritdoc />
        public AgencyDto GetAgency(Guid agencyMasterId)
        {
            // No need to check the toggle as when disabled you won't have a Guid master id.
            var items = _AabApiCache.GetAgencies();
            var item = items.Single(i => i.MasterId == agencyMasterId, $"Agency with master id {agencyMasterId} not found.");
            return item;
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisers()
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            var result = isAabEnabled
                ? _AabApiCache.GetAdvertisers()
                : _TrafficApiCache.GetAdvertisers();
            return result;
        }

        /// <inheritdoc />
        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            AdvertiserDto result;

            if (isAabEnabled)
            {
                var items = _AabApiCache.GetAdvertisers();
                result = items.Single(i => i.Id == advertiserId, $"Advertiser with id {advertiserId} not found.");
            }
            else
            {
                result = _TrafficApiCache.GetAdvertiser(advertiserId);
            }
            
            return result;
        }

        /// <inheritdoc />
        public AdvertiserDto GetAdvertiser(Guid advertiserMasterId)
        {
            // No need to check the toggle as when disabled you won't have a Guid master id.
            var items = _AabApiCache.GetAdvertisers();
            var item = items.Single(i => i.MasterId == advertiserMasterId, $"Advertiser with master id {advertiserMasterId} not found.");
            return item;
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(int advertiserId)
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            List<ProductDto> results;

            if (isAabEnabled)
            {
                var advertiser = GetAdvertiser(advertiserId);
                results = _AabApiCache.GetAdvertiserProducts(advertiser.MasterId.Value);
            }
            else
            {
                results = _TrafficApiCache.GetProductsByAdvertiserId(advertiserId);
            }
            
            return results;
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId)
        {
            // No need to check the toggle as when disabled you won't have a Guid master id.
            var products = _AabApiCache.GetAdvertiserProducts(advertiserMasterId);
            return products;
        }

        /// <inheritdoc />
        public ProductDto GetAdvertiserProduct(Guid advertiserMasterId, Guid productMasterId)
        {
            // No need to check the toggle as when disabled you won't have a Guid master id.
            var products = _AabApiCache.GetAdvertiserProducts(advertiserMasterId);
            var product = products.Single(i => i.MasterId == productMasterId, $"Product '{productMasterId}' not found for advertiser '{advertiserMasterId}'.");
            return product;
        }

        /// <inheritdoc />
        public ProductDto GetProduct(int productId)
        {
            // must check the toggle and fail since products have integer ids from both sources.
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            if (isAabEnabled)
            {
                throw new InvalidOperationException("Disable the AAb Navigation Feature to retrieve a product by an integer id.  Otherwise use method GetAdvertiserProduct.");
            }
            var product = _TrafficApiCache.GetProduct(productId);
            return product;
        }

        public void ClearAgenciesCache()
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            if (isAabEnabled)
            {
                _AabApiCache.ClearAgenciesCache();
            }
            else
            {
                _TrafficApiCache.ClearAgenciesCache();
            }
        }

        public void ClearAdvertisersCache()
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            if (isAabEnabled)
            {
                _AabApiCache.ClearAdvertisersCache();
            }
            else
            {
                _TrafficApiCache.ClearAdvertisersCache();
            }
        }
    }
}