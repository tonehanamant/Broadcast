using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public interface IInventoryProgramsProcessingEngine
    {
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryJob(int jobId);
    }

    public abstract class InventoryProgramsProcessingEngineBase : BroadcastBaseClass, IInventoryProgramsProcessingEngine
    {
        // PRI-25264 : disabling sending the email
        public bool IsSuccessEmailEnabled { get; set; } = false;

        protected readonly IInventoryRepository _InventoryRepository;
        protected readonly IProgramMappingRepository _ProgramMappingRepository;

        private readonly IGenreCache _GenreCache;

        protected InventoryProgramsProcessingEngineBase(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _GenreCache = genreCache;

            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ProgramMappingRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();
        }

        protected abstract IInventoryProgramsJobsRepository _GetJobsRepository();

        protected abstract InventoryProgramsProcessingJobDiagnostics _GetNewDiagnostics();

        protected abstract InventorySource _GetInventorySource(int jobId);

        protected abstract List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics);

        protected abstract void SetPrimaryProgramFromProgramMappings(List<StationInventoryManifest> manifests, Action<string> logger);

        internal void OnDiagnosticMessageUpdate(int jobId, string message)
        {
            _GetJobsRepository().UpdateJobNotes(jobId, message);
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryJob(int jobId)
        {
            var processDiagnostics = _GetNewDiagnostics();
            processDiagnostics.JobId = jobId;
            processDiagnostics.SaveChunkSize = _GetSaveBatchSize();
            processDiagnostics.RequestChunkSize = 10;

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

                SetPrimaryProgramFromProgramMappings(manifests, (message) => _LogInfo(message));

                jobsRepo.SetJobCompleteSuccess(jobId, null, null);
            }
            catch (Exception ex)
            {
                _LogError($"Error caught processing for program names.  JobId = '{jobId}'", ex);
                jobsRepo.SetJobCompleteError(jobId, ex.Message, $"Error caught : {ex.Message}");
                throw;
            }
            finally
            {
                processDiagnostics.RecordStop();
            }
            return processDiagnostics;
        }

        protected StationInventoryManifestDaypartProgram _MapProgramDto(GuideResponseProgramDto guideProgram, int manifestDaypartId,
            InventoryProgramsRequestPackage requestPackage)
        {
            const ProgramSourceEnum PROGRAM_SOURCE = ProgramSourceEnum.Master;

            var sourceGenre = _GenreCache.GetSourceGenreLookupDtoByName(guideProgram.SourceGenre, PROGRAM_SOURCE);
            var maestroGenre = _GenreCache.GetMaestroGenreLookupDtoBySourceGenre(sourceGenre, PROGRAM_SOURCE);

            var program = new StationInventoryManifestDaypartProgram
            {
                StationInventoryManifestDaypartId = manifestDaypartId,
                ProgramName = guideProgram.ProgramName,
                ShowType = guideProgram.ShowType,
                SourceGenreId = sourceGenre.Id,
                ProgramSourceId = (int)PROGRAM_SOURCE,
                MaestroGenreId = maestroGenre.Id,
                StartDate = guideProgram.StartDate,
                EndDate = guideProgram.EndDate,
                StartTime = guideProgram.StartTime,
                EndTime = guideProgram.EndTime
            };
            return program;
        }

        protected virtual int _GetSaveBatchSize()
        {
            var result =
                _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.InventoryProgramsEngineSaveBatchSize,
                    5000);
            return result;
        }
    }
}