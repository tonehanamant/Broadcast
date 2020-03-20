using NUnit.Framework;
using Services.Broadcast.Repositories;
using System;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    [Category("short_running")]
    public class MediaMonthAndWeekAggregateTests
    {
        private MediaMonthAndWeekAggregate _MediaMonthAndWeekAggregate;

        [SetUp]
        public void SetUp()
        {
            _MediaMonthAndWeekAggregate = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IMediaMonthAndWeekAggregateRepository>().GetMediaMonthAggregate();
        }

        [Test]
        public void GetMediaMonthForLastDayOfMediaMonthWithTime()
        {
            var result = _MediaMonthAndWeekAggregate.GetMediaMonthContainingDate(new DateTime(2016, 2, 28, 9, 30, 0));
            Assert.IsNotNull(result);
        }

    }
}
