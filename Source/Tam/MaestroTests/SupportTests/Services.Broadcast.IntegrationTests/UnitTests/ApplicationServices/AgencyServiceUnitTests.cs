using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class AgencyServiceUnitTests
    {
        [Test]
        public void GetsAgencies()
        {
            // Arrange
            var aabEngine = new Mock<IAabEngine>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            var getAgenciesReturn = new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "AgencyOne" },
                new AgencyDto { Id = 2, Name = "AgencyTwo" },
                new AgencyDto { Id = 3, Name = "AgencyThree" }
            };
            var expectedReturnCount = getAgenciesReturn.Count;

            aabEngine.Setup(x => x.GetAgencies()).Returns(getAgenciesReturn);

            var tc = new AgencyService(aabEngine.Object,featureToggle.Object,configurationSettingsHelper.Object);

            // Act
            var result = tc.GetAgencies();

            // Assert
            aabEngine.Verify(x => x.GetAgencies(), Times.Once);
            Assert.AreEqual(expectedReturnCount, result.Count);
        }

        [Test]
        public void ThrowsException_WhenCanNotGetAgencies()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetAgencies";

            var aabEngine = new Mock<IAabEngine>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();

            aabEngine
                .Setup(x => x.GetAgencies())
                .Callback(() => throw new Exception(expectedMessage));

            var tc = new AgencyService(aabEngine.Object,featureToggle.Object,configurationSettingsHelper.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetAgencies());

            // Assert
            aabEngine.Verify(x => x.GetAgencies(), Times.Once);
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}
