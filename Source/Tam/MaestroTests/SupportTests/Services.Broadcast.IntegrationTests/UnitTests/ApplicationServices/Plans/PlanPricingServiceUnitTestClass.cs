using System.Collections.Generic;
using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    public class PlanPricingServiceUnitTestClass : PlanPricingService
    {
        public PlanPricingServiceUnitTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISpotLengthEngine spotLengthEngine,
            IPricingApiClient pricingApiClient,
            IBackgroundJobClient backgroundJobClient,
            IPlanPricingInventoryEngine planPricingInventoryEngine,
            IBroadcastLockingManagerApplicationService lockingManagerApplicationService,
            IDaypartCache daypartCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
            : base(
                  broadcastDataRepositoryFactory, 
                  spotLengthEngine, 
                  pricingApiClient, 
                  backgroundJobClient,
                  planPricingInventoryEngine, 
                  lockingManagerApplicationService,
                  daypartCache,
                  mediaMonthAndWeekAggregateCache)
        {
        }

        public List<PlanPricingApiRequestSpotsDto> UT_GetPricingModelSpots(List<PlanPricingInventoryProgram> programs)
        {
            return _GetPricingModelSpots(programs);
        }

        public bool UT_AreImpressionsValidForPricingModelInput(decimal? impressions)
        {
            return _AreImpressionsValidForPricingModelInput(impressions);
        }

        public bool UT_IsSpotCostValidForPricingModelInput(double? spotCost)
        {
            return _IsSpotCostValidForPricingModelInput(spotCost);
        }

        public List<PlanPricingApiRequestWeekDto> UT_GetPricingModelWeeks(PlanDto plan, List<PricingEstimate> proprietaryEstimates)
        {
            return _GetPricingModelWeeks(plan, proprietaryEstimates);
        }

        public bool UT_AreWeeklyImpressionsValidForPricingModelInput(double? impressions)
        {
            return _IsSpotCostValidForPricingModelInput(impressions);
        }
    }
}