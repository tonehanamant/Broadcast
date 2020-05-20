using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventoryEngineTestClass : PlanPricingInventoryEngine
    {
        public PlanPricingInventoryEngineTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsCalculationEngine impressionsCalculationEngine,
            IGenreCache genreCache,
            IPlanPricingInventoryQuarterCalculatorEngine planPricingInventoryQuarterCalculatorEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IDaypartCache daypartCache)
            : base(
                  broadcastDataRepositoryFactory, 
                  impressionsCalculationEngine, 
                  genreCache,
                  planPricingInventoryQuarterCalculatorEngine,
                  mediaMonthAndWeekAggregateCache,
                  daypartCache)
        {
        }

        public List<PlanPricingInventoryProgram> UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs,
            DisplayDaypart planDisplayDaypartDays)
        {
            return FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planDisplayDaypartDays);
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

        public DisplayDaypart UT_GetPlanDaypartDaysFromPlanFlight(PlanDto plan, List<DateRange> planFlightDateRanges)
        {
            return GetPlanDaypartDaysFromPlanFlight(plan, planFlightDateRanges);
        }

        public void UT_ApplyNTIConversionToNSI(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs,
            DisplayDaypart planDisplayDaypartDays)
        {
            ApplyNTIConversionToNSI(plan, programs, planDisplayDaypartDays);
        }

        public List<PlanPricingInventoryProgram> UT_GetFullPrograms(List<DateRange> dateRanges, int spotLengthId,
            List<int> supportedInventorySourceTypes, List<short> availableMarkets, QuarterDetailDto planQuarter, QuarterDetailDto fallbackQuarter)
        {
            return _GetFullPrograms(dateRanges, spotLengthId, supportedInventorySourceTypes, availableMarkets, planQuarter, fallbackQuarter);
        }
    }
}
