using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration
{
    public class ScxGenerationServiceUnitTestClass : ScxGenerationService
    {
        public string DropFolderPath { get; set; }

        public ScxGenerationServiceUnitTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProprietaryInventoryService proprietaryInventoryService,
            IFileService fileService,
            IQuarterCalculationEngine quarterCalculationEngine,
            IBackgroundJobClient backgroundJobClient,IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(broadcastDataRepositoryFactory, 
                proprietaryInventoryService, fileService, quarterCalculationEngine, backgroundJobClient, featureToggleHelper, configurationSettingsHelper)
        {
        }

        protected override string GetDropFolderPath()
        {
            return DropFolderPath;
        }

        public ScxFileGenerationDetail UT_TransformFromDtoToEntity(ScxFileGenerationDetailDto dto)
        {
            return TransformFromDtoToEntity(dto);
        }
    }
}