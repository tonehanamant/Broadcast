using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IScxGenerationJobRepository : IDataRepository
    {
        ScxGenerationJob GetJobById(int jobId);
        int AddJob(ScxGenerationJob job);
        void UpdateJob(ScxGenerationJob job);
        List<ScxGenerationJob> GetJobsBatch(int limit);
        void SaveScxJobFiles(List<InventoryScxFile> files, ScxGenerationJob job);
    }

    public class ScxGenerationJobRepository : BroadcastRepositoryBase, IScxGenerationJobRepository
    {
        public ScxGenerationJobRepository(ISMSClient pSmsClient, 
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper, 
            IConfigurationWebApiClient configurationWebApiClient) : 
            base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient)
        {
        }

        public int AddJob(ScxGenerationJob job)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var scxJob = new scx_generation_jobs
                    {
                        inventory_source_id = job.InventoryScxDownloadRequest.InventorySourceId,
                        daypart_code_id = job.InventoryScxDownloadRequest.DaypartCodeId,
                        start_date = job.InventoryScxDownloadRequest.StartDate,
                        end_date = job.InventoryScxDownloadRequest.EndDate,
                        status = (int)job.Status,
                        queued_at = job.QueuedAt,
                        requested_by = job.RequestedBy
                    };

                    foreach (var unit in job.InventoryScxDownloadRequest.UnitNames)
                    {
                        var requestUnit = new scx_generation_job_units
                        {
                            unit_name = unit
                        };

                        scxJob.scx_generation_job_units.Add(requestUnit);
                    }

                    context.scx_generation_jobs.Add(scxJob);
                    context.SaveChanges();
                    return scxJob.id;
                });
        }

        public ScxGenerationJob GetJobById(int jobId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var jobs = context.scx_generation_jobs
                        .Include(s => s.scx_generation_job_units)
                        .Where(j => j.id == jobId)
                        .Select(_MapFromDb).Single(@"Unable to find job with id {jobId}");

                    return jobs;
                });
        }

        public void UpdateJob(ScxGenerationJob job)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var fileJob = context.scx_generation_jobs.Find(job.Id);
                    fileJob.status = (int)job.Status;
                    fileJob.completed_at = job.CompletedAt;

                    context.SaveChanges();
                });
        }

        public List<ScxGenerationJob> GetJobsBatch(int limit)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var jobs = context.scx_generation_jobs
                        .Include(s => s.scx_generation_job_units)
                        .Where(j => j.status == (int)BackgroundJobProcessingStatus.Queued)
                        .OrderBy(j => j.queued_at).Take(limit)
                        .Select(_MapFromDb).ToList();

                    return jobs;
                });
        }

        private ScxGenerationJob _MapFromDb(scx_generation_jobs scxJob)
        {
            return new ScxGenerationJob
            {
                Id = scxJob.id,
                InventoryScxDownloadRequest = new InventoryScxDownloadRequest
                {
                    InventorySourceId = scxJob.inventory_source_id,
                    DaypartCodeId = scxJob.daypart_code_id,
                    StartDate = scxJob.start_date,
                    EndDate = scxJob.end_date,
                    UnitNames = scxJob.scx_generation_job_units.Select(u => u.unit_name).ToList()
                },
                Status = (BackgroundJobProcessingStatus)scxJob.status,
                QueuedAt = scxJob.queued_at,
                CompletedAt = scxJob.completed_at,
                RequestedBy = scxJob.requested_by
            };
        }

        public void SaveScxJobFiles(List<InventoryScxFile> files, ScxGenerationJob job)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach(var file in files)
                    {
                        context.scx_generation_job_files.Add(new scx_generation_job_files
                        {
                            scx_generation_job_id = job.Id,
                            file_name = file.FileName,
                            inventory_source_id = file.InventorySource.Id,
                            daypart_code_id = file.DaypartCodeId,
                            start_date = file.StartDate,
                            end_date = file.EndDate,
                            unit_name = file.UnitName
                        });
                    }

                    context.SaveChanges();
                });
        }
    }
}