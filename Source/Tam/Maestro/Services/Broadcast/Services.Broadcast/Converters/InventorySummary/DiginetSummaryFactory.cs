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

        public override InventoryQuarterSummary CreateInventorySummary(InventorySource inventorySource, int householdAudienceId, 
            QuarterDetailDto quarterDetail, List<InventorySummaryManifestDto> inventorySummaryManifests, List<StandardDaypartDto> standardDayparts,
            InventoryAvailability inventoryAvailability)
        {
            
            
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var stationInventoryManifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));
            
            RemoveWeeksNotInQuarter(stationInventoryManifests, quarterDetail);

            _CalculateCPM(stationInventoryManifests, householdAudienceId, out var CPM);

            

            return new InventoryQuarterSummary
            {
                InventorySourceId = inventorySource.Id,
                Quarter = GetInventorySummaryQuarter( quarterDetail),
                TotalDaypartCodes = inventorySummaryManifests.SelectMany(x => x.StandardDaypartIds).Distinct().Count(),
                LastUpdatedDate = DateTime.Now,
                RatesAvailableFromQuarter = inventoryAvailability.StartQuarter,
                RatesAvailableToQuarter = inventoryAvailability.EndQuarter,
                InventoryGaps = inventoryAvailability.InventoryGaps,
                CPM = CPM,
                Details = _GetDetails(inventorySummaryManifests, stationInventoryManifests, householdAudienceId)
            };
        }

        private List<InventoryQuarterSummary.Detail> _GetDetails(List<InventorySummaryManifestDto> allSummaryManifests
            , List<StationInventoryManifest> stationInventoryManifests, int householdAudienceId)
        {
            var result = new List<InventoryQuarterSummary.Detail>();
            var allStandardDayparts = allSummaryManifests.SelectMany(x => x.StandardDaypartIds).Distinct();

            foreach (var standardDaypartId in allStandardDayparts)
            {
                var manifests = stationInventoryManifests.Where(x => x.ManifestDayparts.Any(d => d.StandardDaypart.Id == standardDaypartId));

                _CalculateHouseHoldImpressionsAndCPMUsingDaypartCodePortion(manifests, householdAudienceId, standardDaypartId, out var householdImpressions, out var cpm);

                result.Add(new InventoryQuarterSummary.Detail
                {
                    StandardDaypartId = standardDaypartId,
                    TotalProjectedHouseholdImpressions = householdImpressions,
                    CPM = cpm,
                });
            }

            return result;
        }

        private void _CalculateHouseHoldImpressionsAndCPMUsingDaypartCodePortion(
            IEnumerable<StationInventoryManifest> manifests,
            int householdAudienceId,
            int daypartCode,
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
                var totalTimeDuration = manifestDayparts.Sum(x => x.Daypart.GetTotalDurationInSeconds());
                var totalTimeDurationForDaypartCode = manifestDayparts.Where(x => x.StandardDaypart.Id == daypartCode).Sum(x => x.Daypart.GetTotalDurationInSeconds());

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

        /// <summary>
        /// Loads the inventory summary data into InventorySummaryDto object for diginet.
        /// </summary>
        /// <param name="inventorySource">The inventory source.</param>
        /// <param name="diginetData">The data.</param>
        /// <param name="quarterDetail">Quarter detail data</param>
        /// <returns>InventorySummaryDto object</returns>
        public override InventorySummaryDto LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary diginetData, QuarterDetailDto quarterDetail)
        {
            var result = new DiginetInventorySummaryDto
            {
                Quarter = quarterDetail,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name
            };

            if (diginetData.HasInventorySourceSummary)
            {
                result.InventoryGaps = diginetData.InventoryGaps;
                result.LastUpdatedDate = diginetData.LastUpdatedDate;
                result.RatesAvailableFromQuarter = GetQuarter(diginetData.RatesAvailableFromQuarter.Quarter, diginetData.RatesAvailableFromQuarter.Year);
                result.RatesAvailableToQuarter = GetQuarter(diginetData.RatesAvailableToQuarter.Quarter, diginetData.RatesAvailableToQuarter.Year);
                result.HasInventoryGaps = diginetData.InventoryGaps.Any();
            }

            if (diginetData.HasInventorySourceSummaryQuarterDetails)
            {
                GetLatestInventoryPostingBook(diginetData.ShareBookId, diginetData.HutBookId, out var shareBook, out var hutBook);

                result.HutBook = hutBook;
                result.ShareBook = shareBook;
                result.HouseholdImpressions = diginetData.TotalProjectedHouseholdImpressions;
                result.TotalMarkets = diginetData.TotalMarkets;
                result.TotalStations = diginetData.TotalStations;
                result.TotalDaypartCodes = diginetData.TotalDaypartCodes ?? 0;
                result.HasRatesAvailableForQuarter = diginetData.TotalDaypartCodes > 0;
                result.CPM = diginetData.CPM;
                result.Details = diginetData.Details.Select(x => new DiginetInventorySummaryDto.Detail
                {
                    CPM = x.CPM,
                    Daypart = x.DaypartCode,
                    HouseholdImpressions = x.TotalProjectedHouseholdImpressions
                }).ToList();
            }

            return result;
        }

    }
}
