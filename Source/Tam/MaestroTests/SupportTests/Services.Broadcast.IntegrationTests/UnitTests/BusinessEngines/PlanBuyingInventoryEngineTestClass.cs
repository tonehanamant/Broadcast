using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanBuyingInventoryEngineTestClass : PlanBuyingInventoryEngine
    {
        public PlanBuyingInventoryEngineTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsCalculationEngine impressionsCalculationEngine,
            IPlanBuyingInventoryQuarterCalculatorEngine planBuyingInventoryQuarterCalculatorEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ISpotLengthEngine spotLengthEngine, 
            IFeatureToggleHelper featureToggleHelper)
            : base(
                  broadcastDataRepositoryFactory, 
                  impressionsCalculationEngine,
                  planBuyingInventoryQuarterCalculatorEngine,
                  mediaMonthAndWeekAggregateCache,
                  daypartCache,
                  quarterCalculationEngine,
                  spotLengthEngine, 
                  featureToggleHelper)
        {
        }

        public List<PlanBuyingInventoryProgram> UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(
            List<PlanDaypartDto> dayparts,
            List<PlanBuyingInventoryProgram> programs,
            DisplayDaypart planDisplayDaypartDays)
        {
            return FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(dayparts, programs, planDisplayDaypartDays);
        }

        public List<PlanBuyingInventoryProgram> UT_FilterProgramsByMinAndMaxCPM(
            List<PlanBuyingInventoryProgram> programs,
            decimal? minCPM,
            decimal? maxCPM,
            bool isProprietary)
        {
            return CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, minCPM, maxCPM, isProprietary);
        }

        public void UT_ApplyInflationFactorToSpotCost(List<PlanBuyingInventoryProgram> programs, double? inflationFactor)
        {
            ApplyInflationFactorToSpotCost(programs, inflationFactor);
        }

        public DisplayDaypart UT_GetPlanDaypartDaysFromPlanFlight(List<int> flightDays, List<DateRange> planFlightDateRanges)
        {
            return GetDaypartDaysFromFlight(flightDays, planFlightDateRanges);
        }

        public void UT_ApplyNTIConversionToNSI(
            PlanDto plan,
            List<PlanBuyingInventoryProgram> programs)
        {
            ApplyNTIConversionToNSI(plan, programs);
        }

        public List<PlanBuyingInventoryProgram> UT_GetFullPrograms(
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
