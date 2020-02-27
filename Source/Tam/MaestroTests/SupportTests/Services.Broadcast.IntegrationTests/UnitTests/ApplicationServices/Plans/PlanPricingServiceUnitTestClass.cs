using System.Collections.Generic;
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
            IBroadcastLockingManagerApplicationService lockingManagerApplicationService)
            : base(broadcastDataRepositoryFactory, spotLengthEngine, pricingApiClient, backgroundJobClient,
                planPricingInventoryEngine, lockingManagerApplicationService)
        {

        }

        public List<PlanPricingApiRequestSpotsDto> UT_GetPricingModelSpots(List<PlanPricingInventoryProgram> programs)
        {
            var result = _GetPricingModelSpots(programs);
            return result;
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