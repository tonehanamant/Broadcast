using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class QuarterCalculationEngineTests
    {
        private readonly IQuarterCalculationEngine _Sut;

        public QuarterCalculationEngineTests()
        {
            _Sut = new QuarterCalculationEngine(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, new MediaMonthAndWeekAggregateCache(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory));
        }

        [Test]
        public void GetDatesForTimeframe_PreviousQuarter()
        {
            var dateRange = _Sut.GetDatesForTimeframe(RatesTimeframe.LASTQUARTER, new DateTime(2016, 11, 11));

            Assert.AreEqual(dateRange.Item1, new DateTime(2016, 6, 27));
            Assert.AreEqual(dateRange.Item2, new DateTime(2016, 9, 25, 23, 59, 59));
        }

        [Test]
        public void GetDatesForTimeframe_ThisQuarter()
        {
            var dateRange = _Sut.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, new DateTime(2016, 11, 11));

            Assert.AreEqual(dateRange.Item1, new DateTime(2016, 9, 26));
            Assert.AreEqual(dateRange.Item2, new DateTime(2016, 12, 25, 23, 59, 59));
        }

        [Test]
        public void GetQuarterRangeByDate_NextQuarter()
        {
            var quarterDetails = _Sut.GetQuarterRangeByDate(new DateTime(2016, 11, 11), 1);

            Assert.AreEqual(quarterDetails.StartDate, new DateTime(2016, 12, 26));
            Assert.AreEqual(quarterDetails.EndDate, new DateTime(2017, 3, 26, 23, 59, 59));
        }
    }
}
