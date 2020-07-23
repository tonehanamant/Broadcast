using System.Collections.Generic;
using System.Linq;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class ProposalInventoryRepositoryIntegrationTests
    {
        [Ignore("Not certain why we are ignoring this...")]
        [Test]
        public void GetSortedFilteredInventoryDetails_FiltersOutMarkets()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>();

            var relevantMediaWeeks = new List<int> { 709, 710, 711, 712 };

            var spotLengthRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            var spotLengthMappings = spotLengthRepository.GetSpotLengths();

            var proposalMarketIds = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalMarketsCalculationEngine>().GetProposalMarketsList(17616, 1).Select(m => m.Id).ToList()
                .Take(10).ToList();
            var spotLengths = spotLengthMappings.Where(m => int.Parse(m.Display) <= 30).Select(m => m.Id);
            var result = sut.GetSortedFilteredInventoryDetails(relevantMediaWeeks, proposalMarketIds, spotLengths);
            foreach (var r in result)
            {
                foreach (var s in r.InventoryDetailSlots)
                {
                    foreach (var c in s.InventoryDetailSlotComponents)
                    {
                        if (!proposalMarketIds.Contains(c.MarketCode))
                            Assert.Fail();
                    }
                }
            }
        }
    }
}
