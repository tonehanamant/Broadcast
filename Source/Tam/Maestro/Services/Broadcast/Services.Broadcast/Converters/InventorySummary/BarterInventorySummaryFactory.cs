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

        public BarterInventorySummaryFactory(IInventoryRepository inventoryRepository,
                                             IInventorySummaryRepository inventorySummaryRepository,
                                             IQuarterCalculationEngine quarterCalculationEngine,
                                             IProgramRepository programRepository,
                                             IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                             IMarketCoverageCache marketCoverageCache)

            : base(inventoryRepository, 
                   inventorySummaryRepository, 
                   quarterCalculationEngine, 
                   programRepository, 
                   mediaMonthAndWeekAggregateCache, 
                   marketCoverageCache)
        {
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
            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));

            GetLatestInventoryPostingBook(inventorySummaryManifestFiles, out var shareBook, out var hutBook);

            RemoveWeeksNotInQuarter(manifests, quarterDetail);

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
                LastUpdatedDate = GetLastJobCompletedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2,
                Details = _GetDetails(inventorySummaryManifests, manifests, householdAudienceId),
                ShareBook = shareBook,
                HutBook = hutBook
            };
            
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

                _CalculateHouseHoldImpressionsAndCPM(manifests, householdAudienceId, out var householdImpressions, out var cpm);

                result.Add(new BarterInventorySummaryDto.Detail
                {
                    Daypart = manifestsGrouping.Key,
                    TotalMarkets = marketCodes.Count(),
                    TotalCoverage = MarketCoverageCache.GetMarketCoverages(marketCodes).Sum(x => x.Value),
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

        private void _CalculateHouseHoldImpressionsAndCPM(
            IEnumerable<StationInventoryManifest> manifests,
            int householdAudienceId,
            out double? impressionsResult,
            out decimal? cpmResult)
        {
            impressionsResult = null;
            cpmResult = null;

            manifests = manifests.Where(x => _ManifestHasCalculatedHHImpressionsAndSpotCast(x, householdAudienceId));

            if (!manifests.Any())
                return;

            double impressionsTotal = 0;
            decimal spotCostTotal = 0;

            foreach (var manifest in manifests)
            {
                var hhImpressions = manifest.ManifestAudiences.Single(x => x.Audience.Id == householdAudienceId).Impressions.Value;
                var spotCost = manifest.ManifestRates.First(r => r.SpotLengthId == manifest.SpotLengthId).SpotCost;

                impressionsTotal += hhImpressions;
                spotCostTotal += spotCost;
            }

            impressionsResult = impressionsTotal;
            cpmResult = ProposalMath.CalculateCpm(spotCostTotal, impressionsTotal);
        }

        private bool _ManifestHasCalculatedHHImpressionsAndSpotCast(StationInventoryManifest manifest, int householdAudienceId)
        {
            return manifest.ManifestAudiences.Any(x => x.Audience.Id == householdAudienceId && x.Impressions.HasValue) &&
                   manifest.ManifestRates.Any(x => x.SpotLengthId == manifest.SpotLengthId);
        }
    }
}
