using NUnit.Framework;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Newtonsoft.Json;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    public class MediaMonthCrunchCacheTest
    {

        [SetUp]
        public void SetUp()
        {
            //var dmy = IntegrationTestApplicationServiceFactory.Instance;
        }
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MediaMonthCruchCache_Get_Test()
        {

            MediaMonthCrunchCache.Instance.GetMediaMonthCrunchStatuses();
            var status = MediaMonthCrunchCache.Instance.GetMediaMonthCrunchStatuses();

            var jsonResolver = new IgnorableSerializerContractResolver();

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(status, jsonSettings));

        }
    }
}
