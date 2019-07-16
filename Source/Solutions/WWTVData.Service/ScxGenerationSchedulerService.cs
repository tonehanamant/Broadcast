using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace WWTVData.Service
{
    public class ScxGenerationSchedulerService : ScheduledServiceMethod
    {
        public ScxGenerationSchedulerService() : base(null)
        {
        }

        public override int SecondsBetweenRuns
        {
            get { return BroadcastServiceSystemParameter.ScxGenerationJobIntervalSeconds; }
        }

        public override string ServiceName => "SCX Generation Service";

        protected override DateTime? RunWeeklyWhen => null;

        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;

            try
            {
                var service = ApplicationServiceFactory.GetApplicationService<IScxGenerationService>();
                var jobs = service.GetQueuedJobs(BroadcastServiceSystemParameter.ScxGenerationParallelJobs);

                if (jobs.Count > 0)
                {
                    BaseWindowsService.LogServiceEvent($"Processing {jobs.Count} SCX generation jobs");
                }
                else
                {
                    BaseWindowsService.LogServiceEvent($"No SCX generation jobs found");
                }

                var tasks = new List<Task>();

                foreach (var job in jobs)
                {
                    BaseWindowsService.LogServiceEvent($"Processing SCX generation job {job.Id}");

                    tasks.Add(Task.Run(() =>
                    {
                        service.ProcessScxGenerationJob(job, DateTime.Now);
                    }));
                }

                Task.WaitAll(tasks.ToArray());

            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error processing SCX generation job", e);
                return false;
            }

            return true;
        }
    }
}
