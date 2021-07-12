using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities;
using System;
using Services.Broadcast.Helpers;

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
        void AddJobNote(int jobId, InventoryFileRatingsProcessingJob.Note note);
    }
    public class InventoryFileRatingsJobsRepository : BroadcastRepositoryBase, IInventoryFileRatingsJobsRepository
    {
        public InventoryFileRatingsJobsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public void AddJobNote(int jobId, InventoryFileRatingsProcessingJob.Note note)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.inventory_file_ratings_job_notes.Add(new inventory_file_ratings_job_notes
                    {
                        inventory_file_ratings_job_id = jobId,
                        text = note.Text,
                        created_at = note.CreatedAt
                    });
                    context.SaveChanges();
                });
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
                        queued_at = job.QueuedAt,
                        inventory_file_ratings_job_notes = job.Notes.Select(x => new inventory_file_ratings_job_notes
                        {
                            text = x.Text,
                            created_at = x.CreatedAt
                        }).ToList()
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
                            Id = j.id,
                            InventoryFileId = j.inventory_file_id,
                            Status = (BackgroundJobProcessingStatus)j.status,
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
                            Id = j.id,
                            InventoryFileId = j.inventory_file_id,
                            Status = (BackgroundJobProcessingStatus)j.status,
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
                        .Where(j => j.status == (int)BackgroundJobProcessingStatus.Queued)
                        .OrderBy(j => j.queued_at).Take(limit)
                        .Select(j => new InventoryFileRatingsProcessingJob
                        {
                            Id = j.id,
                            InventoryFileId = j.inventory_file_id,
                            Status = (BackgroundJobProcessingStatus)j.status,
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
                    var fileJob = context.inventory_file_ratings_jobs.Find(job.Id);
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
                        .Where(j => j.status == (int)BackgroundJobProcessingStatus.Queued)
                        .OrderByDescending(j => j.queued_at)
                        .FirstOrDefault();
                    
                    return job == null ? null : new InventoryFileRatingsProcessingJob
                    {
                        Id = job.id,
                        InventoryFileId = job.inventory_file_id,
                        Status = (BackgroundJobProcessingStatus)job.status,
                        QueuedAt = job.queued_at,
                        CompletedAt = job.completed_at
                    };
                });
        }
    }
}
