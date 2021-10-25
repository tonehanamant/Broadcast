using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Common.Services.ApplicationServices;
using Services.Broadcast.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using Tam.Maestro.Common;

namespace Services.Broadcast.Clients
{
    public interface ILogToAmazonS3 : IApplicationService
    {
        void SaveRequest<T>(string bucketName, string keyName, string fileName, T data) where T : class;
    }

    public class LogToAmazonS3 : BroadcastBaseClass, ILogToAmazonS3
    {
        private Lazy<string> _AccessKeyId;
        private Lazy<string> _SecretAccessKey;
        private Lazy<RegionEndpoint> _BucketRegion;       

        public LogToAmazonS3(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _AccessKeyId = new Lazy<string>(_GetAccessKeyId);
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
            var encryptedAccessKeyId =
                _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PricingRequestLogAccessKeyId);
            return encryptedAccessKeyId;
        }
        private string _GetBucketRegion()
        {
            var bucketRegion = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.PricingRequestLogBucketRegion, "us-east-1");
            return bucketRegion;
        }
        private string _GetSecretAccessKey()
        {
            var secretAccessKey = _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PricingRequestLogEncryptedAccessKey);
            return secretAccessKey;
        }
    }
}
