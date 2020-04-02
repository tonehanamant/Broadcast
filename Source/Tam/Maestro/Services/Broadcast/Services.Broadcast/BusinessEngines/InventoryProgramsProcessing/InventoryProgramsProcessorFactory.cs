using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    /// <summary>
    /// A factory to produce an <see cref="InventoryProgramsProcessingEngineBase"/>.
    /// </summary>
    public interface IInventoryProgramsProcessorFactory : IApplicationService
    {
        IInventoryProgramsProcessingEngine GetInventoryProgramsProcessingEngine(
            InventoryProgramsProcessorType jobType);
    }

    /// <summary>
    /// A factory to produce an <see cref="InventoryProgramsProcessingEngineBase"/>.
    /// </summary>
    public class InventoryProgramsProcessorFactory : IInventoryProgramsProcessorFactory
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;

        protected readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly IProgramGuideApiClient _ProgramGuideApiClient;
        private readonly IStationMappingService _StationMappingService;
        private readonly IGenreCache _GenreCache;
        private readonly IFileService _FileService;
        private readonly IEmailerService _EmailerService;
        private readonly IEnvironmentService _EnvironmentService;

        public InventoryProgramsProcessorFactory(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IGenreCache genreCache,
            IFileService fileService,
            IEmailerService emailerService,
            IEnvironmentService environmentService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _ProgramGuideApiClient = programGuideApiClient;
            _StationMappingService = stationMappingService;
            _GenreCache = genreCache;
            _FileService = fileService;
            _EmailerService = emailerService;
            _EnvironmentService = environmentService;
        }

        public IInventoryProgramsProcessingEngine GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType jobType)
        {
            switch (jobType)
            {
                case InventoryProgramsProcessorType.ByFile:
                    return new InventoryProgramsByFileProcessor(
                        _BroadcastDataRepositoryFactory,
                        _ProgramGuideApiClient,
                        _StationMappingService,
                        _GenreCache,
                        _FileService,
                        _EmailerService,
                        _EnvironmentService
                    );
                case InventoryProgramsProcessorType.BySource:
                    return new InventoryProgramsBySourceProcessor(
                        _BroadcastDataRepositoryFactory,
                        _ProgramGuideApiClient,
                        _StationMappingService,
                        _MediaWeekCache,
                        _GenreCache,
                        _FileService,
                        _EmailerService,
                        _EnvironmentService
                        );
                case InventoryProgramsProcessorType.BySourceUnprocessed:
                    return new InventoryProgramsBySourceUnprocessedProcessor(
                        _BroadcastDataRepositoryFactory,
                        _ProgramGuideApiClient,
                        _StationMappingService,
                        _MediaWeekCache,
                        _GenreCache,
                        _FileService,
                        _EmailerService,
                        _EnvironmentService
                    );
                default:
                    throw new NotImplementedException("Unsupported job type.");
            }
        }
    }
}