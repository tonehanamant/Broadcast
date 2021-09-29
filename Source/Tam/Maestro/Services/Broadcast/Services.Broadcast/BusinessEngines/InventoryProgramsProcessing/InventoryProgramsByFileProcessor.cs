using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsByFileProcessor : InventoryProgramsProcessingEngineBase
    {
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;

        public InventoryProgramsByFileProcessor(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(broadcastDataRepositoryFactory,
                genreCache,
                featureToggleHelper,
                configurationSettingsHelper)
        {
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryProgramsByFileJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
        }

        protected override IInventoryProgramsJobsRepository _GetJobsRepository()
        {
            return _InventoryProgramsByFileJobsRepository;
        }

        protected override InventoryProgramsProcessingJobDiagnostics _GetNewDiagnostics()
        {
            return new InventoryProgramsProcessingJobByFileDiagnostics(OnDiagnosticMessageUpdate);
        }

        protected override InventorySource _GetInventorySource(int jobId)
        {
            var fileId = _InventoryProgramsByFileJobsRepository.GetJob(jobId).InventoryFileId;
            var inventorySource = _InventoryFileRepository.GetInventoryFileById(fileId).InventorySource;
            return inventorySource;
        }

        protected override List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            var fileId = _InventoryProgramsByFileJobsRepository.GetJob(jobId).InventoryFileId;
            ((InventoryProgramsProcessingJobByFileDiagnostics)processDiagnostics).RecordRequestParameters(fileId);
            
            var manifests = _InventoryRepository.GetInventoryByFileIdForProgramsProcessing(fileId);
            return manifests;
        }

        protected override void SetPrimaryProgramFromProgramMappings(List<StationInventoryManifest> manifests, Action<string> logger)
        {
            var programMappings = _ProgramMappingRepository.GetProgramMappings();
            var programMappingByInventoryProgramName = programMappings.ToDictionary(x => x.OriginalProgramName, x => x, StringComparer.OrdinalIgnoreCase);
            var batchSize = _GetSaveBatchSize();
            var manifestDayparts = manifests.SelectMany(x => x.ManifestDayparts);
            var manifestDaypartsCount = manifestDayparts.Count();
            var chunks = manifestDayparts.GetChunks(batchSize);

            logger($"Setting primary programs. Total manifest dayparts: {manifestDaypartsCount}. Total chunks: {chunks.Count()}");

            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];

                logger($"Setting primary programs for chunk #{i + 1} / {chunks.Count}, items: {chunk.Count}");

                _SetPrimaryProgramFromProgramMappings(chunk, programMappingByInventoryProgramName);
            }

            logger($"Finished setting primary programs. Total manifest dayparts: {manifestDaypartsCount}");
        }

        private void _SetPrimaryProgramFromProgramMappings(
            List<StationInventoryManifestDaypart> manifestDayparts,
            Dictionary<string, ProgramMappingsDto> programMappingByInventoryProgramName)
        {
            var programSource = ProgramSourceEnum.Maestro;
            var updatedManifestDaypartIds = new List<int>();
            var newManifestDaypartPrograms = new List<StationInventoryManifestDaypartProgram>();

            foreach (var manifestDaypart in manifestDayparts.Where(x => !string.IsNullOrWhiteSpace(x.ProgramName)))
            {
                // If there is no mapping for the program, go to the next daypart
                if (!programMappingByInventoryProgramName.TryGetValue(manifestDaypart.ProgramName, out var mapping))
                    continue;

                // Create the new StationInventoryManifestDaypartProgram
                newManifestDaypartPrograms.Add(new StationInventoryManifestDaypartProgram
                {
                    StationInventoryManifestDaypartId = manifestDaypart.Id.Value,
                    ProgramName = mapping.OfficialProgramName,
                    ProgramSourceId = (int)programSource,
                    MaestroGenreId = mapping.OfficialGenre.Id,
                    SourceGenreId = mapping.OfficialGenre.Id,
                    ShowType = mapping.OfficialShowType.Name,
                    StartTime = manifestDaypart.Daypart.StartTime,
                    EndTime = manifestDaypart.Daypart.EndTime,
                    CreatedDate = _GetCurrentDateTime()
                });

                updatedManifestDaypartIds.Add(manifestDaypart.Id.Value);
            }

            _InventoryRepository.RemovePrimaryProgramFromManifestDayparts(updatedManifestDaypartIds);
            _InventoryRepository.DeleteInventoryPrograms(updatedManifestDaypartIds);

            _InventoryRepository.CreateInventoryPrograms(newManifestDaypartPrograms, _GetCurrentDateTime());
            _InventoryRepository.UpdatePrimaryProgramsForManifestDayparts(updatedManifestDaypartIds);
        }

    }
}