using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.InventorySummary
{
    public abstract class BaseInventorySummaryAbstractFactory
    {
        protected readonly IInventoryRepository InventoryRepository;
        protected readonly IInventorySummaryRepository InventorySummaryRepository;
        protected readonly IMarketCoverageCache MarketCoverageCache;
        protected readonly IInventoryGapCalculationEngine InventoryGapCalculationEngine;

        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IProgramRepository _ProgramRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public BaseInventorySummaryAbstractFactory(IInventoryRepository inventoryRepository,
                                                   IInventorySummaryRepository inventorySummaryRepository,
                                                   IQuarterCalculationEngine quarterCalculationEngine,
                                                   IProgramRepository programRepository,
                                                   IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                                   IMarketCoverageCache marketCoverageCache,
                                                   IInventoryGapCalculationEngine inventoryGapCalculationEngine)
        {
            InventoryRepository = inventoryRepository;
            InventorySummaryRepository = inventorySummaryRepository;
            MarketCoverageCache = marketCoverageCache;
            InventoryGapCalculationEngine = inventoryGapCalculationEngine;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _ProgramRepository = programRepository;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public abstract InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> manifests);

        protected Tuple<QuarterDetailDto, QuarterDetailDto> GetQuartersForInventoryAvailable(List<StationInventoryManifestWeek> weeks)
        {
            if (weeks.Any())
            {
                var ratesAvailableFrom = weeks.Min(x => x.StartDate);
                var ratesAvailableTo = weeks.Max(x => x.EndDate);
                var ratesAvailableFromQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableFrom);
                var ratesAvailableToQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableTo);

                return new Tuple<QuarterDetailDto, QuarterDetailDto>(ratesAvailableFromQuarterDetail, ratesAvailableToQuarterDetail);
            }

            return new Tuple<QuarterDetailDto, QuarterDetailDto>(null, null);
        }      

        protected double? GetHouseholdImpressions(List<InventorySummaryManifestDto> inventorySummaryManifests, int householdAudienceId)
        {
            var manifestIds = inventorySummaryManifests.Select(x => x.ManifestId).ToList();

            return InventorySummaryRepository.GetInventorySummaryHouseholdImpressions(manifestIds, householdAudienceId);
        }

        protected void GetLatestInventoryPostingBook(List<InventorySummaryManifestFileDto> inventorySummaryManifestFileDtos, out MediaMonthDto shareBook, out MediaMonthDto hutBook)
        {
            shareBook = null;
            hutBook = null;

            var latestUploadedFile = inventorySummaryManifestFileDtos
                .Where(x => x.ShareProjectionBookId.HasValue)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefault();

            if (latestUploadedFile != null)
            {
                shareBook = _ToMediaMonthDto(_MediaMonthAndWeekAggregateCache.GetMediaMonthById(latestUploadedFile.ShareProjectionBookId.Value));

                if (latestUploadedFile.HutProjectionBookId.HasValue)
                {
                    hutBook = _ToMediaMonthDto(_MediaMonthAndWeekAggregateCache.GetMediaMonthById(latestUploadedFile.HutProjectionBookId.Value));
                }
            }
        }

        private MediaMonthDto _ToMediaMonthDto(MediaMonth mediaMonth)
        {
            return new MediaMonthDto
            {
                Id = mediaMonth.Id,
                Year = mediaMonth.Year,
                Month = mediaMonth.Month
            };
        }

        protected List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFiles(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var fileIds = inventorySummaryManifests.Where(x => x.FileId != null).Select(x => (int)x.FileId).Distinct().ToList();

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

        protected bool GetIsInventoryUpdating(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Any(x => x.JobStatus == InventoryFileRatingsProcessingStatus.Queued ||
                                                          x.JobStatus == InventoryFileRatingsProcessingStatus.Processing);
        }

        protected DateTime? GetLastJobCompletedDate(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Max(x => x.JobCompletedDate);
        }

        protected void RemoveWeeksNotInQuarter(List<StationInventoryManifest> manifests, QuarterDetailDto quarterDetail)
        {
            foreach (var manifest in manifests)
            {
                manifest.ManifestWeeks = manifest.ManifestWeeks.Where(x => x.StartDate <= quarterDetail.EndDate && x.EndDate >= quarterDetail.StartDate).ToList();
            }
        }

        protected int GetTotalPrograms(IEnumerable<InventorySummaryManifestDto> manifests)
        {
            var manifestIds = manifests.Select(x => x.ManifestId).ToList();
            return _ProgramRepository.GetUniqueProgramNamesByManifests(manifestIds)
                                     .Distinct(StringComparer.OrdinalIgnoreCase)
                                     .Count();
        }

        protected int GetTotalPrograms(IEnumerable<StationInventoryManifest> manifests)
        {
            return manifests.SelectMany(x => x.ManifestDayparts)
                            .Select(x => x.ProgramName)
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count();
        }
    }
}
