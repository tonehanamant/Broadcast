using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPricingRequestLogClient
    {
        /// <summary>
        /// Saves the pricing request.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="data">The data.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        void SavePricingRequest(int planId, int jobId, string data, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);
    }

    public class PricingRequestLogClientAmazonS3 : IPricingRequestLogClient
    {
        private Lazy<string> _BucketName;
        private readonly ILogToAmazonS3 _LogToAmazonS3;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public PricingRequestLogClientAmazonS3(ILogToAmazonS3 requestLogClientAmazonS3, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _BucketName = new Lazy<string>(_GetBucketName);
            _LogToAmazonS3 = requestLogClientAmazonS3;

        }

        public void SavePricingRequest(int planId, int jobId, string data, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            var fileName = _GetFileName(planId, jobId, apiVersion, spotAllocationModelMode);
            var keyName = _GetKeyName(fileName);

            var compressedData = CompressionHelper.GetGzipCompress(data);
            _LogToAmazonS3.UploadFile(_BucketName.Value, keyName, compressedData);
        }

        private string _GetFileName(int planId, int jobId, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString().ToLower();
            var fileName = PlanPricingBuyingFileHelper.GetRequestFileName(environment, planId, jobId, apiVersion, DateTime.Now, spotAllocationModelMode);
            return fileName;
        }

        private string _GetKeyName(string fileName)
        {
            const string keyNamePrefix = "broadcast_pricing_allocations";
            return $"{keyNamePrefix}/{fileName}.gz";
        }
        private string _GetBucketName()
        {
            var bucketName = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PricingRequestLogBucket, "ds-api-logs");
            return bucketName;
        }
    }
}
