using Amazon.S3.Model;
using Common.Services.Repositories;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingRequestLogClient
    {

        /// <summary>
        /// Saves the buying request.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="data">The data.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        void SaveBuyingRequest(int planId, int jobId, string data, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Saves the buying raw inventory.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        string SaveBuyingRawInventory(int planId, int jobId, string data);

        /// <summary>
        /// Gets the buying raw inventory.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns></returns>
        string GetBuyingRawInventory(int jobId);
    }

    public class PlanBuyingRequestLogClientAmazonS3 : IPlanBuyingRequestLogClient
    {
        private Lazy<string> _BucketName;
        private readonly ILogToAmazonS3 _LogToAmazonS3;
        private readonly IPlanBuyingRepository _PlanBuyingRepository;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public PlanBuyingRequestLogClientAmazonS3(ILogToAmazonS3 requestLogClientAmazonS3,
            IConfigurationSettingsHelper configurationSettingsHelper,
            IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
          
            _BucketName = new Lazy<string>(_GetBucketName);
            _LogToAmazonS3 = requestLogClientAmazonS3;
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _PlanBuyingRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
        }

        public void SaveBuyingRequest(int planId, int jobId, string data, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            var fileName = _GetFileName(planId, jobId, apiVersion, spotAllocationModelMode);
            var keyName = _GetKeyName(fileName);

            var compressedData = CompressionHelper.GetGzipCompress(data);
            _LogToAmazonS3.UploadFile(_BucketName.Value, keyName, compressedData);            
        }

        public string SaveBuyingRawInventory(int planId, int jobId, string data)
        {
            var fileName = _GetFileName(planId, jobId);
            var keyName = _GetKeyName(fileName);

            var compressedData = CompressionHelper.GetGzipCompress(data);
            _LogToAmazonS3.UploadFile(_BucketName.Value, keyName, compressedData);

            return fileName;
        }

        public string GetBuyingRawInventory(int jobId)
        {
            var fileName = _GetFileName(jobId);
            var keyName = _GetKeyName(fileName);

            var response = _LogToAmazonS3.DownloadFile(_BucketName.Value, keyName, fileName);
            var zippedRequest = CompressionHelper.GetGzipUncompress(response);

            return zippedRequest;
        }

        private string _GetFileName(int planId, int jobId, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString().ToLower();
            var fileName = PlanPricingBuyingFileHelper.GetRequestFileName(environment, planId, jobId, apiVersion, DateTime.Now, spotAllocationModelMode);
            return fileName;
        }

        private string _GetFileName(int jobId)
        {
            var fileName = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
            return fileName.InventoryRawFile;
        }

        private string _GetFileName(int planId, int jobId)
        {
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString().ToLower();
            var fileName = PlanPricingBuyingFileHelper.GetFileName(environment, planId, jobId, DateTime.Now);
            return fileName;
        }

        private string _GetKeyName(string fileName)
        {
            const string keyNamePrefix = "broadcast_buying_allocations";
            return $"{keyNamePrefix}/{fileName}.gz";
        }
        private string _GetBucketName()
        {
            var bucketName = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PricingRequestLogBucket, "ds-api-logs");
            return bucketName;
        }
    }
}
