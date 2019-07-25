using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.ApplicationServices
{
    public class InventoryFileRatingsProcessingJob
    {
        public int? id;
        public int InventoryFileId;
        public BackgroundJobProcessingStatus Status;
        public DateTime QueuedAt;
        public DateTime? CompletedAt;
    }

    public interface IInventoryRatingsProcessingService : IApplicationService
    {
        void QueueInventoryFileRatingsJob(int inventoryFileId);
        List<InventoryFileRatingsProcessingJob> GetQueuedJobs(int limit);
        InventoryFileRatingsProcessingJob GetJobByFileId(int fileId);

        /// <summary>
        /// Process the specified job returning a list of Summary data required for the inventory aggregation process
        /// </summary>
        /// <param name="jobId">Job id to process</param>
        /// <returns>Summary data required for the inventory aggregation process</returns>
        InventoryAggregationDto ProcessInventoryRatingsJob(int jobId);
        
        void ResetJobStatusToQueued(int jobId);
    }

    public class InventoryRatingsProcessingService : IInventoryRatingsProcessingService
    {
        private readonly IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;
        private readonly IProprietaryRepository _ProprietaryRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IImpressionsService _ImpressionsService;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public InventoryRatingsProcessingService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsService impressionsService,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            INsiPostingBookService nsiPostingBookService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _InventoryFileRatingsJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            _ProprietaryRepository = broadcastDataRepositoryFactory
                .GetDataRepository<IProprietaryRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ImpressionsService = impressionsService;
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _NsiPostingBookService = nsiPostingBookService;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public List<InventoryFileRatingsProcessingJob> GetQueuedJobs(int limit)
        {
            return _InventoryFileRatingsJobsRepository.GetJobsBatch(limit);
        }

        public InventoryFileRatingsProcessingJob GetJobByFileId(int fileId)
        {
            return _InventoryFileRatingsJobsRepository.GetJobByFileId(fileId);
        }

        public void QueueInventoryFileRatingsJob(int inventoryFileId)
        {
            var job = new InventoryFileRatingsProcessingJob
            {
                InventoryFileId = inventoryFileId,
                Status = BackgroundJobProcessingStatus.Queued,
                QueuedAt = DateTime.Now
            };

            _InventoryFileRatingsJobsRepository.AddJob(job);
        }

        /// <summary>
        /// Process the specified job returning a list of Summary data required for the inventory aggregation process
        /// </summary>
        /// <param name="jobId">Job id to process</param>
        /// <returns>Summary data required for the inventory aggregation process</returns>
        public InventoryAggregationDto ProcessInventoryRatingsJob(int jobId)
        {
            const ProposalPlaybackType defaultOpenMarketPlaybackType = ProposalPlaybackType.LivePlus3;

            var job = _InventoryFileRatingsJobsRepository.GetJobById(jobId);

            if(job == null)
            {
                throw new ApplicationException($"Job with id {jobId} was not found");
            }

            if(job.Status != BackgroundJobProcessingStatus.Queued)
            {
                throw new ApplicationException($"Job with id {jobId} already has status {job.Status}");
            }

            try
            {
                job.Status = BackgroundJobProcessingStatus.Processing;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                
                var inventoryFile = _InventoryFileRepository.GetInventoryFileById(job.InventoryFileId);
                var inventorySource = inventoryFile.InventorySource;
                
                if (inventorySource.InventoryType == InventorySourceTypeEnum.Barter)
                {
                    var header =  _ProprietaryRepository.GetInventoryFileHeader(job.InventoryFileId);
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);

                    // process impressions/cost
                    _ImpressionsService.AddProjectedImpressionsToManifests(manifests, header.PlaybackType, header.ShareBookId.Value, header.HutBookId);
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests);

                    // process components impressions
                    _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, header.PlaybackType, header.ShareBookId.Value, header.HutBookId);

                    // update manifest rates and audiences
                    _InventoryRepository.UpdateInventoryManifests(manifests);

                    job.Status = BackgroundJobProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (inventorySource.InventoryType == InventorySourceTypeEnum.ProprietaryOAndO)
                {
                    var header = _ProprietaryRepository.GetInventoryFileHeader(job.InventoryFileId);
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);

                    //process cost
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests, useProvidedImpressions: true);

                    //process components impressions
                    _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, header.PlaybackType, header.ShareBookId.Value, header.HutBookId);

                    //update manifest rates
                    _InventoryRepository.UpdateInventoryManifests(manifests);

                    job.Status = BackgroundJobProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (inventorySource.InventoryType == InventorySourceTypeEnum.Diginet)
                {
                    // nothing to process so just set Succeeded status. Diginet template already have spot cost calculated
                    job.Status = BackgroundJobProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (inventorySource.InventoryType == InventorySourceTypeEnum.Syndication)
                {
                    // nothing to process so just set succeeded status. Syndication template already have spot cost calculated
                    job.Status = BackgroundJobProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (inventorySource.InventoryType == InventorySourceTypeEnum.OpenMarket)
                {
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);

                    // filter out manifests without weeks
                    manifests = manifests.Where(x => x.ManifestWeeks.Any()).ToList();
                    
                    var allMediaMonthIds = manifests.SelectMany(x => x.ManifestWeeks).Select(x => x.MediaWeek.MediaMonthId).Distinct();
                    var allMediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(allMediaMonthIds);
                    var manifestsGroupedByQuarter = manifests
                        .Select(x => new
                        {
                            manifest = x,

                            // all weeks in a manifest belongs to a single quarter so that we can just take a first week and find its quarter
                            // see OpenMarketFileImporter._PopulateProgramsFromAvailLineWithPeriods for details
                            quarter = allMediaMonths.Single(m => m.Id == x.ManifestWeeks.First().MediaWeek.MediaMonthId).QuarterAndYearText
                        })
                        .GroupBy(x => x.quarter);

                    foreach (var manifestsGroup in manifestsGroupedByQuarter)
                    {
                        var quarterManifests = manifestsGroup.Select(x => x.manifest).ToList();

                        // we can take any week of any manifest since all they belong to the same quarter
                        var postingBookDate = quarterManifests.First().ManifestWeeks.First().StartDate;
                        var postingBook = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(postingBookDate);

                        _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(quarterManifests, defaultOpenMarketPlaybackType, postingBook);                       
                    }

                    // save all changes to DB
                    _InventoryRepository.UpdateInventoryManifests(manifests);

                    job.Status = BackgroundJobProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else
                {
                    // Failed for unsupported types
                    job.Status = BackgroundJobProcessingStatus.Failed;
                }
                _InventoryFileRatingsJobsRepository.UpdateJob(job);

                return new InventoryAggregationDto
                {
                    InventorySourceId = inventoryFile.InventorySource.Id,
                    InventorySourceType = inventoryFile.InventorySource.InventoryType
                };
            }
            catch
            {
                job.Status = BackgroundJobProcessingStatus.Failed;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                throw;
            }
        }

        public void ResetJobStatusToQueued(int jobId)
        {
            var job = _InventoryFileRatingsJobsRepository.GetJobById(jobId);
            job.Status = BackgroundJobProcessingStatus.Queued;
            job.CompletedAt = null;
            _InventoryFileRatingsJobsRepository.UpdateJob(job);
        }
    }
}
