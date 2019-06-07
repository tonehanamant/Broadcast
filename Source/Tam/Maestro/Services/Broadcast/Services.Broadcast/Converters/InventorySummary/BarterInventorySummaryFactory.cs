using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class BarterInventorySummaryFactory : BaseInventorySummaryAbstractFactory
    {
        private readonly IMarketCoverageCache _MarketCoverageCache;

        public BarterInventorySummaryFactory(IInventoryRepository inventoryRepository,
                                             IInventorySummaryRepository inventorySummaryRepository,
                                             IQuarterCalculationEngine quarterCalculationEngine,
                                             IProgramRepository programRepository,
                                             IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                             IMarketCoverageCache marketCoverageCache)

            : base(inventoryRepository, inventorySummaryRepository, quarterCalculationEngine, programRepository, mediaMonthAndWeekAggregateCache)
        {
            _MarketCoverageCache = marketCoverageCache;
        }

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var allInventorySourceManifestWeeks = InventoryRepository.GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var totalDaypartsCodes = inventorySummaryManifests.GroupBy(x => x.DaypartCode).Count();

            var result = new BarterInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = inventorySummaryManifests.Any(),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalDaypartCodes = totalDaypartsCodes,
                TotalUnits = _GetTotalUnits(inventorySummaryManifests),
                InventoryPostingBooks = GetInventoryPostingBooks(inventorySummaryManifestFiles),
                LastUpdatedDate = GetLastJobCompletedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2
            };

            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));
            RemoveWeeksNotInQuarter(manifests, quarterDetail);
            result.Details = _GetDetails(inventorySummaryManifests, manifests, householdAudienceId);

            var detailsWithHHImpressions = result.Details.Where(x => x.HouseholdImpressions.HasValue);

            if (detailsWithHHImpressions.Any())
            {
                result.HouseholdImpressions = detailsWithHHImpressions.Sum(x => x.HouseholdImpressions);
            }

            return result;
        }

        private List<BarterInventorySummaryDto.Detail> _GetDetails(List<InventorySummaryManifestDto> allSummaryManifests, List<StationInventoryManifest> allManifests, int householdAudienceId)
        {
            var result = new List<BarterInventorySummaryDto.Detail>();
            var allManifestsGroupedByDaypart = allSummaryManifests.GroupBy(x => x.DaypartCode);

            foreach (var manifestsGrouping in allManifestsGroupedByDaypart)
            {
                var summaryManifests = manifestsGrouping.ToList();
                var summaryManifestIds = summaryManifests.Select(m => m.ManifestId);
                var manifests = allManifests.Where(x => summaryManifestIds.Contains(x.Id.Value));
                var marketCodes = summaryManifests.Where(x => x.MarketCode.HasValue).Select(x => Convert.ToInt32(x.MarketCode.Value)).Distinct();

                CalculateHouseHoldImpressionsAndCPM(manifests, householdAudienceId, out var householdImpressions, out var cpm);

                result.Add(new BarterInventorySummaryDto.Detail
                {
                    Daypart = manifestsGrouping.Key,
                    TotalMarkets = marketCodes.Count(),
                    TotalCoverage = _MarketCoverageCache.GetMarketCoverages(marketCodes).Sum(x => x.Value),
                    HouseholdImpressions = householdImpressions,
                    CPM = cpm,
                    TotalUnits = _GetTotalUnits(summaryManifests)
                });
            }

            return result;
        }
        
        private int _GetTotalUnits(List<InventorySummaryManifestDto> manifests)
        {
            return manifests.GroupBy(x => x.UnitName).Count();
        }
    }
}
