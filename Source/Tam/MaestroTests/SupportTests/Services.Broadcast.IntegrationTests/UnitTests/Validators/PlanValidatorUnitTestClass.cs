using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Validators;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    /// <summary>
    /// Unit test class to expose some protected methods to testing.
    /// </summary>
    /// <seealso cref="Services.Broadcast.Validators.PlanValidator" />
    public class PlanValidatorUnitTestClass : PlanValidator
    {
        public PlanValidatorUnitTestClass(IBroadcastAudiencesCache broadcastAudiencesCache
            , IRatingForecastService ratingForecastService
            , ITrafficApiCache trafficApiCache
            , IDataRepositoryFactory broadcastDataRepositoryFactory
            , ICreativeLengthEngine creativeLengthEngine)
            : base(broadcastAudiencesCache,
                ratingForecastService, trafficApiCache,
                broadcastDataRepositoryFactory, creativeLengthEngine)
        {
        }

        public void UT_ValidateWeeklyBreakdownWeeks(PlanDto plan)
        {
            _ValidateWeeklyBreakdownWeeks(plan);
        }
    }
}