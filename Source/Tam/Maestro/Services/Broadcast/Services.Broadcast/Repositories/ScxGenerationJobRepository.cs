using Common.Services.Extensions;
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

        /// <summary>
        /// Gets the name of the SCX file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        string GetScxFileName(int fileId);
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
                        standard_daypart_id = job.InventoryScxDownloadRequest.StandardDaypartId,
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
                    StandardDaypartId = scxJob.standard_daypart_id,
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
                            standard_daypart_id = file.DaypartCodeId,
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
            var details = (from j in context.scx_generation_jobs 
                    join f in context.scx_generation_job_files on j.id equals f.scx_generation_job_id into fs
                    from f in fs.DefaultIfEmpty()
                    join d in context.standard_dayparts on f.standard_daypart_id equals d.id into ds
                    from d in ds.DefaultIfEmpty()
                    where j.inventory_source_id.Equals(inventorySourceId)
                    select new ScxFileGenerationDetailDto
                    {
                        GenerationRequestDateTime = j.queued_at,
                        GenerationRequestedByUsername = j.requested_by,
                        FileId = f.id,
                        Filename = f.file_name,
                        UnitName = f.unit_name,
                        DaypartCode = d.code,
                        StartDateTime = f.start_date,
                        EndDateTime = f.end_date,
                        ProcessingStatusId = j.status
                    })
                .ToList();
            return details;
        }

        #endregion // #region GetScxFileGenerationDetails

        /// <inheritdoc />
        public string GetScxFileName(int fileId)
        {
             var fileName = _InReadUncommitedTransaction(context =>
                {
                    var file = context.scx_generation_job_files.Single(s => s.id == fileId, $"File with Id {fileId} not found.");
                    return file.file_name;
                }
            );
             return fileName;
        }
    }
}