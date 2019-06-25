using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.Cache;

namespace Services.Broadcast.IntegrationTests.Cache
{
    [TestFixture]
    public class BroadcastAudienceCacheIntegrationTests
    {
        private IBroadcastAudiencesCache _BroadcastAudiencesCache = IntegrationTestApplicationServiceFactory.GetApplicationService<IBroadcastAudiencesCache>();


        [Test]
        [TestCase("P35-49")]
        [TestCase("P 35-49")]
        [TestCase("A 35-49")]
        [TestCase("A35-49")]
        [UseReporter(typeof(DiffReporter))]
        public void GetBroadcastAudienceByCode_A3549(string audienceCode)
        {
            var result = _BroadcastAudiencesCache.GetBroadcastAudienceByCode(audienceCode);
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase("Adults 35-54")]
        [UseReporter(typeof(DiffReporter))]
        public void GetBroadcastAudienceByCode_A3554(string audienceCode)
        {
            var result = _BroadcastAudiencesCache.GetBroadcastAudienceByCode(audienceCode);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase("P18+")]
        [TestCase("P 18+")]
        [TestCase("A 18+")]
        [TestCase("A18+")]
        [UseReporter(typeof(DiffReporter))]
        public void GetBroadcastAudienceByCode_A18(string audienceCode)
        {
            var result = _BroadcastAudiencesCache.GetBroadcastAudienceByCode(audienceCode);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}
