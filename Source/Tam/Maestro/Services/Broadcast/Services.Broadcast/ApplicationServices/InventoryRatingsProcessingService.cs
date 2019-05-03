using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public enum InventoryFileRatingsProcessingStatus
    {
        Queued = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4
    }

    public class InventoryFileRatingsProcessingJob
    {
        public int? id;
        public int InventoryFileId;
        public InventoryFileRatingsProcessingStatus Status;
        public DateTime QueuedAt;
        public DateTime? CompletedAt;
    }
    public interface IInventoryRatingsProcessingService : IApplicationService
    {
        void QueueInventoryFileRatingsJob(int inventoryFileId);
        List<InventoryFileRatingsProcessingJob> GetQueuedJobs(int limit);
        InventoryFileRatingsProcessingJob GetJobByFileId(int fileId);
        void ProcessInventoryRatingsJob(int jobId);
    }
    public class InventoryRatingsProcessingService : IInventoryRatingsProcessingService
    {
        private readonly IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;
        private readonly IProprietaryRepository _ProprietaryRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IImpressionsService _ImpressionsService;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;

        public InventoryRatingsProcessingService
            (IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsService impressionsService,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine)
        {
            _InventoryFileRatingsJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            _ProprietaryRepository = broadcastDataRepositoryFactory
                .GetDataRepository<IProprietaryRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ImpressionsService = impressionsService;
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
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
                Status = InventoryFileRatingsProcessingStatus.Queued,
                QueuedAt = DateTime.Now
            };

            _InventoryFileRatingsJobsRepository.AddJob(job);
        }

        public void ProcessInventoryRatingsJob(int jobId)
        {
            var job = _InventoryFileRatingsJobsRepository.GetJobById(jobId);
            if(job == null)
            {
                throw new ApplicationException($"Job with id {jobId} was not found");
            }
            if(job.Status != InventoryFileRatingsProcessingStatus.Queued)
            {
                throw new ApplicationException($"Job with id {jobId} already has status {job.Status}");
            }

            try
            {
                job.Status = InventoryFileRatingsProcessingStatus.Processing;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);

                //This just processed proprietary files right now. Needs to be consolidated to 
                //var sw = Stopwatch.StartNew();
                var proprietaryFile = _ProprietaryRepository.GetInventoryFileWithHeaderById(job.InventoryFileId);
                //sw.Stop();
                //Debug.WriteLine($"GetInventoryFileWithHeaderById: {sw.ElapsedMilliseconds}");

                if (proprietaryFile.InventorySource.InventoryType == Entities.Enums.InventorySourceTypeEnum.Barter)
                {
                    //load manifests for file
                    //sw = Stopwatch.StartNew();
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);
                    //sw.Stop();
                    //Debug.WriteLine($"_InventoryRepository.GetStationInventoryManifestsByFileId: {sw.ElapsedMilliseconds}");

                    //process impressions/cost
                    //sw = Stopwatch.StartNew();
                    _ImpressionsService.AddProjectedImpressionsToManifests(manifests, proprietaryFile.Header.PlaybackType, proprietaryFile.Header.ShareBookId.Value, proprietaryFile.Header.HutBookId);
                    //sw.Stop();
                    //Debug.WriteLine($"GetProjectedStationImpressions: {sw.ElapsedMilliseconds}");

                    //sw = Stopwatch.StartNew();
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests);
                    //sw.Stop();
                    //Debug.WriteLine($"CalculateSpotCost: {sw.ElapsedMilliseconds}");
                    
                    //process components impressions
                    //sw = Stopwatch.StartNew();
                    _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, proprietaryFile.Header.PlaybackType, proprietaryFile.Header.ShareBookId.Value, proprietaryFile.Header.HutBookId);
                    //sw.Stop();
                    //Debug.WriteLine($"GetProjectedStationImpressionsForComponents: {sw.ElapsedMilliseconds}");
                    
                    //update manifest rates and audiences
                    //sw = Stopwatch.StartNew();
                    _InventoryRepository.UpdateInventoryManifests(manifests);
                    //sw.Stop();
                    //Debug.WriteLine($"UpdateInventoryRatesForManifests: {sw.ElapsedMilliseconds}");

                    job.Status = InventoryFileRatingsProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (proprietaryFile.InventorySource.InventoryType == Entities.Enums.InventorySourceTypeEnum.ProprietaryOAndO)
                {                    
                    //Debug.WriteLine("OandO Source");
                    //process cost
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests, useProvidedImpressions: true);

                    //process components impressions
                    //sw = Stopwatch.StartNew();
                    _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, proprietaryFile.Header.PlaybackType, proprietaryFile.Header.ShareBookId.Value, proprietaryFile.Header.HutBookId);
                    //sw.Stop();
                    //Debug.WriteLine($"GetProjectedStationImpressionsForComponents: {sw.ElapsedMilliseconds}");

                    //update manifest rates
                    _InventoryRepository.UpdateInventoryManifests(manifests);

                    job.Status = InventoryFileRatingsProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (proprietaryFile.InventorySource.InventoryType == Entities.Enums.InventorySourceTypeEnum.Diginet)
                {
                    // nothing to process so just set Succeeded status. Diginet template already have spot cost calculated
                    job.Status = InventoryFileRatingsProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (proprietaryFile.InventorySource.InventoryType == Entities.Enums.InventorySourceTypeEnum.Syndication)
                {
                    // nothing to process so just set succeeded status. Syndication template already have spot cost calculated
                    job.Status = InventoryFileRatingsProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else
                {
                    //Debug.WriteLine("Unknown source");
                    // Failed for unsupported types
                    job.Status = InventoryFileRatingsProcessingStatus.Failed;
                }
                
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
            }
            catch (Exception e)
            {
                //Debug.WriteLine(e);
                job.Status = InventoryFileRatingsProcessingStatus.Failed;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                throw;
            }
        }

    }
}
