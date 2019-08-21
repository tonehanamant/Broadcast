using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Campaigns
{
    public class CampaignServiceDataUnitTests
    {
        #region Constructor

        [Test]
        public void ConstructorTest()
        {
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();

            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object);

            Assert.NotNull(tc);
        }

        #endregion // #region Constructor

        #region GetAdvertisers

        [Test]
        public void GetAdvertisersWithValidResult()
        {
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object);

            var result = tc.GetAdvertisers();

            Assert.IsNotNull(result);
            // this reflects the mocked data until the Sms Api call is implemented.
            Assert.AreEqual(3, result.Count);
        }

        // TODO: Add exception test when the Sms Api call is implemented.

        #endregion // #region GetAdvertisers

        #region GetAgencies

        [Test]
        public void GetAgenciesWithValidResult()
        {
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object);

            var result = tc.GetAgencies();

            Assert.IsNotNull(result);
            // this reflects the mocked data until the Sms Api call is implemented.
            Assert.AreEqual(3, result.Count);
        }

        // TODO: Add exception test when the Sms Api call is implemented.

        #endregion //#region GetAgencies

        #region GetAllCampaigns

        [Test]
        public void GetAllCampaignsWithValidResult()
        {
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var callCount = 0;
            var callReturn = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = 1, Name = "Campaign1", AdvertiserId = 1, AgencyId = 1, Notes = "Notes for CampaignOne.",
                    ModifiedBy = "TestUser", ModifiedDate = new DateTime(2107, 10, 17)
                },
                new CampaignDto
                {
                    Id = 2, Name = "Campaign2", AdvertiserId = 2, AgencyId = 1, Notes = "Notes for CampaignTwo.",
                    ModifiedBy = "TestUser", ModifiedDate = new DateTime(2107, 10, 17)
                },
                new CampaignDto
                {
                    Id = 3, Name = "Campaign3", AdvertiserId = 3, AgencyId = 1, Notes = "Notes for CampaignThree.",
                    ModifiedBy = "TestUser", ModifiedDate = new DateTime(2107, 10, 17)
                }
            };
            campaignRepository.Setup(s => s.GetAllCampaigns())
                .Callback(() => callCount++)
                .Returns(callReturn);
            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object);;

            var result = tc.GetAllCampaigns();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(callReturn.Count, result.Count);
        }

        [Test]
        public void GetAllCampaignsWithException()
        {
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var callCount = 0;
            var callReturn = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = 1, Name = "Campaign1", AdvertiserId = 1, AgencyId = 1, Notes = "Notes for CampaignOne.",
                    ModifiedBy = "TestUser", ModifiedDate = new DateTime(2107, 10, 17)
                },
                new CampaignDto
                {
                    Id = 2, Name = "Campaign2", AdvertiserId = 2, AgencyId = 1, Notes = "Notes for CampaignTwo.",
                    ModifiedBy = "TestUser", ModifiedDate = new DateTime(2107, 10, 17)
                },
                new CampaignDto
                {
                    Id = 3, Name = "Campaign3", AdvertiserId = 3, AgencyId = 1, Notes = "Notes for CampaignThree.",
                    ModifiedBy = "TestUser", ModifiedDate = new DateTime(2107, 10, 17)
                }
            };
            campaignRepository.Setup(s => s.GetAllCampaigns())
                .Callback(() =>
                {
                    callCount++;
                    throw new Exception("This is a test exception thrown from GetAllCampaigns.");
                })
                .Returns(callReturn);
            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object); ;

            var caught = Assert.Throws<Exception>(() => tc.GetAllCampaigns());

            Assert.IsNotNull(caught);
            Assert.AreEqual(1, callCount);
            Assert.IsTrue(caught.Message.Contains("This is a test exception thrown from GetAllCampaigns."));
        }

        #endregion // #region GetAllCampaigns

        #region CreateCampaign

        [Test]
        public void SaveCampaignWithValidResult()
        {
            var campaignToSave = new CampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var username = "TestUser";
            var now = new DateTime(2017, 10, 17);
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var callCount = 0;
            campaignRepository.Setup(s =>
                    s.CreateCampaign(It.IsAny<CampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => callCount++);
            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object); ;

            tc.SaveCampaign(campaignToSave, username, now);

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void SaveCampaignWithException()
        {
            var campaignToSave = new CampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var username = "TestUser";
            var now = new DateTime(2017, 10, 17);
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var callCount = 0;
            campaignRepository.Setup(s =>
                    s.CreateCampaign(It.IsAny<CampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    callCount++;
                    throw new Exception("This is a test exception thrown from CreateCampaign.");
                });
                
            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object); ;

            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaignToSave, username, now));

            Assert.IsNotNull(caught);
            Assert.AreEqual(1, callCount);
            Assert.IsTrue(caught.Message.Contains("This is a test exception thrown from CreateCampaign."));
        }
        #endregion // #region CreateCampaign

        #region UpdateCampaign
        [Test]
        public void SaveCampaignUpdateWithValidResult()
        {
            var campaignToSave = new CampaignDto
            {
                Id = 1,
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var username = "TestUser";
            var now = new DateTime(2017, 10, 17);
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var callCount = 0;
            campaignRepository.Setup(s =>
                    s.UpdateCampaign(It.IsAny<CampaignDto>()))
                .Callback(() => callCount++);
            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object); ;

            tc.SaveCampaign(campaignToSave, username, now);

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void SaveCampaignUpdateWithException()
        {
            var campaignToSave = new CampaignDto
            {
                Id = 1,
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var username = "TestUser";
            var now = new DateTime(2017, 10, 17);
            var smsClient = new Mock<ISMSClient>();
            var campaignRepository = new Mock<ICampaignRepository>();
            var callCount = 0;
            campaignRepository.Setup(s =>
                    s.UpdateCampaign(It.IsAny<CampaignDto>()))
                .Callback(() =>
                {
                    callCount++;
                    throw new Exception("This is a test exception thrown from UpdateCampaign.");
                });

            var tc = new CampaignServiceData(campaignRepository.Object, smsClient.Object); ;

            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaignToSave, username, now));

            Assert.IsNotNull(caught);
            Assert.AreEqual(1, callCount);
            Assert.IsTrue(caught.Message.Contains("This is a test exception thrown from UpdateCampaign."));
        }
        #endregion // #region UpdateCampaign
    }
}