using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.InventorySummary;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventorySummaryService : IApplicationService
    {
        List<InventorySource> GetInventorySources();
        InventoryQuartersDto GetInventoryQuarters(int inventorySourceId, int daypartCodeId);
        InventoryQuartersDto GetInventoryQuarters(DateTime currentDate);
        List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate);
        List<DaypartCodeDto> GetDaypartCodes(int inventorySourceId);
        List<string> GetInventoryUnits(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate);
        List<LookupDto> GetInventorySourceTypes();

        /// <summary>
        /// Aggregates all the information based on the list of inventory source ids
        /// </summary>
        /// <param name="inventorySourceIds">List of inventory source ids</param>
        void AggregateInventorySummaryData(List<int> inventorySourceIds);
    }

    public class InventorySummaryService : IInventorySummaryService
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventorySummaryRepository _InventorySummaryRepository;
        private readonly IProgramRepository _ProgramRepository;
        private readonly IDaypartCodeRepository _DaypartCodeRepository;
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

        public InventorySummaryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                       IQuarterCalculationEngine quarterCalculationEngine,
                                       IBroadcastAudiencesCache audiencesCache,
                                       IMarketCoverageCache marketCoverageCache,
                                       IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                       IInventoryGapCalculationEngine inventoryGapCalculationEngine)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;
            _AudiencesCache = audiencesCache;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>(); ;
            _InventorySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventorySummaryRepository>();
            _ProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramRepository>();
            _DaypartCodeRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartCodeRepository>();
            _MarketCoverageCache = marketCoverageCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _InventoryGapCalculationEngine = inventoryGapCalculationEngine;
            _InventoryLogoRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryLogoRepository>();
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

        public InventoryQuartersDto GetInventoryQuarters(int inventorySourceId, int daypartCodeId)
        {
            var dateRange = _InventoryRepository.GetInventoryStartAndEndDates(inventorySourceId, daypartCodeId);
            return new InventoryQuartersDto { Quarters = _QuarterCalculationEngine.GetAllQuartersForDateRange(dateRange) };
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
            }

            return inventorySummaryDtos;
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

                //check if I should filter by daypart code
                if (data != null && inventorySummaryFilterDto.DaypartCodeId.HasValue)
                {
                    var daypartCodes = data.Details.Select(m => m.DaypartCodeId).Distinct().ToList();

                    if (_ShouldFilterByDaypartCode(daypartCodes, inventorySummaryFilterDto.DaypartCodeId))
                    {
                        continue;
                    }
                }

                //don't add empty objects if I filter by daypart
                if(data == null && inventorySummaryFilterDto.DaypartCodeId.HasValue)
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

                if (data != null && inventorySummaryFilterDto.DaypartCodeId.HasValue)
                {
                    var daypartCodes = data.Details.Select(m => m.DaypartCodeId).Distinct().ToList();

                    if (_ShouldFilterByDaypartCode(daypartCodes, inventorySummaryFilterDto.DaypartCodeId))
                    {
                        continue;
                    }
                }

                yield return _LoadInventorySummary(inventorySource, data, quarterDetail);
            }
        }

        private List<InventorySummaryAggregation> _CreateInventorySummariesForSource(InventorySource inventorySource, 
            BaseInventorySummaryAbstractFactory inventorySummaryFactory,
            int householdAudienceId, DateTime currentDate)
        {
            var result = new List<InventorySummaryAggregation>();
            var inventorySourceDateRange = _GetInventorySourceOrCurrentQuarterDateRange(inventorySource.Id, currentDate);
            var allQuartersBetweenDates =
                _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventorySourceDateRange.Start.Value,
                                                                     inventorySourceDateRange.End.Value);
            List<DaypartCodeDto> daypartCodesAndIds = _DaypartCodeRepository.GetAllActiveDaypartCodes();
            foreach (var quarterDetail in allQuartersBetweenDates)
            {
                var manifests = _GetInventorySummaryManifests(inventorySource, quarterDetail);
                
                if (manifests.Count == 0)   //there is no data in this quarter
                {
                    continue;
                }
                
                var summaryData = inventorySummaryFactory.CreateInventorySummary(inventorySource, householdAudienceId, quarterDetail, manifests, daypartCodesAndIds);
                result.Add(summaryData);
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

        private bool _ShouldFilterByDaypartCode(List<int> daypartCodeIds, int? daypartCodeId)
        {
            if (!daypartCodeId.HasValue)
                return false;
            return !daypartCodeIds.Contains(daypartCodeId.Value);
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
        
        private InventorySummaryDto _LoadInventorySummary(InventorySource inventorySource, InventorySummaryAggregation data, QuarterDetailDto quarterDetail)
        {
            var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);
            if (data != null && data.InventoryGaps.Any())
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
            var inventoriesDateRange = _GetAllInventoriesDateRange() ?? _GetCurrentQuarterDateRange(currentDate);
            return _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventoriesDateRange.Start.Value, inventoriesDateRange.End.Value);
        }

        private DateRange _GetInventorySourceOrCurrentQuarterDateRange(int inventorySourceId, DateTime currentDate)
        {
            return _GetInventorySourceDateRange(inventorySourceId) ??
                   _GetCurrentQuarterDateRange(currentDate);
        }

        private DateRange _GetAllInventoriesDateRange()
        {
            var allInventoriesDateRange = _InventoryRepository.GetAllInventoriesDateRange();

            return _GetDateRangeOrNull(allInventoriesDateRange);
        }

        private DateRange _GetInventorySourceDateRange(int inventorySourceId)
        {
            var inventorySourceDateRange = _InventoryRepository.GetInventorySourceDateRange(inventorySourceId);

            return _GetDateRangeOrNull(inventorySourceDateRange);
        }

        private DateRange _GetDateRangeOrNull(DateRange dateRange)
        {
            var startDate = dateRange.Start;

            if (!startDate.HasValue)
                return null;

            var startDateValue = startDate.Value;
            var endDate = dateRange.End == null ? startDateValue.AddYears(1) : dateRange.End.Value;

            return new DateRange(startDateValue, endDate);
        }

        private DateRange _GetCurrentQuarterDateRange(DateTime currentDate)
        {
            var datesTuple = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, currentDate);
            return new DateRange(datesTuple.Item1, datesTuple.Item2);
        }

        public List<DaypartCodeDto> GetDaypartCodes(int inventorySourceId)
        {
            return _DaypartCodeRepository.GetDaypartCodesByInventorySource(inventorySourceId);
        }

        public List<string> GetInventoryUnits(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return new List<string>();
            }

            var groups = _InventoryRepository.GetInventoryGroups(inventorySourceId, daypartCodeId, startDate, endDate);

            return groups.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Aggregates all the information based on the list of inventory source ids
        /// </summary>
        /// <param name="inventorySourceIds">List of inventory source ids</param>
        public void AggregateInventorySummaryData(List<int> inventorySourceIds)
        {
            var houseHoldAudienceId = _AudiencesCache.GetDefaultAudience().Id;
            
            foreach (var inventorySourceId in inventorySourceIds)
            {                
                var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
                var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);
                var summaryData = _CreateInventorySummariesForSource(inventorySource, inventorySummaryFactory, houseHoldAudienceId, DateTime.Now);
                foreach(var data in summaryData)
                {
                    _InventorySummaryRepository.SaveInventoryAggregatedData(data);
                }                
            }
        }
    }
}
