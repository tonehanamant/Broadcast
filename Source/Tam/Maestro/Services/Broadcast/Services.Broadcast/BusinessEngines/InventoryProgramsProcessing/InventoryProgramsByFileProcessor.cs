using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tam.Maestro.Common;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsByFileProcessor : InventoryProgramsProcessingEngineBase
    {
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;

        public InventoryProgramsByFileProcessor(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IGenreCache genreCache,
            IFileService fileService,
            IEmailerService emailerService,
            IEnvironmentService environmentService)
            : base(broadcastDataRepositoryFactory,
                programGuideApiClient,
                stationMappingService,
                genreCache,
                fileService,
                emailerService,
                environmentService)
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

        protected override InventoryProgramsRequestPackage _BuildRequestPackage(List<StationInventoryManifest> inventoryManifests,
            InventorySource inventorySource, InventoryProgramsProcessingJobDiagnostics processDiagnostics, int jobId)
        {
            var requestElementNumber = 0;

            var rangeStartDate = inventoryManifests.SelectMany(s => s.ManifestWeeks.Select(w => w.StartDate)).Min();
            var rangeEndDate = inventoryManifests.SelectMany(s => s.ManifestWeeks.Select(w => w.EndDate)).Max();

            processDiagnostics.RecordTransformToInputStart(inventoryManifests.Count, rangeStartDate, rangeEndDate);

            var requestMappings = new List<GuideRequestResponseMapping>();
            var requestElements = new List<GuideRequestElementDto>();

            foreach (var manifest in inventoryManifests)
            {
                if (string.IsNullOrWhiteSpace(manifest.Station.Affiliation))
                {
                    // no affiliate indicates unrated so don't query for.
                    continue;
                }

                // a manifest should ALWAYS have a daypart.
                if (manifest.ManifestDayparts.Any() == false)
                {
                    _InventoryProgramsByFileJobsRepository.UpdateJobNotes(jobId, $"WARNING : Inventory Manifest '{manifest.Id}' has no dayparts.");
                    continue;
                }

                var firstDaypart = manifest.ManifestDayparts.OrderBy(d => d.Daypart.StartTime).First();

                var entryStartDate = _GetEntryStartDate(firstDaypart, rangeStartDate);
                var entryEndDate = _GetEntryEndDate(firstDaypart, entryStartDate);

                var entryDaypartStartTimeAsSeconds = firstDaypart.Daypart.StartTime;
                var entryDaypartEndTimeAsSeconds = _GetEndTimeInSeconds(entryDaypartStartTimeAsSeconds);
                var stationCallLetters = _GetManifestStationCallLetters(manifest, inventorySource);

                var requestElementMapping = new GuideRequestResponseMapping
                {
                    RequestElementNumber = ++requestElementNumber,
                    ManifestId = manifest.Id ?? 0,
                    ManifestDaypartId = firstDaypart.Id ?? 0,
                    StartDate = entryStartDate,
                    EndDate = entryEndDate,
                    DaypartText = firstDaypart.Daypart.Preview, // leave this as-is
                    StationCallLetters = stationCallLetters,
                    NetworkAffiliate = manifest.Station.Affiliation
                };
                requestMappings.Add(requestElementMapping);
                requestElements.Add(
                    new GuideRequestElementDto
                    {
                        Id = requestElementMapping.RequestEntryId,
                        StartDate = entryStartDate,
                        EndDate = entryEndDate,
                        StationCallLetters = stationCallLetters,
                        NetworkAffiliate = manifest.Station.Affiliation,
                        Daypart = new GuideRequestDaypartDto
                        {
                            Id = requestElementMapping.RequestEntryId,
                            Name = firstDaypart.Daypart.Preview,
                            Sunday = entryStartDate.DayOfWeek == DayOfWeek.Sunday || entryEndDate.DayOfWeek == DayOfWeek.Sunday,
                            Monday = entryStartDate.DayOfWeek == DayOfWeek.Monday || entryEndDate.DayOfWeek == DayOfWeek.Monday,
                            Tuesday = entryStartDate.DayOfWeek == DayOfWeek.Tuesday || entryEndDate.DayOfWeek == DayOfWeek.Tuesday,
                            Wednesday = entryStartDate.DayOfWeek == DayOfWeek.Wednesday || entryEndDate.DayOfWeek == DayOfWeek.Wednesday,
                            Thursday = entryStartDate.DayOfWeek == DayOfWeek.Thursday || entryEndDate.DayOfWeek == DayOfWeek.Thursday,
                            Friday = entryStartDate.DayOfWeek == DayOfWeek.Friday || entryEndDate.DayOfWeek == DayOfWeek.Friday,
                            Saturday = entryStartDate.DayOfWeek == DayOfWeek.Saturday || entryEndDate.DayOfWeek == DayOfWeek.Saturday,
                            StartTime = entryDaypartStartTimeAsSeconds,
                            EndTime = entryDaypartEndTimeAsSeconds
                        }
                    });
            }

            processDiagnostics.RecordTransformToInputStop(requestElements.Count);

            var requestPackage = new InventoryProgramsRequestPackage
            {
                RequestMappings = requestMappings,
                RequestElements = requestElements,
                InventoryManifests = inventoryManifests,
                StartDateRange = rangeStartDate,
                EndDateRange = rangeEndDate
            };
            return requestPackage;
        }

        protected override List<StationInventoryManifestDaypartProgram> _GetProgramsFromResponse(
            GuideResponseElementDto currentResponse,
            GuideRequestResponseMapping currentMapping,
            InventoryProgramsRequestPackage requestPackage
            )
        {
            var programs = new List<StationInventoryManifestDaypartProgram>();
            requestPackage.InventoryManifests
                .Where(m => m.Id == currentMapping.ManifestId)
                .SelectMany(m => m.ManifestDayparts)
                .Select(d => d.Id.Value)
                .ForEach(daypartId => 
                    currentResponse.Programs.Select(p => 
                        _MapProgramDto(p, daypartId, requestPackage))
                        .ForEach(a => programs.Add(a)));
            return programs;
        }


        protected override string _GetExportedFileReadyNotificationEmailBody(int jobId, string filePath)
        {
            var job = _InventoryProgramsByFileJobsRepository.GetJob(jobId);
            var inventoryFile = _InventoryFileRepository.GetInventoryFileById(job.InventoryFileId);

            var body = new StringBuilder();
            body.AppendLine("Hello,");
            body.AppendLine();
            body.AppendLine($"A ProgramGuide Interface file has been exported.");
            body.AppendLine();
            body.AppendLine($"\tInventory File Id : {inventoryFile.Id}");
            body.AppendLine($"\tInventory File Name : {inventoryFile.FileName}");
            body.AppendLine($"\tInventory Source : {inventoryFile.InventorySource.Name}");
            body.AppendLine();
            body.AppendLine($"File Path :");
            body.AppendLine($"\t{filePath}");
            body.AppendLine();
            body.AppendLine($"Have a nice day.");
            return body.ToString();
        }

        protected override string _GetExportedFileFailedNotificationEmailBody(int jobId)
        {
            var job = _InventoryProgramsByFileJobsRepository.GetJob(jobId);
            var inventoryFile = _InventoryFileRepository.GetInventoryFileById(job.InventoryFileId);

            var body = new StringBuilder();
            body.AppendLine("Hello,");
            body.AppendLine();
            body.AppendLine($"A ProgramGuide Interface file has failed to be exported.");
            body.AppendLine();
            body.AppendLine($"\tInventory File Id : {inventoryFile.Id}");
            body.AppendLine($"\tInventory File Name : {inventoryFile.FileName}");
            body.AppendLine($"\tInventory Source : {inventoryFile.InventorySource.Name}");
            body.AppendLine();
            body.AppendLine($"Have a nice day.");
            return body.ToString();
        }

        protected override string _GetNoInventoryToProcessNotificationEmailBody(int jobId)
        {
            var job = _InventoryProgramsByFileJobsRepository.GetJob(jobId);
            var inventoryFile = _InventoryFileRepository.GetInventoryFileById(job.InventoryFileId);

            var body = new StringBuilder();
            body.AppendLine("Hello,");
            body.AppendLine();
            body.AppendLine($"A ProgramGuide Interface file was not exported because no inventory was found to process.");
            body.AppendLine();
            body.AppendLine($"\tInventory File Id : {inventoryFile.Id}");
            body.AppendLine($"\tInventory File Name : {inventoryFile.FileName}");
            body.AppendLine($"\tInventory Source : {inventoryFile.InventorySource.Name}");
            body.AppendLine();
            body.AppendLine($"Have a nice day.");
            return body.ToString();
        }

        protected override string _GetExportFileName(int jobId)
        {
            var job = _InventoryProgramsByFileJobsRepository.GetJob(jobId);
            return $"{EXPORT_FILE_NAME_SEED}" +
                   $"_FILE_{job.InventoryFileId}" +
                   $"_{_GetCurrentDateTime().ToString(EXPORT_FILE_SUFFIX_TIMESTAMP_FORMAT)}.csv";
        }

        private DateTime _GetEntryStartDate(StationInventoryManifestDaypart daypart, DateTime rangeStartDate)
        {
            const int oneDay = 1;
            var result = rangeStartDate;
            while (daypart.Daypart.Days.Contains(result.DayOfWeek) == false)
            {
                result = result.AddDays(oneDay);
            }
            return result;
        }

        private const int oneDay = 1;
        private const int fiveMinutesAsSeconds = 300;
        private const int fiveMinutesToMidnightAsSeconds = BroadcastConstants.OneDayInSeconds - fiveMinutesAsSeconds;

        private DateTime _GetEntryEndDate(StationInventoryManifestDaypart daypart, DateTime rangeStartDate)
        {
            var result = rangeStartDate;
            if (daypart.Daypart.StartTime >= fiveMinutesToMidnightAsSeconds)
            {
                result = result.AddDays(oneDay);
            }
            return result;
        }

        private int _GetEndTimeInSeconds(int entryDaypartStartTimeAsSeconds)
        {
            var result = entryDaypartStartTimeAsSeconds + fiveMinutesAsSeconds;

            if (result >= BroadcastConstants.OneDayInSeconds)
            {
                result = result - BroadcastConstants.OneDayInSeconds;
            }

            return result;
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
            var programSource = ProgramSourceEnum.Mapped;
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