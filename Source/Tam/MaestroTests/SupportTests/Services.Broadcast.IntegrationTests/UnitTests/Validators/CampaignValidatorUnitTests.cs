using Moq;
using NUnit.Framework;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Validators;
using Services.Broadcast.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    [TestFixture]
    public class CampaignValidatorUnitTests
    {
        [Test]
        [TestCase("", 1, 1, CampaignValidator.InvalidCampaignNameErrorMessage)]
        [TestCase("Campaign1", 0, 1, CampaignValidator.InvalidAdvertiserErrorMessage)]
        [TestCase("Campaign1", 23, 1, CampaignValidator.InvalidAdvertiserErrorMessage)]
        [TestCase("Campaign1", 1, 0, CampaignValidator.InvalidAgencyErrorMessage)]
        [TestCase("Campaign1", 1, 23, CampaignValidator.InvalidAgencyErrorMessage)]
        public void ValidateFailure(string campaignName, int advertiserId, int agencyId, string expectedMessage)
        {
            var item = new CampaignDto
            {
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId
            };

            var trafficApiClientMock = _GetTrafficApiClientMock();
            var tc = new CampaignValidator(trafficApiClientMock.Object);

            var caughtException = Assert.Throws<InvalidOperationException>(() => tc.Validate(item));

            Assert.AreEqual(expectedMessage, caughtException.Message);
        }

        private Mock<ITrafficApiClient> _GetTrafficApiClientMock()
        {
            var getAgenciesReturn = new List<AgencyDto>
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
            
            var trafficApiClientMock = new Mock<ITrafficApiClient>();

            trafficApiClientMock.Setup(s => s.GetAgencies()).Returns(getAgenciesReturn);
            trafficApiClientMock.Setup(s => s.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(getAdvertisersByAgencyIdReturn);

            return trafficApiClientMock;
        }

        [Test]
        [TestCase("Campaign1", 1, 1)]
        public void ValidateSuccess(string campaignName, int advertiserId, int agencyId)
        {
            var item = new CampaignDto
            {
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId
            };

            var trafficApiClientMock = _GetTrafficApiClientMock();
            var tc = new CampaignValidator(trafficApiClientMock.Object);

            Assert.DoesNotThrow(() => tc.Validate(item));
        }

        [Test]
        [TestCase(255, false, null)]
        [TestCase(256, true, "The campaign name is invalid, please provide a valid name")]
        public void ValidateCampaignNameBounds(int length, bool throws, string expectedMessage)
        {
            var campaignName = StringHelper.CreateStringOfLength(length);
            var item = new CampaignDto
            {
                Name = campaignName,
                AdvertiserId = 1,
                AgencyId = 1
            };

            var trafficApiClientMock = _GetTrafficApiClientMock();
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
        [TestCase(1025, true, "The campaign notes are invalid")]
        public void ValidateCampaignNotesBounds(int length, bool throws, string expectedMessage)
        {
            var campaignNotes = StringHelper.CreateStringOfLength(length);
            var item = new CampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = campaignNotes
            };

            var trafficApiClientMock = _GetTrafficApiClientMock();
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
