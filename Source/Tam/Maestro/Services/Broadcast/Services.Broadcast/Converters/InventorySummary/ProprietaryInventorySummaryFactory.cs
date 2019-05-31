using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
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
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> manifests)
        {
            var ratesAvailableTuple = _GetRatesAvailableFromAndTo(inventorySource);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(manifests);
            var totalDaypartsCodes = manifests.Where(x => !string.IsNullOrWhiteSpace(x.DaypartCode)).GroupBy(x => x.DaypartCode).Count();
            var totalUnits = manifests.Count();

            return new ProprietaryInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = GetHasRatesAvailable(manifests),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(manifests),
                TotalStations = GetTotalStations(manifests),
                TotalDaypartCodes = totalDaypartsCodes,
                TotalUnits = totalUnits,
                HouseholdImpressions = GetHouseholdImpressions(manifests, householdAudienceId),
                InventoryPostingBooks = GetInventoryPostingBooks(inventorySummaryManifestFiles),
                LastUpdatedDate = GetLastUpdatedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = ratesAvailableTuple.Item1,
                RatesAvailableToQuarter = ratesAvailableTuple.Item2
            };
        }
    }
}
