using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Helpers;
using System;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingRequestLogClient
    {
        void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto planBuyingApiRequestDto, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);

        void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto_v3 planBuyingApiRequestDto, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);
    }

    public class PlanBuyingRequestLogClientAmazonS3 : IPlanBuyingRequestLogClient
    {
        private Lazy<string> _BucketName;
        private readonly ILogToAmazonS3 _LogToAmazonS3;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsPipelineVariablesEnabled;

        public PlanBuyingRequestLogClientAmazonS3(ILogToAmazonS3 requestLogClientAmazonS3, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        {
          
            _BucketName = new Lazy<string>(_GetBucketName);
            _LogToAmazonS3 = requestLogClientAmazonS3;
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
        }

        public void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto planBuyingApiRequestDto, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            var fileName = _GetFileName(planId, jobId, apiVersion, spotAllocationModelMode);
            var keyName = _GetKeyName(fileName);

            _LogToAmazonS3.SaveRequest(_BucketName.Value, keyName, fileName, planBuyingApiRequestDto);            
        }

        public void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto_v3 planBuyingApiRequestDto, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            var fileName = _GetFileName(planId, jobId, apiVersion, spotAllocationModelMode);
            var keyName = _GetKeyName(fileName);

            _LogToAmazonS3.SaveRequest(_BucketName.Value, keyName, fileName, planBuyingApiRequestDto);
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
            const string keyNamePrefix = "broadcast_buying_allocations";
            return $"{keyNamePrefix}/{fileName}.zip";
        }
        private string _GetBucketName()
        {
            var bucketName = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PricingRequestLogBucket, "ds-api-logs") : BroadcastServiceSystemParameter.PricingRequestLogBucket;
            return bucketName;
        }
    }
}
