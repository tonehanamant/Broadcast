using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using System;
using Services.Broadcast.BusinessEngines.PlanPricing;
using Services.Broadcast.Helpers;
using Services.Broadcast.Validators;

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
                                  IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                  IDateTimeEngine dateTimeEngine,
                                  IWeeklyBreakdownEngine weeklyBreakdownEngine,
                                  IPlanPricingBandCalculationEngine planPricingBandCalculationEngine,
                                  IPlanPricingStationCalculationEngine planPricingStationCalculationEngine,
                                  IPlanPricingMarketResultsEngine planPricingMarketResultsEngine,
                                  IPlanPricingProgramCalculationEngine planPricingProgramCalculationEngine,
                                  IPricingRequestLogClient pricingApiRequestSerializerClient,
                                  IPlanValidator planValidator,
                                  ISharedFolderService sharedFolderService,
                                  IAudienceService audienceService, 
                                  ICreativeLengthEngine creativeLengthEngine,
                                  IAsyncTaskHelper asyncTaskHelper)
        : base(
            broadcastDataRepositoryFactory,
            spotLengthEngine,
            pricingApiClient,
            backgroundJobClient,
            planPricingInventoryEngine,
            lockingManagerApplicationService,
            mediaMonthAndWeekAggregateCache,
            dateTimeEngine,
            weeklyBreakdownEngine,
            planPricingBandCalculationEngine,
            planPricingStationCalculationEngine,
            planPricingMarketResultsEngine,
            planPricingProgramCalculationEngine,
            pricingApiRequestSerializerClient,
            planValidator,
            sharedFolderService,
            audienceService,
            creativeLengthEngine,
            asyncTaskHelper)
        {
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime ?? DateTime.Now;
        }
    }
}