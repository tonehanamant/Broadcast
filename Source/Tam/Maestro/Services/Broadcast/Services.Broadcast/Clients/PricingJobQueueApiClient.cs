using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public class PricingJobSubmitResponse
    {
        public string request_id { get; set; }
        public string task_id { get; set; }
        public PricingApiErrorDto error { get; set; }
    }

    public class PricingApiErrorDto
    {
        public List<string> Messages { get; set; } = new List<string>();
        public string Name { get; set; }
    }

    public class PricingJobFetchRequest
    {
        public string task_id { get; set; }
    }

    public class PricingJobFetchResponse<T>
    {
        public string request_id { get; set; }
        public string task_id { get; set; }
        public string task_status { get; set; }
        public List<T> results { get; set; }
        public PricingApiErrorDto error { get; set; }
    }

    public class PricingJobQueueApiClient : IPricingApiClient
    {
        private readonly Lazy<string> _PlanPricingAllocationsEfficiencyModelSubmitUrl;
        private Lazy<string> _PlanPricingAllocationsEfficiencyModelFetchUrl;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly HttpClient _HttpClient;
        private const string jsonContentType = "application/json";
        private const string gZipHeader =  "gzip";

        public PricingJobQueueApiClient(IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _PlanPricingAllocationsEfficiencyModelSubmitUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelSubmitUrl);
            _PlanPricingAllocationsEfficiencyModelFetchUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelFetchUrl);
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
            var requestSerialized = JsonConvert.SerializeObject(request);
               //.ToGZipCompressed();

            var content = new StringContent(requestSerialized, Encoding.UTF8, jsonContentType);
            //content.Headers.ContentEncoding.Add(gZipHeader); 

            var submitResult = await _HttpClient.PostAsync(_PlanPricingAllocationsEfficiencyModelSubmitUrl.Value, content);
            var submitResponse = await submitResult.Content.ReadAsAsync<PricingJobSubmitResponse>();

            if (submitResponse.error != null)
            {
                var msgs = string.Join(",", submitResponse.error.Messages);

                throw new InvalidOperationException($"Error returned from the pricing api submit. Name : '{submitResponse.error.Name}';  Messages : '{msgs}'");
            }

            return submitResponse;
        }

        private async Task<PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3>> FetchResultAsync(string taskId)
        {
            var fetchRequest = new PricingJobFetchRequest { task_id = taskId };
            var fetchResult = await _HttpClient.PostAsJsonAsync(_PlanPricingAllocationsEfficiencyModelFetchUrl.Value, fetchRequest);

            var fetchResponse = await fetchResult.Content.ReadAsAsync<PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3>>();

            if (fetchResponse.error != null)
            {
                var msgs = string.Join(",", fetchResponse.error.Messages);
                throw new InvalidOperationException($"Error returned from the pricing api fetch. Name : '{fetchResponse.error.Name}';  Messages : '{msgs}'");
            }

            return fetchResponse;
        }
    }
}