using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;

namespace Services.Broadcast.IntegrationTests.UnitTests.PlanAggregation
{
    public class PlanAggregatorUnitTestClass : PlanAggregator
    {
        public PlanAggregatorUnitTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IQuarterCalculationEngine quarterCalculationEngine)
        : base(broadcastDataRepositoryFactory, quarterCalculationEngine)
        {
        }

        public int GetAudiencesByIdsCalledCount { get; set; }
        public int GetAllQuartersBetweenDatesCalledCount { get; set; }

        public void UT_PerformAggregations(PlanDto plan, PlanSummaryDto summary, bool runParallelAggregations = false)
        {
            PerformAggregations(plan, summary, runParallelAggregations);
        }

        public void UT_AggregateFlightDays(PlanDto plan, PlanSummaryDto summary)
        {
            AggregateFlightDays(plan, summary);
        }

        public void UT_AggregateAvailableMarkets(PlanDto plan, PlanSummaryDto summary)
        {
            AggregateAvailableMarkets(plan, summary);
        }

        public void UT_AggregateBlackoutMarkets(PlanDto plan, PlanSummaryDto summary)
        {
            AggregateBlackoutMarkets(plan, summary);
        }

        public void UT_AggregateAudience(PlanDto plan, PlanSummaryDto summary)
        {
            AggregateAudience(plan, summary);
        }

        public void UT_AggregateQuarters(PlanDto plan, PlanSummaryDto summary)
        {
            AggregateQuarters(plan, summary);
        }

        public void UT_AggregateProduct(PlanDto plan, PlanSummaryDto summary)
        {
            AggregateProduct(plan, summary);
        }
    }
}