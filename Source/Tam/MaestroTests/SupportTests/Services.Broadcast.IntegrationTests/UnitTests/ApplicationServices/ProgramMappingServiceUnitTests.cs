using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Hangfire;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Cache;
using Services.Broadcast.IntegrationTests.Stubs;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class ProgramMappingServiceUnitTests
    {
        private ProgramMappingServiceTestClass _ProgramMappingService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IBackgroundJobClient> _BackgroundJobClientMock;
        private Mock<IProgramMappingRepository> _ProgramMappingRepositoryMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IShowTypeRepository> _ShowTypeRepositoryMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;

        private IGenreCache _GenreCache;
        private IShowTypeCache _ShowTypeCache;

        private static bool WRITE_FILE_TO_DISK = false;

        [SetUp]
        public void CreateProgramMappingService()
        {
            // Create Mocks
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _BackgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _ProgramMappingRepositoryMock = new Mock<IProgramMappingRepository>();
            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _ShowTypeRepositoryMock = new Mock<IShowTypeRepository>();
            _SharedFolderServiceMock = new Mock<ISharedFolderService>();
            _GenreCache = new GenreCacheStub();
            _ShowTypeCache = new ShowTypeCacheStub();

            // Setup common mocks
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IProgramMappingRepository>())
                .Returns(_ProgramMappingRepositoryMock.Object);
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);

            _InventoryRepositoryMock
                .Setup(s => s.GetDaypartProgramsForInventoryDayparts(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifestDaypartProgram>
                {
                    new StationInventoryManifestDaypartProgram { Id = 1, StationInventoryManifestDaypartId = 1 }
                });

            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IShowTypeRepository>())
                .Returns(_ShowTypeRepositoryMock.Object);

            _ShowTypeRepositoryMock
                .Setup(s => s.GetShowTypeByName(It.IsAny<string>()))
                .Returns((string showTypeName) =>
                {
                    return new ShowTypeDto
                    {
                        Id = 1,
                        Name = showTypeName
                    };
                });

            // Setup the actual Program Mapping Service
            _ProgramMappingService = new ProgramMappingServiceTestClass(
                _BackgroundJobClientMock.Object,
                _DataRepositoryFactoryMock.Object,
                _SharedFolderServiceMock.Object,
                null, 
                _GenreCache, 
                _ShowTypeCache);
        }

        [Test]
        public void Construction()
        {
            Assert.IsNotNull(_ProgramMappingService);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanProcessNewProgramMappings()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto { OriginalProgramName = "10 NEWS 6PM", OfficialProgramName = "10 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
            };

            var createdProgramMappings = new List<IEnumerable<ProgramMappingsDto>>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((IEnumerable<ProgramMappingsDto> mappings, string uName, DateTime dCreated) =>
                {
                    createdProgramMappings.Add(mappings);
                });

            var updatedProgramMappings = new List<IEnumerable<ProgramMappingsDto>>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.UpdateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((IEnumerable<ProgramMappingsDto> mappings, string uName, DateTime dCreated) =>
                {
                    updatedProgramMappings.Add(mappings);
                });

            _ProgramMappingRepositoryMock
                .Setup(x => x.GetProgramMappingsByOriginalProgramNames(It.IsAny<IEnumerable<string>>()))
                .Returns(new List<ProgramMappingsDto>());

            var modifiedWhen = new DateTime(2020, 05, 13);
            var username = "testUser";

            // Act
            _ProgramMappingService.UT_ProcessProgramMappings(programMappings, modifiedWhen, username);

            // Assert
            var result = new
            {
                createdProgramMappings,
                updatedProgramMappings
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanProcessUpdatedProgramMappings()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto { OriginalProgramName = "10 NEWS 6PM", OfficialProgramName = "10 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
            };

            _ProgramMappingRepositoryMock
                .Setup(x => x.GetProgramMappingsByOriginalProgramNames(It.IsAny<IEnumerable<string>>()))
                .Returns(new List<ProgramMappingsDto>
                {
                    new ProgramMappingsDto
                    {
                        Id = 1,
                        OriginalProgramName = "10 NEWS 6PM",
                        OfficialProgramName = "10 NEWS R",
                        OfficialGenre = new Genre
                        {
                            Id = 2,
                            Name = "Religious"
                        },
                        OfficialShowType = new ShowTypeDto
                        {
                            Id = 2,
                            Name = "Paid Programming"
                        }
                    }
                });

            var createdProgramMappings = new List<IEnumerable<ProgramMappingsDto>>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((IEnumerable<ProgramMappingsDto> mappings, string uName, DateTime dCreated) =>
                {
                    createdProgramMappings.Add(mappings);
                });

            var updatedProgramMappings = new List<IEnumerable<ProgramMappingsDto>>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.UpdateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((IEnumerable<ProgramMappingsDto> mappings, string uName, DateTime dCreated) =>
                {
                    updatedProgramMappings.Add(mappings);
                });

            var modifiedWhen = new DateTime(2020, 05, 13);
            var username = "testUser";

            // Act
            _ProgramMappingService.UT_ProcessProgramMappings(programMappings, modifiedWhen, username);

            // Assert
            var result = new
            {
                createdProgramMappings,
                updatedProgramMappings
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanProcessMultipleNewProgramMappings()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto { OriginalProgramName = "10 NEWS 6PM", OfficialProgramName = "10 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
                new ProgramMappingsFileRequestDto { OriginalProgramName = "NBC 4 NEWS AT 5P WKND", OfficialProgramName = "NBC4 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
                new ProgramMappingsFileRequestDto { OriginalProgramName = "WVLT NEWS SUN 6:30P", OfficialProgramName = "WVLT NEWS", OfficialGenre = "News", OfficialShowType = "News" },
            };

            _ProgramMappingRepositoryMock
                .Setup(x => x.GetProgramMappingsByOriginalProgramNames(It.IsAny<IEnumerable<string>>()))
                .Returns(new List<ProgramMappingsDto>());

            var createdProgramMappings = new List<IEnumerable<ProgramMappingsDto>>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((IEnumerable<ProgramMappingsDto> mappings, string uName, DateTime dCreated) =>
                {
                    createdProgramMappings.Add(mappings);
                });

            var updatedProgramMappings = new List<IEnumerable<ProgramMappingsDto>>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.UpdateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((IEnumerable<ProgramMappingsDto> mappings, string uName, DateTime dCreated) =>
                {
                    updatedProgramMappings.Add(mappings);
                });

            var modifiedWhen = new DateTime(2020, 05, 13);
            var username = "testUser";

            // Act
            _ProgramMappingService.UT_ProcessProgramMappings(programMappings, modifiedWhen, username);

            // Assert
            var result = new
            {
                createdProgramMappings,
                updatedProgramMappings
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(ProgramMappingsDto), "Id");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        [Test]
        public void GenerateUnmappedProgramNameReportTest()
        {
            var expectedData = new List<string>() { "test1", "test2" };
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var inventoryRepository = new Mock<IInventoryRepository>();

            inventoryRepository.Setup(p => p.GetUnmappedPrograms()).Returns(expectedData);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryRepository>())
                .Returns(inventoryRepository.Object);

            var sut = new ProgramMappingService(null, broadcastDataRepositoryFactory.Object, null, null, null, null);

            var reportData = sut.GenerateUnmappedProgramNameReport();
            _WriteStream(reportData);

            Assert.IsNotNull(reportData.Stream);
            Assert.AreEqual("UnmappedProgramReport.xlsx", reportData.Filename);
        }

        private static void _WriteStream(ReportOutput reportOutput)
        {
            if (WRITE_FILE_TO_DISK)
                using (var destinationFileStream =
                    new FileStream($@"C:\Users\sgoel\Desktop\IntegrationTesting\{reportOutput.Filename}.xlsx",
                        FileMode.OpenOrCreate))
                {
                    while (reportOutput.Stream.Position < reportOutput.Stream.Length)
                        destinationFileStream.WriteByte((byte)reportOutput.Stream.ReadByte());
                }
        }
    }
}
