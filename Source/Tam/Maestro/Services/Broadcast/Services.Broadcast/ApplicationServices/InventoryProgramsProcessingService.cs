using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryProgramsProcessingService : IApplicationService
    {
        InventoryProgramsByFileJobEnqueueResultDto QueueProcessInventoryProgramsByFileJob(int fileId, string username);

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
    }

    public class InventoryProgramsProcessingService : IInventoryProgramsProcessingService
    {
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;
        private readonly IInventoryProgramsBySourceJobsRepository _InventoryProgramsBySourceJobsRepository;
        private readonly IInventoryProgramsProcessingEngine _InventoryProgramsProcessingEngine;
        private readonly IEmailerService _EmailerService;


        public InventoryProgramsProcessingService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IInventoryProgramsProcessingEngine inventoryProgramsProcessingEngine,
            IEmailerService emailerService)
        {
            _BackgroundJobClient = backgroundJobClient;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryProgramsByFileJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
            _InventoryProgramsBySourceJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsBySourceJobsRepository>();
            _InventoryProgramsProcessingEngine = inventoryProgramsProcessingEngine;
            _EmailerService = emailerService;
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
            string username, Guid? jobGroupId = null)
        {
            var jobId = _InventoryProgramsBySourceJobsRepository.QueueJob(sourceId, startDate, endDate, username, _GetDateTimeNow(), jobGroupId);
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
                oldJob.EndDate, username, _GetDateTimeNow(), null);
            var newJob = _InventoryProgramsBySourceJobsRepository.GetJob(newJobId);

            _DoEnqueueProcessInventoryProgramsBySourceJob(newJob.Id);

            var result = new InventoryProgramsBySourceJobEnqueueResultDto();
            result.Jobs.Add(newJob);
            return result;
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJob(int jobId)
        {
            try
            {
                var result = _InventoryProgramsProcessingEngine.ProcessInventoryProgramsBySourceJob(jobId);
                return result;
            }
            finally
            {
                _ReportInventoryProgramsBySourceGroupJobCompleted(jobId);
            }
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

            LogHelper.Logger.Info($"{nameof(QueueProcessInventoryProgramsBySourceForWeeksFromDate)} : kicked off Job Group Id '{jobGroupId}' " 
                                    + $"for '{inventorySources.Count}' sources with start date '{startDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}' "
                                    + $"and end date '{endDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}'.");

            return result;
        }

        private void _ReportInventoryProgramsBySourceGroupJobCompleted(int jobId)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);

            if (job.JobGroupId.HasValue == false)
            {
                // only report if the failure occured within a group process.
                // A singular job can be triggered through the api directly or through the Maintenance screen.
                return;
            }

            if (job.Status == InventoryProgramsJobStatus.Completed)
            {
                // nothing to report.
                return;
            }

            var source = _InventoryRepository.GetInventorySource(job.InventorySourceId);

            var priority = MailPriority.Normal;
            var subject = $"Broadcast Process Inventory Programs job completed";

            if (job.Status == InventoryProgramsJobStatus.Error)
            {
                subject += $" with Errors";
                priority = MailPriority.High;
            }
            else if (job.Status == InventoryProgramsJobStatus.Warning)
            {
                subject += $" with Warnings";
            }

            subject += $" : Source '{source.Name}' - Group '{job.JobGroupId}'";

            var body = new StringBuilder();
            body.AppendLine("Hello,");
            body.AppendLine();
            body.AppendLine($"Broadcast Job 'InventoryProgramsBySourceGroupJob' completed.");
            body.AppendLine();
            body.AppendLine($"\tJobGroupID : {job.JobGroupId}");
            body.AppendLine($"\tInventory Source : {source.Name}");

            if (job.Status == InventoryProgramsJobStatus.Error)
            {
                body.AppendLine();
                body.AppendLine($"Error : ");

                body.AppendLine($"\t{job.StatusMessage}");
            }

            if (job.Status == InventoryProgramsJobStatus.Warning)
            {
                body.AppendLine();
                body.AppendLine($"Warning : ");

                body.AppendLine($"\t{job.StatusMessage}");
            }

            body.AppendLine();
            body.AppendLine($"Have a nice day.");

            _SendProcessingBySourceResultReportEmail(subject, body.ToString(), priority);
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

        private void _SendProcessingBySourceResultReportEmail(string subject, string body, MailPriority priority)
        {
            var toEmails = _GetProcessingBySourceResultReportToEmails();
            if (toEmails?.Any() != true)
            {
                throw new InvalidOperationException($"Failed to send notification email.  Email addresses are not configured correctly.");
            }

            _EmailerService.QuickSend(true, body, subject, priority, toEmails);
        }

        protected virtual string[] _GetProcessingBySourceResultReportToEmails()
        {
            var raw = BroadcastServiceSystemParameter.InventoryProcessingNotificationEmails;
            var split = raw.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            return split;
        }
    }
}