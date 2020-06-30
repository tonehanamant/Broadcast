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
        void SavePricingRequest(PlanPricingApiRequestDto planPricingApiRequestDto);
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

        public async void SavePricingRequest(PlanPricingApiRequestDto planPricingApiRequestDto)
        {
            _ValidateParameters();

            var keyName = _GetKeyName();
            var zipName = _GetZipName(keyName);

            using (var client = new AmazonS3Client(_AccessKeyId, _SecretAccessKey, _BucketRegion))
            {
                using (var memoryStream = Helpers.StreamHelper.CreateStreamFromString(SerializationHelper.ConvertToJson(planPricingApiRequestDto)))
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, false))
                    {
                        var fileInArchive = archive.CreateEntry(keyName, CompressionLevel.Optimal);

                        using (var fileToCompressStream = new MemoryStream())
                        {
                            fileToCompressStream.CopyTo(memoryStream);

                            var transferUtility = new TransferUtility(client);

                            await transferUtility.UploadAsync(fileToCompressStream, _BucketName, zipName);
                        }
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

        private string _GetKeyName()
        {
            const string keyNamePrefix = "broadcast-openmarket-allocations";
            var appSettings = new AppSettings();
            var environment = appSettings.Environment.ToString().ToLower();
            var ticks = DateTime.Now.Ticks;
            return $"{keyNamePrefix}/{environment}-request-{ticks}.log";
        }

        private string _GetZipName(string keyName)
        {
            return $"{keyName}.zip";
        }
    }
}
