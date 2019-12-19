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
    public interface IInventoryFileProgramEnrichmentJobsRepository : IDataRepository
    {
        int QueueJob(int fileId, string queuedBy, DateTime queuedAt);

        void UpdateJobStatus(int jobId, InventoryFileProgramEnrichmentJobStatus status);

        void SetJobCompleteError(int jobId, string errorMessage);

        void SetJobCompleteSuccess(int jobId);

        InventoryFileProgramEnrichmentJob GetJob(int jobId);

        InventoryFileProgramEnrichmentJob GetLatestJob();
    }

    public class InventoryFileProgramEnrichmentJobsRepository : BroadcastRepositoryBase, IInventoryFileProgramEnrichmentJobsRepository
    {
        public InventoryFileProgramEnrichmentJobsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public int QueueJob(int fileId, string queuedBy, DateTime queuedAt)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var fileJob = new inventory_file_program_enrichment_jobs
                    {
                        inventory_file_id = fileId,
                        status = (int)InventoryFileProgramEnrichmentJobStatus.Queued,
                        queued_by = queuedBy,
                        queued_at = queuedAt,
                        completed_at = null
                    };

                    context.inventory_file_program_enrichment_jobs.Add(fileJob);
                    context.SaveChanges();

                    return fileJob.id;
                }
            );
        }

        public void UpdateJobStatus(int jobId, InventoryFileProgramEnrichmentJobStatus status)
        {
            _UpdateJob(jobId, status, null, null);
        }

        public void SetJobCompleteError(int jobId, string errorMessage)
        {
            _UpdateJob(jobId, InventoryFileProgramEnrichmentJobStatus.Error, errorMessage, DateTime.Now);
        }

        public void SetJobCompleteSuccess(int jobId)
        {
            _UpdateJob(jobId, InventoryFileProgramEnrichmentJobStatus.Completed, null, DateTime.Now);
        }

        public InventoryFileProgramEnrichmentJob GetJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var job = context.inventory_file_program_enrichment_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                    return new InventoryFileProgramEnrichmentJob
                    {
                        Id = jobId,
                        InventoryFileId = job.inventory_file_id,
                        Status = (InventoryFileProgramEnrichmentJobStatus)job.status,
                        ErrorMessage = job.error_message,
                        QueuedAt = job.queued_at,
                        QueuedBy = job.queued_by,
                        CompletedAt = job.completed_at
                    };
                }
            );
        }

        public InventoryFileProgramEnrichmentJob GetLatestJob()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var job = context.inventory_file_program_enrichment_jobs
                        .Where(j => j.status == (int)InventoryFileProgramEnrichmentJobStatus.Queued)
                        .OrderByDescending(j => j.queued_at)
                        .FirstOrDefault();

                    return job == null ? null : new InventoryFileProgramEnrichmentJob
                    {
                        Id = job.id,
                        InventoryFileId = job.inventory_file_id,
                        Status = (InventoryFileProgramEnrichmentJobStatus)job.status,
                        ErrorMessage = job.error_message,
                        QueuedAt = job.queued_at,
                        QueuedBy = job.queued_by,
                        CompletedAt = job.completed_at
                    };
                });
        }

        private void _UpdateJob(int jobId, InventoryFileProgramEnrichmentJobStatus status, string errorMessage, DateTime? completedAt)
        {
            _InReadUncommitedTransaction(context =>
                {
                    var job = context.inventory_file_program_enrichment_jobs.Single(j => j.id == jobId, $"Job with id '{jobId}' not found.");
                    job.error_message = errorMessage;
                    job.status = (int)status;
                    job.completed_at = completedAt;

                    context.SaveChanges();
                }
            );
        }
    }
}