using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.IO;
using System.IO.Compression;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Clients
{
    public interface IPricingRequestLogClient
    {
        void SavePricingRequest(int planId, PlanPricingApiRequestDto planPricingApiRequestDto);

        void SavePricingRequest(int planId, PlanPricingApiRequestDto_v3 planPricingApiRequestDto);
    }

    public class PricingRequestLogClientAmazonS3 : IPricingRequestLogClient
    {
        private readonly string _BucketName;
        private readonly string _AccessKeyId;
        private readonly string _SecretAccessKey;
        private readonly RegionEndpoint _BucketRegion;

        public PricingRequestLogClientAmazonS3()
        {
            _BucketName = BroadcastServiceSystemParameter.PricingRequestLogBucket;
            _AccessKeyId = BroadcastServiceSystemParameter.PricingRequestLogAccessKeyId;
            _SecretAccessKey = EncryptionHelper.DecryptString(BroadcastServiceSystemParameter.PricingRequestLogEncryptedAccessKey, EncryptionHelper.EncryptionKey);
            _BucketRegion = RegionEndpoint.GetBySystemName(BroadcastServiceSystemParameter.PricingRequestLogBucketRegion);
        }

        public async void SavePricingRequest(int planId, PlanPricingApiRequestDto planPricingApiRequestDto)
        {
            _ValidateParameters();

            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);

            using (var client = new AmazonS3Client(_AccessKeyId, _SecretAccessKey, _BucketRegion))
            {
                using (var pricingApiRequestMemoryStream = Helpers.StreamHelper.CreateStreamFromString(SerializationHelper.ConvertToJson(planPricingApiRequestDto)))
                {
                    using (var fileMemoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(fileMemoryStream, ZipArchiveMode.Create, true))
                        {
                            var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                            using (var fileInArchiveStream = fileInArchive.Open())
                            {
                                pricingApiRequestMemoryStream.CopyTo(fileInArchiveStream);
                            }
                        }

                        var transferUtility = new TransferUtility(client);

                        await transferUtility.UploadAsync(fileMemoryStream, _BucketName, keyName);
                    }
                }
            }
        }

        public async void SavePricingRequest(int planId, PlanPricingApiRequestDto_v3 planPricingApiRequestDto)
        {
            _ValidateParameters();

            var fileName = _GetFileName(planId);
            var keyName = _GetKeyName(fileName);

            using (var client = new AmazonS3Client(_AccessKeyId, _SecretAccessKey, _BucketRegion))
            {
                using (var pricingApiRequestMemoryStream = Helpers.StreamHelper.CreateStreamFromString(SerializationHelper.ConvertToJson(planPricingApiRequestDto)))
                {
                    using (var fileMemoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(fileMemoryStream, ZipArchiveMode.Create, true))
                        {
                            var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                            using (var fileInArchiveStream = fileInArchive.Open())
                            {
                                pricingApiRequestMemoryStream.CopyTo(fileInArchiveStream);
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
                throw new Exception("Invalid parameters for pricing request serialization.");
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
