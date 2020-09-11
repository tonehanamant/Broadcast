using System;
using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventoryEngineTestClass : PlanPricingInventoryEngine
    {
        public PlanPricingInventoryEngineTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsCalculationEngine impressionsCalculationEngine,
            IPlanPricingInventoryQuarterCalculatorEngine planPricingInventoryQuarterCalculatorEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ISpotLengthEngine spotLengthEngine, 
            IFeatureToggleHelper featureToggleHelper)
            : base(
                  broadcastDataRepositoryFactory, 
                  impressionsCalculationEngine,
                  planPricingInventoryQuarterCalculatorEngine,
                  mediaMonthAndWeekAggregateCache,
                  daypartCache,
                  quarterCalculationEngine,
                  spotLengthEngine, 
                  featureToggleHelper)
        {
        }

        public List<PlanPricingInventoryProgram> UT_FilterProgramsByDaypartAndSetStandardDaypart(
            List<PlanDaypartDto> dayparts,
            List<PlanPricingInventoryProgram> programs,
            DisplayDaypart planDisplayDaypartDays)
        {
            return FilterProgramsByDaypartAndSetStandardDaypart(dayparts, programs, planDisplayDaypartDays);
        }

        public List<PlanPricingInventoryProgram> UT_FilterProgramsByMinAndMaxCPM(
            List<PlanPricingInventoryProgram> programs,
            decimal? minCPM,
            decimal? maxCPM)
        {
            return CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, minCPM, maxCPM);
        }

        public void UT_ApplyInflationFactorToSpotCost(List<PlanPricingInventoryProgram> programs, double? inflationFactor)
        {
            ApplyInflationFactorToSpotCost(programs, inflationFactor);
        }

        public DisplayDaypart UT_GetPlanDaypartDaysFromPlanFlight(List<int> flightDays, List<DateRange> planFlightDateRanges, List<int> planDaypartDayIds)
        {
            return _GetDaypartDaysFromFlight(flightDays, planFlightDateRanges, planDaypartDayIds);
        }

        public void UT_ApplyNTIConversionToNSI(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs)
        {
            ApplyNTIConversionToNSI(plan, programs);
        }

        public List<PlanPricingInventoryProgram> UT_GetFullPrograms(
            List<DateRange> dateRanges, 
            List<int> spotLengthIds,
            List<int> supportedInventorySourceTypes, 
            List<short> availableMarkets, 
            QuarterDetailDto planQuarter, 
            List<QuarterDetailDto> fallbackQuarters,
            Guid? processingId = null)
        {
            // right now not testing with the processing Id, so make one up here.
            return _GetFullPrograms(dateRanges, spotLengthIds, supportedInventorySourceTypes, availableMarkets, planQuarter, fallbackQuarters, processingId ?? Guid.NewGuid());
        }

        protected override int _GetThresholdInSecondsForProgramIntersect()
        {
            return 1800;
        }

        public string UT_PlanPricingEndpointVersion { get; set; } = "2";

        protected override string _GetPlanPricingEndpointVersion()
        {
            return UT_PlanPricingEndpointVersion;
        }

        public int UT_NumberOfFallbackQuartersForPricing { get; set; } = 8;

        protected override int _GetNumberOfFallbackQuartersForPricing()
        {
            return UT_NumberOfFallbackQuartersForPricing;
        }
    }
}
