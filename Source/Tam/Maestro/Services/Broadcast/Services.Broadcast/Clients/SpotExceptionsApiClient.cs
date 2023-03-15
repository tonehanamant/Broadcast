using Amazon.Runtime.Internal;
using Newtonsoft.Json;
using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Entities.SpotExceptions.DecisionSync;
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
        /// <summary>
        /// Ingests the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<IngestApiResponse> IngestAsync(IngestApiRequest request);

        /// <summary>
        /// Publishes the synchronize request asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<bool> PublishSyncRequestAsync(ResultsSyncRequest request);

        /// <summary>
        /// Gets the synchronize state asynchronous.
        /// </summary>
        /// <param name="runningSyncId">The running synchronize identifier.</param>
        /// <returns></returns>
        Task<GetSyncStateResponseDto> GetSyncStateAsync(int runningSyncId);

        /// <summary>
        /// Synchronizes the successfully ran by time of day asynchronous.
        /// </summary>
        /// <param name="ranByHour">The ran by hour.</param>
        /// <returns></returns>
        Task<bool> SyncSuccessfullyRanByTimeOfDayAsync(int ranByHour);
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

        /// <inheritdoc />
        public async Task<IngestApiResponse> IngestAsync(IngestApiRequest request)
        {
            var ingestUrl = @"pull-spot-exceptions/api/ingest";
            var ingestContent = new StringContent(JsonSerializerHelper.ConvertToJson(request), Encoding.UTF8, "application/json");

            var client = await _GetSecureHttpClientAsync(AppName_Ingest);
            
            var postReponse = await client.PostAsync(ingestUrl, ingestContent);
            var result = await postReponse.Content.ReadAsAsync<IngestApiResponse>();

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> PublishSyncRequestAsync(ResultsSyncRequest request)
        {
            var requestUrl = @"pull-spot-exception-results/api/Results/notify-data-ready";
            var requestContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var client = await _GetSecureHttpClientAsync(AppName_Results);
            var postResponse = await client.PostAsync(requestUrl, requestContent);

            if (postResponse.IsSuccessStatusCode == false)
            {
                _LogInfo($"Error connecting to ResultsApi for notify-data-ready.  Requested by '{request.RequestedBy}'.");
                throw new InvalidOperationException($"Error connecting to ResultsApi for notify-data-ready : {postResponse}");
            }

            var result = await postResponse.Content.ReadAsAsync<ResultsSyncResponse>();

            if (!result.Success)
            {
                _LogInfo($"Error calling the ResultsApi.  Requested by '{request.RequestedBy}'.");
                throw new InvalidOperationException($"Error calling the ResultsApi for notify-data-ready : {result.Message}");
            }

            _LogInfo($"Successfully notified consumers that results data is ready.  Requested by '{request.RequestedBy}'.");
            return result.Success;
        }

        /// <inheritdoc />
        public async Task<GetSyncStateResponseDto> GetSyncStateAsync(int runningSyncId)
        {
            var requestUrl = $"pull-spot-exception-results/api/Results/get-sync-state?runId={runningSyncId}";

            var client = await _GetSecureHttpClientAsync(AppName_Results);
            var response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode == false)
            {
                _LogInfo($"Error connecting to ResultsApi for get-sync-state with runId '{runningSyncId}'.");
                throw new InvalidOperationException($"Error connecting to ResultsApi for get-sync-state : {response}");
            }

            var result = await response.Content.ReadAsAsync<GetSyncStateResponseDto>();

            if (!result.Success)
            {
                _LogInfo($"Error connecting to ResultsApi for get-sync-state with runId '{runningSyncId}'.");
                throw new InvalidOperationException($"Error connecting to ResultsApi for get-sync-state : {response}");
            }

            _LogInfo($"Successfully verified the state of last running job with a runId '{runningSyncId}'.");

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> SyncSuccessfullyRanByTimeOfDayAsync(int ranByHour)
        {
            var maxRanByDate = DateTime.Now.Add(new TimeSpan(ranByHour, 0, 0));
            _LogInfo($"Attempting to verify a sync successfully ran for the current week by '{maxRanByDate}'.");

            var ingestUrl = @"pull-spot-exceptions/api/ingest/status";
            var ingestContent = new StringContent(JsonSerializerHelper.ConvertToJson(ranByHour), Encoding.UTF8, "application/json");

            var client = await _GetSecureHttpClientAsync(AppName_Ingest);

            var postReponse = await client.PostAsync(ingestUrl, ingestContent);
            var result = await postReponse.Content.ReadAsAsync<bool>();

            if (result.Equals(true))
            {
                _LogInfo($"A sync has successfully run for the current week by '{maxRanByDate}'.");
            }
            else
            {
                _LogInfo($"a sync has not successfully run for the current week by '{maxRanByDate}'.");
            }

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
