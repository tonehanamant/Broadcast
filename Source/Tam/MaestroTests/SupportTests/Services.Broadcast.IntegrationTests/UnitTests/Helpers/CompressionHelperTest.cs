using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Helpers;
using ApprovalTests;
using Amazon.S3.Model;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class CompressionHelperTest
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetGzipCompress()
        {
            // Arrange
            var data = JsonConvert.SerializeObject(_GetRawResult(), Formatting.Indented);
            byte[] output;

            // Act
            output = CompressionHelper.GetGzipCompress(data);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(output));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetGzipUncompress()
        {
            // Arrange
            var data = CompressionHelper.GetGzipCompress(JsonConvert.SerializeObject(_GetRawResult()));

            var stream = new MemoryStream(data);
            var response = new GetObjectResponse()
            {
                ResponseStream = stream

            };

            // Act
            var content = CompressionHelper.GetGzipUncompress(response);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(content.ToString()));
        }

        private static PlanBuyingInventoryRawDto _GetRawResult()
        {
            var data = new PlanBuyingInventoryRawDto
            {
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                PostingType = PostingTypeEnum.NSI,
                AllocatedSpotsRaw = new List<PlanBuyingSpotRaw>
                {
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 11,
                        ContractMediaWeekId = 871,
                        InventoryMediaWeekId = 871,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                            {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}
                        }
                    },
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 11,
                        ContractMediaWeekId = 872,
                        InventoryMediaWeekId = 872,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                            {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    }
                },
                UnallocatedSpotsRaw = new List<PlanBuyingSpotRaw>
                {
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 10,
                        ContractMediaWeekId = 871,
                        InventoryMediaWeekId = 871,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                        {
                            new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}, // stay
                            new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0} // filtered - too big
                        }
                    },
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 10,
                        ContractMediaWeekId = 872,
                        InventoryMediaWeekId = 872,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                        {
                            new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}, // stay
                            new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0} // filtered - too big
                        }
                    }
                }
            };
            return data;
        }
    }
}
