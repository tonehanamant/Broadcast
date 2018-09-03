using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProposalPricingGuideServiceTests
    {
        private readonly IProposalPricingGuideService _ProposalPricingGuideService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalPricingGuideService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanGetOpenMarketPricingGuide()
        {            
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var pricingGuideOpenMarketDto = _ProposalPricingGuideService.GetPricingGuideOpenMarketInventory(proposalDetailId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto));
            }
        }
    }
}
