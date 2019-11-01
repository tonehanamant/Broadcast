using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.PlanPricing;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPlanPricingService : IApplicationService
    {
        List<PlanPricingProgramDto> Run(int planId);
    }

    public class PlanPricingService : IPlanPricingService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDaypartCache _DaypartCache;
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IImpressionsCalculationEngine _ImpressionsCalculationEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly ISpotLengthRepository _SpotLengthRepository;

        public PlanPricingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                  IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                  IDaypartCache daypartCache,
                                  IBroadcastAudiencesCache audienceCache,
                                  IImpressionAdjustmentEngine impressionAdjustmentEngine,
                                  IImpressionsCalculationEngine impressionsCalculationEngine,
                                  ISpotLengthEngine spotLengthEngine)
        {
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DaypartCache = daypartCache;
            _AudienceCache = audienceCache;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _ImpressionsCalculationEngine = impressionsCalculationEngine;
            _SpotLengthEngine = spotLengthEngine;
        }

        public List<PlanPricingProgramDto> Run(int planId)
        {
            var plan = _PlanRepository.GetPlan(planId);
            var programs = _GetPrograms(plan);
            programs = _FilterProgramsByDaypart(plan, programs);
            _ApplyProjectedImpressions(programs, plan);
            _ApplyProvidedImpressions(programs, plan);

            var pricingModelInputDto = new PricingModelInputDto
            {
                Weeks = _GetPricingModelWeeks(plan),
                Spots = _GetPricingModelSpots(programs)
            };

            return _MapToPlanPricingPrograms(programs, plan);
        }

        private List<PlanPricingProgramDto> _MapToPlanPricingPrograms(List<ProposalProgramDto> programs, PlanDto plan)
        {
            var pricingPrograms = new List<PlanPricingProgramDto>();
            var spotLength = _SpotLengthEngine.GetSpotLengthValueById(plan.SpotLengthId);
            var spotLengthsMultipliers = _SpotLengthRepository.GetSpotLengthMultipliers();
            var deliveryMultiplier = spotLengthsMultipliers.Single(s => s.Key == spotLength);

            foreach (var program in programs)
            {
                var programNames = program.ManifestDayparts.Select(d => d.ProgramName);
                var planMarket = plan.AvailableMarkets.FirstOrDefault(m => m.Id == program.Market.Id);
                var marketShareOfVoice = 0d;

                if (planMarket != null)
                    marketShareOfVoice = planMarket.ShareOfVoicePercent ?? 0;

                var pricingProgram = new PlanPricingProgramDto
                {
                    ProgramNames = new List<string>(programNames),
                    SpotLength = spotLength,
                    DeliveryMultiplier = deliveryMultiplier.Value,
                    Station = program.Station,
                    Rate = program.SpotCost,
                    PlanPricingMarket = new PlanPricingMarket
                    {
                        MarketId = program.Market.Id,
                        MarketName = program.Market.Display,
                        MarketShareOfVoice = marketShareOfVoice,
                        // Random value for now.
                        MarketSegment = program.ManifestId % 4 + 1
                    },
                    GuaranteedImpressions = program.ProvidedUnitImpressions ?? 0,
                    ProjectedImpressions = program.UnitImpressions,
                };

                pricingPrograms.Add(pricingProgram);
            }

            return pricingPrograms;
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

        private List<PricingModelSpotsDto> _GetPricingModelSpots(List<ProposalProgramDto> programs)
        {
            var pricingModelSpots = new List<PricingModelSpotsDto>();
            var householdAudienceId = _AudienceCache.GetDefaultAudience().Id;

            foreach (var program in programs)
            {
                foreach (var programWeek in program.FlightWeeks)
                {
                    pricingModelSpots.Add(new PricingModelSpotsDto
                    {
                        Id = program.ManifestId,
                        MediaWeekId = programWeek.MediaWeekId,
                        Impressions = program.ProvidedUnitImpressions ?? 0,
                        Cost = program.SpotCost
                    });
                }
            }

            return pricingModelSpots;
        }

        private List<PricingModelWeekInputDto> _GetPricingModelWeeks(PlanDto plan)
        {
            var pricingModelWeeks = new List<PricingModelWeekInputDto>();

            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                pricingModelWeeks.Add(new PricingModelWeekInputDto
                {
                    MediaWeekId = week.MediaWeekId,
                    Impressions = week.Impressions
                });
            }

            return pricingModelWeeks;
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
    }
}
