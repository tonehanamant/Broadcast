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
        private readonly string _AccessKeyId;
        private readonly string _SecretAccessKey;
        private readonly RegionEndpoint _BucketRegion;

        public PlanBuyingRequestLogClientAmazonS3()
        {
            _BucketName = BroadcastServiceSystemParameter.PricingRequestLogBucket;
            _AccessKeyId = BroadcastServiceSystemParameter.PricingRequestLogAccessKeyId;
            _SecretAccessKey = EncryptionHelper.DecryptString(BroadcastServiceSystemParameter.PricingRequestLogEncryptedAccessKey, EncryptionHelper.EncryptionKey);
            _BucketRegion = RegionEndpoint.GetBySystemName(BroadcastServiceSystemParameter.PricingRequestLogBucketRegion);
        }

        public async void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto planBuyingApiRequestDto)
        {
            _ValidateParameters();

            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);

            using (var client = new AmazonS3Client(_AccessKeyId, _SecretAccessKey, _BucketRegion))
            {
                using (var buyingApiRequestMemoryStream = Helpers.StreamHelper.CreateStreamFromString(SerializationHelper.ConvertToJson(planBuyingApiRequestDto)))
                {
                    using (var fileMemoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(fileMemoryStream, ZipArchiveMode.Create, true))
                        {
                            var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                            using (var fileInArchiveStream = fileInArchive.Open())
                            {
                                buyingApiRequestMemoryStream.CopyTo(fileInArchiveStream);
                            }
                        }

                        var transferUtility = new TransferUtility(client);

                        await transferUtility.UploadAsync(fileMemoryStream, _BucketName, keyName);
                    }
                }
            }
        }

        public async void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto_v3 planBuyingApiRequestDto)
        {
            _ValidateParameters();

            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);

            using (var client = new AmazonS3Client(_AccessKeyId, _SecretAccessKey, _BucketRegion))
            {
                using (var buyingApiRequestMemoryStream = Helpers.StreamHelper.CreateStreamFromString(SerializationHelper.ConvertToJson(planBuyingApiRequestDto)))
                {
                    using (var fileMemoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(fileMemoryStream, ZipArchiveMode.Create, true))
                        {
                            var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                            using (var fileInArchiveStream = fileInArchive.Open())
                            {
                                buyingApiRequestMemoryStream.CopyTo(fileInArchiveStream);
                            }
                        }

                        var transferUtility = new TransferUtility(client);

                        await transferUtility.UploadAsync(fileMemoryStream, _BucketName, keyName);
                    }
                }
            }
        }

        private void _ValidateParameters()
        {
            if (string.IsNullOrEmpty(_BucketName) ||
                string.IsNullOrEmpty(_AccessKeyId) ||
                string.IsNullOrEmpty(_SecretAccessKey) ||
                _BucketRegion == null)
            {
                throw new Exception("Invalid parameters for buying request serialization.");
            }
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
            const string keyNamePrefix = "broadcast-openmarket-allocations";
            return $"{keyNamePrefix}/{fileName}.zip";
        }
    }
}
