using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Cache;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsByFileProcessorTestClass : InventoryProgramsByFileProcessor
    {
        public InventoryProgramsByFileProcessorTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(
                  broadcastDataRepositoryFactory, 
                  genreCache,
                  featureToggleHelper,
                  configurationSettingsHelper)
        {
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override int _GetSaveBatchSize()
        {
            return 1000;
        }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime.HasValue
                ? UT_CurrentDateTime.Value
                : base._GetCurrentDateTime();
        }
    }
}