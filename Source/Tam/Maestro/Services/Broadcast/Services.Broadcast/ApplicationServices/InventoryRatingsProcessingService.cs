using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using Services.Broadcast.Entities;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryRatingsProcessingService : IApplicationService
    {
        void QueueInventoryFileRatingsJob(int inventoryFileId);
        List<InventoryFileRatingsProcessingJob> GetQueuedJobs(int limit);
        InventoryFileRatingsProcessingJob GetJobByFileId(int fileId);

        /// <summary>
        /// Process the specified job returning a list of Summary data required for the inventory aggregation process
        /// </summary>
        /// <param name="jobId">Job id to process</param>
        /// <param name="ignoreStatus"></param>
        /// <returns>
        /// Inventory source id required for the inventory aggregation process
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Job with id {jobId} was not found
        /// or
        /// Job with id {jobId} already has status {job.Status}
        /// </exception>
        int ProcessInventoryRatingsJob(int jobId, bool ignoreStatus);

        [Queue("inventoryrating")]
        int ProcessInventoryRatingsJob(int jobId);

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
        private readonly IBackgroundJobClient _BackgroundJobClient;

        public InventoryRatingsProcessingService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsService impressionsService,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            INsiPostingBookService nsiPostingBookService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBackgroundJobClient backgroundJobClient)
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
            _BackgroundJobClient = backgroundJobClient;
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
                QueuedAt = DateTime.Now,
                Notes = new List<InventoryFileRatingsProcessingJob.Note>
                {
                    new InventoryFileRatingsProcessingJob.Note
                    {
                        Text = $"Automatic ratings processing is turned {(TemporalApplicationSettings.ProcessRatingsAutomatically ? "on" : "off")}",
                        CreatedAt = DateTime.Now
                    }
                }
            };

            var jobId = _InventoryFileRatingsJobsRepository.AddJob(job);

            if (TemporalApplicationSettings.ProcessRatingsAutomatically)
            {
                _BackgroundJobClient.Enqueue<IInventoryRatingsProcessingService>(x =>
                    x.ProcessInventoryRatingsJob(jobId));
            }
        }

        /// <inheritdoc/>
        public int ProcessInventoryRatingsJob(int jobId)
        {
            return ProcessInventoryRatingsJob(jobId, false);
        }
        /// <inheritdoc/>        
        public int ProcessInventoryRatingsJob(int jobId, bool ignoreStatus=false)
        {
            const ProposalPlaybackType defaultOpenMarketPlaybackType = ProposalPlaybackType.LivePlus3;

            var job = _InventoryFileRatingsJobsRepository.GetJobById(jobId);

            if (job == null)
            {
                throw new ApplicationException($"Job with id {jobId} was not found");
            }

            if (!ignoreStatus && job.Status != BackgroundJobProcessingStatus.Queued)
            {
                var message = $"Job with id {jobId} already has status {job.Status}";
                _AddJobNote(jobId, message);
                throw new ApplicationException(message);
            }

            try
            {
                job.Status = BackgroundJobProcessingStatus.Processing;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                _AddJobNote(jobId, $"Started processing. Machine info: {_GetMachineInfo()}");

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
                    //var sw = Stopwatch.StartNew();
                    foreach (var manifestsGroup in manifestsGroupedByQuarter)
                    {
                        var quarterManifests = manifestsGroup.Select(x => x.manifest).ToList();

                        // we can take any week of any manifest since all they belong to the same quarter
                        var postingBookDate = quarterManifests.First().ManifestWeeks.First().StartDate;
                        var postingBook = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(postingBookDate);

                        _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(quarterManifests, defaultOpenMarketPlaybackType, postingBook);                       
                    }
                    //sw.Stop();
                    //Debug.WriteLine($"=====> Completed impression calculation in {sw.Elapsed}ms");

                    // save all changes to DB
                    //sw.Restart();
                    _InventoryRepository.UpdateInventoryManifests(manifests);
                    //sw.Stop();
                    //Debug.WriteLine($"=====> Done saving manifest audiences in {sw.Elapsed}ms");

                    job.Status = BackgroundJobProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else
                {
                    // Failed for unsupported types
                    job.Status = BackgroundJobProcessingStatus.Failed;
                    job.CompletedAt = DateTime.Now;
                    _AddJobNote(jobId, $"Cannot process unsupported type: {inventorySource.InventoryType}");
                }
                _InventoryFileRatingsJobsRepository.UpdateJob(job);

                _BackgroundJobClient.Enqueue<IInventorySummaryService>(
                    x => x.AggregateInventorySummaryData(new List<int> { inventorySource.Id }));

                return inventoryFile.InventorySource.Id;
            }
            catch(Exception ex)
            {
                job.Status = BackgroundJobProcessingStatus.Failed;
                job.CompletedAt = DateTime.Now;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                _AddJobNote(jobId, ex.ToString());
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

        private void _AddJobNote(int jobId, string text)
        {
            _InventoryFileRatingsJobsRepository.AddJobNote(jobId, new InventoryFileRatingsProcessingJob.Note
            {
                Text = text,
                CreatedAt = DateTime.Now
            });
        }

        private static string _GetMachineInfo()
        {
            return $"Name: {Environment.MachineName}, local IPAddress: {GetLocalIPAddress()}, process id: {Process.GetCurrentProcess().Id}";
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "No network adapters with an IPv4 address in the system!";
        }
    }
}
