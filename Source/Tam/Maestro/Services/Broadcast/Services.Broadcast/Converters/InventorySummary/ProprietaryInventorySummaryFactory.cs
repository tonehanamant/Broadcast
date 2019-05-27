using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class ProprietaryInventorySummaryFactory : BaseInventorySummaryAbstractFactory
    {
        public ProprietaryInventorySummaryFactory(IInventoryRepository inventoryRepository,
                                                  IInventorySummaryRepository inventorySummaryRepository,
                                                  IQuarterCalculationEngine quarterCalculationEngine)

            : base(inventoryRepository, inventorySummaryRepository, quarterCalculationEngine)
        {
        }

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail)
        {
            var ratesAvailableTuple = _GetRatesAvailableFromAndTo(inventorySource);
            var inventorySummaryManifests = GetInventorySummaryManifests(inventorySource, quarterDetail);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var totalDaypartsCodes = inventorySummaryManifests.Where(x => !string.IsNullOrWhiteSpace(x.DaypartCode)).GroupBy(x => x.DaypartCode).Count();
            var totalUnits = inventorySummaryManifests.Count();

            return new ProprietaryInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = GetHasRatesAvailable(inventorySummaryManifests),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalDaypartCodes = totalDaypartsCodes,
                TotalUnits = totalUnits,
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
