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
        void ProcessInventoryRatingsJob(int jobId);
    }
    public class InventoryRatingsProcessingService : IInventoryRatingsProcessingService
    {
        private readonly IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;
        private readonly IBarterRepository _BarterRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IImpressionsService _ImpressionsService;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        public InventoryRatingsProcessingService
            (IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsService impressionsService,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine)
        {
            _InventoryFileRatingsJobsRepository = broadcastDataRepositoryFactory
                .GetDataRepository<IInventoryFileRatingsJobsRepository>();
            _BarterRepository = broadcastDataRepositoryFactory
                .GetDataRepository<IBarterRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory
                .GetDataRepository<IInventoryRepository>();
            _ImpressionsService = impressionsService;
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
        }

        public List<InventoryFileRatingsProcessingJob> GetQueuedJobs(int limit)
        {
            return _InventoryFileRatingsJobsRepository.GetJobsBatch(limit);
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

                //This just processed barter files right now. Needs to be consolidated to 
                //var sw = Stopwatch.StartNew();
                var barterFile = _BarterRepository.GetInventoryFileWithHeaderById(job.InventoryFileId);
                //sw.Stop();
                //Debug.WriteLine($"GetInventoryFileWithHeaderById: {sw.ElapsedMilliseconds}");

                if (barterFile.InventorySource.InventoryType == Entities.Enums.InventorySourceTypeEnum.Barter)
                {
                    //Debug.WriteLine("Barter Source");
                    //sw = Stopwatch.StartNew();
                    barterFile.InventoryGroups = _InventoryRepository.GetStationInventoryGroupsByFileId(job.InventoryFileId);
                    //sw.Stop();
                    //Debug.WriteLine($"GetStationInventoryGroupsByFileId: {sw.ElapsedMilliseconds}");

                    var manifests = barterFile.InventoryGroups.SelectMany(x => x.Manifests).ToList();

                    //process impressions/cost
                    //sw = Stopwatch.StartNew();
                    _ImpressionsService.GetProjectedStationImpressions(manifests, barterFile.Header.PlaybackType, barterFile.Header.ShareBookId.Value, barterFile.Header.HutBookId);
                    //sw.Stop();
                    //Debug.WriteLine($"GetProjectedStationImpressions: {sw.ElapsedMilliseconds}");

                    //sw = Stopwatch.StartNew();
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests);
                    //sw.Stop();
                    //Debug.WriteLine($"CalculateSpotCost: {sw.ElapsedMilliseconds}");

                    //update manifest rates
                    //sw = Stopwatch.StartNew();
                    _InventoryRepository.UpdateInventoryRatesForManifests(manifests);
                    //sw.Stop();
                    //Debug.WriteLine($"UpdateInventoryRatesForManifests: {sw.ElapsedMilliseconds}");

                    job.Status = InventoryFileRatingsProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (barterFile.InventorySource.InventoryType == Entities.Enums.InventorySourceTypeEnum.ProprietaryOAndO)
                {
                    //Debug.WriteLine("OandO Source");
                    //process cost
                    var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(job.InventoryFileId);
                    _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests, useProvidedImpressions: true);

                    //update manifest rates
                    _InventoryRepository.UpdateInventoryRatesForManifests(manifests);

                    job.Status = InventoryFileRatingsProcessingStatus.Succeeded;
                    job.CompletedAt = DateTime.Now;
                }
                else if (barterFile.InventorySource.InventoryType == Entities.Enums.InventorySourceTypeEnum.Diginet)
                {
                    // nothing to process for now so just set Succeeded status
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
                System.Diagnostics.Debug.WriteLine(e);
                job.Status = InventoryFileRatingsProcessingStatus.Failed;
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
                throw;
            }
        }
    }
}
