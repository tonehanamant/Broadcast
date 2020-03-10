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
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IPlanDaypartEngine planDaypartEngine)
            : base(
                  broadcastDataRepositoryFactory, 
                  spotLengthEngine, 
                  pricingApiClient, 
                  backgroundJobClient,
                  planPricingInventoryEngine, 
                  lockingManagerApplicationService,
                  daypartCache,
                  mediaMonthAndWeekAggregateCache,
                  planDaypartEngine)
        {
        }

        public List<PlanPricingApiRequestSpotsDto> UT_GetPricingModelSpots(PlanDto plan, List<PlanPricingInventoryProgram> programs)
        {
            return _GetPricingModelSpots(plan, programs);
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

        public decimal UT_CalculatePricingCpm(List<PlanPricingAllocatedSpot> spots, List<PricingEstimate> proprietaryEstimates, double margin)
        {
            return _CalculatePricingCpm(spots, proprietaryEstimates, margin);
        }
    }
}