using Newtonsoft.Json;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface IPricingApiClient
    {
        Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request);
    }

    public class PricingJobQueueApiClient : BroadcastBaseClass, IPricingApiClient
    {
        private readonly Lazy<string> _PlanPricingAllocationsEfficiencyModelSubmitUrl;
        private Lazy<string> _PlanPricingAllocationsEfficiencyModelFetchUrl;
        private readonly Lazy<bool> _IsZippedPricingEnabled;
        private readonly HttpClient _HttpClient;
        private const string jsonContentType = "application/json";
        private const string gZipHeader =  "gzip";

        public PricingJobQueueApiClient(IConfigurationSettingsHelper configurationSettingsHelper, IFeatureToggleHelper featureToggleHelper, HttpClient httpClient)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _PlanPricingAllocationsEfficiencyModelSubmitUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelSubmitUrl);
            _PlanPricingAllocationsEfficiencyModelFetchUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelFetchUrl);
            _IsZippedPricingEnabled = new Lazy<bool>(() => featureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_ZIPPED_PRICING));
            _HttpClient = httpClient;
        }

        public async Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request)
        {
            var submitResponse = await SubmitRequestAsync(request);

            const int fetchPauseMs = 3000;
            PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3> fetchResponse;
            bool continueFetch;

            do
            {
                fetchResponse = await FetchResultAsync(submitResponse.task_id);
                continueFetch = fetchResponse.task_status == "PENDING";
                Thread.Sleep(fetchPauseMs);
            } while (continueFetch);

            var result = new PlanPricingApiSpotsResponseDto_v3
            {
                RequestId = fetchResponse.request_id,
                Results = fetchResponse.results
            };
            return result;
        }

        private string _GetPlanPricingAllocationsEfficiencyModelSubmitUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsEfficiencyModelSubmitUrl);
        }

        private string _GetPlanPricingAllocationsEfficiencyModelFetchUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsEfficiencyModelFetchUrl);
        }

        private async Task<PricingJobSubmitResponse> SubmitRequestAsync(PlanPricingApiRequestDto_v3 request)
        {
            _LogInfo($"Starting: Submitting the pricing request");
            var submitResult = new HttpResponseMessage();
            var requestSerialized = JsonConvert.SerializeObject(request);

            if (_IsZippedPricingEnabled.Value)
            {
                var zippedPayload = CompressionHelper.GetGzipCompress(requestSerialized);

                var content = new ByteArrayContent(zippedPayload);
                content.Headers.Add("Content-Encoding", gZipHeader);
                content.Headers.ContentType = new MediaTypeHeaderValue(jsonContentType);
                submitResult = await _HttpClient.PostAsync(_PlanPricingAllocationsEfficiencyModelSubmitUrl.Value, content);
                _LogInfo($"Submitted the pricing request with a gzip payload");
            }
            else
            {
                var content = new StringContent(requestSerialized, Encoding.UTF8, jsonContentType);
                submitResult = await _HttpClient.PostAsync(_PlanPricingAllocationsEfficiencyModelSubmitUrl.Value, content);
                _LogInfo($"Submitted the pricing request with a json payload");
            }

            var submitResponse = await submitResult.Content.ReadAsAsync<PricingJobSubmitResponse>();

            if (submitResponse.error != null)
            {
                var msgs = string.Join(",", submitResponse.error.Messages);

                throw new InvalidOperationException($"Error returned from the pricing api submit. Name : '{submitResponse.error.Name}';  Messages : '{msgs}'");
            }

            _LogInfo($"Finished: Submitting the pricing request");
            return submitResponse;
        }

        private async Task<PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3>> FetchResultAsync(string taskId)
        {
            _LogInfo($"Starting: Fetching the pricing results");
            var fetchRequest = new PricingJobFetchRequest { task_id = taskId };
            var requestSerialized = JsonConvert.SerializeObject(fetchRequest);
            var content = new StringContent(requestSerialized, Encoding.UTF8, jsonContentType);

            if (_IsZippedPricingEnabled.Value)
            {
                _HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(gZipHeader));

                var fetchResult = await _HttpClient.PostAsync(_PlanPricingAllocationsEfficiencyModelFetchUrl.Value, content);

                var fetchResponse = await fetchResult.Content.ReadAsByteArrayAsync();
                var uncommpressedResult = CompressionHelper.GetGzipUncompress(fetchResponse);
                var response = JsonConvert.DeserializeObject<PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3>>(uncommpressedResult);

                if (response.error != null)
                {
                    var msgs = string.Join(",", response.error.Messages);
                    throw new InvalidOperationException($"Error returned from the pricing api fetch. Name : '{response.error.Name}';  Messages : '{msgs}'");
                }

                _HttpClient.DefaultRequestHeaders.AcceptEncoding.Remove(new StringWithQualityHeaderValue(gZipHeader));

                _LogInfo($"Fetching the pricing request with a gzip payload");
                _LogInfo($"Finished: Fetching the pricing results");
                return response;
            }
            else
            {
                var fetchResult = await _HttpClient.PostAsync(_PlanPricingAllocationsEfficiencyModelFetchUrl.Value, content);
                var fetchResponse = await fetchResult.Content.ReadAsAsync<PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3>>();

                if (fetchResponse.error != null)
                {
                    var msgs = string.Join(",", fetchResponse.error.Messages);
                    throw new InvalidOperationException($"Error returned from the pricing api fetch. Name : '{fetchResponse.error.Name}';  Messages : '{msgs}'");
                }

                _LogInfo($"Fetching the pricing request with a json payload");
                _LogInfo($"Finished: Fetching the pricing results");
                return fetchResponse;
            }
        }
    }
}