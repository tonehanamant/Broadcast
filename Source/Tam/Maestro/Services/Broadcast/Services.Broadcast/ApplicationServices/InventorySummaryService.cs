using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters.InventorySummary;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventorySummaryService : IApplicationService
    {
        List<InventorySource> GetInventorySources();
        InventorySummaryQuarterFilterDto GetInventoryQuarters(DateTime currentDate);
        List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate);
    }

    public class InventorySummaryService : IInventorySummaryService
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventorySummaryRepository _InventorySummaryRepository;
        private readonly IProgramRepository _ProgramRepository;

        public InventorySummaryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                       IQuarterCalculationEngine quarterCalculationEngine,
                                       IBroadcastAudiencesCache audiencesCache)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;
            _AudiencesCache = audiencesCache;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>(); ;
            _InventorySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventorySummaryRepository>();
            _ProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramRepository>();
        }

        public List<InventorySource> GetInventorySources()
        {
            var inventorySources = _InventoryRepository.GetInventorySources();

            return inventorySources.Where(x => x.InventoryType == InventorySourceTypeEnum.Barter ||
                                               x.InventoryType == InventorySourceTypeEnum.OpenMarket).ToList();
        }

        public InventorySummaryQuarterFilterDto GetInventoryQuarters(DateTime currentDate)
        {
            return new InventorySummaryQuarterFilterDto
            {
                Quarters = GetInventorySummaryQuarters(currentDate),
                DefaultQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate)
            };
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

            return inventorySummaryDtos;
        }

        private IEnumerable<InventorySummaryDto> _CreateInventorySummariesForSource(InventorySummaryFilterDto inventorySummaryFilterDto, int householdAudienceId, DateTime currentDate)
        {
            var inventorySourceId = inventorySummaryFilterDto.InventorySourceId.Value;
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
            var inventorySourceDateRange = GetInventorySourceOrCurrentQuarterDateRange(inventorySourceId, currentDate);
            var allQuartersBetweenDates = 
                _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventorySourceDateRange.Start.Value, 
                                                                     inventorySourceDateRange.End.Value);

            foreach (var quarter in allQuartersBetweenDates)
            {
                yield return _CreateInventorySummary(inventorySource, householdAudienceId, quarter);
            }
        }

        private IEnumerable<InventorySummaryDto> _CreateInventorySummaryForQuarter(InventorySummaryFilterDto inventorySummaryFilterDto, int householdAudienceId)
        {
            var inventorySources = GetInventorySources();
            var quarter = inventorySummaryFilterDto.Quarter.Quarter;
            var year = inventorySummaryFilterDto.Quarter.Year;
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(quarter, year);

            foreach (var inventorySource in inventorySources)
            {
                yield return _CreateInventorySummary(inventorySource, householdAudienceId, quarterDetail);
            }
        }

        private InventorySummaryDto _CreateInventorySummary(InventorySource inventorySource,
                                                         int householdAudienceId,
                                                         QuarterDetailDto quarterDetail)
        {
            BaseInventorySummaryAbstractFactory inventorySummaryFactory = null;

            if (inventorySource.InventoryType == InventorySourceTypeEnum.Barter)
            {
                inventorySummaryFactory = new ProprietaryInventorySummaryFactory(_InventoryRepository, 
                                                                                 _InventorySummaryRepository, 
                                                                                 _QuarterCalculationEngine);
            }
            else if (inventorySource.InventoryType == InventorySourceTypeEnum.OpenMarket)
            {
                inventorySummaryFactory = new OpenMarketSummaryFactory(_InventoryRepository, 
                                                                       _InventorySummaryRepository, 
                                                                       _QuarterCalculationEngine, 
                                                                       _ProgramRepository);               
            }

            return inventorySummaryFactory.CreateInventorySummary(inventorySource, householdAudienceId, quarterDetail);
        }

        private List<InventorySummaryQuarter> GetInventorySummaryQuarters(DateTime currentDate)
        {
            var inventoriesDateRange = GetAllInventoriesDateRange() ??
                                       GetCurrentQuarterDateRange(currentDate);
            var allQuartersBetweenDates = _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventoriesDateRange.Start.Value, 
                                                                                               inventoriesDateRange.End.Value);
            return allQuartersBetweenDates.Select(x => new InventorySummaryQuarter
            {
                Quarter = x.Quarter,
                Year = x.Year
            }).ToList();
        }

        private DateRange GetInventorySourceOrCurrentQuarterDateRange(int inventorySourceId, DateTime currentDate)
        {
            return GetInventorySourceDateRange(inventorySourceId) ??
                   GetCurrentQuarterDateRange(currentDate);
        }

        private DateRange GetAllInventoriesDateRange()
        {
            var allInventoriesDateRange = _InventoryRepository.GetAllInventoriesDateRange();

            return GetDateRangeOrNull(allInventoriesDateRange);
        }

        public DateRange GetInventorySourceDateRange(int inventorySourceId)
        {
            var inventorySourceDateRange = _InventoryRepository.GetInventorySourceDateRange(inventorySourceId);

            return GetDateRangeOrNull(inventorySourceDateRange);
        }

        public DateRange GetDateRangeOrNull(DateRange dateRange)
        {
            var startDate = dateRange.Start;

            if (!startDate.HasValue)
                return null;

            var startDateValue = startDate.Value;
            var endDate = dateRange.End == null ? startDateValue.AddYears(1) : dateRange.End.Value;

            return new DateRange(startDateValue, endDate);
        }

        public DateRange GetCurrentQuarterDateRange(DateTime currentDate)
        {
            var datesTuple = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, currentDate);
            return new DateRange(datesTuple.Item1, datesTuple.Item2);
        }
    }
}
