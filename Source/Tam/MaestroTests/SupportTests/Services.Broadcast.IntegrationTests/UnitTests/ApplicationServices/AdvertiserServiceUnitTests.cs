using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class AdvertiserServiceUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsAdvertisersByAgencyId()
        {
            // Arrange
            var trafficApiClientMock = new Mock<ITrafficApiClient>();
            var getAdvertisersByAgencyIdReturn = new List<AdvertiserDto>
            {
                new AdvertiserDto { Id = 1, Name = "AdvertiserOne", AgencyId = 1 },
                new AdvertiserDto { Id = 2, Name = "AdvertiserTwo", AgencyId = 1 },
                new AdvertiserDto { Id = 3, Name = "AdvertiserThree", AgencyId = 1 }
            };

            trafficApiClientMock.Setup(s => s.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(getAdvertisersByAgencyIdReturn);
            
            var tc = new AdvertiserService(trafficApiClientMock.Object);
            var serExpectedResult = IntegrationTestHelper.ConvertToJson(getAdvertisersByAgencyIdReturn);

            // Act
            var result = tc.GetAdvertisersByAgencyId(agencyId: 1);

            // Assert
            trafficApiClientMock.Verify(x => x.GetAdvertisersByAgencyId(1), Times.Once);
            // TODO: Bring this back.  Fails on CD test run build.
            //Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            // TODO: When bring that back remove this 
            Assert.AreEqual(serExpectedResult, IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ThrowsException_WhenCanNotGetAdvertisersByAgencyId()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetAdvertisersByAgencyId";

            var trafficApiClientMock = new Mock<ITrafficApiClient>();

            trafficApiClientMock
                .Setup(s => s.GetAdvertisersByAgencyId(It.IsAny<int>()))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = new AdvertiserService(trafficApiClientMock.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetAdvertisersByAgencyId(agencyId: 1));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}
