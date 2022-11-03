using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Scx;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

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

        /// <summary>
        /// Gets the shared folder file identifier for the given file id.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        Guid? GetSharedFolderFileIdForFile(int fileId);
        /// <summary>
        /// Adds job parameters to DB.
        /// </summary>
        /// <param name="job">Job.</param>
        int AddOpenMarketJob(ScxOpenMarketsGenerationJob job);
        /// <summary>
        /// Get Job Parameters from DB.
        /// </summary>
        /// <param name="jobId">Job.</param>
        ScxOpenMarketsGenerationJob GetOpenMarketsJobById(int jobId);
        /// <summary>
        /// Update status to DB.
        /// </summary>
        /// <param name="job">Job.</param>
        void UpdateOpenMarketJob(ScxOpenMarketsGenerationJob job);
        /// <summary>
        /// save job parameters to file.
        /// </summary>
        /// <param name="files">file.</param>
        /// <param name="job">Job.</param>
        void SaveScxOpenMarketJobFiles(List<OpenMarketInventoryScxFile> files, ScxOpenMarketsGenerationJob job);
    }

    public class ScxGenerationJobRepository : BroadcastRepositoryBase, IScxGenerationJobRepository
    {
        public ScxGenerationJobRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper) :
            base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
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
                    foreach (var file in files)
                    {
                        context.scx_generation_job_files.Add(new scx_generation_job_files
                        {
                            scx_generation_job_id = job.Id,
                            file_name = file.FileName,
                            inventory_source_id = file.InventorySource.Id,
                            standard_daypart_id = file.DaypartCodeId,
                            start_date = file.StartDate,
                            end_date = file.EndDate,
                            unit_name = file.UnitName,
                            shared_folder_files_id = file.SharedFolderFileId
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

        /// <inheritdoc />
        public Guid? GetSharedFolderFileIdForFile(int fileId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = context.scx_generation_job_files
                    .Single(s => s.id == fileId,
                        $"File with Id {fileId} not found.");

                return entity.shared_folder_files_id;
            });
        }

        public int AddOpenMarketJob(ScxOpenMarketsGenerationJob job)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var scxOpenMarketJob = new scx_generation_open_market_jobs
                    {
                        start_date = job.InventoryScxOpenMarketsDownloadRequest.StartDate,
                        end_date = job.InventoryScxOpenMarketsDownloadRequest.EndDate,
                        status = (int)job.Status,
                        export_genre_type_id =(int)job.InventoryScxOpenMarketsDownloadRequest.GenreType,
                        queued_at = job.QueuedAt,
                        requested_by = job.RequestedBy,
                        completed_at = job.CompletedAt,
                        inventory_source_id = 1
                    };
                    context.scx_generation_open_market_jobs.Add(scxOpenMarketJob);
                    context.SaveChanges();
                    var scxOpenMarketJobId = scxOpenMarketJob.id;
                    if (scxOpenMarketJobId > 0)
                    {
                        var scxOpenMarketJobDayparts = new scx_generation_open_market_job_dayparts
                        {
                            standard_daypart_id = job.InventoryScxOpenMarketsDownloadRequest.StandardDaypartId,
                            scx_generation_open_market_job_id = scxOpenMarketJobId
                        };
                        context.scx_generation_open_market_job_dayparts.Add(scxOpenMarketJobDayparts);
                        var scxOpenMarketJobMarkets = new scx_generation_open_market_job_markets
                        {
                            market_code = (short)job.InventoryScxOpenMarketsDownloadRequest.MarketCode,
                            scx_generation_open_market_job_id = scxOpenMarketJobId
                        };
                        context.scx_generation_open_market_job_markets.Add(scxOpenMarketJobMarkets);
                        foreach (var affiliate in job.InventoryScxOpenMarketsDownloadRequest.Affiliates)
                        {
                            var scxOpenMarketJobAffiliate = new scx_generation_open_market_job_affiliates
                            {
                                affiliate = affiliate,
                                scx_generation_open_market_job_id = scxOpenMarketJobId
                            };
                            context.scx_generation_open_market_job_affiliates.Add(scxOpenMarketJobAffiliate);
                        }

                        context.SaveChanges();
                    }
                    return scxOpenMarketJobId;
                });
        }

        public ScxOpenMarketsGenerationJob GetOpenMarketsJobById(int jobId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var jobs = context.scx_generation_open_market_jobs
                    .Include(s=>s.scx_generation_open_market_job_dayparts)
                    .Include(s=>s.scx_generation_open_market_job_markets)
                    .Include(s=>s.scx_generation_open_market_job_affiliates)
                        .Where(j => j.id == jobId).Single(@"Unable to find job with id {jobId}");
                    
                    return _MapForOpenMarket(jobs);
                });
        }
        private ScxOpenMarketsGenerationJob _MapForOpenMarket(scx_generation_open_market_jobs scxOpenMarketsJob)
        {
            return new ScxOpenMarketsGenerationJob
            {
                Id = scxOpenMarketsJob.id,
                InventoryScxOpenMarketsDownloadRequest = new InventoryScxOpenMarketsDownloadRequest
                {
                    StartDate = scxOpenMarketsJob.start_date,
                    EndDate = scxOpenMarketsJob.end_date,
                    GenreType = (OpenMarketInventoryExportGenreTypeEnum)scxOpenMarketsJob.export_genre_type_id,
                    StandardDaypartId = scxOpenMarketsJob.scx_generation_open_market_job_dayparts.Select(x=>x.standard_daypart_id).FirstOrDefault(),
                    MarketCode = scxOpenMarketsJob.scx_generation_open_market_job_markets.Select(x=>x.market_code).FirstOrDefault(),
                    Affiliates = scxOpenMarketsJob.scx_generation_open_market_job_affiliates.Select(x=>x.affiliate).ToList()
                },
                Status = (BackgroundJobProcessingStatus)scxOpenMarketsJob.status,
                QueuedAt = scxOpenMarketsJob.queued_at,
                CompletedAt = scxOpenMarketsJob.completed_at,
                RequestedBy = scxOpenMarketsJob.requested_by
            };
        }
        public void UpdateOpenMarketJob(ScxOpenMarketsGenerationJob job)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var fileJob = context.scx_generation_open_market_jobs.Find(job.Id);
                    fileJob.status = (int)job.Status;
                    fileJob.completed_at = job.CompletedAt;

                    context.SaveChanges();
                });
        }
        public void SaveScxOpenMarketJobFiles(List<OpenMarketInventoryScxFile> files, ScxOpenMarketsGenerationJob job)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var file in files)
                    {
                        context.scx_generation_open_market_job_files.Add(new scx_generation_open_market_job_files
                        {
                            scx_generation_open_market_job_id = job.Id,
                            file_name = file.FileName,
                            standard_daypart_id = file.DaypartCodeId,
                            start_date = file.StartDate,
                            end_date = file.EndDate,
                            shared_folder_files_id = file.SharedFolderFileId,
                            market_code = (short)job.InventoryScxOpenMarketsDownloadRequest.MarketCode,
                            export_genre_type_id = (int)job.InventoryScxOpenMarketsDownloadRequest.GenreType,
                            affiliate = job.InventoryScxOpenMarketsDownloadRequest.Affiliates.FirstOrDefault()
                        });
                    }

                    context.SaveChanges();
                });
        }
       
    }
}