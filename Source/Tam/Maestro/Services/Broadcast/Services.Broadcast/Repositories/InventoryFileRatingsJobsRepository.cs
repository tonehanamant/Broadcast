using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryFileRatingsJobsRepository : IDataRepository
    {
        int AddJob(InventoryFileRatingsProcessingJob job);
        List<InventoryFileRatingsProcessingJob> GetJobsBatch(int limit);
        InventoryFileRatingsProcessingJob GetLatestJob();
        InventoryFileRatingsProcessingJob GetJobById(int jobId);
        void UpdateJob(InventoryFileRatingsProcessingJob job);
        InventoryFileRatingsProcessingJob GetJobByFileId(int fileId);
    }
    public class InventoryFileRatingsJobsRepository : BroadcastRepositoryBase, IInventoryFileRatingsJobsRepository
    {
        public InventoryFileRatingsJobsRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
        ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int AddJob(InventoryFileRatingsProcessingJob job)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var fileJob = new inventory_file_ratings_jobs
                    {
                        inventory_file_id = job.InventoryFileId,
                        status = (int) job.Status,
                        queued_at = job.QueuedAt
                    };

                    context.inventory_file_ratings_jobs.Add(fileJob);
                    context.SaveChanges();

                    return fileJob.id;
                });
        }

        public InventoryFileRatingsProcessingJob GetJobById(int jobId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var jobs = context.inventory_file_ratings_jobs
                        .Where(j => j.id == jobId)
                        .Select(j => new InventoryFileRatingsProcessingJob
                        {
                            id = j.id,
                            InventoryFileId = j.inventory_file_id,
                            Status = (InventoryFileRatingsProcessingStatus)j.status,
                            QueuedAt = j.queued_at,
                            CompletedAt = j.completed_at
                        }).SingleOrDefault();

                    return jobs;
                });
        }

        public InventoryFileRatingsProcessingJob GetJobByFileId(int fileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var jobs = context.inventory_file_ratings_jobs
                        .Where(j => j.inventory_file_id == fileId)
                        .OrderBy(j => j.queued_at)
                        .Select(j => new InventoryFileRatingsProcessingJob
                        {
                            id = j.id,
                            InventoryFileId = j.inventory_file_id,
                            Status = (InventoryFileRatingsProcessingStatus)j.status,
                            QueuedAt = j.queued_at,
                            CompletedAt = j.completed_at
                        }).SingleOrDefault();

                    return jobs;
                });
        }

        public List<InventoryFileRatingsProcessingJob> GetJobsBatch(int limit)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var jobs = context.inventory_file_ratings_jobs
                        .Where(j => j.status == (int)InventoryFileRatingsProcessingStatus.Queued)
                        .OrderBy(j => j.queued_at).Take(limit)
                        .Select(j => new InventoryFileRatingsProcessingJob
                        {
                            id = j.id,
                            InventoryFileId = j.inventory_file_id,
                            Status = (InventoryFileRatingsProcessingStatus)j.status,
                            QueuedAt = j.queued_at,
                            CompletedAt = j.completed_at
                        }).ToList();

                    return jobs;
                });
        }

        public void UpdateJob(InventoryFileRatingsProcessingJob job)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var fileJob = context.inventory_file_ratings_jobs.Find(job.id);
                    fileJob.status = (int)job.Status;
                    fileJob.completed_at = job.CompletedAt;

                    context.SaveChanges();
                });
        }

        public InventoryFileRatingsProcessingJob GetLatestJob()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var job = context.inventory_file_ratings_jobs
                        .Where(j => j.status == (int)InventoryFileRatingsProcessingStatus.Queued)
                        .OrderByDescending(j => j.queued_at)
                        .FirstOrDefault();
                    
                    return job == null ? null : new InventoryFileRatingsProcessingJob
                    {
                        id = job.id,
                        InventoryFileId = job.inventory_file_id,
                        Status = (InventoryFileRatingsProcessingStatus)job.status,
                        QueuedAt = job.queued_at,
                        CompletedAt = job.completed_at
                    };
                });
        }
    }
}
