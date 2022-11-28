using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Aab;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// A client for retrieving the data.
    /// </summary>
    public interface IAgencyAdvertiserBrandApiClient
    {
        /// <summary>
        /// Gets the agencies.
        /// </summary>
        /// <returns></returns>
        List<AgencyDto> GetAgencies();

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        /// <returns></returns>
        List<AdvertiserDto> GetAdvertisers();

        /// <summary>
        /// Gets the advertiser products.
        /// </summary>
        /// <param name="masterId">The master identifier.</param>
        List<ProductDto> GetAdvertiserProducts(Guid masterId);
    }

    /// <summary>
    /// A client for retrieving the data.
    /// </summary>
    public class AgencyAdvertiserBrandApiClient : CadentSecuredClientBase, IAgencyAdvertiserBrandApiClient
    {
        private const string _CoreApiVersion = "api/v2";
        private readonly Lazy<string> _AABApiUrl;
        private readonly HttpClient _HttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyAdvertiserBrandApiClient"/> class.
        /// </summary>
        public AgencyAdvertiserBrandApiClient(HttpClient httpClient,
            IApiTokenManager apiTokenManager, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
                : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        {
            _AABApiUrl = new Lazy<string>(() => $"{_GetAgencyAdvertiserBrandApiUrl()}");
             _HttpClient = httpClient;
        }

        /// <inheritdoc/>
        public List<AgencyDto> GetAgencies()
        {
            try
            {
                List<aab_agency> apiResult = new List<aab_agency>();
                
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                apiResult = httpClient.Get<ApiListResponseTyped<aab_agency>>($"{_CoreApiVersion}/agencies").ResultList;
               
                var result = apiResult.Select(i => new AgencyDto
                {
                    Id = i.id,
                    MasterId = i.company_id,
                    Name = i.name
                }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch agencies data.", ex);
            }
        }

        /// <inheritdoc/>
        public List<AdvertiserDto> GetAdvertisers()
        {
            try
            {
                List<aab_advertiser> apiResult = new List<aab_advertiser>();
               
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                apiResult = httpClient.Get<ApiListResponseTyped<aab_advertiser>>($"{_CoreApiVersion}/advertisers").ResultList;

                var result = apiResult.Select(i => new AdvertiserDto()
                {
                    Id = i.id,
                    MasterId = i.company_id,
                    Name = i.name
                }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch advertisers data.", ex);
            }
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId)
        {
            try
            {
                aab_advertiser advertiserFullInfo = new aab_advertiser();
                
                var httpClient = _GetSecureHttpClientAsync().GetAwaiter().GetResult();
                advertiserFullInfo = httpClient.Get<ApiItemResponseTyped<aab_advertiser>>
                        ($"{_CoreApiVersion}/advertisers/company/{advertiserMasterId}")
                        .Result;

                var products = advertiserFullInfo.products.Select(i => new ProductDto
                {
                    Id = i.id,
                    MasterId = i.product_id,
                    Name = i.name,
                    AdvertiserId = advertiserFullInfo.id,
                    AdvertiserMasterId = advertiserFullInfo.company_id
                }).ToList();

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch products data for advertiser {advertiserMasterId}.", ex);
            }
        }

        private async Task<HttpClient> _GetSecureHttpClientAsync()
        {
            var apiBaseUrl = _GetAgencyAdvertiserBrandCoreApiBaseUrl();
            var applicationId = _GetAgencyAdvertiserBrandCoreApiApplicationId();
            var appName = _GetAgencyAdvertiserBrandCoreApiAppName();

            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);
            return client;
        }

        private string _GetAgencyAdvertiserBrandApiUrl()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.AgencyAdvertiserBrandApiUrl);
            return result;
        }

        private string _GetAgencyAdvertiserBrandCoreApiBaseUrl()
        {
            var apiBaseUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(AgencyAdvertiserBrandCoreApiConfigKeys.ApiBaseUrl);
            return apiBaseUrl;
        }

        private string _GetAgencyAdvertiserBrandCoreApiAppName()
        {
            var appName = _ConfigurationSettingsHelper.GetConfigValue<string>(AgencyAdvertiserBrandCoreApiConfigKeys.AppName);
            return appName;
        }
        private string _GetAgencyAdvertiserBrandCoreApiApplicationId()
        {
            var applicationId = _ConfigurationSettingsHelper.GetConfigValue<string>(AgencyAdvertiserBrandCoreApiConfigKeys.ApplicationId);
            return applicationId;
        }
    }
}