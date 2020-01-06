using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public abstract InventoryQuarterSummary CreateInventorySummary(InventorySource inventorySource,
                                                           int householdAudienceId,
                                                           QuarterDetailDto quarterDetail,
                                                           List<InventorySummaryManifestDto> manifests,
                                                           List<DaypartDefaultDto> daypartDefaults,
                                                           InventoryAvailability inventoryAvailability);
        public virtual InventoryQuarterSummary CreateInventorySummary(InventorySource inventorySource,
                                                   int householdAudienceId,
                                                   QuarterDetailDto quarterDetail,
                                                   List<DaypartDefaultDto> daypartDefaults,
                                                   InventoryAvailability inventoryAvailability)
        {
            throw new NotImplementedException();
        }

        public abstract InventorySummaryDto LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary data, QuarterDetailDto quarterDetail);

        public InventoryAvailability GetInventoryAvailabilityBySource(InventorySource inventorySource)
        {
            var sw = Stopwatch.StartNew();
            var allInventorySourceManifestWeeks = InventoryRepository.
                GetStationInventoryManifestWeeksForInventorySource(inventorySource.Id);
            sw.Stop();
            Debug.WriteLine($"#####> Obtained inventory manifest weeks in {sw.Elapsed}");

            sw.Restart();
            var allQuarterRangeInventoryAvailable = GetQuartersForInventoryAvailable(allInventorySourceManifestWeeks);
            sw.Stop();
            Debug.WriteLine($"#####> Obtained inventory quarters in {sw.Elapsed}");

            sw.Restart();
            var inventoryGaps = InventoryGapCalculationEngine.GetInventoryGaps(allInventorySourceManifestWeeks);
            sw.Stop();
            Debug.WriteLine($"#####> Obtained {inventoryGaps.Count} inventory gaps in {sw.Elapsed}");

            return new InventoryAvailability
            {
                InventoryGaps = inventoryGaps,
                StartQuarter = GetInventorySummaryQuarter(allQuarterRangeInventoryAvailable.Item1),
                EndQuarter = GetInventorySummaryQuarter(allQuarterRangeInventoryAvailable.Item2)

            };

        }

        protected Tuple<QuarterDetailDto, QuarterDetailDto> GetQuartersForInventoryAvailable(List<int> weeks)
        {
            if (weeks.Any())
            {
                var ratesAvailableFrom = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(weeks.Min()).StartDate;
                var ratesAvailableTo = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(weeks.Max()).EndDate;
                var ratesAvailableFromQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableFrom);
                var ratesAvailableToQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableTo);

                return new Tuple<QuarterDetailDto, QuarterDetailDto>(ratesAvailableFromQuarterDetail, ratesAvailableToQuarterDetail);
            }

            return new Tuple<QuarterDetailDto, QuarterDetailDto>(null, null);
        }

        protected QuarterDetailDto GetQuarter(int quarterNumber, int year)
        {
            return _QuarterCalculationEngine.GetQuarterDetail(quarterNumber, year);
        }

        protected QuarterDto GetInventorySummaryQuarter(QuarterDetailDto quarterDetail)
        {
            return new QuarterDto
            {
                Quarter = quarterDetail.Quarter,
                Year = quarterDetail.Year
            };
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

        protected void GetLatestInventoryPostingBook(int? shareBookId, int? hutBookId, out MediaMonthDto shareBook, out MediaMonthDto hutBook)
        {
            shareBook = null;
            hutBook = null;

            if (shareBookId.HasValue)
            {
                shareBook = _ToMediaMonthDto(_MediaMonthAndWeekAggregateCache.GetMediaMonthById(shareBookId.Value));
            }
            if (hutBookId.HasValue)
            {
                hutBook = _ToMediaMonthDto(_MediaMonthAndWeekAggregateCache.GetMediaMonthById(hutBookId.Value));
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
            const int batchSize = 10000;
            int index = 0;
            List<string> totalPrograms = new List<string>();

            var manifestIds = manifests.Select(x => x.ManifestId).ToList();
            
            while (index < manifestIds.Count())
            {
                totalPrograms.AddRange(_ProgramRepository.GetTotalUniqueProgramNamesByManifests(manifestIds.Skip(index).Take(batchSize).ToList()));
                index = index + batchSize;
            }
            return totalPrograms.Distinct(StringComparer.OrdinalIgnoreCase).Count();
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
