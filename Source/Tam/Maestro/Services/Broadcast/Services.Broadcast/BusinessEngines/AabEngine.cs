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
        /// <param name="agencyMasterId">The identifier used by the Aab Api.</param>
        AgencyDto GetAgency(Guid agencyMasterId);

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        List<AdvertiserDto> GetAdvertisers();

        /// <summary>
        /// Gets the advertiser.
        /// </summary>
        /// <param name="advertiserMasterId">The identifier used by the Aab Api.</param>
        AdvertiserDto GetAdvertiser(Guid advertiserMasterId);

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
    public class AabEngine : IAabEngine
    {
        private IAabCache _AabApiCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AabEngine"/> class.
        /// </summary>
        /// <param name="aabApiCache">The aab API cache.</param>
        /// <param name="featureToggleHelper">The feature toggle helper.</param>
        public AabEngine(IAabCache aabApiCache,
            IFeatureToggleHelper featureToggleHelper)
        {
            _AabApiCache = aabApiCache;
        }

        /// <inheritdoc />
        public List<AgencyDto> GetAgencies()
        {

            var result = _AabApiCache.GetAgencies();
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
            var result = _AabApiCache.GetAdvertisers();
            return result;
        }

        /// <inheritdoc />
        public AdvertiserDto GetAdvertiser(Guid advertiserMasterId)
        {
            var items = _AabApiCache.GetAdvertisers();
            var item = items.Single(i => i.MasterId == advertiserMasterId, $"Advertiser with master id {advertiserMasterId} not found.");
            return item;
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId)
        {
            var products = _AabApiCache.GetAdvertiserProducts(advertiserMasterId);
            return products;
        }

        /// <inheritdoc />
        public ProductDto GetAdvertiserProduct(Guid advertiserMasterId, Guid productMasterId)
        {
            var products = GetAdvertiserProducts(advertiserMasterId);
            var product = products.Single(i => i.MasterId == productMasterId, $"Product '{productMasterId}' not found for advertiser '{advertiserMasterId}'.");
            return product;
        }

        public void ClearAgenciesCache()
        {
            _AabApiCache.ClearAgenciesCache();
        }

        public void ClearAdvertisersCache()
        {
            _AabApiCache.ClearAdvertisersCache();
        }
    }
}