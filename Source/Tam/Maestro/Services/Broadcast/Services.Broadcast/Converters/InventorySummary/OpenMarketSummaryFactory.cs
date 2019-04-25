using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class OpenMarketSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        private readonly IProgramRepository _ProgramRepository;

        public OpenMarketSummaryFactory(IInventoryRepository inventoryRepository,
                                        IInventorySummaryRepository inventorySummaryRepository,
                                        IQuarterCalculationEngine quarterCalculationEngine,
                                        IProgramRepository programRepository)

            : base(inventoryRepository, inventorySummaryRepository, quarterCalculationEngine)
        {
            _ProgramRepository = programRepository;
        }

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail)
        {
            var ratesAvailableTuple = _GetRatesAvailableFromAndTo(inventorySource);
            var inventorySummaryManifests = GetInventorySummaryManifests(inventorySource, quarterDetail);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var manifestIds = inventorySummaryManifests.Select(x => x.ManifestId).ToList();
            var totalPrograms = _ProgramRepository.GetUniqueProgramNamesByManifests(manifestIds)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .Count();

            return new OpenMarketInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = GetHasRatesAvailable(inventorySummaryManifests),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalPrograms = totalPrograms,
                HouseholdImpressions = GetHouseholdImpressions(inventorySummaryManifests, householdAudienceId),
                InventoryPostingBooks = GetInventoryPostingBooks(inventorySummaryManifestFiles),
                LastUpdatedDate = GetLastUpdatedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = ratesAvailableTuple.Item1,
                RatesAvailableToQuarter = ratesAvailableTuple.Item2
            };
        }
    }
}
