using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
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

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource, int householdAudienceId, QuarterDetailDto quarterDetail, List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var allInventorySourceManifestWeeks = InventoryRepository.GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var stationInventoryManifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));
            
            RemoveWeeksNotInQuarter(stationInventoryManifests, quarterDetail);

            _CalculateCPM(stationInventoryManifests, householdAudienceId, out var CPM);

            var inventoryGaps = InventoryGapCalculationEngine.GetInventoryGaps(allInventorySourceManifestWeeks, quartersForInventoryAvailable, quarterDetail);

            return new DiginetInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = inventorySummaryManifests.Any(),
                Quarter = quarterDetail,
                TotalDaypartCodes = inventorySummaryManifests.SelectMany(x => x.DaypartCodes).Distinct().Count(),
                LastUpdatedDate = GetLastJobCompletedDate(inventorySummaryManifestFiles),
                IsUpdating = GetIsInventoryUpdating(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2,
                InventoryGaps = inventoryGaps,
                HasInventoryGaps = inventoryGaps.Any(),
                CPM = CPM,
                Details = _GetDetails(inventorySummaryManifests, stationInventoryManifests, householdAudienceId)
            };
        }

        private List<DiginetInventorySummaryDto.Detail> _GetDetails(List<InventorySummaryManifestDto> allSummaryManifests, List<StationInventoryManifest> stationInventoryManifests, int householdAudienceId)
        {
            var result = new List<DiginetInventorySummaryDto.Detail>();
            var allDaypartCodes = allSummaryManifests.SelectMany(x => x.DaypartCodes).Distinct();

            foreach (var daypartCode in allDaypartCodes)
            {
                var manifests = stationInventoryManifests.Where(x => x.ManifestDayparts.Any(d => d.DaypartCode.Code == daypartCode));

                _CalculateHouseHoldImpressionsAndCPMUsingDaypartCodePortion(manifests, householdAudienceId, daypartCode, out var householdImpressions, out var cpm);

                result.Add(new DiginetInventorySummaryDto.Detail
                {
                    Daypart = daypartCode,
                    HouseholdImpressions = householdImpressions,
                    CPM = cpm,
                });
            }

            return result;
        }

        private void _CalculateHouseHoldImpressionsAndCPMUsingDaypartCodePortion(
            IEnumerable<StationInventoryManifest> manifests,
            int householdAudienceId,
            string daypartCode,
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

                var manifestDayparts = manifest.ManifestDayparts.ToList();
                var totalTimeDuration = manifestDayparts.Sum(x => x.Daypart.GetTotalTimeDuration());
                var totalTimeDurationForDaypartCode = manifestDayparts.Where(x => x.DaypartCode.Code == daypartCode).Sum(x => x.Daypart.GetTotalTimeDuration());

                if (totalTimeDuration == 0 || totalTimeDurationForDaypartCode == 0)
                    throw new Exception("Invalid daypart with zero time found");

                var portion = (double)totalTimeDurationForDaypartCode / totalTimeDuration;

                impressionsTotal += hhImpressions * portion;
                spotCostTotal += spotCost * (decimal)portion;
            }

            impressionsResult = impressionsTotal;
            cpmResult = ProposalMath.CalculateCpm(spotCostTotal, impressionsTotal);
        }

        private void _CalculateCPM(
            IEnumerable<StationInventoryManifest> manifests,
            int householdAudienceId,
            out decimal? cpmResult)
        {
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
            
            cpmResult = ProposalMath.CalculateCpm(spotCostTotal, impressionsTotal);
        }

        private bool _ManifestHasProvidedHHImpressionsAndSpotCost(StationInventoryManifest manifest, int householdAudienceId)
        {
            return manifest.ManifestAudiencesReferences.Any(x => x.Audience.Id == householdAudienceId && x.Impressions.HasValue) &&
                   manifest.ManifestRates.Any(x => x.SpotLengthId == manifest.SpotLengthId);
        }
    }
}
