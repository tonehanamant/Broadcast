using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.Aggregates;
using Services.Broadcast.ApplicationServices;
using System;

namespace Services.Broadcast.IntegrationTests.Aggregates
{
    [TestFixture]
    [Category("short_running")]
    public class ScheduleAggregateIntegrationTests
    {
        private readonly IScheduleAggregateFactoryService _ScheduleAggregateFactoryService = 
            IntegrationTestApplicationServiceFactory.GetApplicationService<IScheduleAggregateFactoryService>();

        private readonly SchedulesAggregate _Sut;

        public ScheduleAggregateIntegrationTests()
        {
            _Sut = _ScheduleAggregateFactoryService.GetScheduleAggregate(69);
        }

        [Test]        
        public void GetOrderedSpots()
        {
            var actual = _Sut.GetOrderedSpots();
            Assert.AreEqual(actual, 392);
        }

        [Test]        
        public void GetDeliveredSpots()
        {
            var actual = _Sut.GetDeliveredSpots();
            Assert.AreEqual(actual, 100);
        }

        [Test]
        public void GetDeliveredImpressionsByAudience()
        {
            var actual = _Sut.GetDeliveredImpressionsByAudience(31);
            Assert.AreEqual(1883781.4000000001d, actual);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBroadcastPrePostData()
        {
            var actual = _Sut.GetBroadcastPrePostData(_Sut.GetBvsDetails());
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
        }
    }
}
