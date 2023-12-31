﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    public class MediaMonthCrunchCacheTest
    {

        [SetUp]
        public void SetUp()
        {
            var dmy = IntegrationTestApplicationServiceFactory.Instance;
        }
        [Ignore("Not certain why we are ignoring this...")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MediaMonthCruchCache_Get_Test()
        {

            var d = DaypartCache.Instance.GetDisplayDaypart(12);
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
