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

        [Test]
        public void ProposalProprietaryInventoryService_AppliesMarketsFilter()
        {
            var dto = _ProposalProprietaryInventoryService.GetProposalDetailPricingGuideGridDto(_Request);
            var expectedMarketIds = dto.DisplayFilter.Markets
                                            .Where((x, index) => index == 0)
                                            .Select(m => m.Id)
                                            .ToList();
            dto.Filter.Markets = expectedMarketIds;

            var result = _ProposalProprietaryInventoryService.ApplyFilterOnOpenMarketPricingGuideGrid(dto);

            var resultHasMarketsOnlyWithExpectedIds = result.OpenMarkets.All(m => expectedMarketIds.Contains(m.MarketId));

            Assert.IsTrue(resultHasMarketsOnlyWithExpectedIds);
        }

        [Test]
        public void ProposalProprietaryInventoryService_AppliesAffiliationsFilter()
        {
            var dto = _ProposalProprietaryInventoryService.GetProposalDetailPricingGuideGridDto(_Request);
            var expectedAffiliations = dto.DisplayFilter.Affiliations
                                            .Where((x, index) => index == 0 || index == 1)
                                            .Select(a => a)
                                            .ToList();
            dto.Filter.Affiliations = expectedAffiliations;

            var result = _ProposalProprietaryInventoryService.ApplyFilterOnOpenMarketPricingGuideGrid(dto);

            var resultHasStationsOnlyWithExpectedAffiliations = result.OpenMarkets
                                                            .SelectMany(m => m.Stations)
                                                            .All(s => expectedAffiliations.Contains(s.Affiliation));

            Assert.IsTrue(resultHasStationsOnlyWithExpectedAffiliations);
        }
    }
}
