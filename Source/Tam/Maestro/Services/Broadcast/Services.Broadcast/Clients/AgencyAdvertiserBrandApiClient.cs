using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Aab;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

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
    public class AgencyAdvertiserBrandApiClient : BroadcastBaseClass, IAgencyAdvertiserBrandApiClient
    {
        private const string _CoreApiVersion = "api/v2";
        private readonly Lazy<string> _AABApiUrl;
        private readonly HttpClient _HttpClient;
        private readonly bool _IsAABCoreAPIEnabled;
        private readonly IApiTokenManager _ApiTokenManager;
        private readonly IServiceClientBase _ServiceClientBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyAdvertiserBrandApiClient"/> class.
        /// </summary>
        public AgencyAdvertiserBrandApiClient(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient,
            IApiTokenManager apiTokenManager, IServiceClientBase serviceClientBase)
                : base(featureToggleHelper, configurationSettingsHelper)
        {
            _AABApiUrl = new Lazy<string>(() => $"{_GetAgencyAdvertiserBrandApiUrl()}");
             _HttpClient = httpClient;
            _IsAABCoreAPIEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_CORE_API);
            _ApiTokenManager = apiTokenManager;
            _ServiceClientBase = serviceClientBase;
        }

        /// <inheritdoc/>
        public List<AgencyDto> GetAgencies()
        {
            try
            {
                List<aab_agency> apiResult = new List<aab_agency>();
                if (_IsAABCoreAPIEnabled)
                {
                    var httpClient = _GetSecureHttpClient();
                    apiResult = httpClient.Get<ApiListResponseTyped<aab_agency>>($"{_CoreApiVersion}/agencies").ResultList;
                }
                else
                {
                    var url = $"{_AABApiUrl.Value}/agencies";
                    apiResult = _HttpClient.Get<List<aab_agency>>(url);
                }

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
                if (_IsAABCoreAPIEnabled)
                {
                    var httpClient = _GetSecureHttpClient();
                    apiResult = httpClient.Get<ApiListResponseTyped<aab_advertiser>>($"{_CoreApiVersion}/advertisers").ResultList;
                }
                else
                {
                    var url = $"{_AABApiUrl.Value}/advertisers";
                    apiResult = _HttpClient.Get<List<aab_advertiser>>(url);
                }

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
                if (_IsAABCoreAPIEnabled)
                {
                    var httpClient = _GetSecureHttpClient();
                    advertiserFullInfo = httpClient.Get<ApiItemResponseTyped<aab_advertiser>>
                        ($"{_CoreApiVersion}/advertisers/company/{advertiserMasterId}")
                        .Result;
                }
                else
                {
                    var url = $"{_AABApiUrl.Value}/advertisers/getadvertiserbyid/{advertiserMasterId}";
                    advertiserFullInfo = _HttpClient.Get<aab_advertiser>(url);
                }

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

        private HttpClient _GetSecureHttpClient()
        {
            var apiBaseUrl = _GetAgencyAdvertiserBrandCoreApiBaseUrl();
            var applicationId = _GetAgencyAdvertiserBrandCoreApiApplicationId();
            var appName = _GetAgencyAdvertiserBrandCoreApiAppName();

            var umUrl = _GetUmUrl();
            var accessToken = _ApiTokenManager.GetOrRefreshTokenAsync(umUrl, appName, applicationId)
                .GetAwaiter().GetResult();

            return _ServiceClientBase.GetServiceHttpClient(apiBaseUrl, applicationId, accessToken);
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

        private string _GetUmUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.UmUrl);
        }
    }
}