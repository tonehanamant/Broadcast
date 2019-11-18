using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Utilities.Logging;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryProgramEnrichmentService : IApplicationService
    {
        int QueueInventoryFileProgramEnrichmentJob(int fileId, string username);

        [Queue("inventoryprogramenrichment")]
        InventoryFileProgramEnrichmentJobDiagnostics PerformInventoryFileProgramEnrichmentJob(int jobId);
    }

    public class InventoryProgramEnrichmentService : IInventoryProgramEnrichmentService
    {
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IGenreRepository _GenreRepository;
        private readonly IInventoryFileProgramEnrichmentJobsRepository _InventoryFileProgramEnrichmentJobsRepository;
        private readonly IProgramGuideApiClientSimulator _ProgramGuideApiClient;

        public InventoryProgramEnrichmentService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IProgramGuideApiClientSimulator programGuideApiClient)
        {
            _BackgroundJobClient = backgroundJobClient;
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _InventoryFileProgramEnrichmentJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileProgramEnrichmentJobsRepository>();
            _ProgramGuideApiClient = programGuideApiClient;
        }

        public int QueueInventoryFileProgramEnrichmentJob(int fileId, string username)
        {
            // validate file exists.  Will throw.
            _InventoryFileRepository.GetInventoryFileById(fileId);
            var jobId = _InventoryFileProgramEnrichmentJobsRepository.QueueJob(fileId, username, DateTime.Now);
            _BackgroundJobClient.Enqueue<IInventoryProgramEnrichmentService>(x => x.PerformInventoryFileProgramEnrichmentJob(jobId));
            return jobId;
        }

        /// <remarks>
        /// ProcessDiagnostics used to gather and report info.
        /// Intending to remove ProcessDiagnostics once api client is in place and tested. 
        /// </remarks>
        public InventoryFileProgramEnrichmentJobDiagnostics PerformInventoryFileProgramEnrichmentJob(int jobId)
        {
            const string dateFormat = "MM/dd/yyyy";
            const int genreSourceId = (int)GenreSourceEnum.Dativa;
            const int requestChunkSize = 1000; // PRI-17014 will make this configurable
            const int saveChunkSize = 1000; // PRI-17014 will make this configurable

            var requestElementNumber = 0;
            var processDiagnostics = new InventoryFileProgramEnrichmentJobDiagnostics { JobId = jobId, RequestChunkSize = requestChunkSize, SaveChunkSize = saveChunkSize };

            try
            {
                processDiagnostics.RecordStart();

                var fileId = _InventoryFileProgramEnrichmentJobsRepository.GetJob(jobId).InventoryFileId;
                var inventorySource = _InventoryFileRepository.GetInventoryFileById(fileId).InventorySource;
                processDiagnostics.RecordFileInfo(fileId, inventorySource);

                /*** Gather Inventory ***/
                processDiagnostics.RecordGatherInventoryStart();
                _InventoryFileProgramEnrichmentJobsRepository.UpdateJobStatus(jobId, InventoryFileProgramEnrichmentJobStatus.GatherInventory);

                // to make it work for Diginet remove from the below : 'm.Station != null &&'
                var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(fileId)
                    .Where(m => m.Station != null && m.ManifestDayparts.Any() && m.ManifestWeeks.Any()).ToList();

                processDiagnostics.RecordGatherInventoryStop();

                if (manifests.Any() == false)
                {
                    _InventoryFileProgramEnrichmentJobsRepository.SetJobCompleteSuccess(jobId);
                    return processDiagnostics;
                }

                /*** Transform to ProgramGuideApi Input ***/
                var distinctWeeks = manifests.SelectMany(m => m.ManifestWeeks).Select(w => w.MediaWeek).Distinct()
                    .OrderBy(w => w.StartDate).ToList();
                processDiagnostics.RecordManifestDetails(manifests.Count, distinctWeeks.Count, manifests.Sum(m => m.ManifestDayparts.Count));

                var weekNumber = 0;
                foreach (var week in distinctWeeks)
                {
                    weekNumber++;
                    processDiagnostics.RecordIterationStart(weekNumber, distinctWeeks.Count);
                    processDiagnostics.RecordTransformToInputStart();

                    var startDateString = week.StartDate.ToString(dateFormat);
                    var endDateString = week.EndDate.ToString(dateFormat);

                    var requestMappings = new List<GuideRequestResponseMapping>();
                    var requestElements = new List<GuideRequestElementDto>();

                    var manifestsForWeek = manifests.Where(m => m.ManifestWeeks.Select(w => w.MediaWeek.Id).Contains(week.Id));
                    foreach (var manifest in manifestsForWeek)
                    {
                        foreach (var daypart in manifest.ManifestDayparts.OrderBy(d => d.Daypart.StartTime))
                        {
                            var requestElementMapping = new GuideRequestResponseMapping
                            {
                                RequestElementNumber = ++requestElementNumber,
                                WeekNumber = weekNumber,
                                ManifestId = manifest.Id ?? 0,
                                ManifestDaypartId = daypart.Id ?? 0,
                                WeekStartDte = week.StartDate,
                                WeekEndDate = week.EndDate
                            };
                            requestMappings.Add(requestElementMapping);
                            requestElements.Add(
                                new GuideRequestElementDto
                                {
                                    RequestElementId = requestElementMapping.RequestEntryId,
                                    StartDate = startDateString,
                                    EndDate = endDateString,
                                    NielsenLegacyStationCallLetters = _GetManifestStationCallLetters(manifest, inventorySource),
                                    NetworkAffiliate = _GetManifestNetworkAffiliation(manifest, inventorySource),
                                    Daypart = new GuideRequestDaypartDto
                                    {
                                        RequestDaypartId = requestElementMapping.RequestEntryId,
                                        Daypart = daypart.Daypart.Preview,
                                        Monday = daypart.Daypart.Monday,
                                        Tuesday = daypart.Daypart.Tuesday,
                                        Wednesday = daypart.Daypart.Wednesday,
                                        Thursday = daypart.Daypart.Thursday,
                                        Friday = daypart.Daypart.Friday,
                                        Saturday = daypart.Daypart.Saturday,
                                        Sunday = daypart.Daypart.Sunday,
                                        StartTime = daypart.Daypart.StartTime,
                                        EndTime = daypart.Daypart.EndTime
                                    }
                                });
                        }
                    }

                    processDiagnostics.RecordTransformToInputStop(requestElements.Count);

                    var requestChunks = requestElements.Select((x, i) => new { Index = i, Value = x })
                        .GroupBy(x => x.Index / requestChunkSize)
                        .Select(x => x.Select(v => v.Value).ToList())
                        .ToList();

                    var currentRequestChunkIndex = 0;
                    foreach (var requestChunk in requestChunks)
                    {
                        /*** Call Api ***/
                        _InventoryFileProgramEnrichmentJobsRepository.UpdateJobStatus(jobId, InventoryFileProgramEnrichmentJobStatus.CallApi);

                        currentRequestChunkIndex++;
                        processDiagnostics.RecordIterationStartCallToApi(currentRequestChunkIndex, requestChunks.Count);

                        var programGuideResponse = _ProgramGuideApiClient.GetProgramsForGuide(requestChunk);

                        processDiagnostics.RecordIterationStopCallToApi(programGuideResponse.Count);

                        /*** Apply Api Response ***/
                        processDiagnostics.RecordIterationStartApplyApiResponse();

                        _InventoryFileProgramEnrichmentJobsRepository.UpdateJobStatus(jobId, InventoryFileProgramEnrichmentJobStatus.ApplyProgramData);

                        var genres = _GenreRepository.GetGenresBySourceId(genreSourceId);
                        var programs = new ConcurrentBag<StationInventoryManifestDaypartProgram>();

                        foreach (var mapping in requestMappings)
                        {
                            foreach (var responseEntry in programGuideResponse.Where(e =>
                                e.RequestDaypartId.Equals(mapping.RequestEntryId)))
                            {
                                responseEntry.Programs.Select(p => new StationInventoryManifestDaypartProgram
                                {
                                    StationInventoryManifestDaypartId = mapping.ManifestDaypartId,
                                    ProgramName = p.ProgramName,
                                    ShowType = p.ShowType,
                                    Genre = p.Genre,
                                    GenreSourceId = genreSourceId,
                                    GenreId = genres.Single(g => g.Display.Equals(p.Genre)).Id,
                                    StartDate = DateTime.Parse(responseEntry.StartDate),
                                    EndDate = DateTime.Parse(responseEntry.EndDate),
                                    StartTime = p.StartTime,
                                    EndTime = p.EndTime
                                }).ForEach(a => programs.Add(a));
                            }
                        }

                        processDiagnostics.RecordIterationStopApplyApiResponse(programs.Count);

                        /*** Save the programs ***/
                        _InventoryFileProgramEnrichmentJobsRepository.UpdateJobStatus(jobId, InventoryFileProgramEnrichmentJobStatus.SavePrograms);

                        processDiagnostics.RecordIterationStartSavePrograms();

                        var programSaveChunks = programs.Select((x, i) => new { Index = i, Value = x })
                            .GroupBy(x => x.Index / saveChunkSize)
                            .Select(x => x.Select(v => v.Value).ToList())
                            .ToList();

                        programSaveChunks.ForEach(chunk => _InventoryRepository.UpdateInventoryPrograms(chunk,
                            DateTime.Now, chunk.Select(c => c.StationInventoryManifestDaypartId).ToList(),
                            week.StartDate, week.EndDate));

                        processDiagnostics.RecordIterationStopSavePrograms(programSaveChunks.Count);
                    }
                }

                /*** All done. ***/
                _InventoryFileProgramEnrichmentJobsRepository.SetJobCompleteSuccess(jobId);
            }
            catch (Exception ex)
            {
                LogHelper.Logger.Error(
                    $"Error caught processing an inventory file for program names.  JobId = '{jobId}'", ex);
                _InventoryFileProgramEnrichmentJobsRepository.SetJobCompleteError(jobId, ex.Message);

                throw ex;
            }
            finally
            {
                processDiagnostics.RecordStop();
            }
            return processDiagnostics;
        }

        private string _GetManifestStationCallLetters(StationInventoryManifest manifest, InventorySource source)
        {
            // TODO SDE : maybe a mapping
            if (source.InventoryType == InventorySourceTypeEnum.Diginet)
            {
                return source.Name;
            }
            return manifest.Station.LegacyCallLetters;
        }

        private string _GetManifestNetworkAffiliation(StationInventoryManifest manifest, InventorySource source)
        {
            // TODO SDE : maybe a mapping
            if (source.InventoryType == InventorySourceTypeEnum.Diginet)
            {
                return source.Name;
            }
            return manifest.Station.Affiliation;
        }
    }
}