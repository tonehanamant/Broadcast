using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class MyEventsReportNamingEngineTests
    {
        private IMyEventsReportNamingEngine _MyEventsReportNamingEngine =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IMyEventsReportNamingEngine>();

        [Test]
        public void GetDefaultMyEventsReportNameTestExpectedResultAndSize()
        {
            var expectedResult = "Test Adve ABC 30 06-01-18";
            var expectedLength = 25;
            var daypartCode = "ABC";
            var spotLength = 30;
            var weekStart = new DateTime(2018, 06, 01);
            var advertiser = "Test Advertiser 9";

            var result = _MyEventsReportNamingEngine.GetDefaultMyEventsReportName(daypartCode, spotLength, weekStart, advertiser);

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedLength, result.Length);
        }

        [Test]
        public void GetDefaultMyEventsReportNameTestSmallResultAndSize()
        {
            var expectedResult = "CNN ABC 30 06-01-18";
            var expectedLength = 19;
            var daypartCode = "ABC";
            var spotLength = 30;
            var weekStart = new DateTime(2018, 06, 01);
            var advertiser = "CNN";

            var result = _MyEventsReportNamingEngine.GetDefaultMyEventsReportName(daypartCode, spotLength, weekStart, advertiser);

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedLength, result.Length);
        }
    }
}
