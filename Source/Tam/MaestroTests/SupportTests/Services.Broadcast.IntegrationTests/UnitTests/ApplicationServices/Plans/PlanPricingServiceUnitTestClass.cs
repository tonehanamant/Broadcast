﻿using System;
using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;

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
                                  IWeeklyBreakdownEngine weeklyBreakdownEngine)
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
            weeklyBreakdownEngine
            )
        {
            
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime ?? DateTime.Now;
        }
    }
}