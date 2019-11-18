using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingInventoryEngine : IApplicationService
    {
        List<ProposalProgramDto> GetInventoryForPlan(int planId);
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

        public List<ProposalProgramDto> GetInventoryForPlan(int planId)
        {
            var plan = _PlanRepository.GetPlan(planId);
            var programs = _GetPrograms(plan);
            programs = _FilterProgramsByDaypart(plan, programs);
            _ApplyProjectedImpressions(programs, plan);
            _ApplyProvidedImpressions(programs, plan);
            return programs;
        }

        private List<ProposalProgramDto> _GetPrograms(PlanDto plan)
        {
            var marketCodes = plan.AvailableMarkets.Select(m => (int)m.MarketCode).ToList();
            var planDateRanges = _GetPlanDateRanges(plan);
            var programs = new List<ProposalProgramDto>();

            foreach (var planDateRange in planDateRanges)
            {
                programs.AddRange(_StationProgramRepository.GetPrograms(planDateRange.Start.Value,
                                                                        planDateRange.End.Value,
                                                                        plan.SpotLengthId,
                                                                        BroadcastConstants.OpenMarketSourceId,
                                                                        marketCodes));
            }

            _SetFlightWeeks(programs);

            return programs;
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

        private void _SetFlightWeeks(IEnumerable<ProposalProgramDto> programs)
        {
            if (programs.Any())
            {
                var startDate = programs.Min(p => p.StartDate);
                var endDate = programs.Max(p => p.EndDate) ?? DateTime.MaxValue;
                var mediaWeeksToUse = _MediaMonthAndWeekAggregateCache.GetMediaWeeksByFlight(startDate, endDate);

                foreach (var program in programs)
                {
                    program.FlightWeeks = _GetFlightWeeks(program, mediaWeeksToUse);
                }
            }
        }

        private List<ProposalProgramFlightWeek> _GetFlightWeeks(ProposalProgramDto programDto, List<MediaWeek> mediaWeeksToUse = null)
        {
            var nonNullableEndDate = programDto.EndDate ?? programDto.StartDate.AddYears(1);
            var displayFlighWeeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(programDto.StartDate, nonNullableEndDate, mediaWeeksToUse);
            var flighWeeks = new List<ProposalProgramFlightWeek>();

            foreach (var displayMediaWeek in displayFlighWeeks)
            {
                flighWeeks.Add(new ProposalProgramFlightWeek
                {
                    StartDate = displayMediaWeek.WeekStartDate,
                    EndDate = displayMediaWeek.WeekEndDate,
                    MediaWeekId = displayMediaWeek.Id,
                    Rate = programDto.SpotCost
                });
            }

            return flighWeeks;
        }

        private List<ProposalProgramDto> _FilterProgramsByDaypart(PlanDto plan, List<ProposalProgramDto> programs)
        {
            if (plan.Dayparts == null || !plan.Dayparts.Any())
                return programs;

            var filteredPrograms = new List<ProposalProgramDto>();

            foreach (var planDaypart in plan.Dayparts)
            {
                var planDisplayDaypart = new DisplayDaypart
                {
                    Monday = true,
                    Tuesday = true,
                    Wednesday = true,
                    Thursday = true,
                    Friday = true,
                    Saturday = true,
                    Sunday = true,
                    StartTime = planDaypart.StartTimeSeconds,
                    EndTime = planDaypart.EndTimeSeconds
                };

                filteredPrograms.AddRange(
                    programs.Where(p =>
                        p.ManifestDayparts.Any(d => _DaypartCache.GetDisplayDaypart(d.DaypartId).Intersects(planDisplayDaypart))));
            }

            return filteredPrograms;
        }

        private void _ApplyProvidedImpressions(List<ProposalProgramDto> programs, PlanDto plan)
        {
            _ImpressionsCalculationEngine.ApplyProvidedImpressions(programs, plan.AudienceId, plan.SpotLengthId, plan.Equivalized);
        }

        private void _ApplyProjectedImpressions(IEnumerable<ProposalProgramDto> programs, PlanDto plan)
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
