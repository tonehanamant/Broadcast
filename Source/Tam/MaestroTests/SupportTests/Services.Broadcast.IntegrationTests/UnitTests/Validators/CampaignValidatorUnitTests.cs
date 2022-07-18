using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Validators;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    [TestFixture]
    [Category("short_running")]
    public class CampaignValidatorUnitTests
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ValidateFailure_WhenAgencyIsInvalid(bool isAabEnabled)
        {
            // Arrange
            var item = new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 23,
                AgencyMasterId = Guid.NewGuid(),
                AdvertiserMasterId = Guid.NewGuid()
            };

            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            var aabEngine = new Mock<IAabEngine>();
            aabEngine.Setup(s => s.GetAgency(It.IsAny<Guid>()))
                .Throws<Exception>();

            var tc = new CampaignValidator(aabEngine.Object, featureToggleHelper.Object);

            // Act
            var caughtException = Assert.Throws<CadentException>(() => tc.Validate(item));

            // Assert
            aabEngine.Verify(s => s.GetAgency(It.IsAny<Guid>()), Times.Once);


            Assert.AreEqual(CampaignValidator.InvalidAgencyErrorMessage, caughtException.Message);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ValidateFailure_WhenAdvertiserIsInvalid(bool isAabEnabled)
        {
            // Arrange
            var item = new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 23,
                AgencyMasterId = Guid.NewGuid(),
                AdvertiserMasterId = Guid.NewGuid()
            };

            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            var aabEngine = new Mock<IAabEngine>();
            aabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Throws<Exception>();

            var tc = new CampaignValidator(aabEngine.Object, featureToggleHelper.Object);

            // Act
            var caughtException = Assert.Throws<CadentException>(() => tc.Validate(item));

            // Assert
            aabEngine.Verify(s => s.GetAgency(It.IsAny<Guid>()), Times.Once);

            Assert.AreEqual(CampaignValidator.InvalidAdvertiserErrorMessage, caughtException.Message);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ValidateSuccess(bool isAabEnabled)
        {
            // Arrange
            var item = new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 23,
                AgencyMasterId = Guid.NewGuid(),
                AdvertiserMasterId = Guid.NewGuid()
            };

            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            var aabEngine = new Mock<IAabEngine>();

            Guid? agencyMasterId = null;
            aabEngine.Setup(s => s.GetAgency(It.IsAny<Guid>()))
                .Callback<Guid>((i) => agencyMasterId = i)
                .Returns(new AgencyDto());

            Guid? advertiserMasterId = null;
            aabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Callback<Guid>((i) => advertiserMasterId = i)
                .Returns(new AdvertiserDto());

            var tc = new CampaignValidator(aabEngine.Object, featureToggleHelper.Object);

            // Act
            tc.Validate(item);

            // Assert
            aabEngine.Verify(s => s.GetAgency(It.IsAny<Guid>()), Times.Once);

            Assert.IsTrue(agencyMasterId.HasValue);

            aabEngine.Verify(s => s.GetAdvertiser(It.IsAny<Guid>()), Times.Once);

            Assert.IsTrue(advertiserMasterId.HasValue);


        }

        [Test]
        [TestCase(0, true, "The campaign name is invalid, please provide a valid name")]
        [TestCase(255, false, null)]
        [TestCase(256, true, "Campaign name cannot be longer than 255 characters.")]
        public void ValidateCampaignNameBounds(int length, bool throws, string expectedMessage)
        {
            // Arrange
            var campaignName = StringHelper.CreateStringOfLength(length);
            var item = new SaveCampaignDto
            {
                Name = campaignName,
                AdvertiserId = 1,
                AgencyId = 23,
                AgencyMasterId = Guid.NewGuid(),
                AdvertiserMasterId = Guid.NewGuid()
            };

            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            var aabEngine = new Mock<IAabEngine>();
            var tc = new CampaignValidator(aabEngine.Object, featureToggleHelper.Object);

            // Act
            if (throws)
            {
                var caughtException = Assert.Throws<CadentException>(() => tc.Validate(item));
                // Assert
                Assert.AreEqual(expectedMessage, caughtException.Message);
            }
            else
            {
                // Assert
                Assert.DoesNotThrow(() => tc.Validate(item));
            }
        }

        [Test]
        [TestCase(1024, false, null)]
        [TestCase(1025, true, "Campaign notes cannot be longer than 1024 characters")]
        public void ValidateCampaignNotesBounds(int length, bool throws, string expectedMessage)
        {
            // Arrange
            var campaignNotes = StringHelper.CreateStringOfLength(length);
            var item = new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 23,
                AgencyMasterId = Guid.NewGuid(),
                AdvertiserMasterId = Guid.NewGuid(),
                Notes = campaignNotes
            };

            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            var aabEngine = new Mock<IAabEngine>();
            var tc = new CampaignValidator(aabEngine.Object, featureToggleHelper.Object);

            // Act
            if (throws)
            {
                var caughtException = Assert.Throws<CadentException>(() => tc.Validate(item));

                // Assert
                Assert.AreEqual(expectedMessage, caughtException.Message);
            }
            else
            {
                // Assert
                Assert.DoesNotThrow(() => tc.Validate(item));
            }
        }
    }
}
