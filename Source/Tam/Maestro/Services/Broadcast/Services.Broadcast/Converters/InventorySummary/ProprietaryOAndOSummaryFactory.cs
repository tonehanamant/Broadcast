using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class ProprietaryOAndOSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        public ProprietaryOAndOSummaryFactory(IInventoryRepository inventoryRepository,
                                              IInventorySummaryRepository inventorySummaryRepository,
                                              IQuarterCalculationEngine quarterCalculationEngine,
                                              IProgramRepository programRepository,
                                              IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache) 
            : base(inventoryRepository, inventorySummaryRepository, quarterCalculationEngine, programRepository, mediaMonthAndWeekAggregateCache)
        {
        }

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource, int householdAudienceId, QuarterDetailDto quarterDetail, List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var allInventorySourceManifestWeeks = InventoryRepository.GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);

            var result = new ProprietaryOAndOInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = inventorySummaryManifests.Any(),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalPrograms = GetTotalPrograms(inventorySummaryManifests),
                TotalDaypartCodes = inventorySummaryManifests.GroupBy(x => x.DaypartCode).Count(),
                InventoryPostingBooks = GetInventoryPostingBooks(inventorySummaryManifestFiles),
                LastUpdatedDate = GetLastJobCompletedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2,
                HasInventoryGaps = HasInventoryGapsForDateRange(allInventorySourceManifestWeeks, quartersForInventoryAvailable)
            };

            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));
            RemoveWeeksNotInQuarter(manifests, quarterDetail);

            // This method should be used in the same way as for Barter in the O&O details story: PRI-7631
            // CPM is not used for now, it will be used in PRI-7631
            CalculateHouseHoldImpressionsAndCPM(manifests, householdAudienceId, out var hhImpressions, out var CPM);

            result.HouseholdImpressions = hhImpressions;

            return result;
        }
    }
}
