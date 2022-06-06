using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    [TestFixture]
    public class SpotExceptionsIngestApiResponseExtensionsUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MergeIngestApiResponses()
        {
            var testItem = new IngestApiResponse
            {
                JobId = 12,
                SkipClearStaged = false,
                SkipIngestAndStaged = false,
                ReceivedCounts = new IngestResultCounts
                {
                    RecommendedPlansCount = 10,
                    OutOfSpecCount = 20,
                    UnpostedNoPlanCount = 30,
                    UnpostedNoReelRosterCount = 40
                },
                StagedCounts = new IngestResultCounts
                {
                    RecommendedPlansCount = 50,
                    OutOfSpecCount = 60,
                    UnpostedNoPlanCount = 70,
                    UnpostedNoReelRosterCount = 80
                },
                ProcessedCounts = new IngestResultCounts
                {
                    RecommendedPlansCount = 90,
                    OutOfSpecCount = 10,
                    UnpostedNoPlanCount = 20,
                    UnpostedNoReelRosterCount = 30
                }
            };
            var mergeItem = new IngestApiResponse
            {
                JobId = 24,
                SkipClearStaged = true,
                SkipIngestAndStaged = true,
                ReceivedCounts = new IngestResultCounts
                {
                    RecommendedPlansCount = 1,
                    OutOfSpecCount = 2,
                    UnpostedNoPlanCount = 3,
                    UnpostedNoReelRosterCount = 4
                },
                StagedCounts = new IngestResultCounts
                {
                    RecommendedPlansCount = 5,
                    OutOfSpecCount = 6,
                    UnpostedNoPlanCount = 7,
                    UnpostedNoReelRosterCount = 8
                },
                ProcessedCounts = new IngestResultCounts
                {
                    RecommendedPlansCount = 9,
                    OutOfSpecCount = 1,
                    UnpostedNoPlanCount = 2,
                    UnpostedNoReelRosterCount = 3
                }
            };

            testItem.MergeIngestApiResponses(mergeItem);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(testItem));
        }
    }
}
