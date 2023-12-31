﻿using System;
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
                                                List<StandardDaypartDto> standardDayparts,
                                                InventoryAvailability inventoryAvailability)
        {
            
            
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));

            GetLatestInventoryPostingBook(inventorySummaryManifestFiles, out var shareBook, out var hutBook);

            RemoveWeeksNotInQuarter(manifests, quarterDetail);

            

            // For O&O source, there is always only 1 daypart code for 1 manifest. 
            // The collection is needed because we use a common model InventorySummaryManifestDto for all the sources 
            // and Diginet can have several daypart codes
            var totalDaypartCodes = inventorySummaryManifests.GroupBy(x => x.StandardDaypartIds.Single()).Count();

            var result = new InventoryQuarterSummary
            {
                InventorySourceId = inventorySource.Id,
                Quarter = GetInventorySummaryQuarter(quarterDetail),
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalPrograms = GetTotalPrograms(manifests),
                TotalDaypartCodes = totalDaypartCodes,
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

            // For O&O source, there is always only 1 daypart code for 1 manifest. 
            // The collection is needed because we use a common model InventorySummaryManifestDto for all the sources 
            // and Diginet can have several daypart codes
            var allManifestsGroupedByDaypart = allSummaryManifests.GroupBy(x => x.StandardDaypartIds.Single());

            foreach (var manifestsGrouping in allManifestsGroupedByDaypart)
            {
                var summaryManifests = manifestsGrouping.ToList();
                var summaryManifestIds = summaryManifests.Select(m => m.ManifestId);
                var manifests = allManifests.Where(x => summaryManifestIds.Contains(x.Id.Value));
                var marketCodes = summaryManifests.Where(x => x.MarketCode.HasValue).Select(x => Convert.ToInt32(x.MarketCode.Value)).Distinct();
                var manifestSpots = manifests.SelectMany(x => x.ManifestWeeks).Select(x => x.Spots);

                _CalculateHouseHoldImpressionsAndCPM(manifests, householdAudienceId, out var householdImpressions, out var cpm);

                result.Add(new InventoryQuarterSummary.Detail
                {
                    StandardDaypartId = manifestsGrouping.Key,
                    TotalMarkets = marketCodes.Count(),
                    TotalCoverage = MarketCoverageCache.GetMarketCoverages(marketCodes).Sum(x => x.Value),
                    TotalPrograms = GetTotalPrograms(manifests),
                    TotalProjectedHouseholdImpressions = householdImpressions,
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

        /// <summary>
        /// Loads the inventory summary data into InventorySummaryDto object for proprietary OandO.
        /// </summary>
        /// <param name="inventorySource">The inventory source.</param>
        /// <param name="proprietaryData">The data.</param>
        /// <param name="quarterDetail">Quarter detail data</param>
        /// <returns>InventorySummaryDto object</returns>
        public override InventorySummaryDto LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary proprietaryData, QuarterDetailDto quarterDetail)
        {
            var result = new ProprietaryOAndOInventorySummaryDto
            {
                Quarter = quarterDetail,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name
            };

            if (proprietaryData.HasInventorySourceSummary)
            {
                result.HasInventoryGaps = proprietaryData.InventoryGaps.Any();
                result.InventoryGaps = proprietaryData.InventoryGaps;
                result.LastUpdatedDate = proprietaryData.LastUpdatedDate;
                result.RatesAvailableFromQuarter = GetQuarter(proprietaryData.RatesAvailableFromQuarter.Quarter, proprietaryData.RatesAvailableFromQuarter.Year);
                result.RatesAvailableToQuarter = GetQuarter(proprietaryData.RatesAvailableToQuarter.Quarter, proprietaryData.RatesAvailableToQuarter.Year);
            }

            if (proprietaryData.HasInventorySourceSummaryQuarterDetails)
            {
                GetLatestInventoryPostingBook(proprietaryData.ShareBookId, proprietaryData.HutBookId, out var shareBook, out var hutBook);

                result.HutBook = hutBook;
                result.ShareBook = shareBook;
                result.HouseholdImpressions = proprietaryData.TotalProjectedHouseholdImpressions;
                result.TotalMarkets = proprietaryData.TotalMarkets;
                result.TotalStations = proprietaryData.TotalStations;
                result.TotalDaypartCodes = proprietaryData.TotalDaypartCodes ?? 0;
                result.HasRatesAvailableForQuarter = proprietaryData.TotalMarkets > 0;
                result.TotalPrograms = proprietaryData.TotalPrograms ?? 0;
                result.Details = proprietaryData.Details.Select(x => new ProprietaryOAndOInventorySummaryDto.Detail
                {
                    CPM = x.CPM,
                    Daypart = x.DaypartCode,
                    HouseholdImpressions = x.TotalProjectedHouseholdImpressions,
                    TotalCoverage = x.TotalCoverage,
                    TotalMarkets = x.TotalMarkets,
                    MaxSpotsPerWeek = x.MaxSpotsPerWeek ?? 0,
                    MinSpotsPerWeek = x.MinSpotsPerWeek ?? 0,
                    TotalPrograms = x.TotalPrograms ?? 0
                }).ToList();
            }

            return result;
        }

    }
}
