using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.Entities;
using Services.Broadcast.IntegrationTests.Helpers;
using System;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class CampaignServiceIntegrationTests
    {
        private const string IntegrationTestUser = "IntegrationTestUser";
        private readonly DateTime CreatedDate = new DateTime(2019, 5, 14);
        private readonly ICampaignService _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllCampaignsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetAllCampaigns();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignById()
        {
            var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.GetCampaignById(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignValidCampaignTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                Assert.IsTrue(campaignId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignNameMaxLengthTest()
        {
            const int maxNameLength = 255;
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();
                campaign.Name = StringHelper.CreateStringOfLength(maxNameLength);

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                Assert.IsTrue(campaignId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignNotesMaxLengthTest()
        {
            const int maxNotesLength = 1024;
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();
                campaign.Notes = StringHelper.CreateStringOfLength(maxNotesLength);

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                Assert.IsTrue(campaignId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateCampaignTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();
                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);

                CampaignDto foundCampaign = _CampaignService.GetCampaignById(campaignId);

                foundCampaign.Name = "Updated name of Campaign1";
                int updatedCampaignId = _CampaignService.SaveCampaign(foundCampaign, IntegrationTestUser, CreatedDate);
                CampaignDto updatedCampaign = _CampaignService.GetCampaignById(campaignId);

                Assert.AreEqual(updatedCampaign.Id, campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignInvalidAdvertiserIdTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();

                // Invalid advertiser id.
                campaign.AdvertiserId = 0;

                var exception = Assert.Throws<InvalidOperationException>(() => _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignValidator.InvalidAdvertiserErrorMessage));
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("      ")]
        [TestCase("\t")]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignInvalidCampaignNameTest(string campaignName)
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();

                campaign.Name = campaignName;

                var exception = Assert.Throws<InvalidOperationException>(() => _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignValidator.InvalidCampaignNameErrorMessage));
            }
        }

        private CampaignDto _GetValidCampaign()
        {
            return new CampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(CampaignDto), "Id");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}
