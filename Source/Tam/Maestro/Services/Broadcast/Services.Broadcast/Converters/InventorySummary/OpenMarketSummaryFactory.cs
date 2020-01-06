using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class OpenMarketSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        public OpenMarketSummaryFactory(IInventoryRepository inventoryRepository,
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
                                                                   List<DaypartDefaultDto> daypartDefaults,
                                                                   InventoryAvailability inventoryAvailability)
        {

            var sw = Stopwatch.StartNew();
            var inventorySummaryQuarter = GetInventorySummaryQuarter(quarterDetail);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained InventorySummaryQuarter in {sw.Elapsed}");

            sw.Restart();
            var inventorySummaryTotals = InventoryRepository.GetInventorySummaryDateRangeTotalsForSource
                                            (inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained inventory summary totals in  {sw.Elapsed}");

            return new InventoryQuarterSummary
            {
                InventorySourceId = inventorySource.Id,
                Quarter = inventorySummaryQuarter,
                TotalMarkets = inventorySummaryTotals.TotalMarkets,
                TotalStations = inventorySummaryTotals.TotalStations,
                TotalPrograms = inventorySummaryTotals.TotalPrograms,
                TotalProjectedHouseholdImpressions = inventorySummaryTotals.TotalHouseholdImpressions,
                LastUpdatedDate = DateTime.Now,
                RatesAvailableFromQuarter = inventoryAvailability.StartQuarter,
                RatesAvailableToQuarter = inventoryAvailability.EndQuarter,
                InventoryGaps = inventoryAvailability.InventoryGaps,
                Details = null, //open market does not have details
                ShareBookId = null,
                HutBookId = null
            };
        }

        private double? _GetHouseholdImpressions(IEnumerable<StationInventoryManifest> manifests, int householdAudienceId)
        {
            manifests = manifests.Where(x => _GetAudienceWithPositiveImpressions(x.ManifestAudiencesReferences, householdAudienceId) != null ||
                                             _GetAudienceWithPositiveImpressions(x.ManifestAudiences, householdAudienceId) != null);

            if (!manifests.Any())
                return null;

            return manifests.Sum(x =>
            {
                var providedImpressions = _GetAudienceWithPositiveImpressions(x.ManifestAudiencesReferences, householdAudienceId)?.Impressions;

                if (providedImpressions.HasValue)
                    return providedImpressions.Value;

                var projectedImpressions = _GetAudienceWithPositiveImpressions(x.ManifestAudiences, householdAudienceId).Impressions.Value;

                return projectedImpressions;
            });
        }

        private StationInventoryManifestAudience _GetAudienceWithPositiveImpressions(IEnumerable<StationInventoryManifestAudience> audiences, int audienceId)
        {
                //this is just a workaround. We need to figure out why we get multiple audiences here
                return audiences.FirstOrDefault(x => x.Audience.Id == audienceId && x.Impressions.HasValue && x.Impressions.Value > 0);            
        }

        /// <summary>
        /// Loads the inventory summary data into InventorySummaryDto object for open market.
        /// </summary>
        /// <param name="inventorySource">The inventory source.</param>
        /// <param name="openMarketData">The data.</param>
        /// <param name="quarterDetail">Quarter detail data</param>
        /// <returns>InventorySummaryDto object</returns>
        public override InventorySummaryDto LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary openMarketData, QuarterDetailDto quarterDetail)
        {
            var result = new OpenMarketInventorySummaryDto
            {
                Quarter = quarterDetail,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                Details = null // OpenMarket does not have details
            };

            if (openMarketData.HasInventorySourceSummary)
            {
                result.HasInventoryGaps = openMarketData.InventoryGaps.Any();
                result.InventoryGaps = openMarketData.InventoryGaps;
                result.LastUpdatedDate = openMarketData.LastUpdatedDate;
                result.RatesAvailableFromQuarter = GetQuarter(openMarketData.RatesAvailableFromQuarter.Quarter, openMarketData.RatesAvailableFromQuarter.Year);
                result.RatesAvailableToQuarter = GetQuarter(openMarketData.RatesAvailableToQuarter.Quarter, openMarketData.RatesAvailableToQuarter.Year);
            }

            if (openMarketData.HasInventorySourceSummaryQuarterDetails)
            {
                GetLatestInventoryPostingBook(openMarketData.ShareBookId, openMarketData.HutBookId, out var shareBook, out var hutBook);

                result.HutBook = hutBook;
                result.ShareBook = shareBook;
                result.HouseholdImpressions = openMarketData.TotalProjectedHouseholdImpressions;
                result.TotalMarkets = openMarketData.TotalMarkets;
                result.TotalStations = openMarketData.TotalStations;
                result.TotalPrograms = openMarketData.TotalPrograms ?? 0;
                result.HasRatesAvailableForQuarter = openMarketData.TotalMarkets > 0;
            }

            return result;
        }

        public override InventoryQuarterSummary CreateInventorySummary(InventorySource inventorySource, int householdAudienceId, QuarterDetailDto quarterDetail, List<InventorySummaryManifestDto> manifests, List<DaypartDefaultDto> daypartDefaults, InventoryAvailability inventoryAvailability)
        {
            throw new NotImplementedException();
        }
    }
}
