using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;

namespace Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration
{
    public class ScxGenerationServiceUnitTestClass : ScxGenerationService
    {
        public string DropFolderPath { get; set; }

        public ScxGenerationServiceUnitTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProprietaryInventoryService proprietaryInventoryService,
            IFileService fileService,
            IQuarterCalculationEngine quarterCalculationEngine)
            : base(broadcastDataRepositoryFactory, 
                proprietaryInventoryService, fileService, quarterCalculationEngine)
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