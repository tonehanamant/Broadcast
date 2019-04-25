using Services.Broadcast.ApplicationServices;
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
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventorySummaryRepository _InventorySummaryRepository;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

        public BaseInventorySummaryAbstractFactory(IInventoryRepository inventoryRepository,
                                                   IInventorySummaryRepository inventorySummaryRepository,
                                                   IQuarterCalculationEngine quarterCalculationEngine)
        {
            _InventoryRepository = inventoryRepository;
            _InventorySummaryRepository = inventorySummaryRepository;
            _QuarterCalculationEngine = quarterCalculationEngine;
        }

        public abstract InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail);

        protected Tuple<QuarterDetailDto, QuarterDetailDto> _GetRatesAvailableFromAndTo(InventorySource inventorySource)
        {
            var inventorySourceDateRange = _InventoryRepository.GetInventorySourceDateRange(inventorySource.Id);
            var ratesAvailableFrom = inventorySourceDateRange.Start;
            var ratesAvailableTo = inventorySourceDateRange.End;
            var ratesAvailableFromQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableFrom);
            var ratesAvailableToQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableTo);

            return new Tuple<QuarterDetailDto, QuarterDetailDto>(ratesAvailableFromQuarterDetail,
                                                                 ratesAvailableToQuarterDetail);
        }

        protected List<InventorySummaryManifestDto> GetInventorySummaryManifests(InventorySource inventorySource,
                                                                                 QuarterDetailDto quarterDetail)
        {
            return _InventorySummaryRepository.GetInventorySummaryManifests(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
        }

        protected double? GetHouseholdImpressions(List<InventorySummaryManifestDto> inventorySummaryManifests, int householdAudienceId)
        {
            var manifestIds = inventorySummaryManifests.Select(x => x.ManifestId).ToList();

            return _InventorySummaryRepository.GetInventorySummaryHouseholdImpressions(manifestIds, householdAudienceId);
        }

        protected List<InventorySummaryBookDto> GetInventoryPostingBooks(List<InventorySummaryManifestFileDto> inventorySummaryManifestFileDtos)
        {
            return inventorySummaryManifestFileDtos.
                        GroupBy(x => new { x.HutProjectionBookId, x.ShareProjectionBookId }).
                        Select(x => new InventorySummaryBookDto
                        {
                            HutProjectBookId = x.Key.HutProjectionBookId,
                            ShareProjectionBookId = x.Key.ShareProjectionBookId
                        }).
                        ToList();
        }

        protected List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFiles(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var fileIds = inventorySummaryManifests.Where(x => x.FileId != null).Select(x => (int)x.FileId).ToList();

            return _InventorySummaryRepository.GetInventorySummaryManifestFileDtos(fileIds);
        }

        protected int GetTotalMarkets(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            return inventorySummaryManifests.GroupBy(x => x.MarketCode).Count();
        }

        protected int GetTotalStations(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            return inventorySummaryManifests.Where(x => x.StationId.HasValue).GroupBy(x => x.StationId).Count();
        }

        protected bool GetIsInventoryUpdating(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Any(x => x.JobStatus == InventoryFileRatingsProcessingStatus.Queued ||
                                                          x.JobStatus == InventoryFileRatingsProcessingStatus.Processing);
        }

        protected DateTime? GetLastUpdatedDate(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Max(x => x.LastCompletedDate);
        }

        protected bool GetHasRatesAvailable(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            return inventorySummaryManifests.Count() > 0;
        }
    }
}
