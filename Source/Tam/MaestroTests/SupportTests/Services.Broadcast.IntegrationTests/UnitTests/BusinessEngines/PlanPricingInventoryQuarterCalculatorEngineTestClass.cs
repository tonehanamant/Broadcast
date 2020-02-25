using System;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventoryQuarterCalculatorEngineTestClass : PlanPricingInventoryQuarterCalculatorEngine
    {
        public PlanPricingInventoryQuarterCalculatorEngineTestClass(IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
                : base(quarterCalculationEngine, mediaMonthAndWeekAggregateCache)
        {
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime ?? base._GetCurrentDateTime();
        }
    }
}