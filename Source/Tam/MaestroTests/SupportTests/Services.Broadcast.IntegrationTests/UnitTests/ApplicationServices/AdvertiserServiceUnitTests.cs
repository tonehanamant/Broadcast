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
            var expectedResult = IntegrationTestHelper.ConvertToJson(getAdvertisersByAgencyIdReturn);

            trafficApiClientMock.Setup(s => s.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(getAdvertisersByAgencyIdReturn);
            
            var tc = new AdvertiserService(trafficApiClientMock.Object);

            // Act
            var result = tc.GetAdvertisersByAgencyId(agencyId: 1);

            // Assert
            trafficApiClientMock.Verify(x => x.GetAdvertisersByAgencyId(1), Times.Once);
            Assert.AreEqual(expectedResult, IntegrationTestHelper.ConvertToJson(result));
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
