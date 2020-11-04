using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPricingRequestLogClient
    {
        void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto planPricingApiRequestDto);

        void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto_v3 planPricingApiRequestDto);
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

        public void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto planPricingApiRequestDto)
        {
            var fileName = _GetFileName(planId, jobId);
            var keyName = _GetKeyName(fileName);
            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planPricingApiRequestDto);
        }

        public void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto_v3 planPricingApiRequestDto)
        {
            var fileName = _GetFileName(planId, jobId);
            var keyName = _GetKeyName(fileName);
            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planPricingApiRequestDto);
        }

        private string _GetFileName(int planId, int jobId)
        {
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString().ToLower();
            var fileName = PlanPricingBuyingFileHelper.GetRequestFileName(environment, planId, jobId, DateTime.Now);
            return fileName;
        }

        private string _GetKeyName(string fileName)
        {
            const string keyNamePrefix = "broadcast_pricing_allocations";
            return $"{keyNamePrefix}/{fileName}.zip";
        }
    }
}
