using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using static Services.Broadcast.Entities.ProposalProgramDto;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingInventoryEngine : IApplicationService
    {
        List<PlanPricingInventoryProgram> GetInventoryForPlan(PlanDto plan);
    }

    public class PlanPricingInventoryEngine : IPlanPricingInventoryEngine
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IImpressionsCalculationEngine _ImpressionsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDaypartCache _DaypartCache;

        public PlanPricingInventoryEngine(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                          IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                          IImpressionsCalculationEngine impressionsCalculationEngine,
                                          IDaypartCache daypartCache)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _ImpressionsCalculationEngine = impressionsCalculationEngine;
            _DaypartCache = daypartCache;
        }

        public List<PlanPricingInventoryProgram> GetInventoryForPlan(PlanDto plan)
        {
            var planFlightDateRanges = _GetPlanDateRanges(plan);
            var programs = _GetPrograms(plan, planFlightDateRanges);
            programs = _FilterProgramsByDaypart(plan, programs, planFlightDateRanges);
            _ApplyProjectedImpressions(programs, plan);
            _ApplyProvidedImpressions(programs, plan);
            return programs;
        }

        private List<PlanPricingInventoryProgram> _GetPrograms(PlanDto plan, List<DateRange> planFlightDateRanges)
        {
            var supportedInventorySourceTypes = _GetSupportedInventorySourceTypes();
            var marketCodes = plan.AvailableMarkets.Select(m => (int)m.MarketCode).ToList();
            var programs = new List<PlanPricingInventoryProgram>();

            foreach (var planDateRange in planFlightDateRanges)
            {
                var programsForDateRange = _StationProgramRepository.GetProgramsForPricingModel(
                    planDateRange.Start.Value,
                    planDateRange.End.Value,
                    plan.SpotLengthId,
                    supportedInventorySourceTypes,
                    marketCodes);

                programs.AddRange(programsForDateRange);
            }

            // so that programs are not repeated
            programs = programs.GroupBy(x => x.ManifestId).Select(x =>
            {
                var first = x.First();

                first.MediaWeekIds = x.SelectMany(g => g.MediaWeekIds).Distinct().ToList();

                return first;
            }).ToList();

            return programs;
        }

        private List<int> _GetSupportedInventorySourceTypes()
        {
            var result = new List<InventorySourceTypeEnum>();

            if (BroadcastServiceSystemParameter.EnableOpenMarketInventoryForPricingModel)
                result.Add(InventorySourceTypeEnum.OpenMarket);

            if (BroadcastServiceSystemParameter.EnableBarterInventoryForPricingModel)
                result.Add(InventorySourceTypeEnum.Barter);

            if (BroadcastServiceSystemParameter.EnableProprietaryOAndOInventoryForPricingModel)
                result.Add(InventorySourceTypeEnum.ProprietaryOAndO);

            return result.Select(x => (int)x).ToList();
        }

        private List<DateRange> _GetPlanDateRanges(PlanDto plan)
        {
            var planStartDate = plan.FlightStartDate.Value;
            var planEndDate = plan.FlightEndDate.Value;
            var rangeStartDate = plan.FlightStartDate.Value;
            var rangeEndDate = rangeStartDate;
            var dateRanges = new List<DateRange>();
            var dateDifference = planEndDate - rangeStartDate;
            var dateDifferenceDays = dateDifference.Days;
            var currentDate = rangeStartDate;

            if (!plan.FlightHiatusDays.Any())
            {
                dateRanges.Add(new DateRange(rangeStartDate, planEndDate));

                return dateRanges;
            }

            for (var daysIndex = 0; daysIndex <= dateDifferenceDays; daysIndex++)
            {
                var hiatusDate = planStartDate.AddDays(daysIndex);
                var isHiatus = plan.FlightHiatusDays.Any(h => h == hiatusDate);

                if (!isHiatus)
                    rangeEndDate = planStartDate.AddDays(daysIndex);

                while (isHiatus)
                {
                    daysIndex++;
                    hiatusDate = planStartDate.AddDays(daysIndex);
                    isHiatus = plan.FlightHiatusDays.Any(h => h == hiatusDate);

                    if (!isHiatus)
                    {
                        dateRanges.Add(new DateRange(rangeStartDate, rangeEndDate));
                        rangeStartDate = planStartDate.AddDays(daysIndex);
                        rangeEndDate = rangeStartDate;
                    }
                }

                if (daysIndex == dateDifferenceDays)
                {
                    dateRanges.Add(new DateRange(rangeStartDate, rangeEndDate));
                }
            }

            return dateRanges;
        }

        private List<PlanPricingInventoryProgram> _FilterProgramsByDaypart(
            PlanDto plan, 
            List<PlanPricingInventoryProgram> programs,
            List<DateRange> planFlightDateRanges)
        {
            if (plan.Dayparts == null || !plan.Dayparts.Any())
                return programs;

            var filteredPrograms = new List<PlanPricingInventoryProgram>();
            var planDisplayDaypart = _GetPlanDaypartDaysFromPlanFlight(plan, planFlightDateRanges);

            foreach (var planDaypart in plan.Dayparts)
            {
                planDisplayDaypart.StartTime = planDaypart.StartTimeSeconds;
                planDisplayDaypart.EndTime = planDaypart.EndTimeSeconds;

                var programsThatMatchDaypart = programs.Where(p => _AnyIntersects(p.ManifestDayparts, planDisplayDaypart)).ToList();

                filteredPrograms.AddRange(programsThatMatchDaypart);
            }

            // prevents the same programs being added because of 2 plan dayparts that match program
            filteredPrograms = filteredPrograms
                .GroupBy(x => x.ManifestId)
                .Select(x => x.First())
                .ToList();

            return filteredPrograms;
        }

        private DisplayDaypart _GetPlanDaypartDaysFromPlanFlight(PlanDto plan, List<DateRange> planFlightDateRanges)
        {
            var result = new DisplayDaypart
            {
                Monday = false,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = false,
                Saturday = false,
                Sunday = false
            };

            foreach (var dateRange in planFlightDateRanges)
            {
                var start = dateRange.Start.Value;
                var end = dateRange.End.Value;

                while(start <= end)
                {
                    if (start.DayOfWeek == DayOfWeek.Monday)
                        result.Monday = true;

                    if (start.DayOfWeek == DayOfWeek.Tuesday)
                        result.Tuesday = true;

                    if (start.DayOfWeek == DayOfWeek.Wednesday)
                        result.Wednesday = true;

                    if (start.DayOfWeek == DayOfWeek.Thursday)
                        result.Thursday = true;

                    if (start.DayOfWeek == DayOfWeek.Friday)
                        result.Friday = true;

                    if (start.DayOfWeek == DayOfWeek.Saturday)
                        result.Saturday = true;

                    if (start.DayOfWeek == DayOfWeek.Sunday)
                        result.Sunday = true;

                    if (result.ActiveDays == 7)
                        return result;

                    start = start.AddDays(1);
                }
            }
            
            return result;
        }

        private bool _AnyIntersects(List<ManifestDaypartDto> dayparts, DisplayDaypart daypart)
        {
            return dayparts.Any(x => _DaypartCache.GetDisplayDaypart(x.DaypartId).Intersects(daypart));
        }

        private void _ApplyProvidedImpressions(List<PlanPricingInventoryProgram> programs, PlanDto plan)
        {
            _ImpressionsCalculationEngine.ApplyProvidedImpressions(programs, plan.AudienceId, plan.SpotLengthId, plan.Equivalized);
        }

        private void _ApplyProjectedImpressions(IEnumerable<PlanPricingInventoryProgram> programs, PlanDto plan)
        {
            var impressionsRequest = new ImpressionsRequestDto
            {
                Equivalized = plan.Equivalized,
                HutProjectionBookId = plan.HUTBookId,
                PlaybackType = ProposalPlaybackType.LivePlus3,
                PostType = plan.PostingType,
                ShareProjectionBookId = plan.ShareBookId,
                SpotLengthId = plan.SpotLengthId
            };

            _ImpressionsCalculationEngine.ApplyProjectedImpressions(programs, impressionsRequest, plan.AudienceId);
        }
    }
}
