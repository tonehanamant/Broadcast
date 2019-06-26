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
            var houseHoldAudience = _AudiencesCache.GetDisplayAudienceByCode(BroadcastConstants.HOUSEHOLD_CODE);
            var householdAudienceId = houseHoldAudience.Id;

            if (inventorySummaryFilterDto.InventorySourceId != null)
            {
                inventorySummaryDtos.AddRange(_CreateInventorySummariesForSource(inventorySummaryFilterDto, householdAudienceId, currentDate));
            }
            else
            {
                inventorySummaryDtos.AddRange(_CreateInventorySummaryForQuarter(inventorySummaryFilterDto, householdAudienceId));
            }

            _SetHasLogo(inventorySummaryDtos);

            return inventorySummaryDtos;
        }

        private void _SetHasLogo(List<InventorySummaryDto> inventorySummaryDtos)
        {
            var inventorySources = inventorySummaryDtos.Select(x => x.InventorySourceId).Distinct();
            var inventorySourcesWithLogo = _InventoryLogoRepository.GetInventorySourcesWithLogo(inventorySources);

            foreach(var inventorySummaryDto in inventorySummaryDtos)
            {
                inventorySummaryDto.HasLogo = inventorySourcesWithLogo.Contains(inventorySummaryDto.InventorySourceId);
            }
        }

        private bool _FilterByDaypartCode(List<string> daypartCodes, int? daypartCodeId)
        {
            if (!daypartCodeId.HasValue)
                return false;
            var daypartCodeDto = _DaypartCodeRepository.GetDaypartCodeById(daypartCodeId.Value);
            var daypartCode = daypartCodeDto.Code;
            return !daypartCodes.Contains(daypartCode);
        }

        private IEnumerable<InventorySummaryDto> _CreateInventorySummariesForSource(InventorySummaryFilterDto inventorySummaryFilterDto, int householdAudienceId, DateTime currentDate)
        {
            var inventorySourceId = inventorySummaryFilterDto.InventorySourceId.Value;
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
            var inventorySourceDateRange = _GetInventorySourceOrCurrentQuarterDateRange(inventorySourceId, currentDate);
            var allQuartersBetweenDates = 
                _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventorySourceDateRange.Start.Value, 
                                                                     inventorySourceDateRange.End.Value);

            foreach (var quarterDetail in allQuartersBetweenDates)
            {
                if (_FilterBySourceType(inventorySummaryFilterDto, inventorySource))
                    continue;

                var manifests = _GetInventorySummaryManifests(inventorySource, quarterDetail);
                var daypartCodes = manifests.Select(m => m.DaypartCode).ToList();

                if (_FilterByDaypartCode(daypartCodes, inventorySummaryFilterDto.DaypartCodeId))
                    continue;

                yield return _CreateInventorySummary(inventorySource, householdAudienceId, quarterDetail, manifests);
            }
        }

        private bool _FilterBySourceType(InventorySummaryFilterDto inventorySummaryFilterDto, InventorySource inventorySource)
        {
            return inventorySummaryFilterDto.InventorySourceType.HasValue &&
                   inventorySource.InventoryType != inventorySummaryFilterDto.InventorySourceType;
        }

        private IEnumerable<InventorySummaryDto> _CreateInventorySummaryForQuarter(InventorySummaryFilterDto inventorySummaryFilterDto, int householdAudienceId)
        {
            var inventorySources = GetInventorySources();
            var quarter = inventorySummaryFilterDto.Quarter.Quarter;
            var year = inventorySummaryFilterDto.Quarter.Year;
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(quarter, year);

            foreach (var inventorySource in inventorySources)
            {
                if (_FilterBySourceType(inventorySummaryFilterDto, inventorySource))
                    continue;

                var manifests = _GetInventorySummaryManifests(inventorySource, quarterDetail);
                var daypartCodes = manifests.Select(m => m.DaypartCode).Distinct().ToList();

                if (_FilterByDaypartCode(daypartCodes, inventorySummaryFilterDto.DaypartCodeId))
                    continue;

                yield return _CreateInventorySummary(inventorySource, householdAudienceId, quarterDetail, manifests);
            }
        }

        private List<InventorySummaryManifestDto> _GetInventorySummaryManifests(InventorySource inventorySource,
                                                                                QuarterDetailDto quarterDetail)
        {
            if (inventorySource.InventoryType == InventorySourceTypeEnum.Barter)
            {
                return _InventorySummaryRepository.GetInventorySummaryManifestsForBarterSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.OpenMarket)
            {
                return _InventorySummaryRepository.GetInventorySummaryManifestsForOpenMarketSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.ProprietaryOAndO)
            {
                return _InventorySummaryRepository.GetInventorySummaryManifestsForProprietaryOAndOSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.Syndication ||
                     inventorySource.InventoryType == InventorySourceTypeEnum.Diginet)
            {
                return _InventorySummaryRepository.GetInventorySummaryManifestsForSyndicationOrDiginetSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
            }

            return new List<InventorySummaryManifestDto>();
        }

        private InventorySummaryDto _CreateInventorySummary(InventorySource inventorySource,
                                                            int householdAudienceId,
                                                            QuarterDetailDto quarterDetail,
                                                            List<InventorySummaryManifestDto> manifests)
        {
            BaseInventorySummaryAbstractFactory inventorySummaryFactory = null;

            if (inventorySource.InventoryType == InventorySourceTypeEnum.Barter)
            {
                inventorySummaryFactory = new BarterInventorySummaryFactory(_InventoryRepository, 
                                                                            _InventorySummaryRepository, 
                                                                            _QuarterCalculationEngine,
                                                                            _ProgramRepository,
                                                                            _MediaMonthAndWeekAggregateCache,
                                                                            _MarketCoverageCache,
                                                                            _InventoryGapCalculationEngine);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.OpenMarket)
            {
                inventorySummaryFactory = new OpenMarketSummaryFactory(_InventoryRepository, 
                                                                       _InventorySummaryRepository, 
                                                                       _QuarterCalculationEngine,
                                                                       _ProgramRepository,
                                                                       _MediaMonthAndWeekAggregateCache,
                                                                       _MarketCoverageCache,
                                                                       _InventoryGapCalculationEngine);               
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.ProprietaryOAndO)
            {
                inventorySummaryFactory = new ProprietaryOAndOSummaryFactory(_InventoryRepository,
                                                                             _InventorySummaryRepository,
                                                                             _QuarterCalculationEngine,
                                                                             _ProgramRepository,
                                                                             _MediaMonthAndWeekAggregateCache,
                                                                             _MarketCoverageCache,
                                                                             _InventoryGapCalculationEngine);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.Syndication)
            {
                inventorySummaryFactory = new SyndicationSummaryFactory(_InventoryRepository,
                                                                        _InventorySummaryRepository,
                                                                        _QuarterCalculationEngine,
                                                                        _ProgramRepository,
                                                                        _MediaMonthAndWeekAggregateCache,
                                                                        _MarketCoverageCache,
                                                                        _InventoryGapCalculationEngine);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.Diginet)
            {
                inventorySummaryFactory = new DiginetSummaryFactory(_InventoryRepository,
                                                                    _InventorySummaryRepository,
                                                                    _QuarterCalculationEngine,
                                                                    _ProgramRepository,
                                                                    _MediaMonthAndWeekAggregateCache,
                                                                    _MarketCoverageCache,
                                                                    _InventoryGapCalculationEngine);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.Diginet)
            {
                inventorySummaryFactory = new DiginetSummaryFactory(_InventoryRepository,
                                                                    _InventorySummaryRepository,
                                                                    _QuarterCalculationEngine,
                                                                    _ProgramRepository,
                                                                    _MediaMonthAndWeekAggregateCache,
                                                                    _MarketCoverageCache,
                                                                    _InventoryGapCalculationEngine);
            }

            return inventorySummaryFactory.CreateInventorySummary(inventorySource, householdAudienceId, quarterDetail, manifests);
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

    }
}
