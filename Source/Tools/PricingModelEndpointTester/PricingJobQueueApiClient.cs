using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Services.Broadcast;
using Services.Broadcast.Clients;

namespace PricingModelEndpointTester
{
    public class PricingJobSubmitResponse
    {
        public string request_id { get; set; }
        public string task_id{ get; set; }
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
    }

    public class PricingJobQueueApiClient
    {
        protected const int ASYNC_API_TIMEOUT_MILLISECONDS = 900000;
        private readonly HttpClient _HttpClient;

        public PricingJobQueueApiClient(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public async Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request)
        {
            Console.WriteLine($"Submitting Request...");
            var submitResponse = SubmitRequest(request);
            Console.WriteLine($"Received submission response as task_id : '{submitResponse.Result.task_id}'");

            const int fetchPauseMs = 3000;
            var continueFetch = false;
            var fetchRequest = new PricingJobFetchRequest { task_id = submitResponse.Result.task_id };
            PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3> fetchResponse = null;
            do
            {
                Console.WriteLine($"Fetching for task_id : '{fetchRequest.task_id}'");
                fetchResponse = FetchResult(submitResponse.Result.task_id).Result;
                if (fetchResponse.task_status == "PENDING")
                {
                    continueFetch = true;
                    Console.WriteLine($"Task pending '{fetchRequest.task_id}'.  Pausing for {fetchPauseMs}ms....");
                    Thread.Sleep(fetchPauseMs);
                }
                else
                {
                    Console.WriteLine($"Received results for task_id : '{fetchRequest.task_id}'");
                    continueFetch = false;
                }
            } while (continueFetch);

            var result = new PlanPricingApiSpotsResponseDto_v3
            {
                RequestId = fetchResponse.request_id,
                Results = fetchResponse.results
            };
            return result;
        }

        private async Task<PricingJobSubmitResponse> SubmitRequest(PlanPricingApiRequestDto_v3 request)
        {
            var submitUrl = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/preview/submit";

            var submitResult = await _HttpClient.PostAsJsonAsync(submitUrl, request, new CancellationTokenSource(ASYNC_API_TIMEOUT_MILLISECONDS).Token);
            var submitResponse = submitResult.Content.ReadAsAsync<PricingJobSubmitResponse>().Result;
            return submitResponse;
        }

        private async Task<PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3>> FetchResult(string taskId)
        {
            var fetchUrl = @"https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/preview/fetch";

            var fetchRequest = new PricingJobFetchRequest { task_id = taskId };
            var fetchResult = await _HttpClient.PostAsJsonAsync(fetchUrl, fetchRequest, new CancellationTokenSource(ASYNC_API_TIMEOUT_MILLISECONDS).Token);
            
            // var raw = fetchResult.Content.ReadAsStringAsync();

            var result = fetchResult.Content.ReadAsAsync<PricingJobFetchResponse<PlanPricingApiSpotsResultDto_v3>>().Result;
            return result;
        }
    }
}