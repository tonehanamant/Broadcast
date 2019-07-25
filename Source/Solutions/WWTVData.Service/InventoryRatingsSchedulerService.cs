﻿using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace WWTVData.Service
{
    public class InventoryRatingsSchedulerService : ScheduledServiceMethod
    {
        //private readonly int _SecondsBetweenRuns = 1 * 60; // 1 minute. This should come from a system paremeter
        //private readonly int _NumberOfParallerJobs = 2; // Also take from system params
        public InventoryRatingsSchedulerService() : base(null){}

        public override int SecondsBetweenRuns
        {
            get { return BroadcastServiceSystemParameter.InventoryRatingsJobIntervalSeconds; }
        }

        public override string ServiceName
        {
            get { return "Inventory Ratings Scheduler"; }
        }

        protected override DateTime? RunWeeklyWhen
        {
            get { return null; } //This service runs on an interval
        }

        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;
            //BaseWindowsService.LogServiceEvent("Checking WWTV OutPost files. . .");

            try
            {
                var summaryDataList = new List<int>();
                var service = ApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                var jobs = service.GetQueuedJobs(BroadcastServiceSystemParameter.InventoryRatingsParallelJobs);

                if (jobs.Count > 0) {
                    BaseWindowsService.LogServiceEvent($"Processing {jobs.Count} inventory ratings jobs");
                }
                else
                {
                    BaseWindowsService.LogServiceEvent($"No inventory ratings jobs found");
                }
                var tasks = new List<Task>();
                foreach(var job in jobs)
                {
                    BaseWindowsService.LogServiceEvent($"Processing inventory ratings Job {job.id}");
                    tasks.Add(Task.Run(() =>
                    {
                        summaryDataList.Add(service.ProcessInventoryRatingsJob(job.id.Value));
                    }));
                }
                Task.WaitAll(tasks.ToArray());

                ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().AggregateInventorySummaryData(summaryDataList.Distinct().ToList());

            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error processing inventory ratings job", e);
                return false;
            }

            return true;
        }
    }
}
