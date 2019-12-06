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

        private DateTime? GetFileLastCreatedDate(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Max(x => (DateTime?)x.CreatedDate);
        }

        public override InventoryQuarterSummary CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> quarterInventorySummaryManifests,
                                                                   List<DaypartCodeDto> daypartCodes,
                                                                   InventoryAvailability inventoryAvailability)
        {

            var sw = Stopwatch.StartNew();
            var quarterInventorySummaryManifestFiles = GetInventorySummaryManifestFiles(quarterInventorySummaryManifests);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained {quarterInventorySummaryManifestFiles.Count} manifest files in {sw.Elapsed}");

            sw.Restart();
            var quarterManifestIds = InventoryRepository.GetStationInventoryManifestsByIds(quarterInventorySummaryManifests.Select(x => x.ManifestId));
            sw.Stop();
            Debug.WriteLine($"=======> Obtained {quarterManifestIds.Count} manifest ids in {sw.Elapsed}");

            sw.Restart();
            GetLatestInventoryPostingBook(quarterInventorySummaryManifestFiles, out var shareBook, out var hutBook);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained latest posting books in {sw.Elapsed}");

            sw.Restart();
            var inventorySummaryQuarter = GetInventorySummaryQuarter(quarterDetail);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained InventorySummaryQuarter in {sw.Elapsed}");

            sw.Restart();
            var totalMarkets = GetTotalMarkets(quarterInventorySummaryManifests);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained {totalMarkets} total markets in {sw.Elapsed}");

            sw.Restart();
            var totalStations = GetTotalStations(quarterInventorySummaryManifests);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained {totalStations} total stations in {sw.Elapsed}");

            sw.Restart();
            var totalPrograms = GetTotalPrograms(quarterInventorySummaryManifests);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained {totalPrograms} total programs in {sw.Elapsed}");

            sw.Restart();
            var projectedHouseholdImpressions = _GetHouseholdImpressions(quarterManifestIds, householdAudienceId);
            sw.Stop();
            Debug.WriteLine($"=======> Obtained {projectedHouseholdImpressions} projected household impressions in {sw.Elapsed}");

            return new InventoryQuarterSummary
            {
                InventorySourceId = inventorySource.Id,
                Quarter = inventorySummaryQuarter,
                TotalMarkets = totalMarkets,
                TotalStations = totalStations,
                TotalPrograms = totalPrograms,
                TotalProjectedHouseholdImpressions = projectedHouseholdImpressions,
                LastUpdatedDate = DateTime.Now,
                RatesAvailableFromQuarter = inventoryAvailability.StartQuarter,
                RatesAvailableToQuarter = inventoryAvailability.EndQuarter,
                InventoryGaps = inventoryAvailability.InventoryGaps,
                Details = null, //open market does not have details
                ShareBookId = shareBook?.Id,
                HutBookId = hutBook?.Id
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
            if (openMarketData == null) return new OpenMarketInventorySummaryDto()
            {
                Quarter = quarterDetail,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name
            };
            
            GetLatestInventoryPostingBook(openMarketData.ShareBookId, openMarketData.HutBookId, out var shareBook, out var hutBook);
            return new OpenMarketInventorySummaryDto
            {
                HasInventoryGaps = openMarketData.InventoryGaps.Any(),
                HutBook = hutBook,
                ShareBook = shareBook,
                InventoryGaps = openMarketData.InventoryGaps,
                HouseholdImpressions = openMarketData.TotalProjectedHouseholdImpressions,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                LastUpdatedDate = openMarketData.LastUpdatedDate,
                Quarter = quarterDetail,
                RatesAvailableFromQuarter = GetQuarter(openMarketData.RatesAvailableFromQuarter.Quarter, openMarketData.RatesAvailableFromQuarter.Year),
                RatesAvailableToQuarter = GetQuarter(openMarketData.RatesAvailableToQuarter.Quarter, openMarketData.RatesAvailableToQuarter.Year),
                TotalMarkets = openMarketData.TotalMarkets,
                TotalStations = openMarketData.TotalStations,
                TotalPrograms = openMarketData.TotalPrograms ?? 0,
                HasRatesAvailableForQuarter = openMarketData.TotalMarkets > 0,
                Details = null  //open market does not have details
            };
        }
    }
}
