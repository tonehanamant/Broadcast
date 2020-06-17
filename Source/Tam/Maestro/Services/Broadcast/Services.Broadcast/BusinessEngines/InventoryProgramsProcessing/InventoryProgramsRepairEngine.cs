using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public interface IInventoryProgramsRepairEngine
    {
        /// <summary>
        /// Identify and repair the inventory
        /// </summary>
        void RepairInventoryPrograms(CancellationToken token);
    }

    /// <summary>
    /// Repairs Program Name Mappings issues.
    /// </summary>
    public class InventoryProgramsRepairEngine : BroadcastBaseClass, IInventoryProgramsRepairEngine
    {
        protected readonly IInventoryRepository _InventoryRepository;
        protected readonly IProgramMappingRepository _ProgramMappingRepository;

        public InventoryProgramsRepairEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ProgramMappingRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();
        }

        public void RepairInventoryPrograms(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var manifestDayparts = _GetOrphanedManifestDayparts();

            if (!manifestDayparts.Any())
            {
                _LogInfo("No orphaned manifest dayparts found to repair.  Ending.");
                return;
            }

            token.ThrowIfCancellationRequested();
            _SetPrimaryProgramFromProgramMappings(manifestDayparts, token);
        }

        private List<StationInventoryManifestDaypart> _GetOrphanedManifestDayparts()
        {
            var manifestDayparts = _InventoryRepository.GetOrphanedManifestDayparts();
            return manifestDayparts;
        }

        private void _SetPrimaryProgramFromProgramMappings(List<StationInventoryManifestDaypart> manifestDayparts, CancellationToken token)
        {
            var programMappings = _ProgramMappingRepository.GetProgramMappings();
            var programMappingByInventoryProgramName = programMappings.ToDictionary(x => x.OriginalProgramName, x => x, StringComparer.OrdinalIgnoreCase);
            var batchSize = _GetSaveBatchSize();
            var manifestDaypartsCount = manifestDayparts.Count();
            var chunks = manifestDayparts.GetChunks(batchSize);

            _LogInfo($"Setting primary programs. Total manifest dayparts: {manifestDaypartsCount}. Total chunks: {chunks.Count()}");

            for (var i = 0; i < chunks.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var chunk = chunks[i];

                _LogInfo($"Setting primary programs for chunk #{i + 1} / {chunks.Count}, items: {chunk.Count}");

                _SetPrimaryProgramFromProgramMappings(chunk, programMappingByInventoryProgramName, token);
            }

            _LogInfo($"Finished setting primary programs. Total manifest dayparts: {manifestDaypartsCount}");
        }

        private void _SetPrimaryProgramFromProgramMappings(
            List<StationInventoryManifestDaypart> manifestDayparts,
            Dictionary<string, ProgramMappingsDto> programMappingByInventoryProgramName, CancellationToken token)
        {
            var programSource = ProgramSourceEnum.Mapped;
            var updatedManifestDaypartIds = new List<int>();
            var newManifestDaypartPrograms = new List<StationInventoryManifestDaypartProgram>();

            foreach (var manifestDaypart in manifestDayparts.Where(x => !string.IsNullOrWhiteSpace(x.ProgramName)))
            {
                token.ThrowIfCancellationRequested();

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

            token.ThrowIfCancellationRequested();

            _InventoryRepository.CreateInventoryPrograms(newManifestDaypartPrograms, _GetCurrentDateTime());

            var manifestDaypartIds = manifestDayparts.Where(s => s.Id.HasValue).Select(s => s.Id.Value).ToList();
            _InventoryRepository.UpdatePrimaryProgramsForManifestDayparts(manifestDaypartIds);
        }

        protected virtual int _GetSaveBatchSize()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineSaveBatchSize;
        }
    }
}