using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanBuyingInventoryEngine : IApplicationService
    {
        List<PlanBuyingInventoryProgram> GetInventoryForPlan(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            IEnumerable<int> inventorySourceIds,
            PlanBuyingJobDiagnostic diagnostic);
        void ConvertPostingType(PostingTypeEnum postingType, List<PlanBuyingInventoryProgram> programs);
    }

    public class PlanBuyingInventoryEngine : BroadcastBaseClass, IPlanBuyingInventoryEngine
    {
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IImpressionsCalculationEngine _ImpressionsCalculationEngine;
        private readonly INtiToNsiConversionRepository _NtiToNsiConversionRepository;
        private readonly IDayRepository _DayRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IPlanBuyingInventoryQuarterCalculatorEngine _PlanBuyingInventoryQuarterCalculatorEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDaypartCache _DaypartCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IStandardDaypartRepository _StandardDaypartRepository;

        protected Lazy<bool> _UseTrueIndependentStations;
        protected Lazy<int> _ThresholdInSecondsForProgramIntersect;
        protected Lazy<int> _NumberOfFallbackQuarters;

        private Lazy<List<Day>> _CadentDayDefinitions;
        protected Lazy<Dictionary<int, List<int>>> _DaypartDefaultDayIds;
        private readonly Lazy<bool> _IsPricingBuyingProgramsQueryv2;

        public PlanBuyingInventoryEngine(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                          IImpressionsCalculationEngine impressionsCalculationEngine,
                                          IPlanBuyingInventoryQuarterCalculatorEngine planBuyingInventoryQuarterCalculatorEngine,
                                          IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                          IDaypartCache daypartCache,
                                          IQuarterCalculationEngine quarterCalculationEngine,
                                          ISpotLengthEngine spotLengthEngine,
                                          IFeatureToggleHelper featureToggleHelper,
                                          IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _ImpressionsCalculationEngine = impressionsCalculationEngine;
            _PlanBuyingInventoryQuarterCalculatorEngine = planBuyingInventoryQuarterCalculatorEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;

            _NtiToNsiConversionRepository = broadcastDataRepositoryFactory.GetDataRepository<INtiToNsiConversionRepository>();
            _DayRepository = broadcastDataRepositoryFactory.GetDataRepository<IDayRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _DaypartCache = daypartCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _SpotLengthEngine = spotLengthEngine;
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();

            // register lazy delegates - settings
            _ThresholdInSecondsForProgramIntersect = new Lazy<int>(_GetThresholdInSecondsForProgramIntersectInPricing);
            _NumberOfFallbackQuarters = new Lazy<int>(_GetNumberOfFallbackQuarters);
            _IsPricingBuyingProgramsQueryv2 = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PRICING_BUYING_PROGRAMS_QUERY_V2));

            // register lazy delegates - domain data
            _CadentDayDefinitions = new Lazy<List<Day>>(() => _DayRepository.GetDays());
            _DaypartDefaultDayIds = new Lazy<Dictionary<int, List<int>>>(_GetDaypartDefaultDayIds);
        }

        public List<PlanBuyingInventoryProgram> GetInventoryForPlan(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            IEnumerable<int> inventorySourceIds,
            PlanBuyingJobDiagnostic diagnostic)
        {
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);
            var flightDateRanges = _GetFlightDateRanges(
                plan.FlightStartDate.Value,
                plan.FlightEndDate.Value,
                plan.FlightHiatusDays);

            var planDaypartDayIds = _GetDaypartDayIds(plan.Dayparts);

            var daypartDays = _GetDaypartDaysFromFlight(plan.FlightDays, flightDateRanges, planDaypartDayIds);
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);
            var allPrograms = _GetPrograms(plan, flightDateRanges, inventorySourceIds, diagnostic);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);

            if (_FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PRICING_REMOVE_UNMATCHED))
            {
                ProgramRestrictionsHelper.ApplyGeneralFilterForBuyingPrograms(allPrograms);
            }

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);

            var programs = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, allPrograms, daypartDays,
                _CadentDayDefinitions.Value, _DaypartDefaultDayIds.Value, _ThresholdInSecondsForProgramIntersect.Value);

            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);
            _ApplyInflationFactorToSpotCost(programs, parameters?.InflationFactor);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);
            // Set the plan flight days to programs so impressions are calculated for those days.
            _SetProgramsFlightDays(programs, plan.FlightDays);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);
            _ApplyProjectedImpressions(programs, plan, parameters);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);
            _ApplyProvidedImpressions(programs, plan);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);
            _ApplyNTIConversionToNSI(plan, programs);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);
            var finishedPrograms = _CalculateProgramCpmAndFilterByMinAndMaxCpms(programs, parameters?.MinCPM, parameters?.MaxCPM);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);

            return finishedPrograms;
        }

        public void ConvertPostingType(PostingTypeEnum postingType, List<PlanBuyingInventoryProgram> programs)
        {
            foreach (var program in programs)
            {
                program.PostingType = postingType;
            }
        }
        private void _SetProgramDayparts<T>(List<T> programs) where T : BasePlanInventoryProgram
        {
            var daypartIds = programs.SelectMany(p => p.ManifestDayparts)
                .Select(m => m.Daypart.Id)
                .Distinct().ToList();
            var cachedDayparts = _DaypartCache.GetDisplayDayparts(daypartIds);

            foreach (var program in programs)
            {
                foreach (var programDaypart in program.ManifestDayparts)
                {
                    programDaypart.Daypart = cachedDayparts[programDaypart.Daypart.Id];
                }
            }
        }

        private void _SetProgramsFlightDays<T>(List<T> programs, List<int> flightDays) where T : BasePlanInventoryProgram
        {
            var daypartDefaultIds = _DaypartDefaultDayIds.Value;

            foreach (var program in programs)
            {
                var programStandardDaypartDays = daypartDefaultIds[program.StandardDaypartId];
                var coveredDayNamesSet = _GetCoveredDayNamesHashSet(flightDays, programStandardDaypartDays);

                foreach (var manifestDaypart in program.ManifestDayparts)
                {
                    manifestDaypart.Daypart.Sunday =
                        manifestDaypart.Daypart.Sunday && coveredDayNamesSet.Contains("Sunday");

                    manifestDaypart.Daypart.Monday =
                        manifestDaypart.Daypart.Monday && coveredDayNamesSet.Contains("Monday");

                    manifestDaypart.Daypart.Tuesday =
                        manifestDaypart.Daypart.Tuesday && coveredDayNamesSet.Contains("Tuesday");

                    manifestDaypart.Daypart.Wednesday =
                        manifestDaypart.Daypart.Wednesday && coveredDayNamesSet.Contains("Wednesday");

                    manifestDaypart.Daypart.Thursday =
                        manifestDaypart.Daypart.Thursday && coveredDayNamesSet.Contains("Thursday");

                    manifestDaypart.Daypart.Friday =
                        manifestDaypart.Daypart.Friday && coveredDayNamesSet.Contains("Friday");

                    manifestDaypart.Daypart.Saturday =
                        manifestDaypart.Daypart.Saturday && coveredDayNamesSet.Contains("Saturday");
                }
            }
        }

        internal void _ApplyInflationFactorToSpotCost(List<PlanBuyingInventoryProgram> programs, double? inflationFactor)
        {
            if (!inflationFactor.HasValue)
                return;

            var programsToUpdate = programs.Where(p => p.InventoryBuyingQuarterType == InventoryPricingQuarterType.Fallback);

            if (!programsToUpdate.Any())
                programsToUpdate = programs;

            programsToUpdate
                .SelectMany(x => x.ManifestRates)
                .ForEach(x => x.Cost = _CalculateInflationFactor(x.Cost, inflationFactor.Value));
        }

        private decimal _CalculateInflationFactor(decimal spotCost, double inflationFactor) =>
            spotCost + (spotCost * ((decimal)inflationFactor / 100));

        private List<QuoteProgram> _GetFullPrograms(
            List<DateRange> dateRanges,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<short> marketCodes)
        {
            var totalInventory = new List<QuoteProgram>();
            var stations = _StationRepository.GetBroadcastStationsWithLatestDetailsByMarketCodes(marketCodes);

            foreach (var dateRange in dateRanges)
            {
                var ungatheredStationIds = stations.Select(s => s.Id).ToList();

                var inventoryforDateRange = _StationProgramRepository.GetProgramsForQuoteReport(
                    dateRange.Start.Value,
                    dateRange.End.Value,
                    spotLengthIds,
                    inventorySourceIds,
                    ungatheredStationIds);

                totalInventory.AddRange(inventoryforDateRange);
            }

            return totalInventory;
        }

        internal List<PlanBuyingInventoryProgram> _GetProgramsForBuyingModel(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds,
            Guid? logTxId = null)
        {
            var txId = logTxId ?? Guid.NewGuid();

            var querySw = new Stopwatch();
            querySw.Start();

            try
            {
                List<PlanBuyingInventoryProgram> results;
                if (_IsPricingBuyingProgramsQueryv2.Value)
                {
                    _LogInfo("Querying Programs with v2.", txId);
                    // look for inventory from plan quarter
                    results = _StationProgramRepository.GetProgramsForBuyingModelv2(
                        startDate,
                        endDate,
                        spotLengthIds,
                        inventorySourceIds,
                        stationIds);
                }
                else
                {
                    _LogInfo("Querying Programs with v1.", txId);
                    // look for inventory from plan quarter
                    results = _StationProgramRepository.GetProgramsForBuyingModel(
                        startDate,
                        endDate,
                        spotLengthIds,
                        inventorySourceIds,
                        stationIds);
                }

                querySw.Stop();
                var durationMs = querySw.ElapsedMilliseconds;

                var resultMsg = $"Found {results.Count} results in {durationMs}ms.";
                _LogInfo(resultMsg, txId);

                return results;
            }
            catch (Exception ex)
            {
                querySw.Stop();
                var durationMs = querySw.ElapsedMilliseconds;

                var resultMsg = $"Exception caught in {durationMs}ms.";
                _LogError(resultMsg, txId, ex);

                throw new CadentException(resultMsg, ex);
            }
        }

        /// <summary>
        /// Attempts to gather the full inventory set from the plan's quarter or the fallback quarter.
        /// </summary>
        /// <remarks>
        /// This does not  address if a plan's flight extends beyond a single quarter.
        /// </remarks>
        internal List<PlanBuyingInventoryProgram> _GetFullPrograms(
            List<DateRange> dateRanges,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<short> availableMarkets,
            QuarterDetailDto planQuarter,
            List<QuarterDetailDto> fallbackQuarters)
        {
            var totalInventory = new List<PlanBuyingInventoryProgram>();
            var availableStations = _StationRepository.GetBroadcastStationsWithLatestDetailsByMarketCodes(availableMarkets);

            foreach (var dateRange in dateRanges)
            {
                var ungatheredStationIds = availableStations.Select(s => s.Id).ToList();
                var planInventoryForDateRange = _GetProgramsForBuyingModel(
                    dateRange.Start.Value,
                    dateRange.End.Value,
                    spotLengthIds,
                    inventorySourceIds,
                    ungatheredStationIds);

                planInventoryForDateRange.ForEach(p => p.InventoryBuyingQuarterType = InventoryPricingQuarterType.Plan);
                totalInventory.AddRange(planInventoryForDateRange);
                var gatheredStationIds = planInventoryForDateRange.Select(s => s.Station.Id).Distinct().ToList();
                ungatheredStationIds = ungatheredStationIds.Except(gatheredStationIds).ToList();

                // look for inventory from fallback quarters
                foreach (var fallbackQuarter in fallbackQuarters.OrderByDescending(x => x.Year).ThenByDescending(x => x.Quarter))
                {
                    if (!ungatheredStationIds.Any())
                        break;

                    // multiple DateRanges can be returned if the PlanQuarter has more weeks than the FallbackQuarter
                    var fallbackDateRanges = _PlanBuyingInventoryQuarterCalculatorEngine.GetFallbackDateRanges(dateRange, planQuarter, fallbackQuarter);

                    var fallbackInventory = fallbackDateRanges
                        .SelectMany(fallbackDateRange =>
                            _GetProgramsForBuyingModel(
                                fallbackDateRange.Start.Value,
                                fallbackDateRange.End.Value,
                                spotLengthIds,
                                inventorySourceIds,
                                ungatheredStationIds))
                        .ToList();

                    fallbackInventory.ForEach(p => p.InventoryBuyingQuarterType = InventoryPricingQuarterType.Fallback);
                    totalInventory.AddRange(fallbackInventory);
                    gatheredStationIds = fallbackInventory.Select(s => s.Station.Id).Distinct().ToList();
                    ungatheredStationIds = ungatheredStationIds.Except(gatheredStationIds).ToList();
                }

                if (ungatheredStationIds.Any())
                {
                    _LogWarning($"Unable to gather inventory for DateRange {dateRange.Start.Value:yyyy-MM-dd} to {dateRange.End.Value:yyyy-MM-dd} for {ungatheredStationIds.Count} stations.");
                }
            }

            return totalInventory;
        }

        /// <summary>
        /// This method assumes plan has only one quarter 
        /// that is why it associates fallback weeks only with weeks of the first plan quarter
        /// </summary>
        private void _SetContractWeekForInventory(
            List<PlanBuyingInventoryProgram> programs,
            QuarterDetailDto planQuarter,
            List<QuarterDetailDto> fallbackQuarters)
        {
            _SetContractWeekForPlanQuarterInventory(programs);
            _SetContractWeekForFallbackQuarterInventory(programs, planQuarter, fallbackQuarters);
        }

        private void _SetContractWeekForPlanQuarterInventory(List<PlanBuyingInventoryProgram> programs)
        {
            // for the plan quarter ContractMediaWeekId is the same as InventoryMediaWeekId
            programs
                .Where(x => x.InventoryBuyingQuarterType == InventoryPricingQuarterType.Plan)
                .ForEach(x => x.ManifestWeeks.ForEach(w => w.ContractMediaWeekId = w.InventoryMediaWeekId));
        }

        private void _SetContractWeekForFallbackQuarterInventory(
            List<PlanBuyingInventoryProgram> programs,
            QuarterDetailDto planQuarter,
            List<QuarterDetailDto> fallbackQuarters)
        {
            var orderedPlanMediaWeeks = _MediaMonthAndWeekAggregateCache
                .GetMediaWeeksIntersecting(planQuarter.StartDate, planQuarter.EndDate)
                .OrderBy(x => x.StartDate);
            var planMediaWeekIdsByOrder = orderedPlanMediaWeeks
                .Select((item, index) => new { item, index })
                .ToDictionary(x => x.index, x => x.item.Id);
            var lastPlanMediaWeek = orderedPlanMediaWeeks.Last();

            var fallbackInventoryWeeks = programs
                .Where(x => x.InventoryBuyingQuarterType == InventoryPricingQuarterType.Fallback)
                .SelectMany(x => x.ManifestWeeks)
                .ToList();

            foreach (var fallbackQuarter in fallbackQuarters)
            {
                var fallbackMediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(fallbackQuarter.StartDate, fallbackQuarter.EndDate);
                var fallbackMediaWeekOrderByMediaWeekId = fallbackMediaWeeks
                    .OrderBy(x => x.StartDate)
                    .Select((item, index) => new { item, index })
                    .ToDictionary(x => x.item.Id, x => x.index);

                var inventoryWeeksForQuarter = _GetInventoryWeeksByMediaWeeks(fallbackInventoryWeeks, fallbackMediaWeeks);

                foreach (var week in inventoryWeeksForQuarter)
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
        }

        private List<PlanBuyingInventoryProgram.ManifestWeek> _GetInventoryWeeksByMediaWeeks(
            List<PlanBuyingInventoryProgram.ManifestWeek> allInventoryWeeks,
            List<MediaWeek> mediaWeeks)
        {
            var mediaWeekIdsSet = new HashSet<int>(mediaWeeks.Select(x => x.Id));

            return allInventoryWeeks
                .Where(x => mediaWeekIdsSet.Contains(x.InventoryMediaWeekId))
                .ToList();
        }

        private List<QuoteProgram> _GetPrograms(
            List<DateRange> flightDateRanges,
            List<int> inventorySourceIds,
            List<int> spotLengthIds,
            List<short> marketCodes)
        {
            var programs = _GetFullPrograms(
                flightDateRanges,
                spotLengthIds,
                inventorySourceIds,
                marketCodes);

            // so that programs are not repeated
            programs = programs.GroupBy(x => x.ManifestId).Select(x => x.First()).ToList();

            _SetPrimaryProgramForDayparts(programs);
            _SetProgramDayparts(programs);
            _SetProgramStationLatestMonthDetails(programs);

            return programs;
        }

        private List<PlanBuyingInventoryProgram> _GetPrograms(
            PlanDto plan,
            List<DateRange> planFlightDateRanges,
            IEnumerable<int> inventorySourceIds,
            PlanBuyingJobDiagnostic diagnostic)
        {
            var spotLengthIds = plan.CreativeLengths.Select(x => x.SpotLengthId).ToList();

            var marketCodes = plan.AvailableMarkets.Select(m => m.MarketCode).ToList();
            var planQuarter = _PlanBuyingInventoryQuarterCalculatorEngine.GetPlanQuarter(plan);
            var fallbackQuarters = _QuarterCalculationEngine.GetLastNQuarters(
                new QuarterDto { Quarter = planQuarter.Quarter, Year = planQuarter.Year },
                _NumberOfFallbackQuarters.Value);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_FETCHING_NOT_POPULATED_INVENTORY);
            var programs = _GetFullPrograms(planFlightDateRanges, spotLengthIds, inventorySourceIds, marketCodes, planQuarter, fallbackQuarters);

            // so that programs are not repeated
            programs = programs.GroupBy(x => x.ManifestId).Select(x =>
            {
                var first = x.First();

                // different calls to _StationProgramRepository.GetProgramsForBuyingModel, may fetch different weeks
                first.ManifestWeeks = x
                    .SelectMany(g => g.ManifestWeeks)
                    .GroupBy(w => w.Id)
                    .Select(g => g.First())
                    .ToList();

                return first;
            }).ToList();
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_FETCHING_NOT_POPULATED_INVENTORY);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_MATCHING_INVENTORY_WEEKS_WITH_PLAN_WEEKS);
            _SetContractWeekForInventory(programs, planQuarter, fallbackQuarters);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_MATCHING_INVENTORY_WEEKS_WITH_PLAN_WEEKS);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_PRIMARY_PROGRAM);
            _SetPrimaryProgramForDayparts(programs);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SETTING_PRIMARY_PROGRAM);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYPARTS);
            _SetProgramDayparts(programs);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYPARTS);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_LATEST_STATION_DETAILS);
            _SetProgramStationLatestMonthDetails(programs);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SETTING_LATEST_STATION_DETAILS);

            return programs;
        }

        private void _SetProgramStationLatestMonthDetails<T>(List<T> programs) where T : BasePlanInventoryProgram
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

        private void _SetPrimaryProgramForDayparts<T>(List<T> programs) where T : BasePlanInventoryProgram
        {
            foreach (var manifestDaypart in programs.SelectMany(x => x.ManifestDayparts))
            {
                var primaryProgram = manifestDaypart.Programs.Single(x => x.Id == manifestDaypart.PrimaryProgramId);

                manifestDaypart.PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
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

        private List<DateRange> _GetFlightDateRanges(
            DateTime flightStartDate,
            DateTime flightEndDate,
            List<DateTime> flightHiatusDays)
        {
            var rangeStartDate = flightStartDate;
            var rangeEndDate = rangeStartDate;
            var dateRanges = new List<DateRange>();
            var dateDifference = flightEndDate - rangeStartDate;
            var dateDifferenceDays = dateDifference.Days;
            var currentDate = rangeStartDate;

            if (!flightHiatusDays.Any())
            {
                dateRanges.Add(new DateRange(rangeStartDate, flightEndDate));

                return dateRanges;
            }

            for (var daysIndex = 0; daysIndex <= dateDifferenceDays; daysIndex++)
            {
                var hiatusDate = flightStartDate.AddDays(daysIndex);
                var isHiatus = flightHiatusDays.Any(h => h == hiatusDate);

                if (!isHiatus)
                    rangeEndDate = flightStartDate.AddDays(daysIndex);

                while (isHiatus)
                {
                    daysIndex++;
                    hiatusDate = flightStartDate.AddDays(daysIndex);
                    isHiatus = flightHiatusDays.Any(h => h == hiatusDate);

                    if (!isHiatus)
                    {
                        dateRanges.Add(new DateRange(rangeStartDate, rangeEndDate));
                        rangeStartDate = flightStartDate.AddDays(daysIndex);
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

        internal void _ApplyNTIConversionToNSI(
            PlanDto plan,
            List<PlanBuyingInventoryProgram> programs)
        {
            var conversionRatesByDaypartCodeId = _NtiToNsiConversionRepository
                    .GetLatestNtiToNsiConversionRates()
                    .ToDictionary(x => x.StandardDaypartId, x => x.ConversionRate);

            foreach (var program in programs)
            {
                var conversionRate = conversionRatesByDaypartCodeId[program.StandardDaypartId];
                program.ProjectedImpressions *= conversionRate;
                program.NsiToNtiImpressionConversionRate = conversionRate;

                if (program.ProvidedImpressions.HasValue)
                    program.ProvidedImpressions *= conversionRate;
            }
        }

        private void _ApplyNTIConversionToNSI(
            List<QuoteProgram> programs,
            PostingTypeEnum postingType)
        {
            if (postingType != PostingTypeEnum.NTI)
                return;

            var conversionRatesByDaypartCodeId = _NtiToNsiConversionRepository
                    .GetLatestNtiToNsiConversionRates()
                    .ToDictionary(x => x.StandardDaypartId, x => x.ConversionRate);

            foreach (var program in programs)
            {
                var conversionRate = conversionRatesByDaypartCodeId[program.StandardDaypartId];

                foreach (var audience in program.DeliveryPerAudience)
                {
                    audience.ProjectedImpressions *= conversionRate;

                    if (audience.ProvidedImpressions.HasValue)
                        audience.ProvidedImpressions *= conversionRate;
                }
            }
        }

        internal List<PlanBuyingInventoryProgram> _CalculateProgramCpmAndFilterByMinAndMaxCpms(
            List<PlanBuyingInventoryProgram> programs,
            decimal? minCPM,
            decimal? maxCPM)
        {
            foreach (var program in programs)
            {
                decimal cost;

                // for multi-length, we use impressions for :30 and that`s why the cost must be for :30 as well
                var rate = program.ManifestRates.SingleOrDefault(x => x.SpotLengthId == BroadcastConstants.SpotLengthId30);

                if (rate != null)
                {
                    cost = rate.Cost;
                }
                else
                {
                    // if there is no rate for :30 in the plan, let`s calculate it
                    // the formula that we use during inventory import is 
                    // cost for spot length = cost for :30 * multiplier for the spot length
                    // so, cost for :30 = cost for spot length / multiplier for the spot length

                    rate = program.ManifestRates.First();
                    cost = rate.Cost / (decimal)_SpotLengthEngine.GetSpotCostMultiplierBySpotLengthId(rate.SpotLengthId);
                }

                program.Cpm = ProposalMath.CalculateCpm(cost, program.Impressions);
            }

            if (!minCPM.HasValue && !maxCPM.HasValue)
            {
                return programs;
            }

            var result = new List<PlanBuyingInventoryProgram>();

            foreach (var program in programs)
            {
                if (!(minCPM.HasValue && program.Cpm < minCPM.Value) &&
                    !(maxCPM.HasValue && program.Cpm > maxCPM.Value))
                {
                    result.Add(program);
                }
            }

            return result;
        }

        private List<int> _GetDaypartDayIds(List<PlanDaypartDto> planDayparts)
        {
            var planDefaultDaypartIds = planDayparts.Select(d => d.DaypartCodeId).ToList();
            var dayIds = _StandardDaypartRepository.GetDayIdsFromStandardDayparts(planDefaultDaypartIds);
            return dayIds;
        }

        private HashSet<string> _GetCoveredDayNamesHashSet(List<int> flightDays, List<int> planDaypartDayIds)
        {
            var days = _CadentDayDefinitions.Value;
            var coveredDayIds = flightDays.Intersect(planDaypartDayIds);

            var coveredDayNames = days
                .Where(x => coveredDayIds.Contains(x.Id))
                .Select(x => x.Name);
            var coveredDayNamesHashSet = new HashSet<string>(coveredDayNames);
            return coveredDayNamesHashSet;
        }

        internal DisplayDaypart _GetDaypartDaysFromFlight(List<int> flightDays, List<DateRange> planFlightDateRanges, List<int> planDaypartDayIds)
        {
            var coveredDayNamesSet = _GetCoveredDayNamesHashSet(flightDays, planDaypartDayIds);

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
                    if (start.DayOfWeek == DayOfWeek.Monday && coveredDayNamesSet.Contains("Monday"))
                        result.Monday = true;

                    if (start.DayOfWeek == DayOfWeek.Tuesday && coveredDayNamesSet.Contains("Tuesday"))
                        result.Tuesday = true;

                    if (start.DayOfWeek == DayOfWeek.Wednesday && coveredDayNamesSet.Contains("Wednesday"))
                        result.Wednesday = true;

                    if (start.DayOfWeek == DayOfWeek.Thursday && coveredDayNamesSet.Contains("Thursday"))
                        result.Thursday = true;

                    if (start.DayOfWeek == DayOfWeek.Friday && coveredDayNamesSet.Contains("Friday"))
                        result.Friday = true;

                    if (start.DayOfWeek == DayOfWeek.Saturday && coveredDayNamesSet.Contains("Saturday"))
                        result.Saturday = true;

                    if (start.DayOfWeek == DayOfWeek.Sunday && coveredDayNamesSet.Contains("Sunday"))
                        result.Sunday = true;

                    if (result.ActiveDays == 7)
                        return result;

                    start = start.AddDays(1);
                }
            }

            return result;
        }

        private void _ApplyProvidedImpressions(
            List<PlanBuyingInventoryProgram> programs,
            PlanDto plan)
        {
            // we don`t want to equivalize impressions
            // when BuyingVersion allows MultiLength, because this is done by the buying endpoint
            var equivalized = false;

            // SpotLengthId does not matter for the buying v3, so this code is for the v2
            var spotLengthId = plan.CreativeLengths.First().SpotLengthId;

            _ImpressionsCalculationEngine.ApplyProvidedImpressions(programs, plan.AudienceId, spotLengthId, equivalized);
        }

        private void _ApplyProvidedImpressions(
            List<QuoteProgram> programs,
            QuoteRequestDto request)
        {
            var spotLengthId = request.CreativeLengths.First().SpotLengthId;

            _ImpressionsCalculationEngine.ApplyProvidedImpressions(programs, spotLengthId, request.Equivalized);
        }

        private void _ApplyProjectedImpressions(
            IEnumerable<PlanBuyingInventoryProgram> programs,
            PlanDto plan, ProgramInventoryOptionalParametersDto parameters)
        {
            var hutBookId = plan.HUTBookId;
            var shareBookId = plan.ShareBookId;

            var enableParameterBooks = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_BUYING_NAVIGATION_PANEL_TOOLS);
            if (enableParameterBooks)
            {
                hutBookId = parameters.HUTBookId;
                shareBookId = parameters.ShareBookId;
            }

            var impressionsRequest = new ImpressionsRequestDto
            {
                // we don`t want to equivalize impressions when allows MultiLength, because this is done by the buying endpoint
                Equivalized = false,
                HutProjectionBookId = hutBookId,
                PlaybackType = ProposalPlaybackType.LivePlus3,
                PostType = plan.PostingType,
                ShareProjectionBookId = shareBookId,

                // SpotLengthId does not matter for the buying v3, so this code is for the v2
                SpotLengthId = plan.CreativeLengths.First().SpotLengthId
            };

            _ImpressionsCalculationEngine.ApplyProjectedImpressions(programs, impressionsRequest, plan.AudienceId);
        }

        private void _ApplyProjectedImpressions(
            IEnumerable<QuoteProgram> programs,
            QuoteRequestDto request)
        {
            var impressionsRequest = new ImpressionsRequestDto
            {
                Equivalized = request.Equivalized,
                HutProjectionBookId = request.HUTBookId,
                PlaybackType = ProposalPlaybackType.LivePlus3,
                PostType = request.PostingType,
                ShareProjectionBookId = request.ShareBookId,
                SpotLengthId = request.CreativeLengths.First().SpotLengthId
            };

            var audienceIds = request.SecondaryAudiences
                .Select(x => x.AudienceId)
                .Union(new List<int> { request.AudienceId, BroadcastConstants.HouseholdAudienceId })
                .Distinct()
                .ToList();

            _ImpressionsCalculationEngine.ApplyProjectedImpressions(programs, impressionsRequest, audienceIds);
        }

        private Dictionary<int, List<int>> _GetDaypartDefaultDayIds()
        {
            var daypartDefaultDayIds = new Dictionary<int, List<int>>();
            var daypartDefaultIds = _StandardDaypartRepository.GetAllStandardDayparts().Select(s => s.Id);
            foreach (var daypartDefaultId in daypartDefaultIds)
            {
                var dayIds = _StandardDaypartRepository.GetDayIdsFromStandardDayparts(new List<int> { daypartDefaultId });
                daypartDefaultDayIds[daypartDefaultId] = dayIds;
            }

            return daypartDefaultDayIds;
        }

        private int _GetNumberOfFallbackQuarters()
        {
            var result = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.NumberOfFallbackQuartersForPricing, 8);
            return result;
        }
        private int _GetThresholdInSecondsForProgramIntersectInPricing()
        {
            return _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.ThresholdInSecondsForProgramIntersectInPricing, 1800);
        }
    }
}