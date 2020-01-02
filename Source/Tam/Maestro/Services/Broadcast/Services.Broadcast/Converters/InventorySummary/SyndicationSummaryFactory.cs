using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class SyndicationSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        public SyndicationSummaryFactory(IInventoryRepository inventoryRepository,
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
                                                                   List<InventorySummaryManifestDto> manifests,
                                                                   List<DaypartDefaultDto> daypartDefaults,
                                                                   InventoryAvailability inventoryAvailability)
        {
            
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(manifests);
            

            return new InventoryQuarterSummary
            {
                InventorySourceId = inventorySource.Id,
                Quarter = GetInventorySummaryQuarter(quarterDetail),
                LastUpdatedDate = DateTime.Now,
                TotalPrograms = GetTotalPrograms(manifests),
                RatesAvailableFromQuarter = inventoryAvailability.StartQuarter,
                RatesAvailableToQuarter = inventoryAvailability.EndQuarter,
                InventoryGaps = inventoryAvailability.InventoryGaps,
                Details = null //Syndication does not have details
            };
        }

        /// <summary>
        /// Loads the inventory summary data into InventorySummaryDto object for barter.
        /// </summary>
        /// <param name="inventorySource">The inventory source.</param>
        /// <param name="syndicationData"></param>
        /// <param name="quarterDetail">Quarter detail data</param>
        /// <returns>InventorySummaryDto object</returns>
        public override InventorySummaryDto LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary syndicationData, QuarterDetailDto quarterDetail)
        {
            if (syndicationData == null) return new SyndicationInventorySummaryDto()
            {
                Quarter = quarterDetail,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name
            };

            GetLatestInventoryPostingBook(syndicationData.ShareBookId, syndicationData.HutBookId, out var shareBook, out var hutBook);
            return new SyndicationInventorySummaryDto
            {
                HutBook = hutBook,
                ShareBook = shareBook,
                InventoryGaps = syndicationData.InventoryGaps,
                HouseholdImpressions = syndicationData.TotalProjectedHouseholdImpressions,
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                LastUpdatedDate = syndicationData.LastUpdatedDate,
                Quarter = quarterDetail,
                RatesAvailableFromQuarter = GetQuarter(syndicationData.RatesAvailableFromQuarter.Quarter, syndicationData.RatesAvailableFromQuarter.Year),
                RatesAvailableToQuarter = GetQuarter(syndicationData.RatesAvailableToQuarter.Quarter, syndicationData.RatesAvailableToQuarter.Year),
                TotalMarkets = syndicationData.TotalMarkets,
                TotalStations = syndicationData.TotalStations,
                TotalPrograms = syndicationData.TotalPrograms ?? 0,
                HasRatesAvailableForQuarter = syndicationData.TotalPrograms > 0,
                HasInventoryGaps = syndicationData.InventoryGaps.Any(),                
                Details = null //Syndication does not have details
            };
        }
    }
}
