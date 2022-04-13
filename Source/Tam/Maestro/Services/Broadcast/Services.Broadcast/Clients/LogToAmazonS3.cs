using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Common.Services.ApplicationServices;
using Services.Broadcast.Helpers;
using System;
using System.IO;
using Tam.Maestro.Common;

namespace Services.Broadcast.Clients
{
    public interface ILogToAmazonS3 : IApplicationService
    {

        /// <summary>
        /// Uploads a file to S3
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="data">The data.</param>
        void UploadFile(string bucketName, string keyName, byte[] data);

        /// <summary>
        /// Downloads a file from S3
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        GetObjectResponse DownloadFile(string bucketName, string keyName, string fileName);
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

        public async void UploadFile(string bucketName, string keyName, byte[] data)
        {
            _ValidateParameters(bucketName);

            using (var client = new AmazonS3Client(_AccessKeyId.Value, _SecretAccessKey.Value, _BucketRegion.Value))
            {
                var transferUtility = new TransferUtility(client);

                try
                {
                    using (var ms = new MemoryStream(data))
                    {
                        ms.Position = 0;
                        ms.Write(data, 0, data.Length);
                        await transferUtility.UploadAsync(ms, bucketName, keyName);
                        ms.Close();
                    }
                }
                catch (Exception ex)
                {
                    _LogError("Exception caught attempting UploadAsync", ex);
                }
            }
        }

        public GetObjectResponse DownloadFile(string bucketName, string keyName, string fileName)
        {
            _ValidateParameters(bucketName);

            var client = new AmazonS3Client(_AccessKeyId.Value, _SecretAccessKey.Value, _BucketRegion.Value);
            var response = new GetObjectResponse();
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                response = client.GetObject(request);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when reading object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading object", e.Message);
            }
            return response;
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
            var accessKeyId = EncryptionHelper.DecryptString(encryptedAccessKeyId, EncryptionHelper.EncryptionKey);
            return accessKeyId;
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
