﻿using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.InventorySummary;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventorySummaryService : IApplicationService
    {
        List<InventorySource> GetInventorySources();
        InventoryQuartersDto GetInventoryQuarters(int inventorySourceId, int daypartDefaultId);
        InventoryQuartersDto GetInventoryQuarters(DateTime currentDate);
        List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate);
        List<InventorySummaryDto> GetInventorySummariesWithCache(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate);
        List<DaypartDefaultDto> GetDaypartDefaults(int inventorySourceId);
        List<string> GetInventoryUnits(int inventorySourceId, int daypartDefaultId, DateTime startDate, DateTime endDate);
        List<LookupDto> GetInventorySourceTypes();

        /// <summary>
        /// Aggregates all the information based on the list of inventory source ids
        /// </summary>
        /// <param name="inventorySourceIds">List of inventory source ids</param>
        [Queue("inventorysummaryaggregation")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void AggregateInventorySummaryData(List<int> inventorySourceIds);

        /// <summary>
        /// Aggregates all the information based on the list of inventory source ids
        /// </summary>
        /// <param name="inventorySourceIds">List of inventory source ids</param>
        /// <param name="endDate">Inventory end date</param>
        /// <param name="startDate">Inventory start date</param>
        [Queue("inventorysummaryaggregation")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void AggregateInventorySummaryData(List<int> inventorySourceIds, DateTime? startDate, DateTime? endDate);

        void QueueAggregateInventorySummaryDataJob(int inventorySourceId);
    }

    public class InventorySummaryService : BroadcastBaseClass, IInventorySummaryService
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventorySummaryRepository _InventorySummaryRepository;
        private readonly IProgramRepository _ProgramRepository;
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;
        private readonly List<InventorySourceTypeEnum> SummariesSourceTypes = new List<InventorySourceTypeEnum>
        {
            InventorySourceTypeEnum.Barter,
            InventorySourceTypeEnum.OpenMarket,
            InventorySourceTypeEnum.ProprietaryOAndO,
            InventorySourceTypeEnum.Syndication,
            InventorySourceTypeEnum.Diginet
        };
        private readonly IMarketCoverageCache _MarketCoverageCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IInventoryGapCalculationEngine _InventoryGapCalculationEngine;
        private readonly IInventoryLogoRepository _InventoryLogoRepository;
        private readonly IInventorySummaryCache _InventorySummaryCache;
        private readonly IBackgroundJobClient _BackgroundJobClient;

        public InventorySummaryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                       IQuarterCalculationEngine quarterCalculationEngine,
                                       IBroadcastAudiencesCache audiencesCache,
                                       IMarketCoverageCache marketCoverageCache,
                                       IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                       IInventoryGapCalculationEngine inventoryGapCalculationEngine,
                                       IInventorySummaryCache inventorySummaryCache,
                                       IBackgroundJobClient backgroundJobClient)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;
            _AudiencesCache = audiencesCache;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>(); ;
            _InventorySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventorySummaryRepository>();
            _ProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramRepository>();
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
            _MarketCoverageCache = marketCoverageCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _InventoryGapCalculationEngine = inventoryGapCalculationEngine;
            _InventoryLogoRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryLogoRepository>();
            _InventorySummaryCache = inventorySummaryCache;
            _BackgroundJobClient = backgroundJobClient;
        }

        public List<InventorySource> GetInventorySources()
        {
            var inventorySources = _InventoryRepository.GetInventorySources();

            return inventorySources.Where(x => SummariesSourceTypes.Contains(x.InventoryType)).ToList();
        }

        public List<LookupDto> GetInventorySourceTypes()
        {
            return EnumExtensions.ToLookupDtoList<InventorySourceTypeEnum>()
                .OrderBy(i => i.Display).ToList();
        }

        public InventoryQuartersDto GetInventoryQuarters(DateTime currentDate)
        {
            return new InventoryQuartersDto
            {
                Quarters = _GetInventorySummaryQuarters(currentDate),
                DefaultQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate)
            };
        }

        public InventoryQuartersDto GetInventoryQuarters(int inventorySourceId, int daypartDefaultId)
        {
            var weeks = _InventoryRepository.GetStationInventoryManifestWeeks(inventorySourceId, daypartDefaultId);
            var mediaMonthIds = weeks.Select(x => x.MediaWeek.MediaMonthId).Distinct();
            var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(mediaMonthIds);
            var quarters = mediaMonths
                .GroupBy(x => new { x.Quarter, x.Year }) // take unique quarters
                .Select(x => _QuarterCalculationEngine.GetQuarterDetail(x.Key.Quarter, x.Key.Year))
                .ToList();

            return new InventoryQuartersDto { Quarters = quarters };
        }

        public List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate)
        {
            var inventorySummaryDtos = new List<InventorySummaryDto>();

            if (inventorySummaryFilterDto.InventorySourceId != null)
            {
                inventorySummaryDtos.AddRange(_LoadInventorySummariesForSource(inventorySummaryFilterDto, currentDate));
            }
            else
            {
                inventorySummaryDtos.AddRange(_LoadInventorySummaryForQuarter(inventorySummaryFilterDto));
            }

            if (inventorySummaryDtos.Any(x => x.InventorySourceId != 0))   //if there is summary data in the result set
            {
                _SetHasLogo(inventorySummaryDtos);
                _SetHasInventoryForSource(inventorySummaryDtos, inventorySummaryFilterDto.LatestInventoryUpdatesBySourceId);
            }

            return inventorySummaryDtos;
        }

        public List<InventorySummaryDto> GetInventorySummariesWithCache(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate)
        {
            List<InventorySummaryDto> GetInventorySummariesFunc() => GetInventorySummaries(inventorySummaryFilterDto, currentDate);
            inventorySummaryFilterDto.LatestInventoryUpdatesBySourceId = _InventorySummaryRepository.GetLatestSummaryUpdatesBySource();
            return _InventorySummaryCache.GetOrCreate(inventorySummaryFilterDto, GetInventorySummariesFunc);
        }

        private IEnumerable<InventorySummaryDto> _LoadInventorySummaryForQuarter(InventorySummaryFilterDto inventorySummaryFilterDto)
        {
            var quarter = inventorySummaryFilterDto.Quarter.Quarter;
            var year = inventorySummaryFilterDto.Quarter.Year;
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(quarter, year);
            var inventorySources = GetInventorySources();

            foreach (var inventorySource in inventorySources)
            {
                if (_ShouldFilterBySourceType(inventorySummaryFilterDto, inventorySource))
                {
                    continue;
                }

                var data = _InventorySummaryRepository.GetInventorySummaryDataForSources(inventorySource, quarterDetail.Quarter, quarterDetail.Year);

                if (_ShouldFilterOutDataByDaypartDefault(data, inventorySummaryFilterDto))
                {
                    continue;
                }

                yield return _LoadInventorySummary(inventorySource, data, quarterDetail);
            }
        }

        private IEnumerable<InventorySummaryDto> _LoadInventorySummariesForSource(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate)
        {
            var inventorySourceId = inventorySummaryFilterDto.InventorySourceId.Value;
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);

            var inventorySourceDateRange = _GetInventorySourceOrCurrentQuarterDateRange(inventorySourceId, currentDate);
            var allQuartersBetweenDates =
                _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventorySourceDateRange.Start.Value,
                                                                     inventorySourceDateRange.End.Value);

            foreach (var quarterDetail in allQuartersBetweenDates)
            {
                var data = _InventorySummaryRepository.GetInventorySummaryDataForSources(inventorySource, quarterDetail.Quarter, quarterDetail.Year);

                if (_ShouldFilterOutDataByDaypartDefault(data, inventorySummaryFilterDto))
                {
                    continue;
                }

                yield return _LoadInventorySummary(inventorySource, data, quarterDetail);
            }
        }

        private List<InventoryQuarterSummary> _CreateInventorySummariesForSource(InventorySource inventorySource,
            BaseInventorySummaryAbstractFactory inventorySummaryFactory,
            int householdAudienceId, DateTime currentDate)
        {
            var result = new List<InventoryQuarterSummary>();
            var sw = Stopwatch.StartNew();
            var quarters = _GetInventoryQuarters(null, null, currentDate, inventorySource.Id);
            sw.Stop();
            Debug.WriteLine($"Got inventory quarters in {sw.Elapsed}");

            InventoryAvailability inventoryAvailability;
            try
            {
                inventoryAvailability = inventorySummaryFactory.GetInventoryAvailabilityBySource(inventorySource);
            }
            catch (Exception e)
            {
                var msg = $"Inventory not found for source '{inventorySource.Name}'.";
                Debug.WriteLine(msg);
                _LogError(msg, e);
                return result;
            }

            var daypartDefaultsAndIds = _DaypartDefaultRepository.GetAllDaypartDefaults();
            foreach (var quarterDetail in quarters)
            {
                InventoryQuarterSummary summaryData;
                sw.Restart();
                if ((InventorySourceEnum)inventorySource.Id == InventorySourceEnum.OpenMarket)
                {
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                        (inventorySource, householdAudienceId, quarterDetail,
                        daypartDefaultsAndIds, inventoryAvailability);
                }
                else
                {
                    var manifests = _GetInventorySummaryManifests(inventorySource, quarterDetail);
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                    (inventorySource, householdAudienceId, quarterDetail, manifests,
                    daypartDefaultsAndIds, inventoryAvailability);
                }
                sw.Stop();
                Debug.WriteLine($"Created inventory summary in {sw.Elapsed}");

                // add summary only if there is some inventory during the quarter
                if (summaryData.TotalPrograms > 0 || summaryData.TotalStations > 0 || summaryData.Details?.Any() == true)
                {
                    result.Add(summaryData);
                }
            }

            return result;
        }

        private List<InventoryQuarterSummary> _CreateInventorySummariesForSource(InventorySource inventorySource,
            BaseInventorySummaryAbstractFactory inventorySummaryFactory,
            int householdAudienceId, List<QuarterDetailDto> quarters)
        {
            var result = new List<InventoryQuarterSummary>();
            var sw = Stopwatch.StartNew();

            InventoryAvailability inventoryAvailability;
            try
            {
                inventoryAvailability = inventorySummaryFactory.GetInventoryAvailabilityBySource(inventorySource);
            }
            catch (Exception e)
            {
                var msg = $"Inventory not found for source '{inventorySource.Name}'.";
                Debug.WriteLine(msg);
                _LogError(msg, e);
                return result;
            }

            var daypartDefaultsAndIds = _DaypartDefaultRepository.GetAllDaypartDefaults();
            foreach (var quarterDetail in quarters)
            {
                InventoryQuarterSummary summaryData;
                sw.Restart();
                if ((InventorySourceEnum)inventorySource.Id == InventorySourceEnum.OpenMarket)
                {
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                        (inventorySource, householdAudienceId, quarterDetail,
                        daypartDefaultsAndIds, inventoryAvailability);
                }
                else
                {
                    var manifests = _GetInventorySummaryManifests(inventorySource, quarterDetail);
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                    (inventorySource, householdAudienceId, quarterDetail, manifests,
                    daypartDefaultsAndIds, inventoryAvailability);
                }
                sw.Stop();
                Debug.WriteLine($"Created inventory summary in {sw.Elapsed}");

                // add summary only if there is some inventory during the quarter
                if (summaryData.TotalPrograms > 0 || summaryData.TotalStations > 0 || summaryData.Details?.Any() == true)
                {
                    result.Add(summaryData);
                }
            }

            return result;
        }

        private void _SetHasLogo(List<InventorySummaryDto> inventorySummaryDtos)
        {
            var inventorySources = inventorySummaryDtos.Select(x => x.InventorySourceId).Distinct();
            var inventorySourcesWithLogo = _InventoryLogoRepository.GetInventorySourcesWithLogo(inventorySources);

            foreach (var inventorySummaryDto in inventorySummaryDtos)
            {
                inventorySummaryDto.HasLogo = inventorySourcesWithLogo.Contains(inventorySummaryDto.InventorySourceId);
            }
        }

        private void _SetHasInventoryForSource(List<InventorySummaryDto> inventorySummaryDtos, Dictionary<int, DateTime?> latestInventoryUpdatesBySourceId)
        {
            foreach (var inventorySummaryDto in inventorySummaryDtos)
            {
                if (latestInventoryUpdatesBySourceId.TryGetValue(inventorySummaryDto.InventorySourceId, out var latestInventoryUpdateDate) &&
                    latestInventoryUpdateDate.HasValue)
                {
                    inventorySummaryDto.HasInventoryForSource = true;
                }
                else
                {
                    inventorySummaryDto.HasInventoryForSource = false;
                }
            }
        }

        private bool _ShouldFilterOutDataByDaypartDefault(InventoryQuarterSummary data, InventorySummaryFilterDto inventorySummaryFilterDto)
        {
            var daypartDefaultId = inventorySummaryFilterDto.DaypartDefaultId;

            if (daypartDefaultId.HasValue)
            {
                // don't add empty objects
                if (!data.HasInventorySourceSummary || !data.HasInventorySourceSummaryQuarterDetails)
                {
                    return true;
                }

                var daypartDefaultIds = data.Details.Select(m => m.DaypartDefaultId).Distinct().ToList();

                return !daypartDefaultIds.Contains(daypartDefaultId.Value);
            }

            return false;
        }

        private bool _ShouldFilterBySourceType(InventorySummaryFilterDto inventorySummaryFilterDto, InventorySource inventorySource)
        {
            return inventorySummaryFilterDto.InventorySourceType.HasValue &&
                   inventorySource.InventoryType != inventorySummaryFilterDto.InventorySourceType;
        }

        private List<InventorySummaryManifestDto> _GetInventorySummaryManifests(InventorySource inventorySource, QuarterDetailDto quarterDetail)
        {
            var @switch = new Dictionary<InventorySourceTypeEnum, Func<List<InventorySummaryManifestDto>>> {
                { InventorySourceTypeEnum.Barter,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForBarterSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.OpenMarket,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForOpenMarketSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.ProprietaryOAndO,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForProprietaryOAndOSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.Syndication,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForSyndicationSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.Diginet,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForDiginetSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
            };

            var result = @switch[inventorySource.InventoryType]();

            return result.Any() ? result : new List<InventorySummaryManifestDto>();
        }

        private InventorySummaryDto _LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary data, QuarterDetailDto quarterDetail)
        {
            var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);

            if (data.HasInventorySourceSummary)
            {
                foreach (var gap in data.InventoryGaps)
                {
                    gap.Quarter = _QuarterCalculationEngine.GetQuarterDetail(gap.Quarter.Quarter, gap.Quarter.Year);
                }
            }

            return inventorySummaryFactory.LoadInventorySummary(inventorySource, data, quarterDetail);
        }

        private BaseInventorySummaryAbstractFactory _GetInventorySummaryFactory(InventorySourceTypeEnum inventoryType)
        {
            var @switch = new Dictionary<InventorySourceTypeEnum, BaseInventorySummaryAbstractFactory>
            {
                {
                InventorySourceTypeEnum.Barter, new BarterInventorySummaryFactory(_InventoryRepository,
                                                                            _InventorySummaryRepository,
                                                                            _QuarterCalculationEngine,
                                                                            _ProgramRepository,
                                                                            _MediaMonthAndWeekAggregateCache,
                                                                            _MarketCoverageCache,
                                                                            _InventoryGapCalculationEngine)
                },
                {
                    InventorySourceTypeEnum.OpenMarket, new OpenMarketSummaryFactory(_InventoryRepository,
                                                                       _InventorySummaryRepository,
                                                                       _QuarterCalculationEngine,
                                                                       _ProgramRepository,
                                                                       _MediaMonthAndWeekAggregateCache,
                                                                       _MarketCoverageCache,
                                                                       _InventoryGapCalculationEngine)
                },
                {
                    InventorySourceTypeEnum.ProprietaryOAndO, new ProprietaryOAndOSummaryFactory(_InventoryRepository,
                                                                             _InventorySummaryRepository,
                                                                             _QuarterCalculationEngine,
                                                                             _ProgramRepository,
                                                                             _MediaMonthAndWeekAggregateCache,
                                                                             _MarketCoverageCache,
                                                                             _InventoryGapCalculationEngine)
                },
                {
                     InventorySourceTypeEnum.Syndication, new SyndicationSummaryFactory(_InventoryRepository,
                                                                        _InventorySummaryRepository,
                                                                        _QuarterCalculationEngine,
                                                                        _ProgramRepository,
                                                                        _MediaMonthAndWeekAggregateCache,
                                                                        _MarketCoverageCache,
                                                                        _InventoryGapCalculationEngine)
                },
                {
                    InventorySourceTypeEnum.Diginet, new DiginetSummaryFactory(_InventoryRepository,
                                                                    _InventorySummaryRepository,
                                                                    _QuarterCalculationEngine,
                                                                    _ProgramRepository,
                                                                    _MediaMonthAndWeekAggregateCache,
                                                                    _MarketCoverageCache,
                                                                    _InventoryGapCalculationEngine)
                }
            };

            return @switch[inventoryType];
        }

        private List<QuarterDetailDto> _GetInventorySummaryQuarters(DateTime currentDate)
        {
            var dateRange = _InventoryRepository.GetAllInventoriesDateRange();

            if (dateRange.IsEmpty())
            {
                dateRange = _GetCurrentQuarterDateRange(currentDate);
            }

            return _QuarterCalculationEngine.GetAllQuartersBetweenDates(dateRange.Start.Value, dateRange.End.Value);
        }

        private List<QuarterDetailDto> _GetInventoryQuarters(DateTime? startDate, DateTime? endDate, DateTime currentDate, int inventorySourceId)
        {
            if (startDate.HasValue && endDate.HasValue)
                return _QuarterCalculationEngine.GetAllQuartersBetweenDates(startDate.GetValueOrDefault(), endDate.GetValueOrDefault());

            var dateRange = _GetInventorySourceOrCurrentQuarterDateRange(inventorySourceId, currentDate);

            return _QuarterCalculationEngine.GetAllQuartersBetweenDates(dateRange.Start.GetValueOrDefault(), dateRange.End.GetValueOrDefault());
        }

        private DateRange _GetInventorySourceOrCurrentQuarterDateRange(int inventorySourceId, DateTime currentDate)
        {
            var dateRange = _InventoryRepository.GetInventorySourceDateRange(inventorySourceId);

            if (dateRange.IsEmpty())
            {
                dateRange = _GetCurrentQuarterDateRange(currentDate);
            }

            return dateRange;
        }

        private DateRange _GetCurrentQuarterDateRange(DateTime currentDate)
        {
            var datesTuple = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, currentDate);
            return new DateRange(datesTuple.Item1, datesTuple.Item2);
        }

        public List<DaypartDefaultDto> GetDaypartDefaults(int inventorySourceId)
        {
            return _DaypartDefaultRepository.GetDaypartDefaultsByInventorySource(inventorySourceId);
        }

        public List<string> GetInventoryUnits(int inventorySourceId, int daypartDefaultId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return new List<string>();
            }

            var groups = _InventoryRepository.GetInventoryGroups(inventorySourceId, daypartDefaultId, startDate, endDate);

            return groups.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public void QueueAggregateInventorySummaryDataJob(int inventorySourceId)
        {
            _BackgroundJobClient.Enqueue<IInventorySummaryService>(x => x.AggregateInventorySummaryData(new List<int> { inventorySourceId }));
        }

        /// <inheritdoc />
        public void AggregateInventorySummaryData(List<int> inventorySourceIds)
        {
            var houseHoldAudienceId = _AudiencesCache.GetDefaultAudience().Id;

            foreach (var inventorySourceId in inventorySourceIds)
            {
                var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
                var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);
                var summaryData = _CreateInventorySummariesForSource(inventorySource, inventorySummaryFactory, houseHoldAudienceId, DateTime.Now);

                _InventorySummaryRepository.SaveInventoryAggregatedData(summaryData, inventorySourceId, null);
            }
        }

        /// <inheritdoc 
        public void AggregateInventorySummaryData(List<int> inventorySourceIds, DateTime? startDate, DateTime? endDate)
        {
            var houseHoldAudienceId = _AudiencesCache.GetDefaultAudience().Id;

            foreach (var inventorySourceId in inventorySourceIds)
            {
                var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
                var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);
                var quarters = _GetInventoryQuarters(startDate, endDate, DateTime.Now, inventorySourceId);
                var summaryData = _CreateInventorySummariesForSource(inventorySource, inventorySummaryFactory, houseHoldAudienceId, quarters);

                _InventorySummaryRepository.SaveInventoryAggregatedData(summaryData, inventorySourceId, quarters);
            }
        }
    }
}