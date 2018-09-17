using ApprovalTests;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProposalProprietaryInventoryServiceTests
    {
        private readonly IProposalProprietaryInventoryService _ProposalProprietaryInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();
        private readonly ProposalDetailPricingGuidGridRequestDto _Request = new ProposalDetailPricingGuidGridRequestDto
        {
            ProposalId = 1,
            ProposalDetailId = 1
        };

        [Test]
        public void ReturnsProposalDetailPricingGuideGridDto()
        {
            var result = _ProposalProprietaryInventoryService.GetProposalDetailPricingGuideGridDto(_Request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProposalProprietaryInventoryService_AppliesProgramNameFilter()
        {
            var dto = _ProposalProprietaryInventoryService.GetProposalDetailPricingGuideGridDto(_Request);
            var expectedProgramNames = dto.DisplayFilter.ProgramNames
                                            .Where((name, index) => index == 2 || index == 3)
                                            .ToList();
            dto.Filter.ProgramNames = expectedProgramNames;

            var result = _ProposalProprietaryInventoryService.ApplyFilterOnOpenMarketPricingGuideGrid(dto);

            var resultHasProgramsOnlyWithExpectedNames = result.OpenMarkets
                                                                .SelectMany(m => m.Stations)
                                                                .SelectMany(s => s.Programs)
                                                                .Select(p => p.ProgramName)
                                                                .All(p => expectedProgramNames.Contains(p));

            Assert.IsTrue(resultHasProgramsOnlyWithExpectedNames);
        }
    }
}
