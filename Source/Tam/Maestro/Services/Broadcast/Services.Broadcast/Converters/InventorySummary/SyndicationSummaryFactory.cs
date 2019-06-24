using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class SyndicationSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        public SyndicationSummaryFactory(IInventoryRepository inventoryRepository, 
                                         IInventorySummaryRepository inventorySummaryRepository,
                                         IQuarterCalculationEngine quarterCalculationEngine,
                                         IProgramRepository programRepository,
                                         IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                         IMarketCoverageCache marketCoverageCache,
                                         IInventoryGapCalculationEngine inventoryGapCalculationEngine)
            : base(inventoryRepository, 
                   inventorySummaryRepository, 
                   quarterCalculationEngine, 
                   programRepository, 
                   mediaMonthAndWeekAggregateCache,
                   marketCoverageCache,
                   inventoryGapCalculationEngine)
        {
        }

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource, 
                                                                   int householdAudienceId, 
                                                                   QuarterDetailDto quarterDetail, 
                                                                   List<InventorySummaryManifestDto> manifests)
        {
            var allInventorySourceManifestWeeks = InventoryRepository.GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(manifests);
            var inventoryGaps = InventoryGapCalculationEngine.GetInventoryGaps(allInventorySourceManifestWeeks, quartersForInventoryAvailable, quarterDetail);

            return new SyndicationInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = manifests.Any(),
                Quarter = quarterDetail,
                LastUpdatedDate = GetLastJobCompletedDate(inventorySummaryManifestFiles),
                TotalPrograms = GetTotalPrograms(manifests),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2,
                HasInventoryGaps = inventoryGaps.Any(),
                InventoryGaps = inventoryGaps,
                Details = null //Syndication does not have details
            };
        }
    }
}
