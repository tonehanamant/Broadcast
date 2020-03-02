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
    public interface IInventoryProgramsBySourceJobsRepository : IDataRepository, IInventoryProgramsJobsRepository
    {
        int QueueJob(int sourceId, DateTime startDate, DateTime endDate, string queuedBy,
            DateTime queuedAt, Guid? jobGroupId);

        InventoryProgramsBySourceJob GetJob(int jobId);
    }

    public class InventoryProgramsBySourceJobsRepository : InventoryProgramsJobsRepositoryBase, IInventoryProgramsBySourceJobsRepository
    {
        public InventoryProgramsBySourceJobsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public int QueueJob(int sourceId, DateTime startDate, DateTime endDate, string queuedBy,
            DateTime queuedAt, Guid? jobGroupId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var fileJob = new inventory_programs_by_source_jobs
                {
                    job_group_id = jobGroupId,
                    inventory_source_id = sourceId,
                    start_date = startDate,
                    end_date = endDate,
                    status = (int)InventoryProgramsJobStatus.Queued,
                    queued_by = queuedBy,
                    queued_at = queuedAt,
                    completed_at = null
                };

                context.inventory_programs_by_source_jobs.Add(fileJob);
                context.SaveChanges();

                return fileJob.id;
            }
            );
        }

        public InventoryProgramsBySourceJob GetJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var job = context.inventory_programs_by_source_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                return _MapInventoryProgramsBySourceJob(job);
            });
        }

        protected override void _UpdateJob(int jobId, InventoryProgramsJobStatus status, string statusMessage, DateTime? completedAt)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.inventory_programs_by_source_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                job.status = (int)status;
                job.status_message = _GetSizedStatusMessage(statusMessage);
                job.completed_at = completedAt;

                context.SaveChanges();
            });
        }

        protected override void _UpdateJobNotes(int jobId, string message, DateTime timestamp)
        {
            var note = new inventory_programs_by_source_job_notes()
            {
                job_id = jobId,
                text = message,
                created_at = timestamp
            };
            _InReadUncommitedTransaction(context =>
                {
                    context.inventory_programs_by_source_job_notes.Add(note);

                    context.SaveChanges();
                }
            );
        }

        private InventoryProgramsBySourceJob _MapInventoryProgramsBySourceJob(inventory_programs_by_source_jobs job)
        {
            return new InventoryProgramsBySourceJob
            {
                Id = job.id,
                JobGroupId = job.job_group_id,
                InventorySourceId = job.inventory_source_id,
                StartDate = job.start_date,
                EndDate = job.end_date,
                Status = (InventoryProgramsJobStatus)job.status,
                StatusMessage = job.status_message,
                QueuedAt = job.queued_at,
                QueuedBy = job.queued_by,
                CompletedAt = job.completed_at
            };
        }
    }
}