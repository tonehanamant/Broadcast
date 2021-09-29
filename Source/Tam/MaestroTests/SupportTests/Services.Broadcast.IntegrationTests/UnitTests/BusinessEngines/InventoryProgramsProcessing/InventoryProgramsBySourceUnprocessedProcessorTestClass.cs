using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Cache;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class InventoryProgramsBySourceUnprocessedProcessorTestClass : InventoryProgramsBySourceUnprocessedProcessor
    {
        public InventoryProgramsBySourceUnprocessedProcessorTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(
                broadcastDataRepositoryFactory,
                mediaMonthAndWeekAggregateCache,
                genreCache,
                featureToggleHelper,
                configurationSettingsHelper)
        {
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime.HasValue
                ? UT_CurrentDateTime.Value
                : base._GetCurrentDateTime();
        }

        protected override int _GetSaveBatchSize()
        {
            return 1000;
        }
    }
}