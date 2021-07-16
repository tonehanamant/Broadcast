using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface ITrafficApiClient
    {
        List<AgencyDto> GetFilteredAgencies(string filter);
        AdvertiserDto GetAdvertiser(int advertiserId);

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        /// <returns>List of AdvertiserDto objects</returns>
        List<AdvertiserDto> GetAdvertisers();

        List<ProductDto> GetProductsByAdvertiserId(int advertiserId);
        ProductDto GetProduct(int productId);

        /// <summary>
        /// Gets the agencies.
        /// </summary>
        /// <returns>List of AgencyDto objects</returns>
        List<AgencyDto> GetAgencies();
    }

    public class TrafficApiClient : ITrafficApiClient
    {
        private readonly Lazy<string> _AABApiUrl;
        private readonly HttpClient _HttpClient;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;

        public TrafficApiClient(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _AABApiUrl = new Lazy<string>(()=>$"{_GetAgencyAdvertiserBrandTrafficApiUrl()}");
            _HttpClient = new HttpClient();
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
        }

        public List<AgencyDto> GetFilteredAgencies(string filter)
        {
            var url = $"{_AABApiUrl.Value}/agency?filter={Uri.EscapeDataString(filter)}";

            try
            {
                return _HttpClient.Get<List<AgencyDto>>(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch agencies data with filter '{filter}'.", ex);
            }
        }

        /// <inheritdoc/>
        public List<AgencyDto> GetAgencies()
        {
            var url = $"{_AABApiUrl.Value}/agency/nolimit";

            try
            {
                return _HttpClient.Get<List<AgencyDto>>(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch agencies data.", ex);
            }
        }

        /// <inheritdoc/>
        public List<AdvertiserDto> GetAdvertisers()
        {
            var url = $"{_AABApiUrl.Value}/advertiser/nolimit";

            try
            {
                var advertisers = _HttpClient.Get<List<AdvertiserDto>>(url);

                return advertisers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch advertisers data.", ex);
            }
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            var url = $"{_AABApiUrl.Value}/advertiser/{advertiserId}/products";

            try
            {
                var products = _HttpClient.Get<List<ProductDto>>(url);

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch products data for advertiser {advertiserId}.", ex);
            }
        }

        public ProductDto GetProduct(int productId)
        {
            var url = $"{_AABApiUrl.Value}/product/{productId}";

            try
            {
                var product = _HttpClient.Get<ProductDto>(url);

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch data of the product {productId}", ex);
            }
        }

        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            var url = $"{_AABApiUrl.Value}/advertiser/{advertiserId}";

            try
            {
                var advertiser = _HttpClient.Get<AdvertiserDto>(url);

                return advertiser;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch data of the advertiser {advertiserId}", ex);
            }
        }
        private string _GetAgencyAdvertiserBrandTrafficApiUrl()
        {
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.AgencyAdvertiserBrandTrafficApiUrl) : BroadcastServiceSystemParameter.AgencyAdvertiserBrandTrafficApiUrl;
        }
    }
}
