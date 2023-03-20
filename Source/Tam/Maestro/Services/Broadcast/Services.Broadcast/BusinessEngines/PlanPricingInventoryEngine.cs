using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
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
    public interface IPlanPricingInventoryEngine : IApplicationService
    {
        List<PlanPricingInventoryProgram> GetInventoryForPlan(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            IEnumerable<int> inventorySourceIds,
            PlanPricingJobDiagnostic diagnostic,
            Guid processingId);

        List<QuoteProgram> GetInventoryForQuote(QuoteRequestDto request, Guid processingId);
        void ConvertPostingType(PostingTypeEnum postingType, List<PlanPricingInventoryProgram> programs);
    }

    public class PlanPricingInventoryEngine : BroadcastBaseClass, IPlanPricingInventoryEngine
    {
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
        private readonly IStandardDaypartRepository _StandardDaypartRepository;

        internal Lazy<int> _ThresholdInSecondsForProgramIntersect;
        protected Lazy<int> _NumberOfFallbackQuartersForPricing;
        private readonly Lazy<bool> _IsPricingBuyingProgramsQueryv2;

        protected Lazy<List<Day>> _CadentDayDefinitions;
        /// <summary>
        /// Key = StandardDaypartId
        /// Values = Day Ids for that Daypart Default.
        /// </summary>
        protected Lazy<Dictionary<int, List<int>>> _StandardDaypartDayIds;

        public PlanPricingInventoryEngine(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                          IImpressionsCalculationEngine impressionsCalculationEngine,
                                          IPlanPricingInventoryQuarterCalculatorEngine planPricingInventoryQuarterCalculatorEngine,
                                          IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                          IDaypartCache daypartCache,
                                          IQuarterCalculationEngine quarterCalculationEngine,
                                          ISpotLengthEngine spotLengthEngine,
                                          IFeatureToggleHelper featureToggleHelper,
                                          IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
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
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();

            // register lazy delegates - settings
            _ThresholdInSecondsForProgramIntersect = new Lazy<int>(_GetThresholdInSecondsForProgramIntersectInPricing);
            _NumberOfFallbackQuartersForPricing = new Lazy<int>(_GetNumberOfFallbackQuartersForPricing);
            _IsPricingBuyingProgramsQueryv2 = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PRICING_BUYING_PROGRAMS_QUERY_V2));

            // register lazy delegates - domain data
            _CadentDayDefinitions = new Lazy<List<Day>>(() => _DayRepository.GetDays());
            _StandardDaypartDayIds = new Lazy<Dictionary<int, List<int>>>(_GetStandardDaypartDayIds);
        }

        public List<QuoteProgram> GetInventoryForQuote(QuoteRequestDto request, Guid processingId)
        {
            _LogInfo($"Starting to get inventory for quote.", processingId);

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

            var requestDaypartDayIds = _GetDaypartDayIds(request.Dayparts);
            var daypartDays = _GetDaypartDaysFromFlight(request.FlightDays, flightDateRanges, requestDaypartDayIds);

            var marketCodes = _MarketCoverageRepository
                .GetLatestTop100MarketCoverages().MarketCoveragesByMarketCode
                .Select(x => (short)x.Key)
                .ToList();

            _LogInfo($"Starting to gather the program inventory for the quote.", processingId);
            var foundPrograms = _GetPrograms(
                flightDateRanges,
                inventorySourceIds,
                spotLengthIds,
                marketCodes,
                processingId);

            _LogInfo($"Completed gathering inventory.  Gathered {foundPrograms.Count} records.", processingId);

            _PrepareRestrictionsForQuote(request.Dayparts);
            _LogInfo($"Starting to filter by Daypart... Input Record Count : {foundPrograms.Count};", processingId);

            var programs = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(request.Dayparts, foundPrograms, daypartDays,
                _CadentDayDefinitions.Value, _StandardDaypartDayIds.Value, _ThresholdInSecondsForProgramIntersect.Value);

            _LogInfo($"Filtering complete. Input Record Count : {foundPrograms.Count}; Output Record Count : {programs.Count};", processingId);

            _LogInfo($"Beginning calculations on {programs.Count} records.", processingId);
            _SetProgramsFlightDays(programs, request.FlightDays);
            _ApplyProjectedImpressions(programs, request);
            _ApplyProvidedImpressions(programs, request);
            _ApplyNTIConversionToNSI(programs, request.PostingType);
            _ApplyMargin(programs, request.Margin);
            _CalculateProgramCpm(programs);

            _LogInfo($"Completed and returning {programs.Count} records.", processingId);

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
            PlanPricingJobDiagnostic diagnostic,
            Guid processingId)
        {
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);

            _LogInfo($"Starting to get inventory for plan. Plan Id : {plan.Id};", processingId);

            var flightDateRanges = _GetFlightDateRanges(
                plan.FlightStartDate.Value,
                plan.FlightEndDate.Value,
                plan.FlightHiatusDays);

            var planDaypartDayIds = _GetDaypartDayIds(plan.Dayparts);

            var daypartDays = _GetDaypartDaysFromFlight(plan.FlightDays, flightDateRanges, planDaypartDayIds);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS);

            _LogInfo($"Starting to gather the program inventory for the plan. Plan Id : {plan.Id};", processingId);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);
            var allPrograms = _GetPrograms(plan, flightDateRanges, inventorySourceIds, diagnostic, processingId);

            _LogInfo($"Completed gathering inventory.  Gathered {allPrograms.Count} records. Plan Id : {plan.Id};", processingId);

            if (_FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PRICING_REMOVE_UNMATCHED))
            {
                ProgramRestrictionsHelper.ApplyGeneralFilterForPricingPrograms(allPrograms);
            }

            // we don't expect spots other than 30 length spots for OpenMarket
            //var thirtySecondSpotPrograms = allPrograms.Where(x => x.SpotLengthId == BroadcastConstants.SpotLengthId30).ToList();
            //_LogInfo($"Filtered to only 30 sec spots.  Input Record Count : {allPrograms.Count}; Output Record Count : {thirtySecondSpotPrograms.Count}; Plan Id : {plan.Id};", processingId);

            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FETCHING_INVENTORY_FROM_DB);

            _LogInfo($"Starting to filter by Daypart... Input Record Count : {allPrograms.Count}; Plan Id : {plan.Id}; ", processingId);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);

            var filteredPrograms = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, allPrograms, daypartDays,
                _CadentDayDefinitions.Value, _StandardDaypartDayIds.Value, _ThresholdInSecondsForProgramIntersect.Value);

            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART);

            _LogInfo($"Filtering complete. Input Record Count : {allPrograms.Count}; Output Record Count : {filteredPrograms.Count}; Plan Id : {plan.Id};", processingId);

            _LogInfo($"Beginning calculations on {filteredPrograms.Count} records. Plan Id : {plan.Id};", processingId);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);
            ApplyInflationFactorToSpotCost(filteredPrograms, parameters?.InflationFactor);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_INFLATION_FACTOR);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);
            // Set the plan flight days to programs so impressions are calculated for those days.
            _SetProgramsFlightDays(filteredPrograms, plan.FlightDays);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);
            _ApplyProjectedImpressions(filteredPrograms, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROJECTED_IMPRESSIONS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_HOUSEHOLD_PROJECTED_IMPRESSIONS);
            _ApplyHouseholdProjectedImpressions(filteredPrograms, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_HOUSEHOLD_PROJECTED_IMPRESSIONS);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);
            _ApplyProvidedImpressions(filteredPrograms, plan);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROVIDED_IMPRESSIONS);

            var calculatedFiltered = new List<PlanPricingInventoryProgram>();

            _LogInfo($"Beginning populating Nti Impression conversion rates.", processingId);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);
            _AppendConversionRates(filteredPrograms, plan.PostingType);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI);
            _LogInfo($"Completed populating Nti Impression conversion rates", processingId);

            _LogInfo($"Beginning filtering by calculation results. Input Record Count : {filteredPrograms.Count}; Plan Id : {plan.Id};", processingId);
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);
            calculatedFiltered = CalculateProgramCpmAndFilterByMinAndMaxCpm(filteredPrograms, parameters?.MinCPM, parameters?.MaxCPM);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM);
            _LogInfo($"Completed filtering by calculation results. Input Record Count : {filteredPrograms.Count}; Output Record Count : {calculatedFiltered.Count}; Plan Id : {plan.Id};", processingId);


            _LogInfo($"Completed and returning {calculatedFiltered.Count} records. Plan Id : {plan.Id};", processingId);


            return calculatedFiltered;
        }


        internal void _AppendConversionRates(List<PlanPricingInventoryProgram> programs, PostingTypeEnum postingType)
        {           
            var conversionRatesByDaypartCodeId = _NtiToNsiConversionRepository
                    .GetLatestNtiToNsiConversionRates()
                    .ToDictionary(x => x.StandardDaypartId, x => x.ConversionRate);

            foreach (var program in programs)
            {
                var conversionRate = conversionRatesByDaypartCodeId[program.StandardDaypartId];
                program.NsiToNtiImpressionConversionRate = conversionRate;

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
            var standardDaypartIds = _StandardDaypartDayIds.Value;

            foreach (var program in programs)
            {
                var programStandardDaypartDays = standardDaypartIds[program.StandardDaypartId];
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

        internal void ApplyInflationFactorToSpotCost(List<PlanPricingInventoryProgram> programs, double? inflationFactor)
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
            List<short> marketCodes,
            Guid processingId)
        {
            var totalInventory = new List<QuoteProgram>();
            var stations = _StationRepository.GetBroadcastStationsWithLatestDetailsByMarketCodes(marketCodes);

            var currentlyProcessingDateRangeIndex = 0;
            foreach (var dateRange in dateRanges)
            {
                _LogInfo($"Beginning to gather inventory for date range {currentlyProcessingDateRangeIndex} of {dateRanges.Count}", processingId);
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

        internal List<PlanPricingInventoryProgram> _GetProgramsForPricingModel(
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
                List<PlanPricingInventoryProgram> results;
                if (_IsPricingBuyingProgramsQueryv2.Value)
                {
                    _LogInfo("Querying Programs with v2.", txId);
                    results = _StationProgramRepository.GetProgramsForPricingModelv2(
                        startDate,
                        endDate,
                        spotLengthIds,
                        inventorySourceIds,
                        stationIds);
                }
                else
                {
                    _LogInfo("Querying Programs with v1.", txId);
                    results = _StationProgramRepository.GetProgramsForPricingModel(
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
        internal List<PlanPricingInventoryProgram> _GetFullPrograms(
            List<DateRange> dateRanges,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<short> availableMarkets,
            QuarterDetailDto planQuarter,
            List<QuarterDetailDto> fallbackQuarters,
            Guid processingId)
        {
            var totalInventory = new List<PlanPricingInventoryProgram>();
            var availableStations = _StationRepository.GetBroadcastStationsWithLatestDetailsByMarketCodes(availableMarkets);

            var currentlyProcessingDateRangeIndex = 0;
            foreach (var dateRange in dateRanges)
            {
                currentlyProcessingDateRangeIndex++;
                _LogInfo($"Beginning to gather inventory for date range {currentlyProcessingDateRangeIndex} of {dateRanges.Count}", processingId);

                var ungatheredStationIds = availableStations.Select(s => s.Id).ToList();

                // look for inventory from plan quarter
                var planInventoryForDateRange = _GetProgramsForPricingModel(
                    dateRange.Start.Value,
                    dateRange.End.Value,
                    spotLengthIds,
                    inventorySourceIds,
                    ungatheredStationIds);

                planInventoryForDateRange.ForEach(p => p.InventoryPricingQuarterType = InventoryPricingQuarterType.Plan);
                totalInventory.AddRange(planInventoryForDateRange);
                var gatheredStationIds = planInventoryForDateRange.Select(s => s.Station.Id).Distinct().ToList();
                ungatheredStationIds = ungatheredStationIds.Except(gatheredStationIds).ToList();

                var currentlyProcessingFallbackQuarterIndex = 0;
                // look for inventory from fallback quarters
                foreach (var fallbackQuarter in fallbackQuarters.OrderByDescending(x => x.Year).ThenByDescending(x => x.Quarter))
                {
                    currentlyProcessingFallbackQuarterIndex++;
                    _LogInfo($"Beginning to gather inventory for date range {currentlyProcessingDateRangeIndex} for fallback quarter {currentlyProcessingFallbackQuarterIndex} of {fallbackQuarters.Count}", processingId);

                    if (!ungatheredStationIds.Any())
                        break;

                    // multiple DateRanges can be returned if the PlanQuarter has more weeks than the FallbackQuarter
                    var fallbackDateRanges = _PlanPricingInventoryQuarterCalculatorEngine.GetFallbackDateRanges(dateRange, planQuarter, fallbackQuarter);

                    var fallbackInventory = fallbackDateRanges
                        .SelectMany(fallbackDateRange =>
                            _GetProgramsForPricingModel(
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
            List<short> marketCodes,
            Guid processingId)
        {
            var fullPrograms = _GetFullPrograms(
                flightDateRanges,
                spotLengthIds,
                inventorySourceIds,
                marketCodes,
                processingId);

            _LogInfo($"Found {fullPrograms.Count} programs.", processingId);

            // so that programs are not repeated
            var programs = fullPrograms.GroupBy(x => x.ManifestId).Select(x => x.First()).ToList();

            _LogInfo($"De-duped {fullPrograms.Count} programs to {programs.Count}.", processingId);

            _LogInfo($"Finalizing {programs.Count} programs.", processingId);
            _SetPrimaryProgramForDayparts(programs);
            _SetProgramDayparts(programs);
            _SetProgramStationLatestMonthDetails(programs);

            return programs;
        }

        private List<PlanPricingInventoryProgram> _GetPrograms(
            PlanDto plan,
            List<DateRange> planFlightDateRanges,
            IEnumerable<int> inventorySourceIds,
            PlanPricingJobDiagnostic diagnostic,
            Guid processingId)
        {
            var spotLengthIds = plan.CreativeLengths.Select(x => x.SpotLengthId).ToList();

            var marketCodes = plan.AvailableMarkets.Select(m => m.MarketCode).ToList();
            var planQuarter = _PlanPricingInventoryQuarterCalculatorEngine.GetPlanQuarter(plan);
            var fallbackQuarters = _QuarterCalculationEngine.GetLastNQuarters(
                new QuarterDto { Quarter = planQuarter.Quarter, Year = planQuarter.Year },
                _NumberOfFallbackQuartersForPricing.Value);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FETCHING_NOT_POPULATED_INVENTORY);
            var fullPrograms = _GetFullPrograms(planFlightDateRanges, spotLengthIds, inventorySourceIds, marketCodes, planQuarter, fallbackQuarters, processingId);

            _LogInfo($"Found {fullPrograms.Count} programs.", processingId);

            // so that programs are not repeated
            var programs = fullPrograms.GroupBy(x => x.ManifestId).Select(x =>
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
            _LogInfo($"De-duped {fullPrograms.Count} programs to {programs.Count}.", processingId);

            _LogInfo($"Finalizing {programs.Count} programs.", processingId);
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

        public void ConvertPostingType(PostingTypeEnum postingType, List<PlanPricingInventoryProgram> programs)
        {
            foreach (var program in programs)
            {
                program.PostingType = postingType;
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

        internal List<PlanPricingInventoryProgram> CalculateProgramCpmAndFilterByMinAndMaxCpm(
            List<PlanPricingInventoryProgram> programs,
            decimal? minCPM,
            decimal? maxCPM)
        {
            foreach (var program in programs)
            {
                decimal cost;

                // for multi-length pricing, we use impressions for :30 and that`s why the cost must be for :30 as well
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

                program.Cpm = ProposalMath.CalculateCpm(cost, program.PostingTypeImpressions);
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
            List<PlanPricingInventoryProgram> programs,
            PlanDto plan)
        {
            // we don`t want to equivalize impressions
            // when PricingVersion allows MultiLength, because this is done by the pricing endpoint
            var equivalized = false;

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
                // we don`t want to equivalize impressions
                // when PricingVersion allows MultiLength, because this is done by the pricing endpoint
                Equivalized = false,
                HutProjectionBookId = plan.HUTBookId,
                PlaybackType = ProposalPlaybackType.LivePlus3,
                PostType = plan.PostingType,
                ShareProjectionBookId = plan.ShareBookId,

                // SpotLengthId does not matter for the pricing v3, so this code is for the v2
                SpotLengthId = plan.CreativeLengths.First().SpotLengthId
            };

            _ImpressionsCalculationEngine.ApplyProjectedImpressions(programs, impressionsRequest, plan.AudienceId);
        }

        private void _ApplyHouseholdProjectedImpressions(
           IEnumerable<PlanPricingInventoryProgram> programs,
           PlanDto plan)
        {
            var impressionsRequest = new ImpressionsRequestDto
            {
                // we don`t want to equivalize impressions
                // when PricingVersion allows MultiLength, because this is done by the pricing endpoint
                Equivalized = false,
                HutProjectionBookId = plan.HUTBookId,
                PlaybackType = ProposalPlaybackType.LivePlus3,
                PostType = plan.PostingType,
                ShareProjectionBookId = plan.ShareBookId,

                // SpotLengthId does not matter for the pricing v3, so this code is for the v2
                SpotLengthId = plan.CreativeLengths.First().SpotLengthId
            };

            _ImpressionsCalculationEngine.ApplyHouseholdProjectedImpressions(programs, impressionsRequest);
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

        private Dictionary<int, List<int>> _GetStandardDaypartDayIds()
        {
            var standardDaypartDayIds = new Dictionary<int, List<int>>();
            var standardDaypartIds = _StandardDaypartRepository.GetAllStandardDayparts().Select(s => s.Id);
            foreach (var id in standardDaypartIds)
            {
                var dayIds = _StandardDaypartRepository.GetDayIdsFromStandardDayparts(new List<int> { id });
                standardDaypartDayIds[id] = dayIds;
            }

            return standardDaypartDayIds;
        }

        private int _GetNumberOfFallbackQuartersForPricing()
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