using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Campaigns;
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

                _CampaignService.CreateCampaign(campaign, IntegrationTestUser, CreatedDate);
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

                var exception = Assert.Throws<InvalidOperationException>(() => _CampaignService.CreateCampaign(campaign, IntegrationTestUser, CreatedDate));

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

                var exception = Assert.Throws<InvalidOperationException>(() => _CampaignService.CreateCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignValidator.InvalidCampaignNameErrorMessage));
            }
        }

        private CampaignDto _GetValidCampaign()
        {
            return new CampaignDto
            {
                Name = "Test Campaign",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Hello. I'm a campaign.  Well, part of a test really, so you could say I am a 'test campaign' and therefore don't exist.  How dreary..."
            };
        }
    }
}
