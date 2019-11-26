using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class InventoryServiceUnitTests
    {
        private readonly Mock<IDataRepositoryFactory> _BroadcastDataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
        private readonly Mock<IInventoryFileValidator> _InventoryFileValidatorMock = new Mock<IInventoryFileValidator>();
        private readonly Mock<IDaypartCache> _DaypartCacheMock = new Mock<IDaypartCache>();
        private readonly Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
        private readonly Mock<ISMSClient> _SmsClientMock = new Mock<ISMSClient>();
        private readonly Mock<IProprietarySpotCostCalculationEngine> _ProprietarySpotCostCalculationEngineMock = new Mock<IProprietarySpotCostCalculationEngine>();
        private readonly Mock<IStationInventoryGroupService> _StationInventoryGroupServiceMock = new Mock<IStationInventoryGroupService>();
        private readonly Mock<IBroadcastAudiencesCache> _AudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
        private readonly Mock<IRatingForecastService> _RatingForecastServiceMock = new Mock<IRatingForecastService>();
        private readonly Mock<INsiPostingBookService> _NsiPostingBookServiceMock = new Mock<INsiPostingBookService>();
        private readonly Mock<IDataLakeFileService> _DataLakeFileServiceMock = new Mock<IDataLakeFileService>();
        private readonly Mock<ILockingEngine> _LockingEngineMock = new Mock<ILockingEngine>();
        private readonly Mock<IStationProcessingEngine> _StationProcessingEngineMock = new Mock<IStationProcessingEngine>();
        private readonly Mock<IImpressionsService> _ImpressionsServiceMock = new Mock<IImpressionsService>();
        private readonly Mock<IOpenMarketFileImporter> _OpenMarketFileImporterMock = new Mock<IOpenMarketFileImporter>();
        private readonly Mock<IFileService> _FileServiceMock = new Mock<IFileService>();
        private readonly Mock<IInventoryRatingsProcessingService> _InventoryRatingsServiceMock = new Mock<IInventoryRatingsProcessingService>();
        private readonly Mock<IInventoryProgramEnrichmentService> _InventoryProgramEnrichmentServiceMock = new Mock<IInventoryProgramEnrichmentService>();
        private readonly Mock<IInventoryRepository> _InventoryRepositoryMock = new Mock<IInventoryRepository>();
        private readonly Mock<ISpotLengthRepository> _SpotLengthRepository = new Mock<ISpotLengthRepository>();

        [Test]
        [TestCase(FileStatusEnum.Failed, 0, 0, "Validation Error")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Failed, InventoryFileProgramEnrichmentJobStatus.Completed, "Processing Error")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Succeeded, InventoryFileProgramEnrichmentJobStatus.Error, "Processing Error")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Succeeded, InventoryFileProgramEnrichmentJobStatus.Completed, "Succeeded")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Queued, InventoryFileProgramEnrichmentJobStatus.ApplyProgramData, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Queued, InventoryFileProgramEnrichmentJobStatus.CallApi, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Queued, InventoryFileProgramEnrichmentJobStatus.Completed, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Queued, InventoryFileProgramEnrichmentJobStatus.GatherInventory, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Queued, InventoryFileProgramEnrichmentJobStatus.Queued, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Queued, InventoryFileProgramEnrichmentJobStatus.SavePrograms, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Processing, InventoryFileProgramEnrichmentJobStatus.ApplyProgramData, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Processing, InventoryFileProgramEnrichmentJobStatus.CallApi, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Processing, InventoryFileProgramEnrichmentJobStatus.Completed, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Processing, InventoryFileProgramEnrichmentJobStatus.GatherInventory, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Processing, InventoryFileProgramEnrichmentJobStatus.Queued, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Processing, InventoryFileProgramEnrichmentJobStatus.SavePrograms, "Processing")]
        public void GetInventoryUploadHistory_ReturnsCorrectStatuses(
            FileStatusEnum fileLoadStatus,
            BackgroundJobProcessingStatus ratingProcessingJobStatus,
            InventoryFileProgramEnrichmentJobStatus programEnrichmentJobStatus,
            string expectedStatus)
        {
            _SetupInventoryServiceDependencies();

            _QuarterCalculationEngineMock
                .Setup(x => x.GetQuarterDateRange(It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 4, 1)));

            _InventoryRepositoryMock
                .Setup(x => x.GetInventoryUploadHistoryForInventorySource(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory
                    {
                        FileLoadStatus = fileLoadStatus,
                        RatingProcessingJobStatus = ratingProcessingJobStatus,
                        ProgramEnrichmentJobStatus = programEnrichmentJobStatus
                    }
                });
            
            var service = new InventoryService(
                _BroadcastDataRepositoryFactoryMock.Object,
                _InventoryFileValidatorMock.Object,
                _DaypartCacheMock.Object,
                _QuarterCalculationEngineMock.Object,
                _SmsClientMock.Object,
                _ProprietarySpotCostCalculationEngineMock.Object,
                _StationInventoryGroupServiceMock.Object,
                _AudiencesCacheMock.Object,
                _RatingForecastServiceMock.Object,
                _NsiPostingBookServiceMock.Object,
                _DataLakeFileServiceMock.Object,
                _LockingEngineMock.Object,
                _StationProcessingEngineMock.Object,
                _ImpressionsServiceMock.Object,
                _OpenMarketFileImporterMock.Object,
                _FileServiceMock.Object,
                _InventoryRatingsServiceMock.Object,
                _InventoryProgramEnrichmentServiceMock.Object);

            var result = service.GetInventoryUploadHistory(
                inventorySourceId: 1,
                quarter: null,
                year: null);

            Assert.AreEqual(expectedStatus, result.First().Status);
        }

        private void _SetupInventoryServiceDependencies()
        {
            _SpotLengthRepository
                .Setup(x => x.GetSpotLengthAndIds())
                .Returns(new Dictionary<int, int>());

            _SpotLengthRepository
                .Setup(x => x.GetSpotLengthIdsAndCostMultipliers())
                .Returns(new Dictionary<int, double>());

            _BroadcastDataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);

            _BroadcastDataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepository.Object);
        }
    }
}
