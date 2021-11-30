using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Services.Broadcast.Entities.Plan.Buying;

namespace Services.Broadcast.Clients
{
    //public class PricingJobSubmitResponse
    //{
    //    public string request_id { get; set; }
    //    public string task_id { get; set; }
    //}

    //public class PricingJobFetchRequest
    //{
    //    public string task_id { get; set; }
    //}

    //public class PricingJobFetchResponse<T>
    //{
    //    public string request_id { get; set; }
    //    public string task_id { get; set; }
    //    public string task_status { get; set; }
    //    public List<T> results { get; set; }
    //}

    public class PlanBuyingJobQueueApiClient // : IPlanBuyingApiClient
    {
        /*
        private readonly Lazy<string> _SubmitUrl;
        private readonly Lazy<string> _FetchUrl;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly HttpClient _HttpClient;
        private const string jsonContentType = "application/json";
        private const string gZipHeader =  "gzip";

        public PlanBuyingJobQueueApiClient(IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _SubmitUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelSubmitUrl);
            _FetchUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelFetchUrl);
            _HttpClient = httpClient;
        }

        public async Task<PlanBuyingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanBuyingApiRequestDto_v3 request)
        {
            var submitResponse = await SubmitRequestAsync(request);

            const int fetchPauseMs = 3000;
            PricingJobFetchResponse<PlanBuyingApiSpotsResponseDto_v3> fetchResponse;
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

        private async Task<PricingJobSubmitResponse> SubmitRequestAsync(PlanBuyingApiRequestDto_v3 request)
        {
            var requestSerialized = JsonConvert.SerializeObject(request);
               //.ToGZipCompressed();

            var content = new StringContent(requestSerialized, Encoding.UTF8, jsonContentType);
            //content.Headers.ContentEncoding.Add(gZipHeader); 

            var submitResult = await _HttpClient.PostAsync(_SubmitUrl.Value, content);
            var submitResponse = await submitResult.Content.ReadAsAsync<PricingJobSubmitResponse>();
            return submitResponse;
        }

        private async Task<PricingJobFetchResponse<PlanBuyingApiSpotsResponseDto_v3>> FetchResultAsync(string taskId)
        {
            var fetchRequest = new PricingJobFetchRequest { task_id = taskId };
            var fetchResult = await _HttpClient.PostAsJsonAsync(_FetchUrl.Value, fetchRequest);

            var result = await fetchResult.Content.ReadAsAsync<PricingJobFetchResponse<PlanBuyingApiSpotsResponseDto_v3>>();
            return result;
        }
        */
    }
}