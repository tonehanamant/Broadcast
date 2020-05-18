﻿using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.IntegrationTests.Stubs;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class AdvertiserServiceUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsAdvertisersByAgencyId()
        {
            // Arrange
            const int expectedCallCount = 1;
            const int agencyId = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var trafficApiCache = new TrafficApiCache(trafficApiClient);
            
            var tc = new AdvertiserService(trafficApiCache);

            // Act
            var result = tc.GetAdvertisersByAgencyId(agencyId: agencyId);

            // Assert
            Assert.AreEqual(expectedCallCount, trafficApiClient.GetAdvertisersByAgencyIdCalledCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ThrowsException_WhenCanNotGetAdvertisersByAgencyId()
        {
            // Arrange
            const int expectedCallCount = 1;
            const int agencyId = 666;

            var trafficApiClient = new TrafficApiClientStub();
            var trafficApiCache = new TrafficApiCache(trafficApiClient);

            var tc = new AdvertiserService(trafficApiCache);

            // Act
            Assert.That(() => tc.GetAdvertisersByAgencyId(agencyId: agencyId), 
                Throws.TypeOf<Exception>().With.Message.EqualTo($"Cannot fetch advertisers data for agency {agencyId}."));

            // Assert
            Assert.AreEqual(expectedCallCount, trafficApiClient.GetAdvertisersByAgencyIdCalledCount);
        }
    }
}
