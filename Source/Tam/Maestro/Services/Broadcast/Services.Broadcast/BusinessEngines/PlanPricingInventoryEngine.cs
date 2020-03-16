﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.BusinessEngines.PlanPricingInventoryEngine;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingInventoryEngine : IApplicationService
    {
        List<PlanPricingInventoryProgram> GetInventoryForPlan(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            IEnumerable<int> inventorySourceIds);
    }

    public class PlanPricingInventoryEngine : IPlanPricingInventoryEngine
    {
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IImpressionsCalculationEngine _ImpressionsCalculationEngine;
        private readonly IGenreCache _GenreCache;
        private readonly INtiToNsiConversionRepository _NtiToNsiConversionRepository;
        private readonly IDayRepository _DayRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IPlanPricingInventoryQuarterCalculatorEngine _PlanPricingInventoryQuarterCalculatorEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public PlanPricingInventoryEngine(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                          IImpressionsCalculationEngine impressionsCalculationEngine,
                                          IGenreCache genreCache,
                                          IPlanPricingInventoryQuarterCalculatorEngine planPricingInventoryQuarterCalculatorEngine,
                                          IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _ImpressionsCalculationEngine = impressionsCalculationEngine;
            _GenreCache = genreCache;
            _PlanPricingInventoryQuarterCalculatorEngine = planPricingInventoryQuarterCalculatorEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;

            _NtiToNsiConversionRepository = broadcastDataRepositoryFactory.GetDataRepository<INtiToNsiConversionRepository>();
            _DayRepository = broadcastDataRepositoryFactory.GetDataRepository<IDayRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
        }

        public List<PlanPricingInventoryProgram> GetInventoryForPlan(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            IEnumerable<int> inventorySourceIds)
        {
            var planFlightDateRanges = _GetPlanDateRanges(plan);
            var planDisplayDaypartDays = GetPlanDaypartDaysFromPlanFlight(plan, planFlightDateRanges);
            var programs = _GetPrograms(plan, planFlightDateRanges, inventorySourceIds);

            programs = FilterProgramsByDayparts(plan, programs, planDisplayDaypartDays);
            
            ApplyInflationFactorToSpotCost(programs, parameters?.InflationFactor);
            // Set the plan flight days to programs so impressions are calculated for those days.
            _SetProgramsFlightDays(programs, plan);
            _ApplyProjectedImpressions(programs, plan);
            _ApplyProvidedImpressions(programs, plan);
            ApplyNTIConversionToNSI(plan, programs, planDisplayDaypartDays);

            programs = FilterProgramsByMinAndMaxCPM(programs, parameters?.MinCPM, parameters?.MaxCPM);

            return programs;
        }

        private void _SetProgramDayparts(List<PlanPricingInventoryProgram> programs)
        {
            var daypartCache = DaypartCache.Instance;

            foreach(var program in programs)
            {
                foreach(var programDaypart in program.ManifestDayparts)
                {
                    programDaypart.Daypart = daypartCache.GetDisplayDaypart(programDaypart.Daypart.Id);
                }
            }
        }

        private void _SetProgramsFlightDays(List<PlanPricingInventoryProgram> programs, PlanDto plan)
        {
            var days = _DayRepository.GetDays();
            var flightDayNames = days
                .Where(x => plan.FlightDays.Contains(x.Id))
                .Select(x => x.Name);
            var flightDaysSet = new HashSet<string>(flightDayNames);

            foreach (var program in programs)
            {
                foreach(var manifestDaypart in program.ManifestDayparts)
                {
                    manifestDaypart.Daypart.Sunday = 
                        manifestDaypart.Daypart.Sunday && flightDaysSet.Contains("Sunday");

                    manifestDaypart.Daypart.Monday = 
                        manifestDaypart.Daypart.Monday && flightDaysSet.Contains("Monday");

                    manifestDaypart.Daypart.Tuesday = 
                        manifestDaypart.Daypart.Tuesday && flightDaysSet.Contains("Tuesday");

                    manifestDaypart.Daypart.Wednesday = 
                        manifestDaypart.Daypart.Wednesday && flightDaysSet.Contains("Wednesday");

                    manifestDaypart.Daypart.Thursday = 
                        manifestDaypart.Daypart.Thursday && flightDaysSet.Contains("Thursday");

                    manifestDaypart.Daypart.Friday = 
                        manifestDaypart.Daypart.Friday && flightDaysSet.Contains("Friday");

                    manifestDaypart.Daypart.Saturday = 
                        manifestDaypart.Daypart.Saturday && flightDaysSet.Contains("Saturday");
                }
            }
        }

        protected void ApplyInflationFactorToSpotCost(List<PlanPricingInventoryProgram> programs, double? inflationFactor)
        {
            if (!inflationFactor.HasValue)
                return;

            var fallbackPrograms = programs.Where(p => p.InventoryPricingQuarterType == InventoryPricingQuarterType.Fallback);
            if (fallbackPrograms.Any())
            {
                foreach (var program in fallbackPrograms)
                    program.SpotCost = _CalculateInflationFactor(program.SpotCost, inflationFactor.Value);
            }
            else
            {
                foreach (var program in programs)
                    program.SpotCost = _CalculateInflationFactor(program.SpotCost, inflationFactor.Value);
            }
        }

        private decimal _CalculateInflationFactor(decimal spotCost, double inflationFactor) =>
            spotCost + (spotCost * ((decimal)inflationFactor / 100));

        /// <summary>
        /// Attempts to gather the full inventory set from the plan's quarter or the fallback quarter.
        /// </summary>
        /// <remarks>
        /// This does not  address if a plan's flight extends beyond a single quarter.
        /// </remarks>
        protected List<PlanPricingInventoryProgram> _GetFullPrograms(List<DateRange> dateRanges, int spotLengthId,
            IEnumerable<int> inventorySourceIds, List<short> availableMarkets, QuarterDetailDto planQuarter)
        {
            var totalInventory = new List<PlanPricingInventoryProgram>();
            var fallbackQuarter = _PlanPricingInventoryQuarterCalculatorEngine.GetInventoryFallbackQuarter();  
            
            var availableStations = _StationRepository.GetBroadcastStationsWithLatestDetailsByMarketCodes(availableMarkets);

            foreach (var dateRange in dateRanges)
            {
                var ungatheredStationIds = availableStations.Select(s => s.Id).ToList();

                var inventoryForDateRange = _StationProgramRepository.GetProgramsForPricingModel(
                    dateRange.Start.Value, dateRange.End.Value, spotLengthId, inventorySourceIds,
                    ungatheredStationIds);

                inventoryForDateRange.ForEach(p => p.InventoryPricingQuarterType = InventoryPricingQuarterType.Plan);
                totalInventory.AddRange(inventoryForDateRange);

                var gatheredStationCallsigns = totalInventory.Select(s => s.Station.LegacyCallLetters).Distinct().ToList();
                var fallbackStationIds = availableStations.Where(a => gatheredStationCallsigns.Contains(a.LegacyCallLetters) == false)
                    .Select(s => s.Id).ToList();

                if (fallbackStationIds.Any())
                {
                    // multiple DateRanges can be returned if the PlanQuarter has more weeks than the FallbackQuarter
                    var fallbackDateRanges = _PlanPricingInventoryQuarterCalculatorEngine.GetFallbackDateRanges(dateRange, planQuarter, fallbackQuarter);
                    foreach (var fallbackDateRange in fallbackDateRanges)
                    {
                        var fallbackInventory = _StationProgramRepository.GetProgramsForPricingModel(
                            fallbackDateRange.Start.Value, fallbackDateRange.End.Value, spotLengthId, inventorySourceIds,
                            fallbackStationIds);

                        fallbackInventory.ForEach(p => p.InventoryPricingQuarterType = InventoryPricingQuarterType.Fallback);
                        totalInventory.AddRange(fallbackInventory);
                    }

                    gatheredStationCallsigns = totalInventory.Select(s => s.Station.LegacyCallLetters).Distinct().ToList();
                    var stationsWithoutInventory = availableStations.Where(a => gatheredStationCallsigns.Contains(a.LegacyCallLetters) == false).ToList();

                    if (stationsWithoutInventory.Any())
                    {
                        LogHelper.Logger.Warn($"Unable to gather inventory for DateRange {dateRange.Start.Value.ToString("yyyy-MM-dd")}"
                                              + $" to {dateRange.End.Value.ToString("yyyy-MM-dd")} for {stationsWithoutInventory.Count} stations.");
                    }
                }
            }

            _SetContractWeekForInventory(totalInventory, planQuarter, fallbackQuarter);

            return totalInventory;
        }

        /// <summary>
        /// This method assumes plan has only one quarter 
        /// that is why it associates fallback weeks only with weeks of the first plan quarter
        /// </summary>
        private void _SetContractWeekForInventory(
            List<PlanPricingInventoryProgram> programs,
            QuarterDetailDto planQuarter,
            QuarterDetailDto fallbackQuarter)
        {
            var orderedPlanMediaWeeks = _MediaMonthAndWeekAggregateCache
                .GetMediaWeeksIntersecting(planQuarter.StartDate, planQuarter.EndDate)
                .OrderBy(x => x.StartDate);

            var orderedFallbackMediaWeeks = _MediaMonthAndWeekAggregateCache
                .GetMediaWeeksIntersecting(fallbackQuarter.StartDate, fallbackQuarter.EndDate)
                .OrderBy(x => x.StartDate);

            var lastPlanMediaWeek = orderedPlanMediaWeeks.Last();

            var planMediaWeekIdsByOrder = orderedPlanMediaWeeks
                .Select((item, index) => new { item, index })
                .ToDictionary(x => x.index, x => x.item.Id);

            var fallbackMediaWeekOrderByMediaWeekId = orderedFallbackMediaWeeks
                .Select((item, index) => new { item, index })
                .ToDictionary(x => x.item.Id, x => x.index);

            foreach (var program in programs)
            {
                if (program.InventoryPricingQuarterType == InventoryPricingQuarterType.Fallback)
                {
                    foreach (var week in program.ManifestWeeks)
                    {
                        if (fallbackMediaWeekOrderByMediaWeekId.TryGetValue(week.InventoryMediaWeekId, out var fallbackMediaWeekOrder) &&
                            planMediaWeekIdsByOrder.TryGetValue(fallbackMediaWeekOrder, out var planMediaWeekId))
                        {
                            week.ContractMediaWeekId = planMediaWeekId;
                        }
                        else
                        {
                            // if fallback quarter has more weeks than plan quarter
                            // we associate such out of range weeks with the last plan week
                            week.ContractMediaWeekId = lastPlanMediaWeek.Id;
                        }
                    }
                }
                else
                {
                    // for the plan quarter ContractMediaWeekId is the same as InventoryMediaWeekId
                    program.ManifestWeeks.ForEach(x => x.ContractMediaWeekId = x.InventoryMediaWeekId);
                }
            }
        }

        private List<PlanPricingInventoryProgram> _GetPrograms(PlanDto plan, List<DateRange> planFlightDateRanges, IEnumerable<int> inventorySourceIds)
        {
            var marketCodes = plan.AvailableMarkets.Select(m => m.MarketCode).ToList();
            var planQuarter = _PlanPricingInventoryQuarterCalculatorEngine.GetPlanQuarter(plan);

            var programs = _GetFullPrograms(planFlightDateRanges, plan.SpotLengthId, inventorySourceIds, marketCodes, planQuarter);

            // so that programs are not repeated
            programs = programs.GroupBy(x => x.ManifestId).Select(x =>
            {
                var first = x.First();

                // different calls to _StationProgramRepository.GetProgramsForPricingModel, may fetch different weeks
                first.ManifestWeeks = x
                    .SelectMany(g => g.ManifestWeeks)
                    .GroupBy(w => w.Id)
                    .Select(g => g.First())
                    .ToList();

                return first;
            }).ToList();

            _SetPrimaryProgramFallbackForManifestDayparts(programs);
            _SetProgramDayparts(programs);
            _SetProgramStationLatestMonthDetails(programs);

            return programs;
        }

        private void _SetProgramStationLatestMonthDetails(List<PlanPricingInventoryProgram> programs)
        {
            var distinctIds = programs.Select(x => x.Station.Id).Distinct().ToList();
            var latestStationMonthDetails = _StationRepository.GetLatestStationMonthDetailsForStations(distinctIds);

            foreach (var latestMonthDetail in latestStationMonthDetails)
            {
                var stationsToUpdate = programs.Select(x => x.Station).Where(x => x.Id == latestMonthDetail.StationId).ToList();

                foreach(var station in stationsToUpdate)
                {
                    station.Affiliation = latestMonthDetail.Affiliation;
                    station.MarketCode = latestMonthDetail.MarketCode;
                }
            }
        }

        private void _SetPrimaryProgramFallbackForManifestDayparts(List<PlanPricingInventoryProgram> manifests)
        {
            // If no information from Dativa is available, set up the primary program using the program name from daypart.
            var daypartsWithoutPrimaryPrograms = manifests.SelectMany(x => x.ManifestDayparts).Where(x => x.PrimaryProgram == null);

            foreach (var manifestDaypart in daypartsWithoutPrimaryPrograms)
            {
                var programName = manifestDaypart.ProgramName ?? string.Empty;
                var fallbackGenre = string.Empty;
                
                if (programName.Contains("news", StringComparison.InvariantCultureIgnoreCase))
                {
                    fallbackGenre = "news";
                }

                manifestDaypart.PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                {
                    Name = programName,
                    Genre = fallbackGenre,
                    ShowType = string.Empty,
                };
            }
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

        protected List<PlanPricingInventoryProgram> FilterProgramsByDayparts(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs,
            DisplayDaypart planDisplayDaypartDays)
        {
            var result = new List<PlanPricingInventoryProgram>();

            foreach (var program in programs)
            {
                var inventoryDayparts = _GetInventoryDaypartsThatMatchProgram(plan.Dayparts, planDisplayDaypartDays, program);

                if (inventoryDayparts.Any() && _IsProgramAllowedByRestrictions(inventoryDayparts, program))
                {
                    result.Add(program);
                }
            }

            return result;
        }

        protected void ApplyNTIConversionToNSI(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs,
            DisplayDaypart planDisplayDaypartDays)
        {
            if (plan.PostingType != PostingTypeEnum.NTI)
                return;

            var conversionRatesByDaypartCodeId = _NtiToNsiConversionRepository
                    .GetLatestNtiToNsiConversionRates()
                    .ToDictionary(x => x.DaypartDefaultId, x => x.ConversionRate);

            foreach (var program in programs)
            {
                var inventoryDaypartsThatMatchProgram = _GetInventoryDaypartsThatMatchProgram(plan.Dayparts, planDisplayDaypartDays, program);

                // there should be some plan dayparts that match program otherwise this program would have been filtered out before
                var planDaypartsThatMatchProgram = inventoryDaypartsThatMatchProgram
                        .Select(x => x.PlanDaypart)
                        .Distinct();

                var conversionRate = planDaypartsThatMatchProgram.Average(x => conversionRatesByDaypartCodeId[x.DaypartCodeId]);

                program.ProjectedImpressions = program.ProjectedImpressions * conversionRate;

                if (program.ProvidedImpressions.HasValue)
                    program.ProvidedImpressions = program.ProvidedImpressions * conversionRate;
            }
        }

        protected List<PlanPricingInventoryProgram> FilterProgramsByMinAndMaxCPM(
            List<PlanPricingInventoryProgram> programs,
            decimal? minCPM,
            decimal? maxCPM)
        {
            if (!minCPM.HasValue && !maxCPM.HasValue)
            {
                return programs;
            }

            var result = new List<PlanPricingInventoryProgram>();

            foreach (var program in programs)
            {
                var programCPM =
                    ProposalMath.CalculateCpm(
                        program.SpotCost,
                        program.ProvidedImpressions.HasValue ? program.ProvidedImpressions.Value : program.ProjectedImpressions);

                if (!(minCPM.HasValue && programCPM < minCPM.Value)
                    && !(maxCPM.HasValue && programCPM > maxCPM.Value))
                {
                    result.Add(program);
                }
            }

            return result;
        }

        private bool _IsProgramAllowedByRestrictions(List<ProgramInventoryDaypart> programInventoryDayparts, PlanPricingInventoryProgram program)
        {
            foreach (var inventoryDaypart in programInventoryDayparts)
            {
                if (!_IsProgramAllowedByProgramRestrictions(inventoryDaypart))
                    return false;

                if (!_IsProgramAllowedByGenreRestrictions(inventoryDaypart))
                    return false;

                if (!_IsProgramAllowedByShowTypeRestrictions(inventoryDaypart))
                    return false;

                if (!_IsProgramAllowedByAffiliateRestrictions(inventoryDaypart, program))
                    return false;
            }

            return true;
        }

        private bool _IsProgramAllowedByAffiliateRestrictions(ProgramInventoryDaypart programInventoryDaypart, PlanPricingInventoryProgram program)
        {
            var affiliateRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.AffiliateRestrictions;

            if (affiliateRestrictions == null || affiliateRestrictions.Affiliates.IsEmpty())
                return true;

            var affiliates = affiliateRestrictions.Affiliates.Select(x => x.Display);
            var hasIntersections = affiliates.Contains(program.Station.Affiliation);

            return affiliateRestrictions.ContainType == ContainTypeEnum.Include ?
                 hasIntersections :
                !hasIntersections;
        }

        private bool _IsProgramAllowedByProgramRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var programRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ProgramRestrictions;

            if (programRestrictions == null || programRestrictions.Programs.IsEmpty())
                return true;

            var programNames = programRestrictions.Programs.Select(x => x.Name);
            var manifestDaypartProgramNames = programInventoryDaypart.ManifestDaypart.Programs.Select(x => x.Name);
            var hasIntersections = manifestDaypartProgramNames.ContainsAny(programNames);

            return programRestrictions.ContainType == ContainTypeEnum.Include ?
                hasIntersections :
                !hasIntersections;
        }

        private bool _IsProgramAllowedByGenreRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var genreRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.GenreRestrictions;

            if (genreRestrictions == null || genreRestrictions.Genres.IsEmpty())
                return true;

            var genres = genreRestrictions.Genres.Select(x => x.Display);
            var manifestDaypartGenres = programInventoryDaypart.ManifestDaypart.Programs.Select(x => x.Genre);
            var hasIntersections = manifestDaypartGenres.ContainsAny(genres);

            return genreRestrictions.ContainType == ContainTypeEnum.Include ?
                hasIntersections :
                !hasIntersections;
        }

        private bool _IsProgramAllowedByShowTypeRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var showTypeRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ShowTypeRestrictions;

            if (showTypeRestrictions == null || showTypeRestrictions.ShowTypes.IsEmpty())
                return true;

            var showTypes = showTypeRestrictions.ShowTypes.Select(x => x.Display);
            var manifestDaypartShowTypes = programInventoryDaypart.ManifestDaypart.Programs.Select(x => x.ShowType);
            var hasIntersections = manifestDaypartShowTypes.ContainsAny(showTypes);

            return showTypeRestrictions.ContainType == ContainTypeEnum.Include ?
                hasIntersections :
                !hasIntersections;
        }

        private List<ProgramInventoryDaypart> _GetInventoryDaypartsThatMatchProgram(
            List<PlanDaypartDto> planDayparts,
            DisplayDaypart planDisplayDaypart,
            PlanPricingInventoryProgram program)
        {
            var result = new List<ProgramInventoryDaypart>();

            foreach (var planDaypart in planDayparts)
            {
                planDisplayDaypart.StartTime = planDaypart.StartTimeSeconds;
                planDisplayDaypart.EndTime = planDaypart.EndTimeSeconds;

                var programInventoryDayparts = program.ManifestDayparts
                    .Where(x => x.Daypart.Intersects(planDisplayDaypart))
                    .Select(x => new ProgramInventoryDaypart
                    {
                        PlanDaypart = planDaypart,
                        ManifestDaypart = x
                    });

                result.AddRange(programInventoryDayparts);
            }

            return result;
        }

        protected DisplayDaypart GetPlanDaypartDaysFromPlanFlight(PlanDto plan, List<DateRange> planFlightDateRanges)
        {
            var days = _DayRepository.GetDays();
            var flightDayNames = days
                .Where(x => plan.FlightDays.Contains(x.Id))
                .Select(x => x.Name);

            var flightDaysSet = new HashSet<string>(flightDayNames);
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

                while (start <= end)
                {
                    if (start.DayOfWeek == DayOfWeek.Monday && flightDaysSet.Contains("Monday"))
                        result.Monday = true;

                    if (start.DayOfWeek == DayOfWeek.Tuesday && flightDaysSet.Contains("Tuesday"))
                        result.Tuesday = true;

                    if (start.DayOfWeek == DayOfWeek.Wednesday && flightDaysSet.Contains("Wednesday"))
                        result.Wednesday = true;

                    if (start.DayOfWeek == DayOfWeek.Thursday && flightDaysSet.Contains("Thursday"))
                        result.Thursday = true;

                    if (start.DayOfWeek == DayOfWeek.Friday && flightDaysSet.Contains("Friday"))
                        result.Friday = true;

                    if (start.DayOfWeek == DayOfWeek.Saturday && flightDaysSet.Contains("Saturday"))
                        result.Saturday = true;

                    if (start.DayOfWeek == DayOfWeek.Sunday && flightDaysSet.Contains("Sunday"))
                        result.Sunday = true;

                    if (result.ActiveDays == 7)
                        return result;

                    start = start.AddDays(1);
                }
            }

            return result;
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

        private class ProgramInventoryDaypart
        {
            public PlanDaypartDto PlanDaypart { get; set; }

            public PlanPricingInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
        }

        public class ProgramInventoryOptionalParametersDto
        {
            public decimal? MinCPM { get; set; }

            public decimal? MaxCPM { get; set; }

            public double? InflationFactor { get; set; }

            public double Margin { get; set; }
        }
    }
}
