using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
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
using System.Threading.Tasks;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public interface IInventoryProgramsProcessingEngine
    {
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryJob(int jobId);
    }

    public abstract class InventoryProgramsProcessingEngineBase : IInventoryProgramsProcessingEngine
    {
        private const int SAVE_CHUNK_SIZE = 1000;

        private readonly IGenreRepository _GenreRepository;
        protected readonly IInventoryRepository _InventoryRepository;

        private readonly IProgramGuideApiClient _ProgramGuideApiClient;
        private readonly IStationMappingService _StationMappingService;

        protected InventoryProgramsProcessingEngineBase(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService)
        {
            _ProgramGuideApiClient = programGuideApiClient;
            _StationMappingService = stationMappingService;

            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        protected abstract IInventoryProgramsJobsRepository _GetJobsRepository();

        protected abstract InventoryProgramsProcessingJobDiagnostics _GetNewDiagnostics();

        protected abstract InventorySource _GetInventorySource(int jobId);

        protected abstract List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics);

        protected abstract InventoryProgramsRequestPackage _BuildRequestPackage(List<StationInventoryManifest> inventoryManifests,
            InventorySource inventorySource, InventoryProgramsProcessingJobDiagnostics processDiagnostics, int jobId);

        protected abstract List<StationInventoryManifestDaypartProgram> _GetProgramsFromResponse(GuideResponseElementDto currentResponse,
            GuideRequestResponseMapping currentMapping, InventoryProgramsRequestPackage requestPackage, List<LookupDto> genres);

        internal void OnDiagnosticMessageUpdate(int jobId, string message)
        {
            _GetJobsRepository().UpdateJobNotes(jobId, message);
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryJob(int jobId)
        {
            var processDiagnostics = _GetNewDiagnostics();
            processDiagnostics.JobId = jobId;
            processDiagnostics.SaveChunkSize = SAVE_CHUNK_SIZE;
            processDiagnostics.RequestChunkSize = _GetRequestElementMaxCount();

            var jobsRepo = _GetJobsRepository();

            try
            {
                var inventorySource = _GetInventorySource(jobId);
                processDiagnostics.RecordInventorySource(inventorySource);

                processDiagnostics.RecordStart();

                processDiagnostics.RecordGatherInventoryStart();
                jobsRepo.UpdateJobStatus(jobId, InventoryProgramsJobStatus.GatherInventory, "Beginning step ");

                var manifests = _GatherInventory(jobId, processDiagnostics);

                processDiagnostics.RecordGatherInventoryStop();

                if (manifests.Any() == false)
                {
                    var message = "Job ending because no manifest records found to process.";
                    jobsRepo.SetJobCompleteWarning(jobId, message, message);
                    return processDiagnostics;
                }

                var requestsPackage = _BuildRequestPackage(manifests, inventorySource, processDiagnostics, jobId);
                _ProcessInventory(jobId, requestsPackage, processDiagnostics);
            }
            catch (Exception ex)
            {
                LogHelper.Logger.Error(
                    $"Error caught processing for program names.  JobId = '{jobId}'", ex);
                jobsRepo.SetJobCompleteError(jobId, ex.Message, $"Error caught : {ex.Message} ; {ex.StackTrace}");

                throw;
            }
            finally
            {
                processDiagnostics.RecordStop();
            }
            return processDiagnostics;
        }

        private void _ProcessInventory(int jobId, InventoryProgramsRequestPackage requestPackage, 
            InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            var result = InventoryProgramsJobStatus.Completed;
            var jobsRepository = _GetJobsRepository();
            
            var requestElementMaxCount = _GetRequestElementMaxCount();
            var parallelEnabled = _GetParallelApiCallsEnabled();
            var maxDop = _GetMaxDegreesOfParallelism();

            var genreSourceId = (int)requestPackage.GenreSource;
            var genres = _GenreRepository.GetGenresBySourceId(genreSourceId);
            jobsRepository.UpdateJobNotes(jobId, $"Found {genres.Count} genre records for source {requestPackage.GenreSource} records.");

            var requestChunks = requestPackage.RequestElements.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / requestElementMaxCount)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            var currentRequestChunkIndex = 0;
            foreach (var requestChunk in requestChunks)
            {
                var programs = new List<StationInventoryManifestDaypartProgram>();

                /*** Call Api ***/
                jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.CallApi);

                currentRequestChunkIndex++;
                processDiagnostics.RecordIterationStartCallToApi(currentRequestChunkIndex, requestChunks.Count);

                // the api call
                var programGuideResponse = parallelEnabled
                    ? _MakeParallelCallsToApi(requestChunk, maxDop)
                    : _ProgramGuideApiClient.GetProgramsForGuide(requestChunk);

                processDiagnostics.RecordIterationStopCallToApi(programGuideResponse.Count);

                /*** Apply Api Response ***/
                processDiagnostics.RecordIterationStartApplyApiResponse();
                jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.ApplyProgramData);

                if (programGuideResponse.Any() == false)
                {
                    result = InventoryProgramsJobStatus.Warning;
                    jobsRepository.UpdateJobNotes(jobId, $"Request set {currentRequestChunkIndex} returned no responses.");
                }
                else
                {
                    foreach (var mapping in requestPackage.RequestMappings)
                    {
                        // if a requestMapping doesn't have a response then log it
                        var mappedResponses = programGuideResponse.Where(e => e.RequestDaypartId.Equals(mapping.RequestEntryId)).ToList();

                        if (mappedResponses.Any() == false)
                        {
                            result = InventoryProgramsJobStatus.Warning;
                            jobsRepository.UpdateJobNotes(jobId, $"A request received no response : {mapping}");
                            continue;
                        }

                        foreach (var responseEntry in mappedResponses)
                        {
                            var entryPrograms = _GetProgramsFromResponse(responseEntry, mapping, requestPackage, genres);
                            programs.AddRange(entryPrograms);
                        }
                    }
                }
                processDiagnostics.RecordIterationStopApplyApiResponse(programs.Count);

                /*** Save the programs ***/
                jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.SavePrograms);
                processDiagnostics.RecordIterationStartSavePrograms();
                var programSaveChunksCount = 0;

                // clear out the old programs first.
                var deletePrograms = requestPackage.RequestMappings.Select(m => m.ManifestId).Distinct().ToList();
                var deleteProgramsChunks = deletePrograms.Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / SAVE_CHUNK_SIZE)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                jobsRepository.UpdateJobNotes(jobId, $"Removing programs from {requestPackage.RequestMappings.Count} manifests split into {deleteProgramsChunks.Count} chunks.");
                deleteProgramsChunks.ForEach(chunk => 
                    _InventoryRepository.DeleteInventoryPrograms(chunk.Select(c => c).ToList(),
                        requestPackage.StartDateRange, requestPackage.EndDateRange));

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
                    jobsRepository.UpdateJobNotes(jobId, $"Ending iteration without saving programs.");
                }

                processDiagnostics.RecordIterationStopSavePrograms(programSaveChunksCount);
            }

            if (result == InventoryProgramsJobStatus.Warning)
            {
                jobsRepository.SetJobCompleteWarning(jobId, null, null);
            }
            else
            {
                jobsRepository.SetJobCompleteSuccess(jobId, null, null);
            }
        }

        protected string _GetManifestStationCallLetters(StationInventoryManifest manifest, InventorySource source)
        {
            const StationMapSetNamesEnum programGuideMapSet = StationMapSetNamesEnum.Extended;
            // get the Cadent Callsign for the inventory station
            var cadentCallsign = manifest.Station.LegacyCallLetters;
            // call the mappings service to get all stations mapped to that guy
            var mappings = _StationMappingService.GetStationMappingsByCadentCallLetter(cadentCallsign);
            // find the station callsign for my map_set

            var mappedStations = mappings.Where(m => m.MapSet == programGuideMapSet).Select(s => s.MapValue).ToList();

            // not using single to provide informative messages.
            if (mappedStations.Count == 0)
            {
                throw new Exception($"Mapping for CadentCallsign '{cadentCallsign}' and Map Set '{programGuideMapSet}' not found.");
            }
            if (mappedStations.Count > 1)
            {
                throw new Exception($"Mapping for CadentCallsign '{cadentCallsign}' and Map Set '{programGuideMapSet}' has {mappedStations.Count} mappings when only one expected.");
            }

            var mappedStationInfo = mappedStations.Single();
            return mappedStationInfo;
        }

        private int _GetGenreId(List<LookupDto> genres, string candidate)
        {
            var found = genres.FirstOrDefault(g => g.Display.Equals(candidate, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                throw new InvalidOperationException($"Genre '{candidate}' not found.");
            }
            return found.Id;
        }

        protected StationInventoryManifestDaypartProgram _MapProgramDto(GuideResponseProgramDto guideProgram, int manifestDaypartId, 
            InventoryProgramsRequestPackage requestPackage, List<LookupDto> genres)
        {
            var program = new StationInventoryManifestDaypartProgram
            {
                StationInventoryManifestDaypartId = manifestDaypartId,
                ProgramName = guideProgram.ProgramName,
                ShowType = guideProgram.ShowType,
                Genre = guideProgram.SourceGenre,
                GenreSourceId = (short) requestPackage.GenreSource,
                GenreId = _GetGenreId(genres, guideProgram.SourceGenre),
                StartDate = guideProgram.StartDate,
                EndDate = guideProgram.EndDate,
                StartTime = guideProgram.StartTime,
                EndTime = guideProgram.EndTime
            };
            return program;
        }

        private List<GuideResponseElementDto> _MakeParallelCallsToApi(List<GuideRequestElementDto> requests,
            int maxDop)
        {
            var responses = new ConcurrentBag<GuideResponseElementDto>();
            var errors = new ConcurrentBag<Exception>();

            var parallelRequestsBatchSize = _GetParallelApiCallsBatchSize();
            var requestBatches = requests.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / parallelRequestsBatchSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            // Send calls in parallel
            // Send one request per call
            Parallel.ForEach(requestBatches, new ParallelOptions { MaxDegreeOfParallelism = maxDop }, (requestBatch) =>
            {
                try
                {
                    var response = _ProgramGuideApiClient.GetProgramsForGuide(requestBatch);
                    response.ForEach(r => responses.Add(r));
                }
                catch (Exception e)
                {
                    errors.Add(e);
                }
            });

            if (errors.Any())
            {
                var msg = $"{errors.Count} errors caught while calling Program Guide in parallel.  MaxDop = {maxDop}; " +
                          $"ParallelRequestsBatchSize = {parallelRequestsBatchSize}; RequestCount = {requests.Count}. ";
                throw new AggregateException(msg, errors);
            }

            return responses.ToList();
        }

        protected virtual int _GetRequestElementMaxCount()
        {
            return ProgramGuideApiClient.RequestElementMaxCount;
        }

        protected virtual bool _GetParallelApiCallsEnabled()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineParallelEnabled;
        }

        protected virtual int _GetParallelApiCallsBatchSize()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineParallelBatchSize;
        }

        protected virtual int _GetMaxDegreesOfParallelism()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineMaxDop;
        }
    }
}