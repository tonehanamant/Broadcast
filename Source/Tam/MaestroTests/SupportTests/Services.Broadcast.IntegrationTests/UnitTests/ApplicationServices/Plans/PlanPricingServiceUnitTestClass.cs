using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using System;
using Services.Broadcast.BusinessEngines.PlanPricing;

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
                                  IDateTimeEngine dateTimeEngine,
                                  IWeeklyBreakdownEngine weeklyBreakdownEngine,
                                  IPlanPricingBandCalculationEngine planPricingBandCalculationEngine,
                                  IPlanPricingStationCalculationEngine planPricingStationCalculationEngine,
                                  IPlanPricingMarketResultsEngine planPricingMarketResultsEngine,
                                  IPricingRequestLogClient pricingApiRequestSerializerClient)
        : base(
            broadcastDataRepositoryFactory,
            spotLengthEngine,
            pricingApiClient,
            backgroundJobClient,
            planPricingInventoryEngine,
            lockingManagerApplicationService,
            daypartCache,
            mediaMonthAndWeekAggregateCache,
            dateTimeEngine,
            weeklyBreakdownEngine,
            planPricingBandCalculationEngine,
            planPricingStationCalculationEngine,
            planPricingMarketResultsEngine,
            pricingApiRequestSerializerClient)
        {
            
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime ?? DateTime.Now;
        }
    }
}