using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Newtonsoft.Json;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryRatingsProcessingService : IApplicationService
    {
        void QueueInventoryFileRatingsJob(int inventoryFileId);
        List<InventoryFileRatingsProcessingJob> GetQueuedJobs(int limit);

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
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        int ProcessInventoryRatingsJob(int jobId);

        void ResetJobStatusToQueued(int jobId);
        
        void RequeueInventoryFileRatingsJob(int jobId);
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

        public void RequeueInventoryFileRatingsJob(int jobId)
        {
            const bool ignoreStatus = true;
            _BackgroundJobClient.Enqueue<IInventoryRatingsProcessingService>(x =>
                x.ProcessInventoryRatingsJob(jobId, ignoreStatus));
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

        private void _RecordManifestStats(InventoryFileRatingsJobDiagnostics processDiagnostics, List<StationInventoryManifest> manifests)
        {
            processDiagnostics.ManifestCount = manifests.Count;
            processDiagnostics.StationCount = manifests.Select(m => m.Station.Id).Distinct().Count();
            var manifestCheck = manifests.First();
            processDiagnostics.WeekCount = manifestCheck.ManifestWeeks.Count;
            processDiagnostics.DaypartCount = manifestCheck.ManifestDayparts.Count();
            processDiagnostics.AudienceCount = manifestCheck.ManifestAudiences.Count();
        }

        private void SaveManifestsInChunks(InventoryFileRatingsJobDiagnostics processDiagnostics, int fileId, List<StationInventoryManifest> manifests, int chunkSize)
        {
            _InventoryRepository.DeleteInventoryManifestAudiencesForFile(fileId);

            processDiagnostics.IsChunking = chunkSize > 1;
            processDiagnostics.SaveChunkSize = chunkSize;
            var saveChunks = manifests.GetChunks(chunkSize);
            processDiagnostics.SaveChunkCount = saveChunks.Count();
            foreach (var chunk in saveChunks)
            {
                _InventoryRepository.UpdateInventoryManifests(chunk);
            }
        }

        /// <inheritdoc/>        
        public int ProcessInventoryRatingsJob(int jobId, bool ignoreStatus=false)
        {
            const int manifestSaveChunkSize = 200;
            const ProposalPlaybackType defaultOpenMarketPlaybackType = ProposalPlaybackType.LivePlus3;

            var job = _InventoryFileRatingsJobsRepository.GetJobById(jobId);

            if (job == null)
            {
                throw new ApplicationException($"Job with id {jobId} was not found");
            }

            var processDiagnostics = new InventoryFileRatingsJobDiagnostics {JobId = jobId};

            try
            {
                var inventoryFile = _InventoryFileRepository.GetInventoryFileById(job.InventoryFileId);
                if (!ignoreStatus && job.Status != BackgroundJobProcessingStatus.Queued)
                {
                    var message = $"Job with id {jobId} already has status {job.Status}.";
                    _AddJobNote(jobId, message);
                    return inventoryFile.InventorySource.Id;
                }

                job.Status = BackgroundJobProcessingStatus.Processing;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                _AddJobNote(jobId, $"Started processing. Machine info: {_GetMachineInfo()}");

                var inventorySource = inventoryFile.InventorySource;
                processDiagnostics.RecordJobStart(job.Id, inventoryFile.Id, inventorySource, DateTime.Now);

                if (inventorySource.InventoryType == InventorySourceTypeEnum.Barter)
                {
                    processDiagnostics.RecordGatherInventoryStart();
                    var header = _ProprietaryRepository.GetInventoryFileHeader(job.InventoryFileId);
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);
                    _RecordManifestStats(processDiagnostics, manifests);

                    processDiagnostics.RecordGatherInventoryStop();

                    processDiagnostics.RecordProcessImpressionsTotalStart();

                    // process impressions/cost
                    _ImpressionsService.AddProjectedImpressionsToManifests(manifests, header.PlaybackType,
                        header.ShareBookId.Value, header.HutBookId);
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests);

                    // process components impressions
                    _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, header.PlaybackType,
                        header.ShareBookId.Value, header.HutBookId);

                    processDiagnostics.RecordProcessImpressionsTotalStop();

                    // update manifest rates and audiences
                    processDiagnostics.RecordSaveManifestsStart();
                    SaveManifestsInChunks(processDiagnostics, job.InventoryFileId, manifests, manifestSaveChunkSize);
                    processDiagnostics.RecordSaveManifestsStop();

                    job.Status = BackgroundJobProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (inventorySource.InventoryType == InventorySourceTypeEnum.ProprietaryOAndO)
                {
                    processDiagnostics.RecordGatherInventoryStart();
                    var header = _ProprietaryRepository.GetInventoryFileHeader(job.InventoryFileId);
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);

                    _RecordManifestStats(processDiagnostics, manifests);
                    processDiagnostics.RecordGatherInventoryStop();

                    processDiagnostics.RecordProcessImpressionsTotalStart();
                    //process cost
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests, useProvidedImpressions: true);

                    //process components impressions
                    _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, header.PlaybackType,
                        header.ShareBookId.Value, header.HutBookId);

                    processDiagnostics.RecordProcessImpressionsTotalStop();

                    //update manifest rates
                    processDiagnostics.RecordSaveManifestsStart();
                    SaveManifestsInChunks(processDiagnostics, job.InventoryFileId, manifests, manifestSaveChunkSize);
                    processDiagnostics.RecordSaveManifestsStop();

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
                    processDiagnostics.RecordGatherInventoryStart();
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);
                    // filter out manifests without weeks
                    manifests = manifests.Where(x => x.ManifestWeeks.Any()).ToList();

                    _RecordManifestStats(processDiagnostics, manifests);
                    processDiagnostics.RecordGatherInventoryStop();

                    processDiagnostics.RecordOrganizeInventoryStart();
                    var allMediaMonthIds = manifests.SelectMany(x => x.ManifestWeeks)
                        .Select(x => x.MediaWeek.MediaMonthId).Distinct();
                    var allMediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(allMediaMonthIds);
                    var manifestsGroupedByQuarter = manifests
                        .Select(x => new
                        {
                            manifest = x,

                            // all weeks in a manifest belongs to a single quarter so that we can just take a first week and find its quarter
                            // see OpenMarketFileImporter._PopulateProgramsFromAvailLineWithPeriods for details
                            quarter = allMediaMonths.Single(m => m.Id == x.ManifestWeeks.First().MediaWeek.MediaMonthId)
                                .QuarterAndYearText
                        })
                        .GroupBy(x => x.quarter).ToList();

                    processDiagnostics.QuartersCovered = manifestsGroupedByQuarter.Select(s => s.Key).ToList();
                    processDiagnostics.RecordOrganizeInventoryStop();

                    processDiagnostics.RecordProcessImpressionsTotalStart();

                    foreach (var manifestsGroup in manifestsGroupedByQuarter)
                    {
                        var quarterManifests = manifestsGroup.Select(x => x.manifest).ToList();

                        processDiagnostics.RecordDeterminePostingBookStart();
                        // we can take any week of any manifest since all they belong to the same quarter
                        var postingBookDate = quarterManifests.First().ManifestWeeks.First().StartDate;
                        var postingBookId =
                            _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(postingBookDate);
                        processDiagnostics.RecordDeterminePostingBookStop(manifestsGroup.Key, postingBookId);

                        processDiagnostics.RecordAddImpressionsStart();
                        _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(quarterManifests, defaultOpenMarketPlaybackType, postingBookId);
                        processDiagnostics.RecordAddImpressionsStop();
                    }
                    processDiagnostics.RecordProcessImpressionsTotalStop();

                    processDiagnostics.RecordSaveManifestsStart();
                    SaveManifestsInChunks(processDiagnostics, job.InventoryFileId, manifests, manifestSaveChunkSize);
                    processDiagnostics.RecordSaveManifestsStop();

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

                processDiagnostics.RecordJobCompleted(DateTime.Now);

                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                _AddJobNote(jobId, JsonConvert.SerializeObject(processDiagnostics));

                if (_ShouldTriggerInventorySourceAggregation())
                {
                    _BackgroundJobClient.Enqueue<IInventorySummaryService>(x => x.AggregateInventorySummaryData(new List<int> { inventorySource.Id }, inventoryFile.EffectiveDate, inventoryFile.EndDate));
                }

                return inventoryFile.InventorySource.Id;
            }
            catch (Exception ex)
            {
                var completedAt = DateTime.Now;
                job.Status = BackgroundJobProcessingStatus.Failed;
                job.CompletedAt = completedAt;
                processDiagnostics.RecordJobCompleted(completedAt);

                _AddJobNote(jobId, JsonConvert.SerializeObject(processDiagnostics));

                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                _AddJobNote(jobId, ex.ToString());
                throw;
            }
        }

        private bool _ShouldTriggerInventorySourceAggregation()
        {
            const string configKeyTriggerInventorySourceAggregation = "TriggerInventorySourceAggregation";
            if (ConfigurationManager.AppSettings.AllKeys.Contains(configKeyTriggerInventorySourceAggregation))
            {
                return bool.TryParse(ConfigurationManager.AppSettings[configKeyTriggerInventorySourceAggregation], out var flag) ? flag : true;
            }
            return true;
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
