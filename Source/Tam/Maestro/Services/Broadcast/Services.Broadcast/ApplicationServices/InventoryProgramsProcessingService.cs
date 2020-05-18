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
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

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

        string ImportInventoryProgramsResults(Stream fileStream, string fileName);

        InventoryProgramsBySourceJobEnqueueResultDto QueueProcessInventoryProgramsBySourceJobUnprocessed(int sourceId, DateTime startDate, DateTime endDate,
            string username);

        [Queue("inventoryprogramsprocessing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryProgramsBySourceJobUnprocessed(int jobId);

        /// <summary>
        /// Pick up the enrichment result files from the drop folder and marshal them to the import process.
        /// </summary>
        [Queue("processprogramenrichedinventoryfiles")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void ProcessProgramEnrichedInventoryFiles();
    }

    public class InventoryProgramsProcessingService : BroadcastBaseClass,  IInventoryProgramsProcessingService
    {
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;
        private readonly IInventoryProgramsBySourceJobsRepository _InventoryProgramsBySourceJobsRepository;
        private readonly IEmailerService _EmailerService;

        private readonly IInventoryProgramsProcessorFactory _InventoryProgramsProcessorFactory;

        public InventoryProgramsProcessingService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IEmailerService emailerService,
            IInventoryProgramsProcessorFactory inventoryProgramsProcessorFactory)
        {
            _BackgroundJobClient = backgroundJobClient;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryProgramsByFileJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
            _InventoryProgramsBySourceJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsBySourceJobsRepository>();
            _EmailerService = emailerService;
            _InventoryProgramsProcessorFactory = inventoryProgramsProcessorFactory;
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
            try
            {
                var engine = _InventoryProgramsProcessorFactory.GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType.BySource);
                var result = engine.ProcessInventoryJob(jobId);
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

        public string ImportInventoryProgramsResults(Stream fileStream, string fileName)
        {
            // either processor type will work
            var engine = _InventoryProgramsProcessorFactory.GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType.ByFile);
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);
            return result;
        }

        /// <inheritdoc />
        public void ProcessProgramEnrichedInventoryFiles()
        {
            try
            {
                const int dayOffset = -1;
                var engine = _InventoryProgramsProcessorFactory.GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType.ByFile);
                engine.ImportInventoryProgramResultsFromDirectory(dayOffset);
            }
            catch (Exception ex)
            {
                _LogError("Exception caught attempting to process enriched inventory files from the directory.", ex);
                // rethrow so that the caller (background service) Will mark it's job also as a fail
                throw;
            }
        }

        private void _ReportInventoryProgramsBySourceGroupJobCompleted(int jobId)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);

            if (job.JobGroupId.HasValue == false)
            {
                // only report if the failure occurred within a group process.
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

            var toEmails = _GetProcessingBySourceResultReportToEmails();
            if (toEmails?.Any() != true)
            {
                throw new InvalidOperationException($"Failed to send notification email.  Email addresses are not configured correctly.");
            }

            // PRI-25264 : disabling sending the email
            // the engine will send on error
            //_EmailerService.QuickSend(false, body.ToString(), subject, priority, toEmails);
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

        protected virtual string[] _GetProcessingBySourceResultReportToEmails()
        {
            var raw = BroadcastServiceSystemParameter.InventoryProcessingNotificationEmails;
            var split = raw.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            return split;
        }
    }
}