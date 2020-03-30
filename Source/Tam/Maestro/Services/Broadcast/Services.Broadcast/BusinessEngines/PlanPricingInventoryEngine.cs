using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
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
            IEnumerable<int> inventorySourceIds,
            PlanPricingJobDiagnostic diagnostic);
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
            IEnumerable<int> inventorySourceIds,
            PlanPricingJobDiagnostic diagnostic)
        {
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);
            var planFlightDateRanges = _GetPlanDateRanges(plan);
            var planDisplayDaypartDays = GetPlanDaypartDaysFromPlanFlight(plan, planFlightDateRanges);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);
            var programs = _GetPrograms(plan, planFlightDateRanges, inventorySourceIds, diagnostic);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);
            programs = FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planDisplayDaypartDays);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);
            ApplyInflationFactorToSpotCost(programs, parameters?.InflationFactor);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);
            // Set the plan flight days to programs so impressions are calculated for those days.
            _SetProgramsFlightDays(programs, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);
            _ApplyProjectedImpressions(programs, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);
            _ApplyProvidedImpressions(programs, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);
            ApplyNTIConversionToNSI(plan, programs, planDisplayDaypartDays);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);
            programs = FilterProgramsByMinAndMaxCPM(programs, parameters?.MinCPM, parameters?.MaxCPM);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);

            return programs;
        }

        private void _SetProgramDayparts(List<PlanPricingInventoryProgram> programs)
        {
            var daypartCache = DaypartCache.Instance;

            foreach (var program in programs)
            {
                foreach (var programDaypart in program.ManifestDayparts)
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
                foreach (var manifestDaypart in program.ManifestDayparts)
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
            IEnumerable<int> inventorySourceIds, List<short> availableMarkets, QuarterDetailDto planQuarter, QuarterDetailDto fallbackQuarter)
        {
            var totalInventory = new List<PlanPricingInventoryProgram>();
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

        private List<PlanPricingInventoryProgram> _GetPrograms(
            PlanDto plan, 
            List<DateRange> planFlightDateRanges, 
            IEnumerable<int> inventorySourceIds,
            PlanPricingJobDiagnostic diagnostic)
        {
            var marketCodes = plan.AvailableMarkets.Select(m => m.MarketCode).ToList();
            var planQuarter = _PlanPricingInventoryQuarterCalculatorEngine.GetPlanQuarter(plan);
            var fallbackQuarter = _PlanPricingInventoryQuarterCalculatorEngine.GetInventoryFallbackQuarter();

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FETCHING_NOT_POPULATED_INVENTORY);
            var programs = _GetFullPrograms(planFlightDateRanges, plan.SpotLengthId, inventorySourceIds, marketCodes, planQuarter, fallbackQuarter);

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
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FETCHING_NOT_POPULATED_INVENTORY);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_MATCHING_INVENTORY_WEEKS_WITH_PLAN_WEEKS);
            _SetContractWeekForInventory(programs, planQuarter, fallbackQuarter);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_MATCHING_INVENTORY_WEEKS_WITH_PLAN_WEEKS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_PRIMARY_PROGRAM);
            _SetPrimaryProgramForDayparts(programs);
            _SetPrimaryProgramFallbackForManifestDayparts(programs);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_PRIMARY_PROGRAM);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYPARTS);
            _SetProgramDayparts(programs);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYPARTS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_LATEST_STATION_DETAILS);
            _SetProgramStationLatestMonthDetails(programs);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_LATEST_STATION_DETAILS);

            return programs;
        }

        private void _SetProgramStationLatestMonthDetails(List<PlanPricingInventoryProgram> programs)
        {
            var distinctIds = programs.Select(x => x.Station.Id).Distinct().ToList();
            var latestStationMonthDetails = _StationRepository.GetLatestStationMonthDetailsForStations(distinctIds);

            foreach (var latestMonthDetail in latestStationMonthDetails)
            {
                var stationsToUpdate = programs.Select(x => x.Station).Where(x => x.Id == latestMonthDetail.StationId).ToList();

                foreach (var station in stationsToUpdate)
                {
                    station.Affiliation = latestMonthDetail.Affiliation;
                    station.MarketCode = latestMonthDetail.MarketCode;
                }
            }
        }

        private void _SetPrimaryProgramForDayparts(List<PlanPricingInventoryProgram> manifests)
        {
            var daypartsWithPrimaryProgram = manifests
                .SelectMany(x => x.ManifestDayparts)
                .Where(x => x.PrimaryProgramId != null);

            foreach (var manifestDaypart in daypartsWithPrimaryProgram)
            {
                var primaryProgram = manifestDaypart.Programs.Single(x => x.Id == manifestDaypart.PrimaryProgramId);

                manifestDaypart.PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                {
                    Id = primaryProgram.Id,
                    Name = primaryProgram.Name,
                    Genre = primaryProgram.Genre,
                    ShowType = primaryProgram.ShowType,
                    StartTime = primaryProgram.StartTime,
                    EndTime = primaryProgram.EndTime
                };
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
                    fallbackGenre = "News";
                }

                manifestDaypart.PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                {
                    Name = programName,
                    Genre = fallbackGenre,
                    ShowType = string.Empty
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

        /// <summary>
        /// This works only for OpenMarket. 
        /// When we start using other sources, the PlanPricingInventoryProgram model structure should be reviewed
        /// Other sources may have more than 1 daypart that this logic does not assume
        /// </summary>
        protected List<PlanPricingInventoryProgram> FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs,
            DisplayDaypart planDays)
        {
            var result = new List<PlanPricingInventoryProgram>();

            foreach (var program in programs)
            {
                var planDayparts = _GetPlanDaypartsThatMatchProgramByTimeAndDays(plan.Dayparts, planDays, program);
                planDayparts = _GetPlanDaypartsThatMatchProgramByRestrictions(planDayparts, program);

                var planDaypartWithMostIntersectingTime = _FindPlanDaypartWithMostIntersectingTime(planDayparts);

                if (planDaypartWithMostIntersectingTime != null)
                {
                    program.StandardDaypartId = planDaypartWithMostIntersectingTime.PlanDaypart.DaypartCodeId;

                    result.Add(program);
                }
            }

            return result;
        }

        private ProgramInventoryDaypart _FindPlanDaypartWithMostIntersectingTime(List<ProgramInventoryDaypart> programInventoryDayparts)
        {
            return programInventoryDayparts
                .OrderByDescending(x =>
                {
                    var planDaypartTimeRange = new TimeRange
                    {
                        StartTime = x.PlanDaypart.StartTimeSeconds,
                        EndTime = x.PlanDaypart.EndTimeSeconds
                    };

                    var inventoryDaypartTimeRange = new TimeRange
                    {
                        StartTime = x.ManifestDaypart.Daypart.StartTime,
                        EndTime = x.ManifestDaypart.Daypart.EndTime
                    };

                    return DaypartTimeHelper.GetIntersectingTotalTime(planDaypartTimeRange, inventoryDaypartTimeRange);
                })
                .FirstOrDefault();
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
                var conversionRate = conversionRatesByDaypartCodeId[program.StandardDaypartId];

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

        private List<ProgramInventoryDaypart> _GetPlanDaypartsThatMatchProgramByRestrictions(List<ProgramInventoryDaypart> programInventoryDayparts, PlanPricingInventoryProgram program)
        {
            var result = new List<ProgramInventoryDaypart>();

            foreach (var inventoryDaypart in programInventoryDayparts)
            {
                if (!_IsProgramAllowedByProgramRestrictions(inventoryDaypart))
                    continue;

                if (!_IsProgramAllowedByGenreRestrictions(inventoryDaypart))
                    continue;

                if (!_IsProgramAllowedByShowTypeRestrictions(inventoryDaypart))
                    continue;

                if (!_IsProgramAllowedByAffiliateRestrictions(inventoryDaypart, program))
                    continue;

                result.Add(inventoryDaypart);
            }

            return result;
        }

        private bool _IsProgramAllowedByAffiliateRestrictions(ProgramInventoryDaypart programInventoryDaypart, PlanPricingInventoryProgram program)
        {
            var affiliateRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.AffiliateRestrictions;

            if (affiliateRestrictions == null || affiliateRestrictions.Affiliates.IsEmpty())
                return true;

            var restrictedAffiliates = affiliateRestrictions.Affiliates.Select(x => x.Display);
            var hasIntersections = restrictedAffiliates.Contains(program.Station.Affiliation);

            return affiliateRestrictions.ContainType == ContainTypeEnum.Include ?
                 hasIntersections :
                !hasIntersections;
        }

        private bool _IsProgramAllowedByProgramRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var programRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ProgramRestrictions;

            if (programRestrictions == null || programRestrictions.Programs.IsEmpty())
                return true;

            var restrictedProgramNames = programRestrictions.Programs.Select(x => x.Name);

            if (programRestrictions.ContainType == ContainTypeEnum.Include)
            {
                var primaryProgramName = programInventoryDaypart.ManifestDaypart.PrimaryProgram.Name;

                return restrictedProgramNames.Contains(primaryProgramName);
            }
            else
            {
                var manifestDaypartProgramNames = programInventoryDaypart.ManifestDaypart.Programs.Select(x => x.Name);
                var hasIntersections = restrictedProgramNames.ContainsAny(manifestDaypartProgramNames);

                return !hasIntersections;
            }
        }

        private bool _IsProgramAllowedByGenreRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var genreRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.GenreRestrictions;

            if (genreRestrictions == null || genreRestrictions.Genres.IsEmpty())
                return true;

            var restrictedGenres = genreRestrictions.Genres.Select(x => x.Display);

            if (genreRestrictions.ContainType == ContainTypeEnum.Include)
            {
                var primaryProgramGenre = programInventoryDaypart.ManifestDaypart.PrimaryProgram.Genre;

                return restrictedGenres.Contains(primaryProgramGenre);
            }
            else
            {
                var manifestDaypartProgramGenres = programInventoryDaypart.ManifestDaypart.Programs.Select(x => x.Genre);
                var hasIntersections = restrictedGenres.ContainsAny(manifestDaypartProgramGenres);

                return !hasIntersections;
            }
        }

        private bool _IsProgramAllowedByShowTypeRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var showTypeRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ShowTypeRestrictions;

            if (showTypeRestrictions == null || showTypeRestrictions.ShowTypes.IsEmpty())
                return true;

            var restrictedShowTypes = showTypeRestrictions.ShowTypes.Select(x => x.Display);

            if (showTypeRestrictions.ContainType == ContainTypeEnum.Include)
            {
                var primaryProgramShowType = programInventoryDaypart.ManifestDaypart.PrimaryProgram.ShowType;

                return restrictedShowTypes.Contains(primaryProgramShowType);
            }
            else
            {
                var manifestDaypartShowTypes = programInventoryDaypart.ManifestDaypart.Programs.Select(x => x.ShowType);
                var hasIntersections = restrictedShowTypes.ContainsAny(manifestDaypartShowTypes);

                return !hasIntersections;
            }
        }

        private List<ProgramInventoryDaypart> _GetPlanDaypartsThatMatchProgramByTimeAndDays(
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
