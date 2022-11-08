using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Helpers;
using Services.Broadcast.Helpers.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface ISpotExceptionsApiClient
    {
        Task<IngestApiResponse> IngestAsync(IngestApiRequest request);

        Task<bool> PublishSyncRequestAsync(ResultsSyncRequest request);
    }

    public class SpotExceptionsApiClient : CadentSecuredClientBase, ISpotExceptionsApiClient
    {
        const string AppName_Ingest = "BroadcastSEIngest";
        const string AppName_Results = "BroadcastSEResultsPush";

        public SpotExceptionsApiClient(IApiTokenManager apiTokenManager,
            IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
                : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        {
        }

        public async Task<IngestApiResponse> IngestAsync(IngestApiRequest request)
        {
            var ingestUrl = @"pull-spot-exceptions/api/ingest";
            var ingestContent = new StringContent(JsonSerializerHelper.ConvertToJson(request), Encoding.UTF8, "application/json");

            var client = await _GetSecureHttpClientAsync(AppName_Ingest);
            
            var postReponse = await client.PostAsync(ingestUrl, ingestContent);
            var result = await postReponse.Content.ReadAsAsync<IngestApiResponse>();

            return result;
        }

        public async Task<bool> PublishSyncRequestAsync(ResultsSyncRequest request)
        {
            var requestUrl = @"pull-spot-exception-results/api/Results/notify-data-ready";
            var requestContent = new StringContent(JsonSerializerHelper.ConvertToJson(request), Encoding.UTF8, "application/json");

            var client = await _GetSecureHttpClientAsync(AppName_Results);

            var postResponse = await client.PostAsync(requestUrl, requestContent);
            var result = await postResponse.Content.ReadAsAsync<bool>();

            return result;
        }

        private async Task<HttpClient> _GetSecureHttpClientAsync(string appName)
        {
            var apiBaseUrl = _GetApiBaseUrl();
            var applicationId = _GetApplicationId();

            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);

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
    }
}
