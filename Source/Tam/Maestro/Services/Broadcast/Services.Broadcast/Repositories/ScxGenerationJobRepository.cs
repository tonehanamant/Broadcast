﻿using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
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

        /// <summary>
        /// Gets the SCX file generation details.
        /// </summary>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        /// <returns></returns>
        List<ScxFileGenerationDetailDto> GetScxFileGenerationDetails(int inventorySourceId);
    }

    public class ScxGenerationJobRepository : BroadcastRepositoryBase, IScxGenerationJobRepository
    {
        public ScxGenerationJobRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper, 
            IConfigurationWebApiClient configurationWebApiClient) : 
            base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient)
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

        #region GetScxFileGenerationDetails

        /// <inheritdoc />
        public List<ScxFileGenerationDetailDto> GetScxFileGenerationDetails(int inventorySourceId)
        {
            var result = _InReadUncommitedTransaction(context =>
            {
                var details = GetGenerationDetails(context, inventorySourceId);
                return details;
            });
            return result;
        }

        private static List<ScxFileGenerationDetailDto> GetGenerationDetails(BroadcastContext context, int inventorySourceId)
        {
            var details = (from f in context.scx_generation_job_files
                    join j in context.scx_generation_jobs on f.scx_generation_job_id equals j.id
                    join d in context.daypart_codes on f.daypart_code_id equals d.id
                    where f.inventory_source_id.Equals(inventorySourceId)
                    select new ScxFileGenerationDetailDto
                    {
                        GenerationRequestDateTime = j.queued_at,
                        GenerationRequestedByUsername = j.requested_by,
                        FileName = f.file_name,
                        UnitName = f.unit_name,
                        DaypartCodeId = d.id,
                        DaypartCodeName = d.full_name,
                        StartDateTime = f.start_date,
                        EndDateTime = f.end_date,
                        ProcessingStatusId = j.status
                    })
                .ToList();
            return details;
        }
        
        #endregion // #region GetScxFileGenerationDetails
    }
}