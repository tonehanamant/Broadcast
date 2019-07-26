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

        public override InventorySummaryAggregation CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> inventorySummaryManifests,
                                                                   List<DaypartCodeDto> daypartCodes)
        {
            var allInventorySourceManifestWeeks = InventoryRepository.GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));

            GetLatestInventoryPostingBook(inventorySummaryManifestFiles, out var shareBook, out var hutBook);

            var inventoryGaps = InventoryGapCalculationEngine.GetInventoryGaps(allInventorySourceManifestWeeks, quartersForInventoryAvailable, quarterDetail);

            return new InventorySummaryAggregation
            {
                InventorySourceId = inventorySource.Id,
                Quarter = GetInventorySummaryQuarter(quarterDetail),
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalPrograms = GetTotalPrograms(inventorySummaryManifests),
                TotalProjectedHouseholdImpressions = _GetHouseholdImpressions(manifests, householdAudienceId),
                LastUpdatedDate = GetFileLastCreatedDate(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = GetInventorySummaryQuarter(quartersForInventoryAvailable.Item1),
                RatesAvailableToQuarter = GetInventorySummaryQuarter(quartersForInventoryAvailable.Item2),
                InventoryGaps = inventoryGaps,
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
            return audiences.SingleOrDefault(x => x.Audience.Id == audienceId && x.Impressions.HasValue && x.Impressions.Value > 0);
        }

        /// <summary>
        /// Loads the inventory summary data into InventorySummaryDto object for open market.
        /// </summary>
        /// <param name="inventorySource">The inventory source.</param>
        /// <param name="openMarketData">The data.</param>
        /// <param name="quarterDetail">Quarter detail data</param>
        /// <returns>InventorySummaryDto object</returns>
        public override InventorySummaryDto LoadInventorySummary(InventorySource inventorySource, InventorySummaryAggregation openMarketData, QuarterDetailDto quarterDetail)
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
