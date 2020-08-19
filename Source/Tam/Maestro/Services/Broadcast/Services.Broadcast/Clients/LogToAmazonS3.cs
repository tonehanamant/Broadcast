using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Common.Services.ApplicationServices;
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
        private readonly string _AccessKeyId;
        private readonly string _SecretAccessKey;
        private readonly RegionEndpoint _BucketRegion;

        public LogToAmazonS3()
        {
            _AccessKeyId = BroadcastServiceSystemParameter.PricingRequestLogAccessKeyId;
            _SecretAccessKey = EncryptionHelper.DecryptString(BroadcastServiceSystemParameter.PricingRequestLogEncryptedAccessKey, EncryptionHelper.EncryptionKey);
            _BucketRegion = RegionEndpoint.GetBySystemName(BroadcastServiceSystemParameter.PricingRequestLogBucketRegion);
        }

        public async void SaveRequest<T>(string bucketName, string keyName, string fileName, T data) where T: class
        {
            _ValidateParameters(bucketName);

            using (var client = new AmazonS3Client(_AccessKeyId, _SecretAccessKey, _BucketRegion))
            {
                using (var apiRequestMemoryStream = Helpers.StreamHelper.CreateStreamFromString(SerializationHelper.ConvertToJson(data)))
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
                string.IsNullOrEmpty(_AccessKeyId) ||
                string.IsNullOrEmpty(_SecretAccessKey) ||
                _BucketRegion == null)
            {
                throw new Exception("Invalid Amazon parameters for request serialization.");
            }
        }

    }
}
