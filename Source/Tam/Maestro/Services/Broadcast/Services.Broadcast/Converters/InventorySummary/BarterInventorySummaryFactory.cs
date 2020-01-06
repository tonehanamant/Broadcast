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

        public override InventoryQuarterSummary CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> inventorySummaryManifests,
                                                                   List<DaypartDefaultDto> daypartDefaults,
                                                                   InventoryAvailability inventoryAvailability)
        {
            
            
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);

            // For Barter source, there is always only 1 daypart code for 1 manifest. 
            // The collection is needed because we use a common model InventorySummaryManifestDto for all the sources 
            // and Diginet can have several daypart codes
            var totalDaypartsCodes = inventorySummaryManifests.GroupBy(x => x.DaypartDefaultIds.Single()).Count();
            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));

            GetLatestInventoryPostingBook(inventorySummaryManifestFiles, out var shareBook, out var hutBook);

            RemoveWeeksNotInQuarter(manifests, quarterDetail);

            var result = new InventoryQuarterSummary
            {
                InventorySourceId = inventorySource.Id,
                Quarter = GetInventorySummaryQuarter(quarterDetail),
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalDaypartCodes = totalDaypartsCodes,
                TotalUnits = _GetTotalUnits(inventorySummaryManifests),
                LastUpdatedDate = DateTime.Now,
                RatesAvailableFromQuarter = inventoryAvailability.StartQuarter,
                RatesAvailableToQuarter = inventoryAvailability.EndQuarter,
                InventoryGaps = inventoryAvailability.InventoryGaps,
                Details = _GetDetails(inventorySummaryManifests, manifests, householdAudienceId),
                ShareBookId = shareBook?.Id,
                HutBookId = hutBook?.Id
            };
            
            var detailsWithHHImpressions = result.Details.Where(x => x.TotalProjectedHouseholdImpressions.HasValue);

            if (detailsWithHHImpressions.Any())
            {
                result.TotalProjectedHouseholdImpressions = detailsWithHHImpressions.Sum(x => x.TotalProjectedHouseholdImpressions);
            }

            return result;
        }

        private List<InventoryQuarterSummary.Detail> _GetDetails(List<InventorySummaryManifestDto> allSummaryManifests, List<StationInventoryManifest> allManifests, int householdAudienceId)
        {
            var result = new List<InventoryQuarterSummary.Detail>();

            // For Barter source, there is always only 1 daypart code for 1 manifest. 
            // The collection is needed because we use a common model InventorySummaryManifestDto for all the sources 
            // and Diginet can have several daypart codes
            var allManifestsGroupedByDaypart = allSummaryManifests.GroupBy(x => x.DaypartDefaultIds.Single());

            foreach (var manifestsGrouping in allManifestsGroupedByDaypart)
            {
                var summaryManifests = manifestsGrouping.ToList();
                var summaryManifestIds = summaryManifests.Select(m => m.ManifestId);
                var manifests = allManifests.Where(x => summaryManifestIds.Contains(x.Id.Value));
                var marketCodes = summaryManifests.Where(x => x.MarketCode.HasValue).Select(x => Convert.ToInt32(x.MarketCode.Value)).Distinct();

                _CalculateHouseHoldImpressionsAndCPM(manifests, householdAudienceId, out var householdImpressions, out var cpm);

                result.Add(new InventoryQuarterSummary.Detail
                {
                    DaypartDefaultId = manifestsGrouping.Key,
                    TotalMarkets = marketCodes.Count(),
                    TotalCoverage = MarketCoverageCache.GetMarketCoverages(marketCodes).Sum(x => x.Value),
                    TotalProjectedHouseholdImpressions = householdImpressions,
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

        /// <summary>
        /// Loads the inventory summary data into InventorySummaryDto object for barter.
        /// </summary>
        /// <param name="inventorySource">The inventory source.</param>
        /// <param name="barterData">The data.</param>
        /// <param name="quarterDetail">Quarter detail data</param>
        /// <returns>InventorySummaryDto object</returns>
        public override InventorySummaryDto LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary barterData, QuarterDetailDto quarterDetail)
        {
            var result = new BarterInventorySummaryDto
            {
                Quarter = quarterDetail,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name
            };

            if (barterData.HasInventorySourceSummary)
            {
                result.HasInventoryGaps = barterData.InventoryGaps.Any();
                result.InventoryGaps = barterData.InventoryGaps;
                result.LastUpdatedDate = barterData.LastUpdatedDate;
                result.RatesAvailableFromQuarter = GetQuarter(barterData.RatesAvailableFromQuarter.Quarter, barterData.RatesAvailableFromQuarter.Year);
                result.RatesAvailableToQuarter = GetQuarter(barterData.RatesAvailableToQuarter.Quarter, barterData.RatesAvailableToQuarter.Year);
            }

            if (barterData.HasInventorySourceSummaryQuarterDetails)
            {
                GetLatestInventoryPostingBook(barterData.ShareBookId, barterData.HutBookId, out var shareBook, out var hutBook);

                result.HutBook = hutBook;
                result.ShareBook = shareBook;
                result.HouseholdImpressions = barterData.TotalProjectedHouseholdImpressions;
                result.TotalMarkets = barterData.TotalMarkets;
                result.TotalStations = barterData.TotalStations;
                result.TotalUnits = barterData.TotalUnits ?? 0;
                result.TotalDaypartCodes = barterData.TotalDaypartCodes ?? 0;
                result.HasRatesAvailableForQuarter = barterData.TotalUnits > 0;
                result.Details = barterData.Details.Select(x => new BarterInventorySummaryDto.Detail
                {
                    CPM = x.CPM,
                    TotalUnits = x.TotalUnits ?? 0,
                    Daypart = x.DaypartCode,
                    HouseholdImpressions = x.TotalProjectedHouseholdImpressions,
                    TotalCoverage = x.TotalCoverage,
                    TotalMarkets = x.TotalMarkets
                }).ToList();
            }

            return result;
        }

    }
}
