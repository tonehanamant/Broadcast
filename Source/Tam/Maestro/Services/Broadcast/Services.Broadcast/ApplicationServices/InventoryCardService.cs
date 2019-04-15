using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryCardService : IApplicationService
    {
       InventoryCardInitialData GetInitialData(DateTime currentDate);
        List<InventoryCardDto> GetInventoryCards(InventoryCardFilterDto inventoryCardFilterDto, DateTime currentDate);
        string GetInventoryLogo();
        void GetInventoryCardDetails();
    }

    public class InventoryCardService : IInventoryCardService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IInventoryCardRepository _InventoryCardRepository;

        public InventoryCardService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                    IQuarterCalculationEngine quarterCalculationEngine,
                                    IBroadcastAudiencesCache audiencesCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _AudiencesCache = audiencesCache;
            _InventoryCardRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryCardRepository>();
        }

        public InventoryCardInitialData GetInitialData(DateTime currentDate)
        {
            return new InventoryCardInitialData
            {
                InventorySources = _InventoryCardRepository.GetAllInventorySources(),
                Quarters = _GetInitialDataQuarters(currentDate),
                DefaultQuarter = _GetDefaultQuarter(currentDate)
            }; ;
        }     

        public List<InventoryCardDto> GetInventoryCards(InventoryCardFilterDto inventoryCardFilterDto, DateTime currentDate)
        {
            var inventoryCardDtos = new List<InventoryCardDto>();
            var houseHoldAudience = _AudiencesCache.GetDisplayAudienceByCode(BroadcastConstants.HOUSEHOLD_CODE);
            var householdAudienceId = houseHoldAudience.Id;

            if (inventoryCardFilterDto.InventorySourceId == null)
            {
                var inventorySources = _InventoryCardRepository.GetAllInventorySources();
                var quarter = inventoryCardFilterDto.Quarter.Quarter;
                var year = inventoryCardFilterDto.Quarter.Year;
                var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(quarter, year);

                foreach (var inventorySource in inventorySources)
                { 
                    inventoryCardDtos.Add(_CreateInventoryCard(inventorySource, householdAudienceId, quarterDetail));
                }
            }
            else
            {
                var inventorySourceId = inventoryCardFilterDto.InventorySourceId.Value;
                var inventorySource = _InventoryCardRepository.GetInventorySource(inventorySourceId);
                var inventorySourceDateRange = _GetInventorySourceDateRange(inventorySourceId) ?? 
                                               _GetCurrentQuarterDateRange(currentDate);
                var startDate = inventorySourceDateRange.Item1;
                var endDate = inventorySourceDateRange.Item2;
                var allQuartersBetweenDates = _QuarterCalculationEngine.GetAllQuartersBetweenDates(startDate, endDate);

                foreach (var quarter in allQuartersBetweenDates)
                {
                    inventoryCardDtos.Add(_CreateInventoryCard(inventorySource, householdAudienceId, quarter));
                }
            }

            return inventoryCardDtos;
        }

        public string GetInventoryLogo()
        {
            throw new NotImplementedException();
        }

        public void GetInventoryCardDetails()
        {
            throw new NotImplementedException();
        }

        private InventoryCardQuarter _GetDefaultQuarter(DateTime dateTime)
        {
            return _GetInventoryQuarter(_GetQuarterDetailForDate(dateTime));
        }

        private List<InventoryCardQuarter> _GetInitialDataQuarters(DateTime currentDate)
        {
            var inventoriesDateRange = _GetAllInventoriesDateRange() ?? 
                                       _GetCurrentQuarterDateRange(currentDate);
            var allQuartersBetweenDates = _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventoriesDateRange.Item1, inventoriesDateRange.Item2);
            return allQuartersBetweenDates.Select(x => new InventoryCardQuarter
            {
                Quarter = x.Quarter,
                Year = x.Year
            }).ToList();
        }

        private Tuple<DateTime, DateTime> _GetCurrentQuarterDateRange(DateTime currentDate)
        {
            var dateRangeTuple = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, currentDate);

            return new Tuple<DateTime, DateTime>(dateRangeTuple.Item1, dateRangeTuple.Item2);
        }

        private Tuple<DateTime, DateTime> _GetAllInventoriesDateRange()
        {
            var allInventoriesDateRange = _InventoryCardRepository.GetAllInventoriesDateRange();

            return _GetDateRangeTupleOrNull(allInventoriesDateRange);
        }

        private Tuple<DateTime, DateTime> _GetInventorySourceDateRange(int inventorySourceId)
        {
            var inventorySourceDateRange = _InventoryCardRepository.GetInventorySourceDateRange(inventorySourceId);

            return _GetDateRangeTupleOrNull(inventorySourceDateRange);
        }

        private Tuple<DateTime, DateTime> _GetDateRangeTupleOrNull(Tuple<DateTime?, DateTime?> dateRangeTuple)
        {
            var startDate = dateRangeTuple.Item1;

            if (!startDate.HasValue)
                return null;

            var startDateValue = startDate.Value;
            var endDate = dateRangeTuple.Item2 == null ? startDateValue.AddYears(1) : dateRangeTuple.Item2.Value;

            return new Tuple<DateTime, DateTime>(startDateValue, endDate);
        }

        private InventoryCardDto _CreateInventoryCard(InventorySource inventorySource,
                                                      int householdAudienceId,
                                                      QuarterDetailDto quarterDetail)
        {
            var quarterDateRangeTuple = quarterDetail.QuarterDateRangeTuple;
            var inventoryCardManifestDtos = _InventoryCardRepository.GetInventoryCardManifests(inventorySource, quarterDateRangeTuple);
            var hasRatesAvailable = inventoryCardManifestDtos.Count() > 0;
            var manifestIds = inventoryCardManifestDtos.Select(x => x.ManifestId).ToList();
            var inventoryFileIds = inventoryCardManifestDtos.Where(x => x.FileId != null).Select(x => (int)x.FileId).ToList();
            var householdImpressions = _InventoryCardRepository.GetInventoryCardHouseholdImpressions(manifestIds, householdAudienceId);
            var inventoryCardManifestFileDtos = _InventoryCardRepository.GetInventoryCardManifestFileDtos(inventoryFileIds);
            var inventoryPostingBooks = _GetInventoryPostingBooks(inventoryCardManifestFileDtos);
            var inventoryQuarter = _GetInventoryQuarter(quarterDetail);
            var totalMarkets = inventoryCardManifestDtos.GroupBy(x => x.MarketCode).Count();
            var totalStations = inventoryCardManifestDtos.GroupBy(x => x.StationId).Count();
            var totalDaypartsCodes = inventoryCardManifestDtos.GroupBy(x => x.DaypartCode).Count();
            var totalUnits = inventoryCardManifestDtos.Count();
            var inventorySourceDateRange = _GetInventorySourceDateRange(inventorySource.Id);
            var ratesAvailableFrom = inventorySourceDateRange?.Item1;
            var ratesAvailableTo = inventorySourceDateRange?.Item2;
            var ratesAvailableFromQuarterDetail = _GetQuarterDetailForDate(ratesAvailableFrom);
            var ratesAvailableToQuarterDetail = _GetQuarterDetailForDate(ratesAvailableTo);
            var ratesAvailableFromInventoryQuarter = _GetInventoryQuarter(ratesAvailableFromQuarterDetail);
            var ratesAvailableToInventoryQuarter = _GetInventoryQuarter(ratesAvailableToQuarterDetail);
            var lastUpdatedDate = inventoryCardManifestFileDtos.Max(x => (DateTime?)x.CreatedDate);
            var isUpdating = inventoryCardManifestFileDtos.Any(x => x.Status == FileStatusEnum.Pending);

            return new InventoryCardDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = hasRatesAvailable,
                Quarter = inventoryQuarter,
                TotalMarkets = totalMarkets,
                TotalStations = totalStations,
                TotalDaypartCodes = totalDaypartsCodes,
                TotalUnits = totalUnits,
                HouseholdImpressions = householdImpressions,
                InventoryPostingBooks = inventoryPostingBooks,
                LastUpdatedDate = lastUpdatedDate,
                IsUpdating = isUpdating,
                RatesAvailableFromQuarter = ratesAvailableFromInventoryQuarter,
                RatesAvailableToQuarter = ratesAvailableToInventoryQuarter
            };
        }

        private List<InventoryCardBooks> _GetInventoryPostingBooks(List<InventoryCardManifestFileDto> inventoryCardManifestFileDtos)
        {
            return inventoryCardManifestFileDtos.
                        Where(x => x.HutProjectionBookId.HasValue || x.ShareProjectionBookId.HasValue).
                        GroupBy(x => new { x.HutProjectionBookId, x.ShareProjectionBookId }).
                        Select(x => new InventoryCardBooks
                        {
                            HutProjectBookId = x.Key.HutProjectionBookId,
                            ShareProjectionBookId = x.Key.ShareProjectionBookId
                        }).
                        ToList();
        }

        private InventoryCardQuarter _GetInventoryQuarter(QuarterDetailDto quarterDetail)
        {
            if (quarterDetail == null)
                return null;

            return new InventoryCardQuarter
            {
                Quarter = quarterDetail.Quarter,
                Year = quarterDetail.Year
            };
        }

        private QuarterDetailDto _GetQuarterDetailForDate(DateTime? datetime)
        {
            if (!datetime.HasValue)
                return null;

            return _QuarterCalculationEngine.GetQuarterRangeByDate(datetime.Value, 0);
        }
    }
}
