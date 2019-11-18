﻿using Hangfire;
using Services.Broadcast.ApplicationServices.Plan;
using System;

namespace Services.Broadcast.ApplicationServices
{
    public class RecurringJobsFactory
    {
        private readonly IRecurringJobManager _RecurringJobManager;
        private readonly IPlanService _PlanService;

        public RecurringJobsFactory(
            IRecurringJobManager recurringJobManager,
            IPlanService planService)
        {
            _RecurringJobManager = recurringJobManager;
            _PlanService = planService;
        }

        public void AddOrUpdateRecurringJobs()
        {
            _RecurringJobManager.AddOrUpdate(
                "plan-automatic-status-transition",
                () => _PlanService.AutomaticStatusTransitions(DateTime.Today, "automated status update", DateTime.Now, false),
                Cron.Daily,
                TimeZoneInfo.Local);
        }
    }
}
