using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryProgramsByFileJobsRepository : IDataRepository, IInventoryProgramsJobsRepository
    {
        int QueueJob(int fileId, string queuedBy, DateTime queuedAt);

        InventoryProgramsByFileJob GetJob(int jobId);

        InventoryProgramsByFileJob GetLatestJob();
    }

    public class InventoryProgramsByFileJobsRepository : InventoryProgramsJobsRepositoryBase, IInventoryProgramsByFileJobsRepository
    {
        public InventoryProgramsByFileJobsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public int QueueJob(int fileId, string queuedBy, DateTime queuedAt)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var fileJob = new inventory_programs_by_file_jobs
                    {
                        inventory_file_id = fileId,
                        status = (int)InventoryProgramsJobStatus.Queued,
                        queued_by = queuedBy,
                        queued_at = queuedAt,
                        completed_at = null
                    };

                    context.inventory_programs_by_file_jobs.Add(fileJob);
                    context.SaveChanges();

                    return fileJob.id;
                }
            );
        }

        public InventoryProgramsByFileJob GetJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var job = context.inventory_programs_by_file_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                    return _MapInventoryProgramsByFileJob(job);
                }
            );
        }

        public InventoryProgramsByFileJob GetLatestJob()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var job = context.inventory_programs_by_file_jobs
                        .Where(j => j.status == (int)InventoryProgramsJobStatus.Queued)
                        .OrderByDescending(j => j.queued_at)
                        .FirstOrDefault();

                    return job == null ? null : _MapInventoryProgramsByFileJob(job);
                });
        }

        protected override void _UpdateJob(int jobId, InventoryProgramsJobStatus status, string statusMessage, DateTime? completedAt)
        {
            _InReadUncommitedTransaction(context =>
                {
                    var job = context.inventory_programs_by_file_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                    job.status = (int)status;
                    job.status_message = _GetSizedStatusMessage(statusMessage);
                    job.completed_at = completedAt;

                    context.SaveChanges();
                }
            );
        }

        protected override void _UpdateJobNotes(int jobId, string message, DateTime timestamp)
        {
            var note = new inventory_programs_by_file_job_notes
            {
                job_id = jobId,
                text = message,
                created_at = timestamp
            };
            _InReadUncommitedTransaction(context =>
                {
                    context.inventory_programs_by_file_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                    context.inventory_programs_by_file_job_notes.Add(note);

                    context.SaveChanges();
                }
            );
        }

        private InventoryProgramsByFileJob _MapInventoryProgramsByFileJob(inventory_programs_by_file_jobs job)
        {
            return new InventoryProgramsByFileJob
            {
                Id = job.id,
                InventoryFileId = job.inventory_file_id,
                Status = (InventoryProgramsJobStatus)job.status,
                StatusMessage = job.status_message,
                QueuedAt = job.queued_at,
                QueuedBy = job.queued_by,
                CompletedAt = job.completed_at
            };
        }
    }
}