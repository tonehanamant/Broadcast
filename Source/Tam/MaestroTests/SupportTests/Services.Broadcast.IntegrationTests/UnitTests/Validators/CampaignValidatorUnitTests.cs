using Moq;
using NUnit.Framework;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Cache;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Validators;
using Services.Broadcast.Clients;
using Common.Services.Extensions;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    [TestFixture]
    public class CampaignValidatorUnitTests
    {
        [Test]
        [TestCase("Campaign1", 1, 0, CampaignValidator.InvalidAgencyErrorMessage)]
        [TestCase("Campaign1", 1, 23, CampaignValidator.InvalidAgencyErrorMessage)]
        public void ValidateFailure_WhenAgencyIsInvalid(string campaignName, int advertiserId, int agencyId, string expectedMessage)
        {
            var item = new SaveCampaignDto
            {
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId
            };

            var trafficApiCacheMock = _GetTrafficApiCacheMock();

            var tc = new CampaignValidator(trafficApiCacheMock.Object);

            var caughtException = Assert.Throws<InvalidOperationException>(() => tc.Validate(item));

            trafficApiCacheMock.Verify(s => s.GetAgency(It.IsAny<int>()), Times.Once);
            Assert.AreEqual(expectedMessage, caughtException.Message);
        }

        [Test]
        [TestCase("Campaign1", 0, 1, CampaignValidator.InvalidAdvertiserErrorMessage)]
        [TestCase("Campaign1", 23, 1, CampaignValidator.InvalidAdvertiserErrorMessage)]
        public void ValidateFailure_WhenAdvertiserIsInvalid(string campaignName, int advertiserId, int agencyId, string expectedMessage)
        {
            var item = new SaveCampaignDto
            {
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId
            };

            var trafficApiCacheMock = _GetTrafficApiCacheMock();
            trafficApiCacheMock.Setup(x => x.GetAdvertiser(It.IsAny<int>())).Throws(new Exception());
            trafficApiCacheMock.Setup(x => x.GetAgency(It.IsAny<int>())).Returns(new AgencyDto());
            
            var tc = new CampaignValidator(trafficApiCacheMock.Object);

            var caughtException = Assert.Throws<InvalidOperationException>(() => tc.Validate(item));

            Assert.AreEqual(expectedMessage, caughtException.Message);
        }

        private Mock<ITrafficApiCache> _GetTrafficApiCacheMock()
        {
            var agencies = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, Name = "AgencyOne"},
                new AgencyDto{ Id = 2, Name = "AgencyTwo"},
                new AgencyDto{ Id = 3, Name = "AgencyThree"}
            };
            var getAdvertisersByAgencyIdReturn = new List<AdvertiserDto>
            {
                new AdvertiserDto {Id = 1, Name = "AdvertiserOne"},
                new AdvertiserDto {Id = 2, Name = "AdvertiserTwo"},
                new AdvertiserDto {Id = 3, Name = "AdvertiserThree"}
            };

            var trafficApiCacheMock = new Mock<ITrafficApiCache>();

            trafficApiCacheMock.Setup(s => s.GetAgency(It.IsAny<int>()))
                .Returns<int>((i) => agencies.Single(a => a.Id == i, "Agency not found"));

            trafficApiCacheMock.Setup(s => s.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(getAdvertisersByAgencyIdReturn);
            trafficApiCacheMock.Setup(s => s.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(getAdvertisersByAgencyIdReturn);

            return trafficApiCacheMock;
        }

        [Test]
        [TestCase("Campaign1", 1, 1)]
        public void ValidateSuccess(string campaignName, int advertiserId, int agencyId)
        {
            var item = new SaveCampaignDto
            {
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId
            };

            var trafficApiClientMock = _GetTrafficApiCacheMock();
            var tc = new CampaignValidator(trafficApiClientMock.Object);

            Assert.DoesNotThrow(() => tc.Validate(item));
        }

        [Test]
        [TestCase(0, true, "The campaign name is invalid, please provide a valid name")]
        [TestCase(255, false, null)]
        [TestCase(256, true, "Campaign name cannot be longer than 255 characters.")]
        public void ValidateCampaignNameBounds(int length, bool throws, string expectedMessage)
        {
            var campaignName = StringHelper.CreateStringOfLength(length);
            var item = new SaveCampaignDto
            {
                Name = campaignName,
                AdvertiserId = 1,
                AgencyId = 1
            };

            var trafficApiClientMock = _GetTrafficApiCacheMock();
            var tc = new CampaignValidator(trafficApiClientMock.Object);

            if (throws)
            {
                var caughtException = Assert.Throws<InvalidOperationException>(() => tc.Validate(item) );

                Assert.AreEqual(expectedMessage, caughtException.Message);
            }
            else
            {
                Assert.DoesNotThrow(() => tc.Validate(item));
            }
        }

        [Test]
        [TestCase(1024, false, null)]
        [TestCase(1025, true, "Campaign notes cannot be longer than 1024 characters")]
        public void ValidateCampaignNotesBounds(int length, bool throws, string expectedMessage)
        {
            var campaignNotes = StringHelper.CreateStringOfLength(length);
            var item = new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = campaignNotes
            };

            var trafficApiClientMock = _GetTrafficApiCacheMock();
            var agencyCache = _GetTrafficApiCacheMock();
            var tc = new CampaignValidator(trafficApiClientMock.Object);

            if (throws)
            {
                var caughtException = Assert.Throws<InvalidOperationException>(() => tc.Validate(item) );

                Assert.AreEqual(expectedMessage, caughtException.Message);
            }
            else
            {
                Assert.DoesNotThrow(() => tc.Validate(item));
            }
        }
    }
}
