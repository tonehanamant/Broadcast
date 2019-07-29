using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Campaigns
{
    [TestFixture]
    public class CampaignValidatorUnitTests
    {
        #region Constructor

        [Test]
        public void ConstructorTest()
        {
            var data = new Mock<ICampaignServiceData>();
            
            var tc = new CampaignValidator(data.Object);

            Assert.IsNotNull(tc);
        }

        #endregion // #region Constructor

        #region Validate

        [Test]
        [TestCase("", 1, 1, CampaignValidator.InvalidCampaignNameErrorMessage)]
        [TestCase("Campaign1", 0, 1, CampaignValidator.InvalidAdvertiserErrorMessage)]
        [TestCase("Campaign1", 23, 1, CampaignValidator.InvalidAdvertiserErrorMessage)]
        [TestCase("Campaign1", 1, 0, CampaignValidator.InvalidAgencyErrorMessage)]
        [TestCase("Campaign1", 1, 23, CampaignValidator.InvalidAgencyErrorMessage)]
        public void ValidateFailure(string campaignName, int advertiserId, int agencyId, 
            string expectedMessage)
        {
            var item = new Entities.CampaignDto
            {
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId
            };
            var data = new Mock<ICampaignServiceData>();
            var getAgenciesReturn = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, Name = "AgencyOne"},
                new AgencyDto{ Id = 2, Name = "AgencyTwo"},
                new AgencyDto{ Id = 3, Name = "AgencyThree"}

            };
            data.Setup(s => s.GetAgencies())
                .Returns(getAgenciesReturn);
            var getAdvertisersReturn = new List<AdvertiserDto>
            {
                new AdvertiserDto {Id = 1, Name = "AdvertiserOne"},
                new AdvertiserDto {Id = 2, Name = "AdvertiserTwo"},
                new AdvertiserDto {Id = 3, Name = "AdvertiserThree"}
            };
            data.Setup(s => s.GetAdvertisers())
                .Returns(getAdvertisersReturn);
            var tc = new CampaignValidator(data.Object);

            var caughtException = Assert.Throws<InvalidOperationException>(() =>
                tc.Validate(item)
            );

            Assert.AreEqual(expectedMessage, caughtException.Message);
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
            var data = new Mock<ICampaignServiceData>();
            var getAgenciesReturn = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, Name = "AgencyOne"},
                new AgencyDto{ Id = 2, Name = "AgencyTwo"},
                new AgencyDto{ Id = 3, Name = "AgencyThree"}

            };
            data.Setup(s => s.GetAgencies())
                .Returns(getAgenciesReturn);
            var getAdvertisersReturn = new List<AdvertiserDto>
            {
                new AdvertiserDto {Id = 1, Name = "AdvertiserOne"},
                new AdvertiserDto {Id = 2, Name = "AdvertiserTwo"},
                new AdvertiserDto {Id = 3, Name = "AdvertiserThree"}
            };
            data.Setup(s => s.GetAdvertisers())
                .Returns(getAdvertisersReturn);
            var tc = new CampaignValidator(data.Object);

            Assert.DoesNotThrow(() => tc.Validate(item));
        }

        #endregion // #region Validate
    }
}
