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

        /// <summary>
        /// Gets the Open market history details
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns>history details</returns>
        List<ScxOpenMarketFileGenerationDetailDto> GetOpenMarketScxFileGenerationDetails(int sourceId);
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
                        _AddOpenMarketJobMarkets(job,context,scxOpenMarketJobId);
                        _AddOpenMarketJobDayparts(job, context, scxOpenMarketJobId);
                        _AddOpenMarketJobAffiliate(job, context, scxOpenMarketJobId);
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
                    .Include(s => s.scx_generation_open_market_job_dayparts)
                    .Include(s => s.scx_generation_open_market_job_markets)
                    .Include(s => s.scx_generation_open_market_job_affiliates)
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
                    DaypartIds = scxOpenMarketsJob.scx_generation_open_market_job_dayparts.Select(x => x.standard_daypart_id).ToList(),
                    MarketRanks = string.Join(",",scxOpenMarketsJob.scx_generation_open_market_job_markets.Select(x => x.rank).ToList()),
                    Affiliates = scxOpenMarketsJob.scx_generation_open_market_job_affiliates.Select(x => x.affiliate).ToList()
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
                            standard_daypart_id = string.Join(",",file.DaypartIds.ToList()),
                            start_date = job.InventoryScxOpenMarketsDownloadRequest.StartDate,
                            end_date = job.InventoryScxOpenMarketsDownloadRequest.EndDate,
                            shared_folder_files_id = file.SharedFolderFileId,
                            rank = job.InventoryScxOpenMarketsDownloadRequest.MarketRanks,
                            export_genre_type_id = (int)job.InventoryScxOpenMarketsDownloadRequest.GenreType,
                            affiliate = string.Join(",", job.InventoryScxOpenMarketsDownloadRequest.Affiliates.ToList())
                        });
                    }

                    context.SaveChanges();
                });
        }


        public List<ScxOpenMarketFileGenerationDetailDto> GetOpenMarketScxFileGenerationDetails(int sourceId)
        {
            var result = _InReadUncommitedTransaction(context =>
            {
                var details = GetOpenMarketGenerationDetails(context, sourceId);
                return details;
            });
            return result;
        }

        private static List<ScxOpenMarketFileGenerationDetailDto> GetOpenMarketGenerationDetails(BroadcastContext context, int inventorySourceId)
        {
            var details = (from f in context.scx_generation_open_market_job_files
                           join j in context.scx_generation_open_market_jobs on f.scx_generation_open_market_job_id equals j.id into fs
                           from j in fs.DefaultIfEmpty()
                           join d in context.scx_generation_open_market_job_dayparts on f.scx_generation_open_market_job_id equals d.scx_generation_open_market_job_id into ds
                           from d in ds.DefaultIfEmpty()
                           join s in context.standard_dayparts on d.standard_daypart_id equals s.id into sd
                           from s in sd.DefaultIfEmpty()
                           join x in context.scx_generation_open_market_job_affiliates on f.scx_generation_open_market_job_id equals x.scx_generation_open_market_job_id into xy
                           from x in xy.DefaultIfEmpty()
                           where j.inventory_source_id.Equals(inventorySourceId)
                           select new ScxOpenMarketFileGenerationDetailDto
                           {
                               GenerationRequestDateTime = j.queued_at,
                               GenerationRequestedByUsername = j.requested_by,
                               FileId = f.id,
                               Filename = f.file_name,
                               Affilates = x.scx_generation_open_market_jobs.scx_generation_open_market_job_affiliates.Select(x => x.affiliate).ToList(),
                               DaypartCodes = f.scx_generation_open_market_jobs.scx_generation_open_market_job_dayparts.Select(x => x.standard_dayparts.code).ToList(),
                               StartDateTime = f.start_date,
                               EndDateTime = f.end_date,
                               ProcessingStatusId = j.status
                           })
                .ToList();
            return details;
        }
        private void _AddOpenMarketJobMarkets(ScxOpenMarketsGenerationJob job, BroadcastContext context, int scxOpenMarketJobId)
        {
            List<int> completeRange = new List<int>();
            List<int> subList = new List<int>();
            var rangeOfList = job.InventoryScxOpenMarketsDownloadRequest.MarketRanks.Split(';');
            foreach (var range in rangeOfList)
            {
                if (range.Length > 0)
                {
                    if (range.Contains('-'))
                    {
                        var newRange = range.Split('-');
                        if (newRange[0].Length > 0 && newRange[1].Length > 0)
                        {
                            subList = Enumerable.Range(Convert.ToInt32(newRange[0]), Convert.ToInt32(newRange[1])).ToList<int>();
                            completeRange.AddRange(subList);
                        }
                        else if (newRange[0].Length > 0 && newRange[1].Length == 0)
                        {
                            subList.Add(Convert.ToInt32(newRange[0]));
                            completeRange.AddRange(subList);
                        }
                        else if (newRange[0].Length == 0 && newRange[1].Length > 0)
                        {
                            subList.Add(Convert.ToInt32(newRange[1]));
                            completeRange.AddRange(subList);
                        }
                        else if (newRange[0].Length == 0 && newRange[1].Length == 0)
                        {
                            //do nothing
                        }
                        
                    }
                    else
                    {
                        completeRange.Add(Convert.ToInt32(range));
                    }
                }
            }
            foreach (var marketRank in completeRange)
            {
                var scxOpenMarketJobMarkets = new scx_generation_open_market_job_markets
                {
                    rank = marketRank,
                    scx_generation_open_market_job_id = scxOpenMarketJobId
                };
                context.scx_generation_open_market_job_markets.Add(scxOpenMarketJobMarkets);
            }
        }
        private void _AddOpenMarketJobDayparts(ScxOpenMarketsGenerationJob job, BroadcastContext context, int scxOpenMarketJobId)
        {
            job.InventoryScxOpenMarketsDownloadRequest.DaypartIds.ForEach(daypart =>
            {
                context.scx_generation_open_market_job_dayparts.Add(new scx_generation_open_market_job_dayparts
                {
                    standard_daypart_id = daypart,
                    scx_generation_open_market_job_id = scxOpenMarketJobId
                });
            });
        }
        private void _AddOpenMarketJobAffiliate(ScxOpenMarketsGenerationJob job, BroadcastContext context,int scxOpenMarketJobId)
        {
            job.InventoryScxOpenMarketsDownloadRequest.Affiliates.ForEach(affiliate =>
            {
                context.scx_generation_open_market_job_affiliates.Add(new scx_generation_open_market_job_affiliates
                {
                    affiliate = affiliate,
                    scx_generation_open_market_job_id = scxOpenMarketJobId
                });
            });
        }
    }
}