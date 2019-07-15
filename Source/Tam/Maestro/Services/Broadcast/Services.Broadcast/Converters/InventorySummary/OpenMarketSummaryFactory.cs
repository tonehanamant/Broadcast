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

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var allInventorySourceManifestWeeks = InventoryRepository.GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(inventorySummaryManifests);
            var manifests = InventoryRepository.GetStationInventoryManifestsByIds(inventorySummaryManifests.Select(x => x.ManifestId));

            GetLatestInventoryPostingBook(inventorySummaryManifestFiles, out var shareBook, out var hutBook);

            var inventoryGaps = InventoryGapCalculationEngine.GetInventoryGaps(allInventorySourceManifestWeeks, quartersForInventoryAvailable, quarterDetail);

            return new OpenMarketInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = inventorySummaryManifests.Any(),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(inventorySummaryManifests),
                TotalStations = GetTotalStations(inventorySummaryManifests),
                TotalPrograms = GetTotalPrograms(inventorySummaryManifests),
                HouseholdImpressions = _GetHouseholdImpressions(manifests, householdAudienceId),
                LastUpdatedDate = GetFileLastCreatedDate(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2,
                HasInventoryGaps = inventoryGaps.Any(),
                InventoryGaps = inventoryGaps,
                Details = null, //open market does not have details
                ShareBook = shareBook,
                HutBook = hutBook
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
    }
}
