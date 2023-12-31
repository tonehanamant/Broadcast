﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryProgramsProcessingService : IApplicationService
    {
        InventoryProgramsByFileJobEnqueueResultDto QueueProcessInventoryProgramsByFileJob(int fileId, string username);

        InventoryProgramsByFileJobEnqueueResultDto QueueProcessInventoryProgramsByFileJobByFileName(string fileName, string username);

        InventoryProgramsByFileJobEnqueueResultDto ReQueueProcessInventoryProgramsByFileJob(int jobId, string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsByFileJob(int jobId);

        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceJob(int sourceId, DateTime startDate, DateTime endDate, 
            string username, Guid? jobGroupId = null);

        InventoryProgramsBySourceJobEnqueueResultDto ReQueueProcessInventoryProgramsBySourceJob(int jobId, string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJob(int jobId);

        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromDate(DateTime orientByDate, string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromNow(string username);

        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceJobUnprocessed(int sourceId, DateTime startDate, DateTime endDate,
            string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJobUnprocessed(int jobId);

        int QueueRepairInventoryProgramsJob();

        string CancelQueueRepairInventoryProgramsJob(int hangfireJobId);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void PerformRepairInventoryPrograms(CancellationToken token);
    }

    public class InventoryProgramsProcessingService : BroadcastBaseClass,  IInventoryProgramsProcessingService
    {
        // PRI-25264 : disabling sending the email

        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;
        private readonly IInventoryProgramsBySourceJobsRepository _InventoryProgramsBySourceJobsRepository;
        private readonly IEmailerService _EmailerService;
        private readonly IInventoryProgramsRepairEngine _InventoryProgramsRepairEngine;

        private readonly IInventoryProgramsProcessorFactory _InventoryProgramsProcessorFactory;

        public InventoryProgramsProcessingService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IEmailerService emailerService,
            IInventoryProgramsProcessorFactory inventoryProgramsProcessorFactory,
            IInventoryProgramsRepairEngine inventoryProgramsRepairEngine, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _BackgroundJobClient = backgroundJobClient;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryProgramsByFileJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
            _InventoryProgramsBySourceJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsBySourceJobsRepository>();
            _EmailerService = emailerService;
            _InventoryProgramsProcessorFactory = inventoryProgramsProcessorFactory;
            _InventoryProgramsRepairEngine = inventoryProgramsRepairEngine;
        }

        public InventoryProgramsByFileJobEnqueueResultDto QueueProcessInventoryProgramsByFileJob(int fileId, string username)
        {
            // validate file exists.  Will throw.
            _InventoryFileRepository.GetInventoryFileById(fileId);

            var job = new InventoryProgramsByFileJob { InventoryFileId = fileId, QueuedBy = username, QueuedAt = _GetDateTimeNow() };
            var jobId = _InventoryProgramsByFileJobsRepository.SaveEnqueuedJob(job);
            job.Id = jobId;

            _DoEnqueueProcessInventoryProgramsByFileJob(job.Id);

            var result = new InventoryProgramsByFileJobEnqueueResultDto {Job = job};
            return result;
        }

        public InventoryProgramsByFileJobEnqueueResultDto QueueProcessInventoryProgramsByFileJobByFileName(string fileName,
            string username)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("A filename is required.");
            }

            var trimmedFileName = fileName.Trim();
            var latestFileId = _InventoryFileRepository.GetLatestInventoryFileIdByName(trimmedFileName);

            if (latestFileId == 0)
            {
                throw new InvalidOperationException($"File '{trimmedFileName}' not found.");
            }

            return QueueProcessInventoryProgramsByFileJob(latestFileId, username);
        }

        public InventoryProgramsByFileJobEnqueueResultDto ReQueueProcessInventoryProgramsByFileJob(int jobId, string username)
        {
            var oldJob = _InventoryProgramsByFileJobsRepository.GetJob(jobId);
            var newJob = new InventoryProgramsByFileJob { InventoryFileId = oldJob.InventoryFileId, QueuedBy = username, QueuedAt = _GetDateTimeNow() };
            var newJobId = _InventoryProgramsByFileJobsRepository.SaveEnqueuedJob(newJob);
            newJob.Id = newJobId;

            _DoEnqueueProcessInventoryProgramsByFileJob(newJob.Id);

            var result = new InventoryProgramsByFileJobEnqueueResultDto { Job = newJob };
            return result;
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsByFileJob(int jobId)
        {
            var engine = _InventoryProgramsProcessorFactory.GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType.ByFile);
            var result = engine.ProcessInventoryJob(jobId);
            return result;
        }

        public InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceJob(int sourceId, DateTime startDate, DateTime endDate,
            string username, Guid? jobGroupId = null)
        {
            var job = new InventoryProgramsBySourceJob
            {
                InventorySourceId = sourceId,
                StartDate =  startDate,
                EndDate = endDate,
                QueuedBy = username,
                QueuedAt = _GetDateTimeNow(),
                JobGroupId = jobGroupId
            };

            var jobId = _InventoryProgramsBySourceJobsRepository.SaveEnqueuedJob(job);
            job.Id = jobId;

            _DoEnqueueProcessInventoryProgramsBySourceJob(job.Id);

            var result = new InventoryProgramsBySourceJobEnqueueResultDto { Jobs = new List<InventoryProgramsBySourceJob> {job} };
            return result;
        }

        public InventoryProgramsBySourceJobEnqueueResultDto ReQueueProcessInventoryProgramsBySourceJob(int jobId,
            string username)
        {
            var oldJob = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            var newJob = new InventoryProgramsBySourceJob
            {
                InventorySourceId = oldJob.InventorySourceId,
                StartDate = oldJob.StartDate,
                EndDate = oldJob.EndDate,
                QueuedBy = username,
                QueuedAt = _GetDateTimeNow(),
                JobGroupId = oldJob.JobGroupId
            };

            var newJobId = _InventoryProgramsBySourceJobsRepository.SaveEnqueuedJob(newJob);
            newJob.Id = newJobId;

            _DoEnqueueProcessInventoryProgramsBySourceJob(newJob.Id);

            var result = new InventoryProgramsBySourceJobEnqueueResultDto();
            result.Jobs.Add(newJob);
            return result;
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJob(int jobId)
        {
            var engine = _InventoryProgramsProcessorFactory.GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType.BySource);
            var result = engine.ProcessInventoryJob(jobId);
            return result;
        }

        public InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromNow(string username)
        {
            return QueueProcessInventoryProgramsBySourceForWeeksFromDate(_GetDateTimeNow(), username);
        }

        public InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceForWeeksFromDate(DateTime orientByDate, string username)
        {
            var jobGroupId = Guid.NewGuid();
            var result = new InventoryProgramsBySourceJobEnqueueResultDto{JobGroupId = jobGroupId };

            const int weeksBack = 2;
            const int weeksForward = 3;
            const int daysInAWeek = 7;

            var startDate = orientByDate.AddDays(-1 * daysInAWeek * weeksBack);
            var endDate = orientByDate.AddDays(daysInAWeek * weeksForward);

            var inventorySources = _InventoryRepository.GetInventorySources();

            foreach (var inventorySource in inventorySources)
            {
                var sourceEnqueueResult = QueueProcessInventoryProgramsBySourceJob(inventorySource.Id, startDate, endDate, username, jobGroupId);
                result.Jobs.AddRange(sourceEnqueueResult.Jobs);
            }

            _LogInfo($"{nameof(QueueProcessInventoryProgramsBySourceForWeeksFromDate)} : kicked off Job Group Id '{jobGroupId}' " 
                                    + $"for '{inventorySources.Count}' sources with start date '{startDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}' "
                                    + $"and end date '{endDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}'.");

            return result;
        }

        public InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceJobUnprocessed(int sourceId,
            DateTime startDate, DateTime endDate, string username)
        {
            var job = new InventoryProgramsBySourceJob
            {
                InventorySourceId = sourceId,
                StartDate = startDate,
                EndDate = endDate,
                QueuedBy = username,
                QueuedAt = _GetDateTimeNow(),
                JobGroupId = null
            };
            var jobId = _InventoryProgramsBySourceJobsRepository.SaveEnqueuedJob(job);
            job.Id = jobId;

            _DoEnqueueProcessInventoryProgramsBySourceJobUnprocessed(job.Id);

            var result = new InventoryProgramsBySourceJobEnqueueResultDto { Jobs = new List<InventoryProgramsBySourceJob> { job } };
            return result;
        }

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJobUnprocessed(int jobId)
        {
            var engine = _InventoryProgramsProcessorFactory.GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType.BySourceUnprocessed);
            var result = engine.ProcessInventoryJob(jobId);
            return result;
        }

        public int QueueRepairInventoryProgramsJob()
        {
            var hangfireId = _BackgroundJobClient.Enqueue<IInventoryProgramsProcessingService>(x => x.PerformRepairInventoryPrograms(CancellationToken.None));
            // it always comes back as an int.
            var hangfireIdAsInt = int.Parse(hangfireId);
            return hangfireIdAsInt;
        }

        public string CancelQueueRepairInventoryProgramsJob(int hangfireJobId)
        {
            _LogInfo($"Canceling RepairInventoryProgramsJob with hangfireJobId '{hangfireJobId}'.");
            try
            {
                _BackgroundJobClient.Delete(hangfireJobId.ToString());
                return $"Hangfire job '{hangfireJobId}' has been canceled.";
            }
            catch (Exception ex)
            {
                _LogError($"Exception caught attempting to cancel hangfire job '{hangfireJobId}'.", ex);

                var msg = $"Error caught attempting to cancel hangfire job '{hangfireJobId}'.  See log for details.";
                return msg;
            }
        }

        public void PerformRepairInventoryPrograms(CancellationToken token)
        {
            _LogInfo("Processing RepairInventoryPrograms starting.");
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                _InventoryProgramsRepairEngine.RepairInventoryPrograms(token);
            }
            finally
            {
                sw.Stop();
                _LogInfo($"Processing RepairInventoryPrograms Completed in {sw.ElapsedMilliseconds} ms.");
            }
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

        protected virtual void _DoEnqueueProcessInventoryProgramsBySourceJobUnprocessed(int jobId)
        {
            _BackgroundJobClient.Enqueue<IInventoryProgramsProcessingService>(x => x.ProcessInventoryProgramsBySourceJobUnprocessed(jobId));
        }
    }
}