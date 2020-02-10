using System.Collections.Generic;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
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

        public bool UT_IsValidForModelInput(decimal? value)
        {
            return _AreImpressionsValidForPricingModelInput(value);
        }

        public bool UT_IsValidForModelInput(double? value)
        {
            return _IsSpotCostValidForPricingModelInput(value);
        }
    }
}