using System;
using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Validators;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class InventoryServiceUnitTestClass : InventoryService
    {
        public InventoryServiceUnitTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IInventoryFileValidator inventoryFileValidator,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ISMSClient smsClient,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            IStationInventoryGroupService stationInventoryGroupService,
            IBroadcastAudiencesCache audiencesCache,
            IRatingForecastService ratingForecastService,
            INsiPostingBookService nsiPostingBookService,
            IDataLakeFileService dataLakeFileService,
            ILockingEngine lockingEngine,
            IStationProcessingEngine stationProcessingEngine,
            IImpressionsService impressionsService,
            IOpenMarketFileImporter openMarketFileImporter,
            IFileService fileService,
            IInventoryRatingsProcessingService inventoryRatingsService,
            IInventoryProgramEnrichmentService inventoryProgramEnrichmentService)
        : base(broadcastDataRepositoryFactory,
            inventoryFileValidator,
            daypartCache,
            quarterCalculationEngine,
            smsClient,
            proprietarySpotCostCalculationEngine,
            stationInventoryGroupService,
            audiencesCache,
            ratingForecastService,
            nsiPostingBookService,
            dataLakeFileService,
            lockingEngine,
            stationProcessingEngine,
            impressionsService,
            openMarketFileImporter,
            fileService,
            inventoryRatingsService,
            inventoryProgramEnrichmentService)
        {
        }

        public int GetInventoryUploadErrorsFolderCalledCount { get; set; } = 0;

        protected override string _GetInventoryUploadErrorsFolder()
        {
            GetInventoryUploadErrorsFolderCalledCount++;
            return "BroadcastServiceSystemParameter.InventoryUploadErrorsFolder";
        }

        public DateTime? UTDateTimeNow { get; set; }

        protected override DateTime _GetDateTimeNow()
        {
            return UTDateTimeNow ?? DateTime.Now;
        }
    }
}