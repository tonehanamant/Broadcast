using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsBySourceProcessor : InventoryProgramsProcessingEngineBase
    {
        protected readonly IInventoryProgramsBySourceJobsRepository _InventoryProgramsBySourceJobsRepository;
        protected readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private List<int> _MediaWeekIds = null;

        public InventoryProgramsBySourceProcessor(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(broadcastDataRepositoryFactory,
                genreCache,
                featureToggleHelper,
                configurationSettingsHelper)
        {
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _InventoryProgramsBySourceJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsBySourceJobsRepository>();
        }

        protected override IInventoryProgramsJobsRepository _GetJobsRepository()
        {
            return _InventoryProgramsBySourceJobsRepository;
        }

        protected override InventoryProgramsProcessingJobDiagnostics _GetNewDiagnostics()
        {
            return new InventoryProgramsProcessingJobBySourceDiagnostics(OnDiagnosticMessageUpdate);
        }

        protected override InventorySource _GetInventorySource(int jobId)
        {
            var sourceId = _InventoryProgramsBySourceJobsRepository.GetJob(jobId).InventorySourceId;
            var inventorySource = _InventoryRepository.GetInventorySource(sourceId);
            return inventorySource;
        }

        protected List<int> _GetMediaWeekIds(DateTime startDate, DateTime endDate, InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            if (_MediaWeekIds == null)
            {
                var mediaWeekIds = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(startDate, endDate).Select(w => w.Id).ToList();
                ((InventoryProgramsProcessingJobBySourceDiagnostics)processDiagnostics).RecordMediaWeekIds(mediaWeekIds);
                _MediaWeekIds = mediaWeekIds;
            }

            return _MediaWeekIds;
        }

        protected override List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            ((InventoryProgramsProcessingJobBySourceDiagnostics)processDiagnostics).RecordRequestParameters(job.InventorySourceId, job.StartDate, job.EndDate);

            var mediaWeekIds = _GetMediaWeekIds(job.StartDate, job.EndDate, processDiagnostics);

            var manifests = _InventoryRepository.GetInventoryBySourceForProgramsProcessing(job.InventorySourceId, mediaWeekIds);
            return manifests;
        }

        protected override void SetPrimaryProgramFromProgramMappings(List<StationInventoryManifest> manifests, Action<string> logger)
        {
            // not needed for now
        }
    }
}