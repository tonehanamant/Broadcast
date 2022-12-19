﻿using Newtonsoft.Json;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingApiClient
    {
        Task<PlanBuyingApiSpotsResponseDto_v3> GetBuyingSpotsResultAsync(PlanBuyingApiRequestDto_v3 request);
    }

    public class PlanBuyingJobQueueApiClient : BroadcastBaseClass, IPlanBuyingApiClient
    {
        private readonly Lazy<string> _SubmitUrl;
        private readonly Lazy<string> _FetchUrl;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly HttpClient _HttpClient;
        private readonly Lazy<bool> _IsZippedPricingEnabled;
        private const string jsonContentType = "application/json";
        private const string gZipHeader =  "gzip";

        public PlanBuyingJobQueueApiClient(IConfigurationSettingsHelper configurationSettingsHelper, IFeatureToggleHelper featureToggleHelper, HttpClient httpClient)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _SubmitUrl = new Lazy<string>(_GetSubmitUrl);
            _FetchUrl = new Lazy<string>(_GetFetchUrl);
            _HttpClient = httpClient;
            _IsZippedPricingEnabled = new Lazy<bool>(() => featureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_ZIPPED_PRICING));
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
            var submitResult = new HttpResponseMessage();
            var requestSerialized = JsonConvert.SerializeObject(request);

            if (_IsZippedPricingEnabled.Value)
            {
                var zippedPayload = CompressionHelper.GetGzipCompress(requestSerialized);

                var content = new ByteArrayContent(zippedPayload);
                content.Headers.Add("Content-Encoding", gZipHeader);
                content.Headers.ContentType = new MediaTypeHeaderValue(jsonContentType);
                submitResult = await _HttpClient.PostAsync(_SubmitUrl.Value, content);
            }
            else
            {
                var content = new StringContent(requestSerialized, Encoding.UTF8, jsonContentType);
                submitResult = await _HttpClient.PostAsync(_SubmitUrl.Value, content);
            }

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
            var fetchRequest = new PricingJobFetchRequest { task_id = taskId };
            var fetchResult = new HttpResponseMessage();

            if (_IsZippedPricingEnabled.Value)
            {
                var requestSerialized = JsonConvert.SerializeObject(fetchRequest);
                var content = new StringContent(requestSerialized, Encoding.UTF8, jsonContentType);
                _HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(gZipHeader));

                fetchResult = await _HttpClient.PostAsJsonAsync(_FetchUrl.Value, fetchRequest);


                var fetchResponse = await fetchResult.Content.ReadAsByteArrayAsync();
                var uncommpressedResult = CompressionHelper.GetGzipUncompress(fetchResponse);
                var response = JsonConvert.DeserializeObject<BuyingJobFetchResponse<PlanBuyingApiSpotsResultDto_v3>>(uncommpressedResult);

                if (!fetchResult.IsSuccessStatusCode)
                {
                    var msgs = string.Join(",", fetchResult.ReasonPhrase);
                    throw new InvalidOperationException($"Error returned from the buying api fetch. Name : '{fetchResult.ReasonPhrase}';  Messages : '{msgs}'");
                }

                return response;
            }
            else
            {
                fetchResult = await _HttpClient.PostAsJsonAsync(_FetchUrl.Value, fetchRequest);
                var fetchResponse = await fetchResult.Content.ReadAsAsync<BuyingJobFetchResponse<PlanBuyingApiSpotsResultDto_v3>>();

                if (fetchResponse.error != null)
                {
                    var msgs = string.Join(",", fetchResponse.error.Messages);
                    throw new InvalidOperationException($"Error returned from the buying api fetch. Name : '{fetchResponse.error.Name}';  Messages : '{msgs}'");
                }

                return fetchResponse;
            }
        }
    }
}