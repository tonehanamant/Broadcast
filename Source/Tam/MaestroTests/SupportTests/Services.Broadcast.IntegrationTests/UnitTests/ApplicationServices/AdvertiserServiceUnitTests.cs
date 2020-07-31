using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class AdvertiserServiceUnitTests
    {
        [SetUp]
        public void SetUp()
        {
            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsAdvertisersByAgencyId()
        {
            // Arrange
            const int expectedCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var trafficApiCache = new TrafficApiCache(trafficApiClient);

            var tc = new AdvertiserService(trafficApiCache);

            // Act
            var result = tc.GetAdvertisers();

            // Assert
            Assert.AreEqual(expectedCallCount, trafficApiClient.GetAdvertisersCalledCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}
