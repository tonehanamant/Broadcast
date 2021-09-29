using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
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
        private readonly IGenreCache _GenreCache;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public InventoryProgramsProcessorFactory(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _GenreCache = genreCache;
            _FeatureToggleHelper = featureToggleHelper;
            _ConfigurationSettingsHelper = configurationSettingsHelper;
        }

        public IInventoryProgramsProcessingEngine GetInventoryProgramsProcessingEngine(InventoryProgramsProcessorType jobType)
        {
            switch (jobType)
            {
                case InventoryProgramsProcessorType.ByFile:
                    return new InventoryProgramsByFileProcessor(
                        _BroadcastDataRepositoryFactory,
                        _GenreCache,
                        _FeatureToggleHelper,
                        _ConfigurationSettingsHelper
                    );
                case InventoryProgramsProcessorType.BySource:
                    return new InventoryProgramsBySourceProcessor(
                        _BroadcastDataRepositoryFactory,
                        _MediaWeekCache,
                        _GenreCache,
                        _FeatureToggleHelper,
                        _ConfigurationSettingsHelper
                        );
                case InventoryProgramsProcessorType.BySourceUnprocessed:
                    return new InventoryProgramsBySourceUnprocessedProcessor(
                        _BroadcastDataRepositoryFactory,
                        _MediaWeekCache,
                        _GenreCache,
                        _FeatureToggleHelper,
                        _ConfigurationSettingsHelper
                    );
                default:
                    throw new NotImplementedException("Unsupported job type.");
            }
        }
    }
}