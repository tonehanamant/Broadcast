﻿using System.Collections.Generic;
using System.Linq;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;

namespace Services.Broadcast.IntegrationTests.UnitTests.CampaignAggregation
{
    public class CampaignAggregatorUnitTestClass : CampaignAggregator
    {
        public CampaignAggregatorUnitTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory)
            : base(broadcastDataRepositoryFactory)
        {
        }

        public void UT_AggregateFlightInfo(List<PlanDto> plans, List<PlanDto> filteredPlans, CampaignSummaryDto summary)
        {
            AggregateFlightInfo(plans, filteredPlans, summary);
        }
    }
}