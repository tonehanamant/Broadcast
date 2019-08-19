using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using Services.Broadcast.IntegrationTests.Helpers;

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

            if (throws)
            {
                var caughtException = Assert.Throws<InvalidOperationException>(() =>
                    tc.Validate(item)
                );

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

            if (throws)
            {
                var caughtException = Assert.Throws<InvalidOperationException>(() =>
                    tc.Validate(item)
                );

                Assert.AreEqual(expectedMessage, caughtException.Message);
            }
            else
            {
                Assert.DoesNotThrow(() => tc.Validate(item));
            }
        }

        #endregion // #region Validate
    }
}
