using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryProgramsProcessingService : IApplicationService
    {
        InventoryProgramsByFileJobEnqueueResultDto QueueProcessInventoryProgramsByFileJob(int fileId, string username);

        InventoryProgramsByFileJobEnqueueResultDto ReQueueProcessInventoryProgramsByFileJob(int jobId, string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsByFileJob(int jobId);

        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceJob(int sourceId, DateTime startDate, DateTime endDate, string username);

        InventoryProgramsBySourceJobEnqueueResultDto ReQueueProcessInventoryProgramsBySourceJob(int jobId, string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJob(int jobId);

        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromDate(DateTime orientByDate, string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromNow(string username);
    }

    public class InventoryProgramsProcessingService : IInventoryProgramsProcessingService
    {
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;
        private readonly IInventoryProgramsBySourceJobsRepository _InventoryProgramsBySourceJobsRepository;
        private readonly IInventoryProgramsProcessingEngine _InventoryProgramsProcessingEngine;
        
        public InventoryProgramsProcessingService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IInventoryProgramsProcessingEngine inventoryProgramsProcessingEngine)
        {
            _BackgroundJobClient = backgroundJobClient;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryProgramsByFileJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
            _InventoryProgramsBySourceJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsBySourceJobsRepository>();
            _InventoryProgramsProcessingEngine = inventoryProgramsProcessingEngine;
        }

        public InventoryProgramsByFileJobEnqueueResultDto QueueProcessInventoryProgramsByFileJob(int fileId, string username)
        {
            // validate file exists.  Will throw.
            _InventoryFileRepository.GetInventoryFileById(fileId);
            var jobId = _InventoryProgramsByFileJobsRepository.QueueJob(fileId, username, _GetDateTimeNow());
            var job = _InventoryProgramsByFileJobsRepository.GetJob(jobId);

            _DoEnqueueProcessInventoryProgramsByFileJob(job.Id);

            var result = new InventoryProgramsByFileJobEnqueueResultDto {Job = job};
            return result;
        }

        public InventoryProgramsByFileJobEnqueueResultDto ReQueueProcessInventoryProgramsByFileJob(int jobId, string username)
        {
            var oldJob = _InventoryProgramsByFileJobsRepository.GetJob(jobId);
            var newJobId = _InventoryProgramsByFileJobsRepository.QueueJob(oldJob.InventoryFileId, username, _GetDateTimeNow());
            var newJob = _InventoryProgramsByFileJobsRepository.GetJob(newJobId);

            _DoEnqueueProcessInventoryProgramsByFileJob(newJob.Id);

            var result = new InventoryProgramsByFileJobEnqueueResultDto { Job = newJob };
            return result;
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsByFileJob(int jobId)
        {
            var result = _InventoryProgramsProcessingEngine.ProcessInventoryProgramsByFileJob(jobId);
            return result;
        }

        public InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceJob(int sourceId, DateTime startDate, DateTime endDate,
            string username)
        {
            var jobId = _InventoryProgramsBySourceJobsRepository.QueueJob(sourceId, startDate, endDate, username, _GetDateTimeNow());
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);

            _DoEnqueueProcessInventoryProgramsBySourceJob(job.Id);

            var result = new InventoryProgramsBySourceJobEnqueueResultDto { Jobs = new List<InventoryProgramsBySourceJob> {job} };
            return result;
        }

        public InventoryProgramsBySourceJobEnqueueResultDto ReQueueProcessInventoryProgramsBySourceJob(int jobId,
            string username)
        {
            var oldJob = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            var newJobId = _InventoryProgramsBySourceJobsRepository.QueueJob(oldJob.InventorySourceId, oldJob.StartDate,
                oldJob.EndDate, username, _GetDateTimeNow());
            var newJob = _InventoryProgramsBySourceJobsRepository.GetJob(newJobId);

            _DoEnqueueProcessInventoryProgramsBySourceJob(newJob.Id);

            var result = new InventoryProgramsBySourceJobEnqueueResultDto();
            result.Jobs.Add(newJob);
            return result;
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJob(int jobId)
        {
            var result = _InventoryProgramsProcessingEngine.ProcessInventoryProgramsBySourceJob(jobId);
            return result;
        }

        public InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromNow(string username)
        {
            return QueueProcessInventoryProgramsBySourceForWeeksFromDate(_GetDateTimeNow(), username);
        }

        public InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromDate(DateTime orientByDate, string username)
        {
            var result = new InventoryProgramsBySourceJobEnqueueResultDto();

            const int weeksBack = 2;
            const int weeksForward = 3;
            const int daysInAWeek = 7;

            var startDate = orientByDate.AddDays(-1 * daysInAWeek * weeksBack);
            var endDate = orientByDate.AddDays(daysInAWeek * weeksForward);

            var inventorySources = _InventoryRepository.GetInventorySources();

            foreach (var inventorySource in inventorySources)
            {
                var sourceEnqueueResult = QueueProcessInventoryProgramsBySourceJob(inventorySource.Id, startDate, endDate, username);
                result.Jobs.AddRange(sourceEnqueueResult.Jobs);
            }

            return result;
        }

        protected virtual DateTime _GetDateTimeNow()
        {
            return DateTime.Now;
        }

        protected virtual void _DoEnqueueProcessInventoryProgramsByFileJob(int jobId)
        {
            _BackgroundJobClient.Enqueue<IInventoryProgramsProcessingService>(x => x.ProcessInventoryProgramsByFileJob(jobId));
        }

        protected virtual void _DoEnqueueProcessInventoryProgramsBySourceJob(int jobId)
        {
            _BackgroundJobClient.Enqueue<IInventoryProgramsProcessingService>(x => x.ProcessInventoryProgramsBySourceJob(jobId));
        }
    }
}