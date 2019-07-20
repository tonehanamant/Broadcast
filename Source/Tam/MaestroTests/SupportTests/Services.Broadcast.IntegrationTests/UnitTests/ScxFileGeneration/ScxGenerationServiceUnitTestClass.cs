using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Scx;

namespace Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration
{
    public class ScxGenerationServiceUnitTestClass : ScxGenerationService
    {
        public IScxFileGenerationHistorian ScxFileGenerationHistorian { get; set; }
        public string DropFolderPath { get; set; }

        public ScxGenerationServiceUnitTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProprietaryInventoryService proprietaryInventoryService,
            IFileService fileService)
            : base(broadcastDataRepositoryFactory, 
                proprietaryInventoryService, fileService)
        {
        }

        protected override IScxFileGenerationHistorian GetScxFileGenerationHistorian()
        {
            return ScxFileGenerationHistorian;
        }

        protected override string GetDropFolderPath()
        {
            return DropFolderPath;
        }
    }
}