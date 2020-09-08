using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces.Common;
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

        List<QuoteProgram> GetInventoryForQuote(QuoteRequestDto request);
    }

    public class PlanPricingInventoryEngine : BroadcastBaseClass, IPlanPricingInventoryEngine
    {
        private readonly int ThresholdInSecondsForProgramIntersect;

        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IImpressionsCalculationEngine _ImpressionsCalculationEngine;
        private readonly INtiToNsiConversionRepository _NtiToNsiConversionRepository;
        private readonly IDayRepository _DayRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IPlanPricingInventoryQuarterCalculatorEngine _PlanPricingInventoryQuarterCalculatorEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDaypartCache _DaypartCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public PlanPricingInventoryEngine(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                          IImpressionsCalculationEngine impressionsCalculationEngine,
                                          IPlanPricingInventoryQuarterCalculatorEngine planPricingInventoryQuarterCalculatorEngine,
                                          IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                          IDaypartCache daypartCache,
                                          IQuarterCalculationEngine quarterCalculationEngine,
                                          ISpotLengthEngine spotLengthEngine,
                                          IFeatureToggleHelper featureToggleHelper)
        {
            ThresholdInSecondsForProgramIntersect = BroadcastServiceSystemParameter.ThresholdInSecondsForProgramIntersectInPricing;

            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _ImpressionsCalculationEngine = impressionsCalculationEngine;
            _PlanPricingInventoryQuarterCalculatorEngine = planPricingInventoryQuarterCalculatorEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;

            _NtiToNsiConversionRepository = broadcastDataRepositoryFactory.GetDataRepository<INtiToNsiConversionRepository>();
            _DayRepository = broadcastDataRepositoryFactory.GetDataRepository<IDayRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _DaypartCache = daypartCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _SpotLengthEngine = spotLengthEngine;
            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _FeatureToggleHelper = featureToggleHelper;
        }

        public List<QuoteProgram> GetInventoryForQuote(QuoteRequestDto request)
        {
            var inventorySourceTypes = new List<InventorySourceTypeEnum> { InventorySourceTypeEnum.OpenMarket };
            var inventorySourceIds = _InventoryRepository
                .GetInventorySources()
                .Where(x => inventorySourceTypes.Contains(x.InventoryType))
                .Select(x => x.Id)
                .ToList();

            var flightDateRanges = _GetFlightDateRanges(
                request.FlightStartDate,
                request.FlightEndDate,
                request.FlightHiatusDays);

            var spotLengthIds = request.CreativeLengths.Select(x => x.SpotLengthId).Take(1).ToList();

            var daypartDays = GetDaypartDaysFromFlight(request.FlightDays, flightDateRanges);

            var marketCodes = _MarketCoverageRepository
                .GetLatestTop100MarketCoverages().MarketCoveragesByMarketCode
                .Select(x => (short)x.Key)
                .ToList();

            var foundPrograms = _GetPrograms(
                flightDateRanges,
                inventorySourceIds,
                spotLengthIds,
                marketCodes);

            _PrepareRestrictionsForQuote(request.Dayparts);
            var programs = FilterProgramsByDaypartAndSetStandardDaypart(request.Dayparts, foundPrograms, daypartDays);

            _SetProgramsFlightDays(programs, request.FlightDays);
            _ApplyProjectedImpressions(programs, request);
            _ApplyProvidedImpressions(programs, request);
            _ApplyNTIConversionToNSI(programs, request.PostingType);
            _ApplyMargin(programs, request.Margin);
            _CalculateProgramCpm(programs);

            return programs;
        }

        private void _PrepareRestrictionsForQuote(List<PlanDaypartDto> dayparts)
        {
            foreach (var daypart in dayparts)
            {
                if (daypart.Restrictions == null)
                    continue;

                var restrictions = daypart.Restrictions;

                // for quote report we only use program restrictions and they always has Include type
                restrictions.AffiliateRestrictions = null;
                restrictions.ShowTypeRestrictions = null;
                restrictions.GenreRestrictions = null;

                restrictions.ProgramRestrictions.ContainType = ContainTypeEnum.Include;
            }
        }

        public void _ApplyMargin(List<QuoteProgram> programs, double margin)
        {
            programs
                .SelectMany(x => x.ManifestRates)
                .ForEach(x => x.Cost = GeneralMath.CalculateCostWithMargin(x.Cost, margin));
        }

        public List<PlanPricingInventoryProgram> GetInventoryForPlan(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            IEnumerable<int> inventorySourceIds,
            PlanPricingJobDiagnostic diagnostic)
        {
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);
            var flightDateRanges = _GetFlightDateRanges(
                plan.FlightStartDate.Value,
                plan.FlightEndDate.Value,
                plan.FlightHiatusDays);
            var daypartDays = GetDaypartDaysFromFlight(plan.FlightDays, flightDateRanges);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);
            var programs = _GetPrograms(plan, flightDateRanges, inventorySourceIds, diagnostic);

            // we don't expect spots other than 30 length spots for OpenMarket
            programs = programs.Where(x => x.SpotLengthId == BroadcastConstants.SpotLengthId30).ToList();

            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);
            programs = FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, daypartDays);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);
            ApplyInflationFactorToSpotCost(programs, parameters?.InflationFactor);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);
            // Set the plan flight days to programs so impressions are calculated for those days.
            _SetProgramsFlightDays(programs, plan.FlightDays);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);
            _ApplyProjectedImpressions(programs, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);
            _ApplyProvidedImpressions(programs, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);
            ApplyNTIConversionToNSI(plan, programs);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);
            programs = CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, parameters?.MinCPM, parameters?.MaxCPM);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);

            return programs;
        }

        private void _SetProgramDayparts<T>(List<T> programs) where T: BasePlanInventoryProgram
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

        private void _SetProgramsFlightDays<T>(List<T> programs, List<int> flightDays) where T: BasePlanInventoryProgram
        {
            var days = _DayRepository.GetDays();
            var flightDayNames = days
                .Where(x => flightDays.Contains(x.Id))
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

            var programsToUpdate = programs.Where(p => p.InventoryPricingQuarterType == InventoryPricingQuarterType.Fallback);

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

        /// <summary>
        /// Attempts to gather the full inventory set from the plan's quarter or the fallback quarter.
        /// </summary>
        /// <remarks>
        /// This does not  address if a plan's flight extends beyond a single quarter.
        /// </remarks>
        protected List<PlanPricingInventoryProgram> _GetFullPrograms(
            List<DateRange> dateRanges, 
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds, 
            List<short> availableMarkets, 
            QuarterDetailDto planQuarter, 
            List<QuarterDetailDto> fallbackQuarters)
        {
            var totalInventory = new List<PlanPricingInventoryProgram>();
            var availableStations = _StationRepository.GetBroadcastStationsWithLatestDetailsByMarketCodes(availableMarkets);

            foreach (var dateRange in dateRanges)
            {
                var ungatheredStationIds = availableStations.Select(s => s.Id).ToList();

                // look for inventory from plan quarter
                var planInventoryForDateRange = _StationProgramRepository.GetProgramsForPricingModel(
                    dateRange.Start.Value, 
                    dateRange.End.Value,
                    spotLengthIds, 
                    inventorySourceIds,
                    ungatheredStationIds);

                planInventoryForDateRange.ForEach(p => p.InventoryPricingQuarterType = InventoryPricingQuarterType.Plan);
                totalInventory.AddRange(planInventoryForDateRange);
                var gatheredStationIds = planInventoryForDateRange.Select(s => s.Station.Id).Distinct().ToList();
                ungatheredStationIds = ungatheredStationIds.Except(gatheredStationIds).ToList();

                // look for inventory from fallback quarters
                foreach (var fallbackQuarter in fallbackQuarters.OrderByDescending(x => x.Year).ThenByDescending(x => x.Quarter))
                {
                    if (!ungatheredStationIds.Any())
                        break;

                    // multiple DateRanges can be returned if the PlanQuarter has more weeks than the FallbackQuarter
                    var fallbackDateRanges = _PlanPricingInventoryQuarterCalculatorEngine.GetFallbackDateRanges(dateRange, planQuarter, fallbackQuarter);

                    var fallbackInventory = fallbackDateRanges
                        .SelectMany(fallbackDateRange =>
                            _StationProgramRepository.GetProgramsForPricingModel(
                                fallbackDateRange.Start.Value,
                                fallbackDateRange.End.Value,
                                spotLengthIds,
                                inventorySourceIds,
                                ungatheredStationIds))
                        .ToList();

                    fallbackInventory.ForEach(p => p.InventoryPricingQuarterType = InventoryPricingQuarterType.Fallback);
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
            List<PlanPricingInventoryProgram> programs,
            QuarterDetailDto planQuarter,
            List<QuarterDetailDto> fallbackQuarters)
        {
            _SetContractWeekForPlanQuarterInventory(programs);
            _SetContractWeekForFallbackQuarterInventory(programs, planQuarter, fallbackQuarters);
        }

        private void _SetContractWeekForPlanQuarterInventory(List<PlanPricingInventoryProgram> programs)
        {
            // for the plan quarter ContractMediaWeekId is the same as InventoryMediaWeekId
            programs
                .Where(x => x.InventoryPricingQuarterType == InventoryPricingQuarterType.Plan)
                .ForEach(x => x.ManifestWeeks.ForEach(w => w.ContractMediaWeekId = w.InventoryMediaWeekId));
        }

        private void _SetContractWeekForFallbackQuarterInventory(
            List<PlanPricingInventoryProgram> programs,
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
                .Where(x => x.InventoryPricingQuarterType == InventoryPricingQuarterType.Fallback)
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

        private List<PlanPricingInventoryProgram.ManifestWeek> _GetInventoryWeeksByMediaWeeks(
            List<PlanPricingInventoryProgram.ManifestWeek> allInventoryWeeks, 
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

        private List<PlanPricingInventoryProgram> _GetPrograms(
            PlanDto plan, 
            List<DateRange> planFlightDateRanges, 
            IEnumerable<int> inventorySourceIds,
            PlanPricingJobDiagnostic diagnostic)
        {
            var spotLengthIds = BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "2" ?
                plan.CreativeLengths.Select(x => x.SpotLengthId).Take(1).ToList() :
                plan.CreativeLengths.Select(x => x.SpotLengthId).ToList();

            var marketCodes = plan.AvailableMarkets.Select(m => m.MarketCode).ToList();
            var planQuarter = _PlanPricingInventoryQuarterCalculatorEngine.GetPlanQuarter(plan);
            var fallbackQuarters = _QuarterCalculationEngine.GetLastNQuarters(
                new QuarterDto { Quarter = planQuarter.Quarter, Year = planQuarter.Year },
                BroadcastServiceSystemParameter.NumberOfFallbackQuartersForPricing);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FETCHING_NOT_POPULATED_INVENTORY);
            var programs = _GetFullPrograms(planFlightDateRanges, spotLengthIds, inventorySourceIds, marketCodes, planQuarter, fallbackQuarters);

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
            _SetContractWeekForInventory(programs, planQuarter, fallbackQuarters);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_MATCHING_INVENTORY_WEEKS_WITH_PLAN_WEEKS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_PRIMARY_PROGRAM);
            _SetPrimaryProgramForDayparts(programs);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_PRIMARY_PROGRAM);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYPARTS);
            _SetProgramDayparts(programs);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYPARTS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_LATEST_STATION_DETAILS);
            _SetProgramStationLatestMonthDetails(programs);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_LATEST_STATION_DETAILS);

            return programs;
        }

        private void _SetProgramStationLatestMonthDetails<T>(List<T> programs) where T: BasePlanInventoryProgram
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

        private void _SetPrimaryProgramForDayparts<T>(List<T> programs) where T: BasePlanInventoryProgram
        {
            foreach (var manifestDaypart in programs.SelectMany(x => x.ManifestDayparts))
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

        /// <summary>
        /// This works only for OpenMarket. 
        /// When we start using other sources, the PlanPricingInventoryProgram model structure should be reviewed
        /// Other sources may have more than 1 daypart that this logic does not assume
        /// </summary>
        protected List<T> FilterProgramsByDaypartAndSetStandardDaypart<T>(
            List<PlanDaypartDto> dayparts,
            List<T> programs,
            DisplayDaypart planDays) where T: BasePlanInventoryProgram
        {
            var result = new List<T>();

            foreach (var program in programs)
            {
                var planDaypartsMatchedByTimeAndDays = _GetPlanDaypartsThatMatchProgramByTimeAndDays(dayparts, planDays, program);
                var planDaypartsMatchByRestrictions = _GetPlanDaypartsThatMatchProgramByRestrictions(planDaypartsMatchedByTimeAndDays, program);
                var planDaypartWithMostIntersectingTime = _FindPlanDaypartWithMostIntersectingTime(planDaypartsMatchByRestrictions);

                if (planDaypartWithMostIntersectingTime != null)
                {
                    program.StandardDaypartId = planDaypartWithMostIntersectingTime.PlanDaypart.DaypartCodeId;

                    result.Add(program);
                }
            }

            return result;
        }

        private ProgramInventoryDaypart _FindPlanDaypartWithMostIntersectingTime<T>(List<T> programInventoryDayparts) where T: ProgramInventoryDaypart
        {
            return programInventoryDayparts
                .Select(x => 
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

                    return new
                    {
                        programInventoryDaypart = x,
                        instersectionTime = DaypartTimeHelper.GetIntersectingTotalTime(planDaypartTimeRange, inventoryDaypartTimeRange)
                    };
                })
                .Where(x => x.instersectionTime >= ThresholdInSecondsForProgramIntersect)
                .OrderByDescending(x => x.instersectionTime)
                .Select(x => x.programInventoryDaypart)
                .FirstOrDefault();
        }

        protected void ApplyNTIConversionToNSI(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs)
        {
            if (plan.PostingType != PostingTypeEnum.NTI)
                return;

            var conversionRatesByDaypartCodeId = _NtiToNsiConversionRepository
                    .GetLatestNtiToNsiConversionRates()
                    .ToDictionary(x => x.DaypartDefaultId, x => x.ConversionRate);

            foreach (var program in programs)
            {
                var conversionRate = conversionRatesByDaypartCodeId[program.StandardDaypartId];

                program.ProjectedImpressions *= conversionRate;

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
                    .ToDictionary(x => x.DaypartDefaultId, x => x.ConversionRate);

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

        protected List<PlanPricingInventoryProgram> CalculateProgramCpmAndFilterByMinAndMaxCpm(
            List<PlanPricingInventoryProgram> programs,
            decimal? minCPM,
            decimal? maxCPM)
        {
            foreach (var program in programs)
            {
                decimal cost;

                // for pricing v2, there is always 1 spot length for inventory
                if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "2")
                {
                    cost = program.ManifestRates.Single().Cost;
                }
                // for pricing v3, we use impressions for :30 and that`s why the cost must be for :30 as well
                else
                {
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
                }

                program.Cpm = ProposalMath.CalculateCpm(cost, program.Impressions);
            }

            if (!minCPM.HasValue && !maxCPM.HasValue)
            {
                return programs;
            }

            var result = new List<PlanPricingInventoryProgram>();

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

        private void _CalculateProgramCpm(List<QuoteProgram> programs)
        {
            foreach (var program in programs)
            {
                var cost = program.ManifestRates.Single().Cost;

                foreach (var audience in program.DeliveryPerAudience)
                {
                    audience.CPM = ProposalMath.CalculateCpm(cost, audience.Impressions);
                }
            }
        }

        private List<ProgramInventoryDaypart> _GetPlanDaypartsThatMatchProgramByRestrictions(List<ProgramInventoryDaypart> programInventoryDayparts, BasePlanInventoryProgram program)
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

        private bool _IsProgramAllowedByAffiliateRestrictions(ProgramInventoryDaypart programInventoryDaypart, BasePlanInventoryProgram program)
        {
            var affiliateRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.AffiliateRestrictions;

            if (affiliateRestrictions == null || affiliateRestrictions.Affiliates.IsEmpty())
                return true;

            var restrictedAffiliates = affiliateRestrictions.Affiliates.Select(x => x.Display);
            bool hasIntersections;
            if (restrictedAffiliates.Any(x => x.Equals("IND")) 
                && _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.USE_TRUE_INDEPENDENT_STATIONS))
            {
                hasIntersections = program.Station.IsTrueInd == true;
            }
            else
            {
                hasIntersections = restrictedAffiliates.Contains(program.Station.Affiliation);
            }

            return affiliateRestrictions.ContainType == ContainTypeEnum.Include ?
                 hasIntersections :
                !hasIntersections;
        }

        private bool _IsProgramAllowedByProgramRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var programRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ProgramRestrictions;

            if (programRestrictions == null || programRestrictions.Programs.IsEmpty())
            {
                return true;
            }

            var restrictedProgramNames = programRestrictions.Programs.Select(x => x.Name.ToLowerInvariant());
            var primaryProgramName = programInventoryDaypart.ManifestDaypart.PrimaryProgram.Name.ToLowerInvariant();
            var isInTheList = restrictedProgramNames.Contains(primaryProgramName);

            if (programRestrictions.ContainType == ContainTypeEnum.Include)
            {
                return isInTheList;
            }

            return isInTheList == false;
        }

        private bool _IsProgramAllowedByGenreRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var genreRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.GenreRestrictions;

            if (genreRestrictions == null || genreRestrictions.Genres.IsEmpty())
            {
                return true;
            }

            var restrictedGenres = genreRestrictions.Genres.Select(x => x.Display);
            var primaryProgramGenre = programInventoryDaypart.ManifestDaypart.PrimaryProgram.Genre;
            var isInTheList = restrictedGenres.Contains(primaryProgramGenre);

            if (genreRestrictions.ContainType == ContainTypeEnum.Include)
            {
                return isInTheList;
            }

            return isInTheList == false;
        }

        private bool _IsProgramAllowedByShowTypeRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var showTypeRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ShowTypeRestrictions;

            if (showTypeRestrictions == null || showTypeRestrictions.ShowTypes.IsEmpty())
            {
                return true;
            }

            var restrictedShowTypes = showTypeRestrictions.ShowTypes.Select(x => x.Display);
            var primaryProgramShowType = programInventoryDaypart.ManifestDaypart.PrimaryProgram.ShowType;
            var isInTheList = restrictedShowTypes.Contains(primaryProgramShowType);

            if (showTypeRestrictions.ContainType == ContainTypeEnum.Include)
            {
                return isInTheList;
            }

            return isInTheList == false;
        }

        private List<ProgramInventoryDaypart> _GetPlanDaypartsThatMatchProgramByTimeAndDays(
            List<PlanDaypartDto> planDayparts,
            DisplayDaypart planDisplayDaypart,
            BasePlanInventoryProgram program)
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

        protected DisplayDaypart GetDaypartDaysFromFlight(List<int> flightDays, List<DateRange> planFlightDateRanges)
        {
            var days = _DayRepository.GetDays();
            var flightDayNames = days
                .Where(x => flightDays.Contains(x.Id))
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

        private void _ApplyProvidedImpressions(
            List<PlanPricingInventoryProgram> programs, 
            PlanDto plan)
        {
            // we don`t want to equivalize impressions
            // when PricingVersion == 3, because this is done by the pricing endpoint
            var equivalized = BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "3" ? false : plan.Equivalized;

            // SpotLengthId does not matter for the pricing v3, so this code is for the v2
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
            IEnumerable<PlanPricingInventoryProgram> programs,
            PlanDto plan)
        {
            var impressionsRequest = new ImpressionsRequestDto
            {
                // we don`t want to equivalize impressions when PricingVersion == 3, because this is done by the pricing endpoint
                Equivalized = BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "2" ? plan.Equivalized : false,
                HutProjectionBookId = plan.HUTBookId,
                PlaybackType = ProposalPlaybackType.LivePlus3,
                PostType = plan.PostingType,
                ShareProjectionBookId = plan.ShareBookId,

                // SpotLengthId does not matter for the pricing v3, so this code is for the v2
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

        private class ProgramInventoryDaypart
        {
            public PlanDaypartDto PlanDaypart { get; set; }

            public BasePlanInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
        }
    }
}
