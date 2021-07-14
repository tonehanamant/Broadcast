using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Common.Services.ApplicationServices;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface ILogToAmazonS3 : IApplicationService
    {
        void SaveRequest<T>(string bucketName, string keyName, string fileName, T data) where T : class;
    }

    public class LogToAmazonS3 : ILogToAmazonS3
    {
        private Lazy<string> _AccessKeyId;
        private Lazy<string> _SecretAccessKey;
        private Lazy<RegionEndpoint> _BucketRegion;       
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsPipelineVariablesEnabled;

        public LogToAmazonS3(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _AccessKeyId = new Lazy<string>(_GetAccessKeyId);
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));         
            _SecretAccessKey = new Lazy<string>(()=>EncryptionHelper.DecryptString(_GetSecretAccessKey(), EncryptionHelper.EncryptionKey));
            _BucketRegion = new Lazy<RegionEndpoint>(()=>RegionEndpoint.GetBySystemName(_GetBucketRegion()));
        }

        public async void SaveRequest<T>(string bucketName, string keyName, string fileName, T data) where T: class
        {
            _ValidateParameters(bucketName);

            using (var client = new AmazonS3Client(_AccessKeyId.Value, _SecretAccessKey.Value, _BucketRegion.Value))
            {
                using (var apiRequestMemoryStream = Helpers.BroadcastStreamHelper.CreateStreamFromString(SerializationHelper.ConvertToJson(data)))
                {
                    using (var fileMemoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(fileMemoryStream, ZipArchiveMode.Create, true))
                        {
                            var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                            using (var fileInArchiveStream = fileInArchive.Open())
                            {
                                apiRequestMemoryStream.CopyTo(fileInArchiveStream);
                            }
                        }

                        var transferUtility = new TransferUtility(client);

                        await transferUtility.UploadAsync(fileMemoryStream, bucketName, keyName);
                    }
                }
            }
        }

        private void _ValidateParameters(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName) ||
                string.IsNullOrEmpty(_AccessKeyId.Value) ||
                string.IsNullOrEmpty(_SecretAccessKey.Value) ||
                _BucketRegion.Value == null)
            {
                throw new Exception("Invalid Amazon parameters for request serialization.");
            }
        }
        private string _GetAccessKeyId()
        {
            var accessKeyId = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PricingRequestLogAccessKeyId, "AKIAQJ5IV4IZZV35MPAM") : BroadcastServiceSystemParameter.PricingRequestLogAccessKeyId;
            return accessKeyId;
        }
        private string _GetBucketRegion()
        {
            var bucketRegion = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PricingRequestLogBucketRegion, "us-east-1") : BroadcastServiceSystemParameter.PricingRequestLogBucketRegion;
            return bucketRegion;
        }
        private string _GetSecretAccessKey()
        {
            var secretAccessKey = _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PricingRequestLogEncryptedAccessKey, "8WBxyR8JMnMGgdIk6I2aJkurXbm2Hgkwz1SV/hTsOoUtZ6UYnfBGQvCMaqNnrxjh") : BroadcastServiceSystemParameter.PricingRequestLogEncryptedAccessKey;
            return secretAccessKey;
        }
    }
}
