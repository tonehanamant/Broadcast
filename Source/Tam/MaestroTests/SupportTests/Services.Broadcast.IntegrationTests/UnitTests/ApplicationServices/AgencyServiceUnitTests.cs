﻿using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class AgencyServiceUnitTests
    {
        [Test]
        public void GetsAgencies()
        {
            // Arrange
            var trafficApiClientMock = new Mock<ITrafficApiClient>();
            var getAgenciesReturn = new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "AgencyOne" },
                new AgencyDto { Id = 2, Name = "AgencyTwo" },
                new AgencyDto { Id = 3, Name = "AgencyThree" }
            };
            var expectedResult = IntegrationTestHelper.ConvertToJson(getAgenciesReturn);

            trafficApiClientMock.Setup(x => x.GetAgencies()).Returns(getAgenciesReturn);

            var tc = new AgencyService(trafficApiClientMock.Object);

            // Act
            var result = tc.GetAgencies();

            // Assert
            trafficApiClientMock.Verify(x => x.GetAgencies(), Times.Once);
            Assert.AreEqual(expectedResult, IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ThrowsException_WhenCanNotGetAgencies()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetAgencies";

            var trafficApiClientMock = new Mock<ITrafficApiClient>();

            trafficApiClientMock
                .Setup(x => x.GetAgencies())
                .Callback(() => throw new Exception(expectedMessage));

            var tc = new AgencyService(trafficApiClientMock.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetAgencies());

            // Assert
            trafficApiClientMock.Verify(x => x.GetAgencies(), Times.Once);
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}