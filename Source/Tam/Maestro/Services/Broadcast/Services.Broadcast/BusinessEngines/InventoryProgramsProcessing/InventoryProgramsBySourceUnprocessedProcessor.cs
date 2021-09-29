using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using System.Collections.Generic;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    /// <summary>
    /// Process Source Inventory that doesn't have programs for media weeks within the date range.
    /// The given date range is translated into media weeks.
    /// </summary>
    public class InventoryProgramsBySourceUnprocessedProcessor : InventoryProgramsBySourceProcessor
    {
        public InventoryProgramsBySourceUnprocessedProcessor(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(broadcastDataRepositoryFactory,
                mediaMonthAndWeekAggregateCache,
                genreCache,
                featureToggleHelper,
                configurationSettingsHelper)
        {
        }

        protected override List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            _InventoryProgramsBySourceJobsRepository.UpdateJobNotes(jobId, "Looking for inventory that doesn't have programs. ****");

            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            ((InventoryProgramsProcessingJobBySourceDiagnostics)processDiagnostics).RecordRequestParameters(job.InventorySourceId, job.StartDate, job.EndDate);

            var mediaWeekIds = _GetMediaWeekIds(job.StartDate, job.EndDate, processDiagnostics);

            var manifests = _InventoryRepository.GetInventoryBySourceWithUnprocessedPrograms(job.InventorySourceId, mediaWeekIds);
            _InventoryProgramsBySourceJobsRepository.UpdateJobNotes(jobId, $"Found {manifests.Count} inventory that did not have programs.");
            return manifests;
        }
    }
}