using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
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
        public void CreateCampaignValidCampaignTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();            

                var createdCampaign = _CampaignService.CreateCampaign(campaign, IntegrationTestUser, CreatedDate);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(createdCampaign, _GetJsonSettings()));
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

                var exception = Assert.Throws<Exception>(() => _CampaignService.CreateCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignService.InvalidAdvertiserErrorMessage));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignInvalidStartAndEndDateTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();

                // End date before start date.
                campaign.EndDate = new DateTime(2019, 5, 13);

                var exception = Assert.Throws<Exception>(() => _CampaignService.CreateCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignService.InvalidDatesErrorMessage));
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("      ")]
        [TestCase("\t")]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignInvalidCampaignNameTest(string campaignName)
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();

                campaign.Name = campaignName;

                var exception = Assert.Throws<Exception>(() => _CampaignService.CreateCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignService.InvalidCampaignNameErrorMessage));
            }
        }

        private CampaignDto _GetValidCampaign()
        {
            return new CampaignDto
            {
                Name = "Test Campaign",
                AdvertiserId = 37674,
                AgencyId = 1,
                StartDate = new DateTime(2019, 5, 14),
                EndDate = new DateTime(2019, 5, 18),
                Budget = 543.12m,
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
