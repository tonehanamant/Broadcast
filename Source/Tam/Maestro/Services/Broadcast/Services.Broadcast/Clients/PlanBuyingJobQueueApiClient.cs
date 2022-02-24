﻿using Newtonsoft.Json;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingApiClient
    {
        Task<PlanBuyingApiSpotsResponseDto_v3> GetBuyingSpotsResultAsync(PlanBuyingApiRequestDto_v3 request);
    }

    public class PlanBuyingJobQueueApiClient : IPlanBuyingApiClient
    {
        private readonly Lazy<string> _SubmitUrl;
        private readonly Lazy<string> _FetchUrl;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly HttpClient _HttpClient;
        private const string jsonContentType = "application/json";
        private const string gZipHeader =  "gzip";

        public PlanBuyingJobQueueApiClient(IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _SubmitUrl = new Lazy<string>(_GetSubmitUrl);
            _FetchUrl = new Lazy<string>(_GetFetchUrl);
            _HttpClient = httpClient;
        }

        public async Task<PlanBuyingApiSpotsResponseDto_v3> GetBuyingSpotsResultAsync(PlanBuyingApiRequestDto_v3 request)
        {
            var submitResponse = await SubmitRequestAsync(request);

            const int fetchPauseMs = 3000;
            BuyingJobFetchResponse<PlanBuyingApiSpotsResultDto_v3> fetchResponse;
            bool continueFetch;

            do
            {
                fetchResponse = await FetchResultAsync(submitResponse.task_id);
                continueFetch = fetchResponse.task_status == "PENDING";
                Thread.Sleep(fetchPauseMs);
            } while (continueFetch);

            var result = new PlanBuyingApiSpotsResponseDto_v3
            {
                RequestId = fetchResponse.request_id,
                Results = fetchResponse.results
            };
            return result;
        }

        private string _GetSubmitUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsEfficiencyModelSubmitUrl);
        }

        private string _GetFetchUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsEfficiencyModelFetchUrl);
        }

        private async Task<BuyingJobSubmitResponse> SubmitRequestAsync(PlanBuyingApiRequestDto_v3 request)
        {
            var requestSerialized = JsonConvert.SerializeObject(request);
               //.ToGZipCompressed();

            var content = new StringContent(requestSerialized, Encoding.UTF8, jsonContentType);
            //content.Headers.ContentEncoding.Add(gZipHeader); 

            var submitResult = await _HttpClient.PostAsync(_SubmitUrl.Value, content);
            var submitResponse = await submitResult.Content.ReadAsAsync<BuyingJobSubmitResponse>();

            if (submitResponse.error != null)
            {
                var msgs = string.Join(",", submitResponse.error.Messages);
                throw new InvalidOperationException($"Error returned from the pricing api submit. Name : '{submitResponse.error.Name}';  Messages : '{msgs}'");
            }

            return submitResponse;
        }

        private async Task<BuyingJobFetchResponse<PlanBuyingApiSpotsResultDto_v3>> FetchResultAsync(string taskId)
        {
            var fetchRequest = new BuyingJobFetchRequest { task_id = taskId };
            var fetchResult = await _HttpClient.PostAsJsonAsync(_FetchUrl.Value, fetchRequest);

            var fetchResponse = await fetchResult.Content.ReadAsAsync<BuyingJobFetchResponse<PlanBuyingApiSpotsResultDto_v3>>();

            if (fetchResponse.error != null)
            {
                var msgs = string.Join(",", fetchResponse.error.Messages);
                throw new InvalidOperationException($"Error returned from the pricing api fetch. Name : '{fetchResponse.error.Name}';  Messages : '{msgs}'");
            }

            return fetchResponse;
        }
    }
}