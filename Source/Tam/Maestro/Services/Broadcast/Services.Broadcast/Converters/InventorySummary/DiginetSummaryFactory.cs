using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class DiginetSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        public DiginetSummaryFactory(IInventoryRepository inventoryRepository,
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
            var stationInventoryManifest = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));

            RemoveWeeksNotInQuarter(stationInventoryManifest, quarterDetail);

            _CalculateHouseHoldImpressionsAndCPM(stationInventoryManifest, householdAudienceId, out var hhImpressions, out var CPM);

            return new DiginetInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = inventorySummaryManifests.Any(),
                Quarter = quarterDetail,
                TotalDaypartCodes = inventorySummaryManifests.GroupBy(x => x.DaypartCode).Count(),
                LastUpdatedDate = GetLastJobCompletedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2,
                HasInventoryGaps = HasInventoryGapsForDateRange(allInventorySourceManifestWeeks, quartersForInventoryAvailable),
                CPM = CPM,
                Details = _GetDetails(inventorySummaryManifests, stationInventoryManifest, householdAudienceId, inventorySummaryManifestFiles)
            };
        }

        private List<DiginetInventorySummaryDto.Detail> _GetDetails(List<InventorySummaryManifestDto> allSummaryManifests, List<StationInventoryManifest> stationInventoryManifest, int householdAudienceId, List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            var result = new List<DiginetInventorySummaryDto.Detail>();
            var allFilesGroupedByDaypart = inventorySummaryManifestFiles.GroupBy(x => x.DaypartCode);

            foreach (var fileGrouping in allFilesGroupedByDaypart)
            {
                var files = fileGrouping.ToList();
                var fileIds = files.Select(m => m.FileId);
                var manifests = stationInventoryManifest.Where(x => fileIds.Contains(x.InventoryFileId.Value));

                _CalculateHouseHoldImpressionsAndCPM(manifests, householdAudienceId, out var householdImpressions, out var cpm);

                result.Add(new DiginetInventorySummaryDto.Detail
                {
                    Daypart = fileGrouping.Key,
                    HouseholdImpressions = householdImpressions,
                    CPM = cpm,
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
