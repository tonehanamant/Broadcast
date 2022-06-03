using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Helpers;
using Services.Broadcast.Helpers.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface ISpotExceptionsIngestApiClient
    {
        Task<IngestApiResponse> IngestAsync(IngestApiRequest request);
    }

    public class SpotExceptionsIngestApiClient : CadentSecuredClientBase, ISpotExceptionsIngestApiClient
    {

        public SpotExceptionsIngestApiClient(IApiTokenManager apiTokenManager,
            IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
                : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        {
        }

        public async Task<IngestApiResponse> IngestAsync(IngestApiRequest request)
        {
            var ingestUrl = @"pull-spot-exceptions/api/ingest";
            var ingestContent = new StringContent(JsonSerializerHelper.ConvertToJson(request), Encoding.UTF8, "application/json");

            var client = await _GetSecureHttpClientAsync();
            
            var postReponse = await client.PostAsync(ingestUrl, ingestContent);
            var result = await postReponse.Content.ReadAsAsync<IngestApiResponse>();

            return result;
        }
        
        private async Task<HttpClient> _GetSecureHttpClientAsync()
        {
            var apiBaseUrl = _GetApiBaseUrl();
            var applicationId = _GetApplicationId();
            var appName = _GetAppName();
            var apiGwId = _GetApiGwId();

            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);
            client.DefaultRequestHeaders.Add("x-apigw-api-id", apiGwId);

            client.Timeout = new TimeSpan(2, 0, 0);

            return client;
        }

        private string _GetApiBaseUrl()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValue<string>(SpotExceptionsIngestApiConfigKeys.ApiBaseUrl);
            return result;
        }

        private string _GetApplicationId()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValue<string>(SpotExceptionsIngestApiConfigKeys.ApplicationId);
            return result;
        }

        private string _GetAppName()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValue<string>(SpotExceptionsIngestApiConfigKeys.AppName);
            return result;
        }

        private string _GetApiGwId()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValue<string>(SpotExceptionsIngestApiConfigKeys.ApiGwId);
            return result;
        }
    }
}
