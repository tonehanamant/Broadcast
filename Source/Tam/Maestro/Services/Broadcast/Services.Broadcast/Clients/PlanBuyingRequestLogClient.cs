using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.IO;
using System.IO.Compression;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingRequestLogClient
    {
        void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto planBuyingApiRequestDto);

        void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto_v3 planBuyingApiRequestDto);
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

        public void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto planBuyingApiRequestDto)
        {
            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);

            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planBuyingApiRequestDto);            
        }

        public void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto_v3 planBuyingApiRequestDto)
        {
            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);

            _LogToAmazonS3.SaveRequest(_BucketName, keyName, fileName, planBuyingApiRequestDto);
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
            const string keyNamePrefix = "broadcast_buying_allocations";
            return $"{keyNamePrefix}/{fileName}.zip";
        }
    }
}
