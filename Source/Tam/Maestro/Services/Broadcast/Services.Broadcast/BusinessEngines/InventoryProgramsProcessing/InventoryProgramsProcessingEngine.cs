using Common.Services.Repositories;
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

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public interface IInventoryProgramsProcessingEngine
    {
        InventoryProgramsProcessingJobByFileDiagnostics ProcessInventoryProgramsByFileJob(int jobId);

        InventoryProgramsProcessingJobBySourceDiagnostics ProcessInventoryProgramsBySourceJob(int jobId);
    }

    public class InventoryProgramsProcessingEngine : IInventoryProgramsProcessingEngine
    {
        private const int SAVE_CHUNK_SIZE = 1000;

        private readonly IGenreRepository _GenreRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;
        private readonly IInventoryProgramsBySourceJobsRepository _InventoryProgramsBySourceJobsRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly IProgramGuideApiClient _ProgramGuideApiClient;

        public InventoryProgramsProcessingEngine(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IProgramGuideApiClient programGuideApiClient)
        {
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryProgramsByFileJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
            _InventoryProgramsBySourceJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsBySourceJobsRepository>();
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _ProgramGuideApiClient = programGuideApiClient;
        }

        public InventoryProgramsProcessingJobByFileDiagnostics ProcessInventoryProgramsByFileJob(int jobId)
        {
            var processDiagnostics = new InventoryProgramsProcessingJobByFileDiagnostics (onMessageUpdated_ByFileJobs)
            {
                JobId = jobId,
                SaveChunkSize = SAVE_CHUNK_SIZE,
                RequestChunkSize = _GetRequestElementMaxCount()
            };

            try
            {
                processDiagnostics.RecordStart();

                var fileId = _InventoryProgramsByFileJobsRepository.GetJob(jobId).InventoryFileId;
                processDiagnostics.RecordRequestParameters(fileId);

                var inventorySource = _InventoryFileRepository.GetInventoryFileById(fileId).InventorySource;
                processDiagnostics.RecordInventorySource(inventorySource);

                /*** Gather Inventory ***/
                processDiagnostics.RecordGatherInventoryStart();
                _InventoryProgramsByFileJobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.GatherInventory, "Beginning step ");

                // to make it work for Diginet remove from the below : 'm.Station != null &&'
                var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(fileId)
                    .Where(m => m.Station != null && m.ManifestDayparts.Any() && m.ManifestWeeks.Any()).ToList();

                processDiagnostics.RecordGatherInventoryStop();

                if (manifests.Any() == false)
                {
                    _InventoryProgramsByFileJobsRepository.SetJobCompleteWarning(jobId, "Job ending because no manifest records found to process.");
                    return processDiagnostics;
                }
                var result = _ProcessInventory(jobId, inventorySource, GenreSourceEnum.Dativa, manifests, null, processDiagnostics, _InventoryProgramsByFileJobsRepository);

                if (result == InventoryProgramsJobStatus.Warning)
                {
                    _InventoryProgramsByFileJobsRepository.SetJobCompleteWarning(jobId);
                }
                else
                {
                    _InventoryProgramsByFileJobsRepository.SetJobCompleteSuccess(jobId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logger.Error(
                    $"Error caught processing an inventory file for program names.  JobId = '{jobId}'", ex);
                _InventoryProgramsByFileJobsRepository.SetJobCompleteError(jobId, $"Error caught : {ex.Message} ; {ex.StackTrace}");

                throw;
            }
            finally
            {
                processDiagnostics.RecordStop();
            }
            return processDiagnostics;
        }

        public InventoryProgramsProcessingJobBySourceDiagnostics ProcessInventoryProgramsBySourceJob(int jobId)
        {
            var processDiagnostics = new InventoryProgramsProcessingJobBySourceDiagnostics(onMessageUpdated_BySourceJobs)
            {
                JobId = jobId,
                SaveChunkSize = SAVE_CHUNK_SIZE,
                RequestChunkSize = _GetRequestElementMaxCount()
            };

            try
            {
                processDiagnostics.RecordStart();

                var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
                processDiagnostics.RecordRequestParameters(job.InventorySourceId, job.StartDate, job.EndDate);

                var inventorySource = _InventoryRepository.GetInventorySource(job.InventorySourceId);
                var mediaWeekIds = _MediaWeekCache.GetDisplayMediaWeekByFlight(job.StartDate, job.EndDate).Select(w => w.Id).ToList();
                processDiagnostics.RecordInventorySource(inventorySource);
                processDiagnostics.RecordMediaWeekIds(mediaWeekIds);

                // this could be a lot of data, so iteration on weeks.
                var finalResult = InventoryProgramsJobStatus.Completed;
                foreach (var mediaWeekId in mediaWeekIds)
                {
                    _InventoryProgramsBySourceJobsRepository.UpdateJobMessage(jobId, $"Beginning iteration for media week id '{mediaWeekId}'.");
                    
                    /*** Gather Inventory ***/
                    processDiagnostics.RecordGatherInventoryStart();
                    _InventoryProgramsBySourceJobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.GatherInventory);
                    // to make it work for Diginet remove from the below : 'm.Station != null &&'
                    var manifests = _InventoryRepository.GetInventoryManifestsBySourceAndMediaWeek(inventorySource.Id, new List<int> { mediaWeekId });
                    processDiagnostics.RecordGatherInventoryStop();

                    if (manifests.Any() == false)
                    {
                        finalResult = InventoryProgramsJobStatus.Warning;
                        _InventoryProgramsBySourceJobsRepository.UpdateJobMessage(jobId, $"Iteration for media week id '{mediaWeekId}' is ending because no manifest records found to process.");
                        continue;
                    }

                    var iterationResult = _ProcessInventory(jobId, inventorySource, GenreSourceEnum.Dativa, manifests, mediaWeekId, processDiagnostics, _InventoryProgramsBySourceJobsRepository);
                    if (iterationResult == InventoryProgramsJobStatus.Warning)
                    {
                        finalResult = InventoryProgramsJobStatus.Warning;
                        _InventoryProgramsBySourceJobsRepository.UpdateJobMessage(jobId, $"Iteration for media week id '{mediaWeekId}' ended in warning.");
                    }
                }
                _InventoryProgramsBySourceJobsRepository.UpdateJobMessage(jobId, $"Completed '{mediaWeekIds.Count}' iterations for media weeks.");

                if (finalResult == InventoryProgramsJobStatus.Warning)
                {
                    _InventoryProgramsBySourceJobsRepository.SetJobCompleteWarning(jobId);
                }
                else
                {
                    _InventoryProgramsBySourceJobsRepository.SetJobCompleteSuccess(jobId);
                }
                
            }
            catch (Exception ex)
            {
                LogHelper.Logger.Error($"Error caught processing an inventory source for program names.  JobId = '{jobId}'", ex);
                _InventoryProgramsBySourceJobsRepository.SetJobCompleteError(jobId, $"Error caught : {ex.Message} ; {ex.StackTrace}");

                throw;
            }
            finally
            {
                processDiagnostics.RecordStop();
            }
            return processDiagnostics;
        }

        private InventoryProgramsJobStatus _ProcessInventory(int jobId, InventorySource inventorySource, GenreSourceEnum genreSource, List<StationInventoryManifest> inventoryManifests, int? processMediaWeekId,
            InventoryProgramsProcessingJobDiagnostics processDiagnostics, IInventoryProgramsJobsRepository jobsRepository)
        {
            var result = InventoryProgramsJobStatus.Completed;
            var requestElementNumber = 0;
            var requestElementMaxCount = _GetRequestElementMaxCount(); 
            var genreSourceId = (int) genreSource;
            var genres = _GenreRepository.GetGenresBySourceId(genreSourceId);

            jobsRepository.UpdateJobMessage(jobId, $"Processing {inventoryManifests.Count} manifest records.");
            jobsRepository.UpdateJobMessage(jobId, $"Found {genres.Count} genre records for source {genreSource} records.");
            
            /*** Transform to ProgramGuideApi Input ***/
            var distinctWeeks = inventoryManifests.SelectMany(m => m.ManifestWeeks).Select(w => w.MediaWeek).Distinct()
                .OrderBy(w => w.StartDate).ToList();

            if (processMediaWeekId.HasValue)
            {
                distinctWeeks = distinctWeeks.Where(s => s.Id == processMediaWeekId).ToList();
                if (distinctWeeks.Any() == false)
                {
                    throw new InvalidOperationException($"ProcessMediaWeekId of {processMediaWeekId} not found in the manifests.");
                }
            }

            processDiagnostics.RecordManifestDetails(inventoryManifests.Count, distinctWeeks.Count, inventoryManifests.Sum(m => m.ManifestDayparts.Count));

            var weekNumber = 0;
            foreach (var week in distinctWeeks)
            {
                weekNumber++;
                processDiagnostics.RecordIterationStart(weekNumber, distinctWeeks.Count);
                processDiagnostics.RecordTransformToInputStart();

                var requestMappings = new List<GuideRequestResponseMapping>();
                var requestElements = new List<GuideRequestElementDto>();

                var manifestsForWeek = inventoryManifests.Where(m => m.ManifestWeeks.Select(w => w.MediaWeek.Id).Contains(week.Id));
                foreach (var manifest in manifestsForWeek)
                {
                    foreach (var daypart in manifest.ManifestDayparts.OrderBy(d => d.Daypart.StartTime))
                    {
                        var stationCallLetters = _GetManifestStationCallLetters(manifest, inventorySource);
                        var networkAffiliate = _GetManifestNetworkAffiliation(manifest, inventorySource);

                        var requestElementMapping = new GuideRequestResponseMapping
                        {
                            RequestElementNumber = ++requestElementNumber,
                            WeekNumber = weekNumber,
                            ManifestId = manifest.Id ?? 0,
                            ManifestDaypartId = daypart.Id ?? 0,
                            WeekStartDte = week.StartDate,
                            WeekEndDate = week.EndDate,
                            DaypartText = daypart.Daypart.Preview,
                            StationCallLetters = stationCallLetters,
                            NetworkAffiliate = networkAffiliate
                        };
                        requestMappings.Add(requestElementMapping);
                        requestElements.Add(
                            new GuideRequestElementDto
                            {
                                Id = requestElementMapping.RequestEntryId,
                                StartDate = week.StartDate,
                                EndDate = week.EndDate,
                                StationCallLetters = stationCallLetters,
                                NetworkAffiliate = networkAffiliate,
                                Daypart = new GuideRequestDaypartDto
                                {
                                    Id = requestElementMapping.RequestEntryId,
                                    Name = daypart.Daypart.Preview,
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
                    .GroupBy(x => x.Index / requestElementMaxCount)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                var currentRequestChunkIndex = 0;
                foreach (var requestChunk in requestChunks)
                {
                    var programs = new ConcurrentBag<StationInventoryManifestDaypartProgram>();

                    /*** Call Api ***/
                    jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.CallApi);

                    currentRequestChunkIndex++;
                    processDiagnostics.RecordIterationStartCallToApi(currentRequestChunkIndex, requestChunks.Count);

                    var programGuideResponse = _ProgramGuideApiClient.GetProgramsForGuide(requestChunk);

                    processDiagnostics.RecordIterationStopCallToApi(programGuideResponse.Count);

                    if (programGuideResponse.Any() == false)
                    {
                        result = InventoryProgramsJobStatus.Warning;
                        jobsRepository.UpdateJobMessage(jobId, $"Request set {currentRequestChunkIndex} returned no responses.");
                    }
                    else
                    {
                        /*** Apply Api Response ***/
                        processDiagnostics.RecordIterationStartApplyApiResponse();
                        jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.ApplyProgramData);

                        foreach (var mapping in requestMappings)
                        {
                            // if a requestMapping doesn't have a response then log it
                            var mappedResponses = programGuideResponse.Where(e => e.RequestDaypartId.Equals(mapping.RequestEntryId)).ToList();

                            if (mappedResponses.Any() == false)
                            {
                                jobsRepository.UpdateJobMessage(jobId, $"A request received no response : {mapping}");
                                result = InventoryProgramsJobStatus.Warning;
                                continue;
                            }

                            foreach (var responseEntry in mappedResponses)
                            {
                                responseEntry.Programs.Select(p => new StationInventoryManifestDaypartProgram
                                {
                                    StationInventoryManifestDaypartId = mapping.ManifestDaypartId,
                                    ProgramName = p.ProgramName,
                                    ShowType = p.ShowType,
                                    Genre = p.SourceGenre,
                                    GenreSourceId = genreSourceId,
                                    GenreId = genres.Single(g => g.Display.Equals(p.SourceGenre)).Id,
                                    StartDate = p.StartDate,
                                    EndDate = p.EndDate,
                                    StartTime = p.StartTime,
                                    EndTime = p.EndTime
                                }).ForEach(a => programs.Add(a));
                            }
                        }

                        processDiagnostics.RecordIterationStopApplyApiResponse(programs.Count);
                    }

                    /*** Save the programs ***/
                    jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.SavePrograms);
                    processDiagnostics.RecordIterationStartSavePrograms();
                    var programSaveChunksCount = 0;

                    // clear out the old programs first.
                    var deleteProgramsForDayparts = requestMappings.Select(m => m.ManifestDaypartId).Distinct().ToList();
                    var deleteProgramsForDaypartsChunks = deleteProgramsForDayparts.Select((x, i) => new { Index = i, Value = x })
                        .GroupBy(x => x.Index / SAVE_CHUNK_SIZE)
                        .Select(x => x.Select(v => v.Value).ToList())
                        .ToList();

                    jobsRepository.UpdateJobMessage(jobId, $"Removing programs from {requestMappings.Count} manifests split into {deleteProgramsForDaypartsChunks.Count} chunks.");
                    deleteProgramsForDaypartsChunks.ForEach(chunk => 
                        _InventoryRepository.DeleteInventoryProgramsFromManifestDayparts(chunk.Select(c => c).ToList(), 
                            week.StartDate, week.EndDate));

                    if (programs.Any())
                    {
                        var programSaveChunks = programs.Select((x, i) => new { Index = i, Value = x })
                            .GroupBy(x => x.Index / SAVE_CHUNK_SIZE)
                            .Select(x => x.Select(v => v.Value).ToList())
                            .ToList();
                        programSaveChunksCount = programSaveChunks.Count;

                        programSaveChunks.ForEach(chunk => _InventoryRepository.UpdateInventoryPrograms(chunk, DateTime.Now));
                    }
                    else
                    {
                        result = InventoryProgramsJobStatus.Warning;
                        jobsRepository.UpdateJobMessage(jobId, $"Ending iteration without saving programs.");
                    }

                    processDiagnostics.RecordIterationStopSavePrograms(programSaveChunksCount);
                }
            }

            return result;
        }

        internal void onMessageUpdated_ByFileJobs(int jobId, string message)
        {
            _InventoryProgramsByFileJobsRepository.UpdateJobMessage(jobId, message);
        }

        internal void onMessageUpdated_BySourceJobs(int jobId, string message)
        {
            _InventoryProgramsBySourceJobsRepository.UpdateJobMessage(jobId, message);
        }

        protected virtual int _GetRequestElementMaxCount()
        {
            return ProgramGuideApiClient.RequestElementMaxCount;
        }

        private string _GetManifestStationCallLetters(StationInventoryManifest manifest, InventorySource source)
        {
            // TODO : maybe a mapping
            if (source.InventoryType == InventorySourceTypeEnum.Diginet)
            {
                return source.Name;
            }
            return manifest.Station.LegacyCallLetters;
        }

        private string _GetManifestNetworkAffiliation(StationInventoryManifest manifest, InventorySource source)
        {
            // TODO : maybe a mapping
            if (source.InventoryType == InventorySourceTypeEnum.Diginet)
            {
                return source.Name;
            }
            return manifest.Station.Affiliation;
        }

    }
}