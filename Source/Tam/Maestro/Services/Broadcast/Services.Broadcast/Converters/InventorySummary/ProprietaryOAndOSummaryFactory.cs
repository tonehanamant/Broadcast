using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class ProprietaryOAndOSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        public ProprietaryOAndOSummaryFactory(IInventoryRepository inventoryRepository,
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

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource, int householdAudienceId, QuarterDetailDto quarterDetail, List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var allInventorySourceManifestWeeks = InventoryRepository.GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));

            GetLatestInventoryPostingBook(inventorySummaryManifestFiles, out var shareBook, out var hutBook);

            RemoveWeeksNotInQuarter(manifests, quarterDetail);

            var result = new ProprietaryOAndOInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = inventorySummaryManifests.Any(),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalPrograms = GetTotalPrograms(manifests),
                TotalDaypartCodes = inventorySummaryManifests.GroupBy(x => x.DaypartCode).Count(),
                LastUpdatedDate = GetLastJobCompletedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2,
                HasInventoryGaps = HasInventoryGapsForDateRange(allInventorySourceManifestWeeks, quartersForInventoryAvailable),
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

        private List<ProprietaryOAndOInventorySummaryDto.Detail> _GetDetails(List<InventorySummaryManifestDto> allSummaryManifests, List<StationInventoryManifest> allManifests, int householdAudienceId)
        {
            var result = new List<ProprietaryOAndOInventorySummaryDto.Detail>();
            var allManifestsGroupedByDaypart = allSummaryManifests.GroupBy(x => x.DaypartCode);

            foreach (var manifestsGrouping in allManifestsGroupedByDaypart)
            {
                var summaryManifests = manifestsGrouping.ToList();
                var summaryManifestIds = summaryManifests.Select(m => m.ManifestId);
                var manifests = allManifests.Where(x => summaryManifestIds.Contains(x.Id.Value));
                var marketCodes = summaryManifests.Where(x => x.MarketCode.HasValue).Select(x => Convert.ToInt32(x.MarketCode.Value)).Distinct();
                var manifestSpots = manifests.SelectMany(x => x.ManifestWeeks).Select(x => x.Spots);

                _CalculateHouseHoldImpressionsAndCPM(manifests, householdAudienceId, out var householdImpressions, out var cpm);
                
                result.Add(new ProprietaryOAndOInventorySummaryDto.Detail
                {
                    Daypart = manifestsGrouping.Key,
                    TotalMarkets = marketCodes.Count(),
                    TotalCoverage = MarketCoverageCache.GetMarketCoverages(marketCodes).Sum(x => x.Value),
                    TotalPrograms = GetTotalPrograms(manifests),
                    HouseholdImpressions = householdImpressions,
                    CPM = cpm,
                    MinSpotsPerWeek = manifestSpots.Min(),
                    MaxSpotsPerWeek = manifestSpots.Max()
                });
            }

            return result;
        }

        private void _CalculateHouseHoldImpressionsAndCPM(
            IEnumerable<StationInventoryManifest> manifests,
            int householdAudienceId,
            out double? impressionsResult,
            out decimal? cpmResult)
        {
            impressionsResult = null;
            cpmResult = null;

            manifests = manifests.Where(x => _ManifestHasProvidedHHImpressionsAndSpotCost(x, householdAudienceId));

            if (!manifests.Any())
                return;

            double impressionsTotal = 0;
            decimal spotCostTotal = 0;

            foreach (var manifest in manifests)
            {
                var hhImpressions = manifest.ManifestAudiencesReferences.Single(x => x.Audience.Id == householdAudienceId).Impressions.Value;
                var spotCost = manifest.ManifestRates.First(r => r.SpotLengthId == manifest.SpotLengthId).SpotCost;

                impressionsTotal += hhImpressions;
                spotCostTotal += spotCost;
            }

            impressionsResult = impressionsTotal;
            cpmResult = ProposalMath.CalculateCpm(spotCostTotal, impressionsTotal);
        }

        private bool _ManifestHasProvidedHHImpressionsAndSpotCost(StationInventoryManifest manifest, int householdAudienceId)
        {
            return manifest.ManifestAudiencesReferences.Any(x => x.Audience.Id == householdAudienceId && x.Impressions.HasValue) &&
                   manifest.ManifestRates.Any(x => x.SpotLengthId == manifest.SpotLengthId);
        }
    }
}
