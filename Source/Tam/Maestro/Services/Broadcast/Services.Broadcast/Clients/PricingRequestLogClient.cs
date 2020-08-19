using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.IO;
using System.IO.Compression;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPricingRequestLogClient
    {
        void SavePricingRequest(int planId, PlanPricingApiRequestDto planPricingApiRequestDto);

        void SavePricingRequest(int planId, PlanPricingApiRequestDto_v3 planPricingApiRequestDto);
    }

    public class PricingRequestLogClientAmazonS3 : IPricingRequestLogClient
    {
        private readonly string _BucketName;
        private readonly ILogToAmazonS3 _LogToAmazonS3;

        public PricingRequestLogClientAmazonS3(ILogToAmazonS3 requestLogClientAmazonS3)
        {
            _BucketName = BroadcastServiceSystemParameter.PricingRequestLogBucket;
            _LogToAmazonS3 = requestLogClientAmazonS3;
        }

        public void SavePricingRequest(int planId, PlanPricingApiRequestDto planPricingApiRequestDto)
        {
            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);
            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planPricingApiRequestDto);
        }

        public void SavePricingRequest(int planId, PlanPricingApiRequestDto_v3 planPricingApiRequestDto)
        {
            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);
            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planPricingApiRequestDto);
        }

        private string _GetFileName(int planId)
        {
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString().ToLower();
            var ticks = DateTime.Now.Ticks;
            return $"{environment}-request-{planId}-{ticks}.log";
        }

        private string _GetKeyName(string fileName)
        {
            const string keyNamePrefix = "broadcast_pricing_allocations";
            return $"{keyNamePrefix}/{fileName}.zip";
        }
    }
}
