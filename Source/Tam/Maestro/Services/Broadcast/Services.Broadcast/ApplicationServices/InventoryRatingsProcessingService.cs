using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            job.Status = InventoryFileRatingsProcessingStatus.Processing;
            _InventoryFileRatingsJobsRepository.UpdateJob(job);
            try
            {

                //This just processed barter files right now. Needs to be consolidated to 
                var barterFile = _BarterRepository.GetBarterInventoryFileById(job.InventoryFileId);
                barterFile.InventoryGroups = _InventoryRepository.GetStationInventoryGroupsByFileId(job.InventoryFileId);
                var manifests = barterFile.InventoryGroups.SelectMany(x => x.Manifests).ToList();

                //process impressions/cost    
                _ImpressionsService.GetProjectedStationImpressions(manifests, barterFile.Header.PlaybackType, barterFile.Header.ShareBookId, barterFile.Header.HutBookId);
                _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests);

                //update manifest rates
                _InventoryRepository.UpdateInventoryRatesForManifests(manifests);

                job.Status = InventoryFileRatingsProcessingStatus.Succeeded;
            }
            catch (Exception e)
            {
                //need to log the error

                job.Status = InventoryFileRatingsProcessingStatus.Failed;
            }
            finally
            {
                _InventoryFileRatingsJobsRepository.UpdateJob(job);
            }
        }
    }
}
