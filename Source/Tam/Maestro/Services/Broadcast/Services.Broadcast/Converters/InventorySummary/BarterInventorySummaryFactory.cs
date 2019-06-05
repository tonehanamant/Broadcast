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
                                             IMarketCoverageCache marketCoverageCache)

            : base(inventoryRepository, inventorySummaryRepository, quarterCalculationEngine)
        {
            _MarketCoverageCache = marketCoverageCache;
        }

        private bool GetIsInventoryUpdating(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Any(x => x.JobStatus == InventoryFileRatingsProcessingStatus.Queued ||
                                                          x.JobStatus == InventoryFileRatingsProcessingStatus.Processing);
        }

        private DateTime? GetLastJobCompletedDate(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Max(x => x.JobCompletedDate);
        }

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(inventorySource);           
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
            _RemoveWeeksNotInQuarter(manifests, quarterDetail);
            result.Details = _GetDetails(inventorySummaryManifests, manifests, householdAudienceId);

            var detailsWithHHImpressions = result.Details.Where(x => x.HouseholdImpressions.HasValue);

            if (detailsWithHHImpressions.Any())
            {
                result.HouseholdImpressions = detailsWithHHImpressions.Sum(x => x.HouseholdImpressions);
            }

            return result;
        }

        private void _RemoveWeeksNotInQuarter(List<StationInventoryManifest> manifests, QuarterDetailDto quarterDetail)
        {
            foreach (var manifest in manifests)
            {
                manifest.ManifestWeeks = manifest.ManifestWeeks.Where(x => x.StartDate <= quarterDetail.EndDate && x.EndDate >= quarterDetail.StartDate).ToList();
            }
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
                    TotalCoverage = _MarketCoverageCache.GetMarketCoverages(marketCodes).Sum(x => x.Value),
                    HouseholdImpressions = householdImpressions,
                    CPM = cpm,
                    TotalUnits = _GetTotalUnits(summaryManifests)
                });
            }

            return result;
        }

        private void _CalculateHouseHoldImpressionsAndCPM(IEnumerable<StationInventoryManifest> manifests, int householdAudienceId, out double? impressionsResult, out decimal? cpmResult)
        {
            impressionsResult = null;
            cpmResult = null;

            // impressions for component audience HH and spot cost might be not calculated yet. Skip such manifests
            var manifestsWithHHAudiencesAndRates = manifests.Where(x => 
                x.ManifestAudiences.Any(a => a.Audience.Id == householdAudienceId && a.Impressions.HasValue) &&
                x.ManifestRates.Any(r => r.SpotLengthId == x.SpotLengthId));

            if (!manifestsWithHHAudiencesAndRates.Any())
                return;

            double impressionsTotal = 0;
            decimal cpmTotal = 0;

            var manifestsGroupedByMediaWeek = manifestsWithHHAudiencesAndRates
                .SelectMany(x => x.ManifestWeeks, (manifest, week) => new { manifest, mediaWeekId = week.MediaWeek.Id })
                .GroupBy(x => x.mediaWeekId);

            foreach (var grouping in manifestsGroupedByMediaWeek)
            {
                var weekManifests = grouping.Select(x => x.manifest).ToList();
                double weekImpressionsTotal = 0;
                decimal weekCpmTotal = 0;

                foreach (var manifest in weekManifests)
                {
                    var hhImpressions = manifest.ManifestAudiences.Single(x => x.Audience.Id == householdAudienceId).Impressions.Value;
                    var spotCost = manifest.ManifestRates.First(r => r.SpotLengthId == manifest.SpotLengthId).SpotCost;
                    var hhCPM = ProposalMath.CalculateCpm(spotCost, hhImpressions);

                    weekImpressionsTotal += hhImpressions;
                    weekCpmTotal += hhCPM;
                }

                var weekImpressionsAverage = weekImpressionsTotal / weekManifests.Count;
                var weekCPMAverage = weekCpmTotal / weekManifests.Count;

                impressionsTotal += weekImpressionsAverage;
                cpmTotal += weekCPMAverage;
            }

            impressionsResult = impressionsTotal;
            cpmResult = cpmTotal / manifestsGroupedByMediaWeek.Count();
        }

        private int _GetTotalUnits(List<InventorySummaryManifestDto> manifests)
        {
            return manifests.GroupBy(x => x.UnitName).Count();
        }
    }
}
