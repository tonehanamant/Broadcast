using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using ApprovalTests;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class AgencyServiceUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsAgencies()
        {
            // Arrange
            var trafficApiCache = new Mock<ITrafficApiCache>();
            var getAgenciesReturn = new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "AgencyOne" },
                new AgencyDto { Id = 2, Name = "AgencyTwo" },
                new AgencyDto { Id = 3, Name = "AgencyThree" }
            };

            trafficApiCache.Setup(x => x.GetAgencies()).Returns(getAgenciesReturn);

            var tc = new AgencyService(trafficApiCache.Object);

            // Act
            var result = tc.GetAgencies();

            // Assert
            trafficApiCache.Verify(x => x.GetAgencies(), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ThrowsException_WhenCanNotGetAgencies()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetAgencies";

            var trafficApiCache = new Mock<ITrafficApiCache>();

            trafficApiCache
                .Setup(x => x.GetAgencies())
                .Callback(() => throw new Exception(expectedMessage));

            var tc = new AgencyService(trafficApiCache.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetAgencies());

            // Assert
            trafficApiCache.Verify(x => x.GetAgencies(), Times.Once);
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}
