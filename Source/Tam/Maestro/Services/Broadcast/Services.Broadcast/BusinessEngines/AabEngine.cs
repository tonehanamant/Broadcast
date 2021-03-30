﻿using Common.Services.Extensions;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
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
    }

    /// <summary>
    /// Gets Aab data from the correct source based on the system configuration.
    /// </summary>
    /// <seealso cref="Services.Broadcast.BusinessEngines.IAabEngine" />
    public class AabEngine : IAabEngine
    {
        //bp-2101. Remove the AabCache code.
        //private IAabCache _AabApiCache;
        private IAgencyAdvertiserBrandApiClient _AgencyAdvertiserBrandApiClient;
        private ITrafficApiCache _TrafficApiCache;
        private IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsAabEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="AabEngine"/> class.
        /// </summary>
        /// <param name="AgencyAdvertiserBrandApiClient">The aab API cache.</param>
        /// <param name="trafficApiCache">The traffic API cache.</param>
        /// <param name="featureToggleHelper">The feature toggle helper.</param>
        public AabEngine(IAgencyAdvertiserBrandApiClient AgencyAdvertiserBrandApiClient,
            ITrafficApiCache trafficApiCache,
            IFeatureToggleHelper featureToggleHelper)
        {
            //_AabApiCache = aabApiCache;
            _AgencyAdvertiserBrandApiClient = AgencyAdvertiserBrandApiClient;
            _TrafficApiCache = trafficApiCache;
            _FeatureToggleHelper = featureToggleHelper;
            _IsAabEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION));
        }

        /// <inheritdoc />
        public List<AgencyDto> GetAgencies()
        {
            
            var result = _IsAabEnabled.Value
                ? _AgencyAdvertiserBrandApiClient.GetAgencies()
                : _TrafficApiCache.GetAgencies();
            return result;
        }

        /// <inheritdoc />
        public AgencyDto GetAgency(int agencyId)
        {
            
            AgencyDto result;
            if (_IsAabEnabled.Value)
            {
                var items = _AgencyAdvertiserBrandApiClient.GetAgencies();
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
            var items = _AgencyAdvertiserBrandApiClient.GetAgencies();
            var item = items.Single(i => i.MasterId == agencyMasterId, $"Agency with master id {agencyMasterId} not found.");
            return item;
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisers()
        {
            
            var result = _IsAabEnabled.Value
                ? _AgencyAdvertiserBrandApiClient.GetAdvertisers()
                : _TrafficApiCache.GetAdvertisers();
            return result;
        }

        /// <inheritdoc />
        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            
            AdvertiserDto result;

            if (_IsAabEnabled.Value)
            {
                var items = _AgencyAdvertiserBrandApiClient.GetAdvertisers();
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
            var items = _AgencyAdvertiserBrandApiClient.GetAdvertisers();
            var item = items.Single(i => i.MasterId == advertiserMasterId, $"Advertiser with master id {advertiserMasterId} not found.");
            return item;
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(int advertiserId)
        {
            
            List<ProductDto> results;

            if (_IsAabEnabled.Value)
            {
                var advertiser = GetAdvertiser(advertiserId);
                results = _AgencyAdvertiserBrandApiClient.GetAdvertiserProducts(advertiser.MasterId.Value);
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
            var products = _AgencyAdvertiserBrandApiClient.GetAdvertiserProducts(advertiserMasterId);
            return products;
        }

        /// <inheritdoc />
        public ProductDto GetAdvertiserProduct(Guid advertiserMasterId, Guid productMasterId)
        {
            // No need to check the toggle as when disabled you won't have a Guid master id.
            var products = _AgencyAdvertiserBrandApiClient.GetAdvertiserProducts(advertiserMasterId);
            var product = products.Single(i => i.MasterId == productMasterId, $"Product '{productMasterId}' not found for advertiser '{advertiserMasterId}'.");
            return product;
        }

        /// <inheritdoc />
        public ProductDto GetProduct(int productId)
        {
            // must check the toggle and fail since products have integer ids from both sources.
            
            if (_IsAabEnabled.Value)
            {
                throw new InvalidOperationException("Disable the AAb Navigation Feature to retrieve a product by an integer id.  Otherwise use method GetAdvertiserProduct.");
            }
            var product = _TrafficApiCache.GetProduct(productId);
            return product;
        }
    }
}