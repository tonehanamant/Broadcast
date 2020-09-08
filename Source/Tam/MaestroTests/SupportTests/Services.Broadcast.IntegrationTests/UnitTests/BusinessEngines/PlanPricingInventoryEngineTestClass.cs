using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
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

        public List<PlanPricingInventoryProgram> UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(
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

        public DisplayDaypart UT_GetPlanDaypartDaysFromPlanFlight(List<int> flightDays, List<DateRange> planFlightDateRanges)
        {
            return GetDaypartDaysFromFlight(flightDays, planFlightDateRanges);
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
            List<QuarterDetailDto> fallbackQuarters)
        {
            return _GetFullPrograms(dateRanges, spotLengthIds, supportedInventorySourceTypes, availableMarkets, planQuarter, fallbackQuarters);
        }
    }
}
