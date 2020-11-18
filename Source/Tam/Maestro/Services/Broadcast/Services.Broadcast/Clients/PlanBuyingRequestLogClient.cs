﻿using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Helpers;
using System;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingRequestLogClient
    {
        void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto planBuyingApiRequestDto, string apiVersion);

        void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto_v3 planBuyingApiRequestDto, string apiVersion);
    }

    public class PlanBuyingRequestLogClientAmazonS3 : IPlanBuyingRequestLogClient
    {
        private readonly string _BucketName;
        private readonly ILogToAmazonS3 _LogToAmazonS3;

        public PlanBuyingRequestLogClientAmazonS3(ILogToAmazonS3 requestLogClientAmazonS3)
        {
            _BucketName = BroadcastServiceSystemParameter.PricingRequestLogBucket;
            _LogToAmazonS3 = requestLogClientAmazonS3;
        }

        public void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto planBuyingApiRequestDto, string apiVersion)
        {
            var fileName = _GetFileName(planId, jobId, apiVersion);
            var keyName = _GetKeyName(fileName);

            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planBuyingApiRequestDto);            
        }

        public void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto_v3 planBuyingApiRequestDto, string apiVersion)
        {
            var fileName = _GetFileName(planId, jobId, apiVersion);
            var keyName = _GetKeyName(fileName);

            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planBuyingApiRequestDto);
        }

        private string _GetFileName(int planId, int jobId, string apiVersion)
        {
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString().ToLower();
            var fileName = PlanPricingBuyingFileHelper.GetRequestFileName(environment, planId, jobId, apiVersion, DateTime.Now);
            return fileName;
        }

        private string _GetKeyName(string fileName)
        {
            const string keyNamePrefix = "broadcast_buying_allocations";
            return $"{keyNamePrefix}/{fileName}.zip";
        }
    }
}
