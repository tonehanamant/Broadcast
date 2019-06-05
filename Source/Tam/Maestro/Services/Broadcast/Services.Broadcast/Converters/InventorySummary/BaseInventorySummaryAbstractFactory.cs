using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public abstract class BaseInventorySummaryAbstractFactory
    {
        protected readonly IInventoryRepository InventoryRepository;
        protected readonly IInventorySummaryRepository InventorySummaryRepository;

        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

        public BaseInventorySummaryAbstractFactory(IInventoryRepository inventoryRepository,
                                                   IInventorySummaryRepository inventorySummaryRepository,
                                                   IQuarterCalculationEngine quarterCalculationEngine)
        {
            InventoryRepository = inventoryRepository;
            InventorySummaryRepository = inventorySummaryRepository;
            _QuarterCalculationEngine = quarterCalculationEngine;
        }

        public abstract InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> manifests);

        protected Tuple<QuarterDetailDto, QuarterDetailDto> GetQuartersForInventoryAvailable(InventorySource inventorySource)
        {
            var inventorySourceDateRange = InventoryRepository.GetInventorySourceDateRange(inventorySource.Id);
            var ratesAvailableFrom = inventorySourceDateRange.Start;
            var ratesAvailableTo = inventorySourceDateRange.End;
            var ratesAvailableFromQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableFrom);
            var ratesAvailableToQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableTo);

            return new Tuple<QuarterDetailDto, QuarterDetailDto>(ratesAvailableFromQuarterDetail,
                                                                 ratesAvailableToQuarterDetail);
        }      

        protected double? GetHouseholdImpressions(List<InventorySummaryManifestDto> inventorySummaryManifests, int householdAudienceId)
        {
            var manifestIds = inventorySummaryManifests.Select(x => x.ManifestId).ToList();

            return InventorySummaryRepository.GetInventorySummaryHouseholdImpressions(manifestIds, householdAudienceId);
        }

        protected List<InventorySummaryBookDto> GetInventoryPostingBooks(List<InventorySummaryManifestFileDto> inventorySummaryManifestFileDtos)
        {
            return inventorySummaryManifestFileDtos.
                        Where(x => x.ShareProjectionBookId.HasValue).
                        GroupBy(x => new { x.ShareProjectionBookId, x.HutProjectionBookId }).
                        Select(x => new InventorySummaryBookDto
                        {
                            ShareProjectionBookId = x.Key.ShareProjectionBookId,
                            HutProjectionBookId = x.Key.HutProjectionBookId
                        }).
                        ToList();
        }

        protected List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFiles(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var fileIds = inventorySummaryManifests.Where(x => x.FileId != null).Select(x => (int)x.FileId).ToList();

            return InventorySummaryRepository.GetInventorySummaryManifestFileDtos(fileIds);
        }

        protected int GetTotalMarkets(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            return inventorySummaryManifests.Where(x => x.MarketCode.HasValue).GroupBy(x => x.MarketCode).Count();
        }

        protected int GetTotalStations(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            return inventorySummaryManifests.Where(x => x.StationId.HasValue).GroupBy(x => x.StationId).Count();
        }
    }
}
