using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class AgencyCacheTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAgencies()
        {
            var trafficApiClient = new Mock<ITrafficApiClient>();
            var agencies = new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "AgencyOne" },
                new AgencyDto { Id = 2, Name = "AgencyTwo" },
                new AgencyDto { Id = 3, Name = "AgencyThree" }
            };
            trafficApiClient.Setup(s => s.GetFilteredAgencies("A"))
                .Returns(agencies);
            trafficApiClient.Setup(s => s.GetFilteredAgencies(It.IsNotIn(new[] {"A"})))
                .Returns(new List<AgencyDto>());

            var cache = new AgencyCache(trafficApiClient.Object);
            
            var serExpectedResult = IntegrationTestHelper.ConvertToJson(agencies);
            const int expectedCallCount = 68; // number of allowed chars

            var result = cache.GetAgencies();

            trafficApiClient.Verify(s => s.GetFilteredAgencies(It.IsAny<string>()), Times.Exactly(expectedCallCount));
            // TODO: Bring this back.  Fails on CD test run build.
            //Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            // TODO: When bring that back remove this 
            Assert.AreEqual(serExpectedResult, IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAgency()
        {
            const int agencyId = 3;

            var trafficApiClient = new Mock<ITrafficApiClient>();
            var agencies = new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "AgencyOne" },
                new AgencyDto { Id = 2, Name = "AgencyTwo" },
                new AgencyDto { Id = 3, Name = "AgencyThree" }
            };
            trafficApiClient.Setup(s => s.GetFilteredAgencies("A"))
                .Returns(agencies);
            trafficApiClient.Setup(s => s.GetFilteredAgencies(It.IsNotIn(new[] { "A" })))
                .Returns(new List<AgencyDto>());
            var cache = new AgencyCache(trafficApiClient.Object);

            var serExpectedResult = IntegrationTestHelper.ConvertToJson(agencies.First(a => a.Id == agencyId));

            var result = cache.GetAgency(agencyId);

            // TODO: Bring this back.  Fails on CD test run build.
            //Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            // TODO: When bring that back remove this 
            Assert.AreEqual(serExpectedResult, IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAgency_WithAgencyNotFound()
        {
            const int agencyId = 5;
            var trafficApiClient = new Mock<ITrafficApiClient>();
            var agencies = new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "AgencyOne" },
                new AgencyDto { Id = 2, Name = "AgencyTwo" },
                new AgencyDto { Id = 3, Name = "AgencyThree" }
            };
            trafficApiClient.Setup(s => s.GetFilteredAgencies("A"))
                .Returns(agencies);
            trafficApiClient.Setup(s => s.GetFilteredAgencies(It.IsNotIn(new[] { "A" })))
                .Returns(new List<AgencyDto>());
            var cache = new AgencyCache(trafficApiClient.Object);
            const int expectedCallCount = 68; // number of allowed chars

            Assert.Throws<Exception>(() => cache.GetAgency(agencyId), $"Agency with id '{agencyId}' not found.");
            trafficApiClient.Verify(s => s.GetFilteredAgencies(It.IsAny<string>()), Times.Exactly(expectedCallCount));
        }
    }
}