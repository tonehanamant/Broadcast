using Services.Broadcast.Entities.ReelRosterIscis;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Tam.Maestro.Common;

namespace Services.Broadcast.Clients
{
    public interface IReelIsciApiClient
    {
        List<ReelRosterIsciDto> GetReelRosterIscis(DateTime startDate, int numberOfDays);
    }

    public class ReelIsciApiClient : CadentSecuredClientBase,IReelIsciApiClient
    {
        public const string ReelIsciApiDateFormat = "yyyy-MM-dd";
        private const string headerKey_ApiKey = "x-api-key";

        private Lazy<string> _ApiUrl;
        private Lazy<string> _ApiKey;
        
        private readonly HttpClient _HttpClient;
        private readonly Lazy<bool> _IsUmReelRosterEnabled;

        public ReelIsciApiClient(IApiTokenManager apiTokenManager,
                IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient)
            : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper) 
        {          

            _ApiUrl = new Lazy<string>(_GetApiUrl);
            _ApiKey = new Lazy<string>(_GetApiKey);
            _HttpClient = httpClient;
            _IsUmReelRosterEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_UM_REEL_ROSTER));
        }

        public List<ReelRosterIsciDto> GetReelRosterIscis(DateTime startDate, int numberOfDays)
        {
            const string apiOperation = @"/submit_reel_roster_query";
            if (numberOfDays < 1)
            {
                numberOfDays = 1;
            }
            var startDateString = startDate.ToString(ReelIsciApiDateFormat);
            var paramString = $"?num_days={numberOfDays}&start_date={startDateString}";

            var queryUrl = $"{_ApiUrl.Value}{apiOperation}{paramString}";

            var apiReturnRaw = _PostAndGet(queryUrl, _ApiKey.Value).Result;

            var result = apiReturnRaw.Select(r =>
                new ReelRosterIsciDto
                {
                    Isci = r.ISCI_Name,
                    SpotLengthDuration = r.Length,
                    StartDate = r.Start_Date,
                    EndDate = r.End_Date,
                    AdvertiserNames = r.Advertiser
                }).ToList();

            return result;
        }

        protected virtual async Task<List<ReelRosterIsciEntity>> _PostAndGet(string url, string apiKey)
        {
            List<ReelRosterIsciEntity> result = null;
            HttpResponseMessage serviceResponse = null;
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequestMessage.Headers.Add(headerKey_ApiKey, apiKey);
                var httpClient = await _GetSecureHttpClientAsync(url);
                if (_IsUmReelRosterEnabled.Value)
                {
                    serviceResponse = httpClient.SendAsync(httpRequestMessage).Result;
                }
                else
                {
                    serviceResponse = _HttpClient.SendAsync(httpRequestMessage).Result;
                }

                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception($"Error connecting to ReelIsciApi for get. : {serviceResponse}");
                }

                var rawResult = serviceResponse.Content.ReadAsAsync<ReelRosterIsciResponseEntity>().Result;
                if (!rawResult.Success)
                {
                    throw new InvalidOperationException($"Error calling the ReelIsciApi for post data during get : {rawResult.Message}");
                }

                result = rawResult.Data;
            }
            catch (Exception e)
            {
                throw new Exception("Error calling the ReelIsciApi for post data during get.", e);
            }

            return result;
        }

        private async Task<HttpClient> _GetSecureHttpClientAsync(string apiBaseUrl)
        {           
            var applicationId = _GetReelIsciApplicationId();
            var appName = _GetReelIsciApiAppName();
            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);
            return client;
        }

        private string _GetApiUrl()
        {
            var apiUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(ReelIsciApiClientConfigKeys.ApiUrlBase);
            return apiUrl;
        }

        private string _GetApiKey()
        {
            var encryptedDevApiKey = _ConfigurationSettingsHelper.GetConfigValue<string>(ReelIsciApiClientConfigKeys.EncryptedApiKey);
            var apiKey = EncryptionHelper.DecryptString(encryptedDevApiKey, EncryptionHelper.EncryptionKey); ;
            return apiKey;
        }

        private string _GetReelIsciApplicationId()
        {
            var applicationId = _ConfigurationSettingsHelper.GetConfigValue<string>(ReelIsciApiClientConfigKeys.ApplicationId);
            return applicationId;
        }

        private string _GetReelIsciApiAppName()
        {
            var appName = _ConfigurationSettingsHelper.GetConfigValue<string>(ReelIsciApiClientConfigKeys.AppName);
            return appName;
        }
    }
}
