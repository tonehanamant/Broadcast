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
        private Mock<IGenreRepository> _GenreRepositoryMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;

        private IGenreCache _GenreCache;
        private IShowTypeCache _ShowTypeCache;

        private List<DateTime> _getManifestDaypartByNameCalls;
        private List<DateTime> _deleteInventoryCalls;
        private List<DateTime> _createInventoryCalls;
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
            _GenreRepositoryMock = new Mock<IGenreRepository>();
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

            _getManifestDaypartByNameCalls = new List<DateTime>();
            _InventoryRepositoryMock
                .Setup(s => s.GetManifestDaypartsForProgramName(It.IsAny<string>()))
                .Callback(() => _getManifestDaypartByNameCalls.Add(DateTime.Now))
                .Returns((string name) =>
                {
                    return new List<StationInventoryManifestDaypart>
                    {
                        new StationInventoryManifestDaypart
                        {
                            Id = 1,
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 1900,
                                EndTime = 2800
                            }
                        }
                    };
                });

            _InventoryRepositoryMock
                .Setup(s => s.GetDaypartProgramsForInventoryDayparts(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifestDaypartProgram>
                {
                    new StationInventoryManifestDaypartProgram { Id = 1, StationInventoryManifestDaypartId = 1 }
                });

            _deleteInventoryCalls = new List<DateTime>();
            _InventoryRepositoryMock
                .Setup(s => s.DeleteInventoryPrograms(It.IsAny<List<int>>()))
                .Callback(() => _deleteInventoryCalls.Add(DateTime.Now));

            _createInventoryCalls = new List<DateTime>();
            _InventoryRepositoryMock
                .Setup(s => s.CreateInventoryPrograms(It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback(() => _createInventoryCalls.Add(DateTime.Now));

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

            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IGenreRepository>())
                .Returns(_GenreRepositoryMock.Object);
            _GenreRepositoryMock
                .Setup(s => s.GetGenreByName(It.IsAny<string>(), It.IsAny<ProgramSourceEnum>()))
                .Returns((string genreName, ProgramSourceEnum source) =>
                {
                    return new Genre
                    {
                        Id = (int)source,
                        ProgramSourceId = (int)source,
                        Name = genreName
                    };
                });

            // Setup the actual Program Mapping Service
            _ProgramMappingService = new ProgramMappingServiceTestClass(
                _BackgroundJobClientMock.Object, _DataRepositoryFactoryMock.Object, _SharedFolderServiceMock.Object, null
                , _GenreCache, _ShowTypeCache);
        }

        [Test]
        public void Construction()
        {
            Assert.IsNotNull(_ProgramMappingService);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Ignore("Test is failing. Need to investigate")]
        public void CanProcessNewProgramMappings()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto { OriginalProgramName = "10 NEWS 6PM", OfficialProgramName = "10 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
            };

            var checkForExistingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.MappingExistsForOriginalProgramName(It.IsAny<string>()))
                .Callback(() => checkForExistingCalls.Add(DateTime.Now))
                .Returns(false);

            var createdProgramMapping = new ProgramMappingsDto();

            var createNewMappingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.CreateProgramMapping(It.IsAny<ProgramMappingsDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((ProgramMappingsDto mapping, string uName, DateTime dCreated) =>
                {
                    createdProgramMapping = mapping;
                    createNewMappingCalls.Add(DateTime.Now);
                })
                .Returns(1);

            var savedPrimaryPrograms = new List<IEnumerable<StationInventoryManifestDaypart>>();
            _InventoryRepositoryMock
                .Setup(x => x.UpdatePrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<StationInventoryManifestDaypart>>()))
                .Callback<IEnumerable<StationInventoryManifestDaypart>>(x => savedPrimaryPrograms.Add(x));

            var ingestedRecordsCount = 0;
            var updatedInventoryCount = 0;
            var modifiedWhen = new DateTime(2020, 05, 13);
            var username = "testUser";

            // Act
            _ProgramMappingService.UT_ProcessProgramMappings(programMappings, modifiedWhen, username, ref updatedInventoryCount, ref ingestedRecordsCount);

            // Assert
            Assert.AreEqual(1, checkForExistingCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, createNewMappingCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, _getManifestDaypartByNameCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, _deleteInventoryCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, _createInventoryCalls.Count, "Invalid call count.");

            Assert.AreEqual(1, ingestedRecordsCount, "Only one record should be ingested.");
            Assert.AreEqual(1, updatedInventoryCount, "Only one inventory item should be updated.");

            var result = new
            {
                createdProgramMapping,
                savedPrimaryPrograms
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Ignore("Test is failing. Need to investigate")]
        public void CanProcessUpdatedProgramMappings()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto { OriginalProgramName = "10 NEWS 6PM", OfficialProgramName = "10 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
            };

            var checkForExistingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.MappingExistsForOriginalProgramName(It.IsAny<string>()))
                .Callback(() => checkForExistingCalls.Add(DateTime.Now))
                .Returns(true);
            _ProgramMappingRepositoryMock
               .Setup(s => s.GetProgramMappingByOriginalProgramName(It.IsAny<string>()))
               .Returns(new ProgramMappingsDto
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
               });

            var programMappingToBeUpdated = new ProgramMappingsDto();

            var createNewMappingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.CreateProgramMapping(It.IsAny<ProgramMappingsDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    createNewMappingCalls.Add(DateTime.Now);
                })
                .Returns(1);

            var updateExistingMappingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.UpdateProgramMapping(It.IsAny<ProgramMappingsDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((ProgramMappingsDto mapping, string uName, DateTime dUpdated) =>
                {
                    programMappingToBeUpdated = mapping;
                    updateExistingMappingCalls.Add(DateTime.Now);
                });

            var savedPrimaryPrograms = new List<IEnumerable<StationInventoryManifestDaypart>>();
            _InventoryRepositoryMock
                .Setup(x => x.UpdatePrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<StationInventoryManifestDaypart>>()))
                .Callback<IEnumerable<StationInventoryManifestDaypart>>(x => savedPrimaryPrograms.Add(x));

            var ingestedRecordsCount = 0;
            var updatedInventoryCount = 0;
            var modifiedWhen = new DateTime(2020, 05, 13);
            var username = "testUser";

            // Act
            _ProgramMappingService.UT_ProcessProgramMappings(programMappings, modifiedWhen, username, ref updatedInventoryCount, ref ingestedRecordsCount);

            // Assert
            Assert.AreEqual(1, checkForExistingCalls.Count, "Invalid call count.");
            Assert.AreEqual(0, createNewMappingCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, updateExistingMappingCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, _getManifestDaypartByNameCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, _deleteInventoryCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, _createInventoryCalls.Count, "Invalid call count.");

            Assert.AreEqual(1, ingestedRecordsCount, "Only one record should be ingested.");
            Assert.AreEqual(1, updatedInventoryCount, "Only one inventory item should be updated.");

            var result = new
            {
                programMappingToBeUpdated,
                savedPrimaryPrograms
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Ignore("Test is failing. Need to investigate")]
        public void CanProcessMultipleNewProgramMappings()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto { OriginalProgramName = "10 NEWS 6PM", OfficialProgramName = "10 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
                new ProgramMappingsFileRequestDto { OriginalProgramName = "NBC 4 NEWS AT 5P WKND", OfficialProgramName = "NBC4 NEWS", OfficialGenre = "News", OfficialShowType = "News" },
                new ProgramMappingsFileRequestDto { OriginalProgramName = "WVLT NEWS SUN 6:30P", OfficialProgramName = "WVLT NEWS", OfficialGenre = "News", OfficialShowType = "News" },
            };

            var checkForExistingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.MappingExistsForOriginalProgramName(It.IsAny<string>()))
                .Callback(() => checkForExistingCalls.Add(DateTime.Now))
                .Returns(false);

            var programMappingsToBeCreated = new List<ProgramMappingsDto>();

            var createNewMappingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.CreateProgramMapping(It.IsAny<ProgramMappingsDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((ProgramMappingsDto mapping, string uName, DateTime dCreated) =>
                {
                    programMappingsToBeCreated.Add(mapping);
                    createNewMappingCalls.Add(DateTime.Now);
                })
                .Returns(1);

            var updateExistingMappingCalls = new List<DateTime>();
            _ProgramMappingRepositoryMock
                .Setup(s => s.UpdateProgramMapping(It.IsAny<ProgramMappingsDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    updateExistingMappingCalls.Add(DateTime.Now);
                });

            var savedPrimaryPrograms = new List<IEnumerable<StationInventoryManifestDaypart>>();
            _InventoryRepositoryMock
                .Setup(x => x.UpdatePrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<StationInventoryManifestDaypart>>()))
                .Callback<IEnumerable<StationInventoryManifestDaypart>>(x => savedPrimaryPrograms.Add(x));

            var ingestedRecordsCount = 0;
            var updatedInventoryCount = 0;
            var modifiedWhen = new DateTime(2020, 05, 13);
            var username = "testUser";

            // Act
            _ProgramMappingService.UT_ProcessProgramMappings(programMappings, modifiedWhen, username, ref updatedInventoryCount, ref ingestedRecordsCount);

            // Assert
            Assert.AreEqual(3, checkForExistingCalls.Count, "Invalid call count.");
            Assert.AreEqual(3, createNewMappingCalls.Count, "Invalid call count.");
            Assert.AreEqual(0, updateExistingMappingCalls.Count, "Invalid call count.");
            Assert.AreEqual(3, _getManifestDaypartByNameCalls.Count, "Invalid call count.");
            Assert.AreEqual(3, _deleteInventoryCalls.Count, "Invalid call count.");
            Assert.AreEqual(3, _createInventoryCalls.Count, "Invalid call count.");

            Assert.AreEqual(3, ingestedRecordsCount, "Only one record should be ingested.");
            Assert.AreEqual(3, updatedInventoryCount, "Only one inventory item should be updated.");

            var result = new
            {
                programMappingsToBeCreated,
                savedPrimaryPrograms
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
        }

        [Test]
        [Ignore("Test is failing. Need to investigate")]
        public void CanEnrichInventoryProgramIfDaypartHasNoPrograms()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto {
                    OriginalProgramName = "10 NEWS 6PM"
                    , OfficialProgramName = "10 NEWS"
                    , OfficialGenre = "News"
                    , OfficialShowType = "News"
                }
            };

            // Manifest Daypart has no Programs associated
            _InventoryRepositoryMock
                .Setup(s => s.GetDaypartProgramsForInventoryDayparts(It.IsAny<List<int>>(), It.IsAny<ProgramSourceEnum>()))
                .Returns(new List<StationInventoryManifestDaypartProgram>() {
                    new StationInventoryManifestDaypartProgram
                    {
                        Id = 1,
                        StationInventoryManifestDaypartId = 1
                    }
                });

            _ProgramMappingRepositoryMock
                .Setup(s => s.MappingExistsForOriginalProgramName(It.IsAny<string>()))
                .Returns(false);

            _ProgramMappingRepositoryMock
                .Setup(s => s.CreateProgramMapping(It.IsAny<ProgramMappingsDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(1);

            var ingestedRecordsCount = 0;
            var updatedInventoryCount = 0;
            var modifiedWhen = new DateTime(2020, 05, 13);
            var username = "testUser";

            // Act
            _ProgramMappingService.UT_ProcessProgramMappings(programMappings, modifiedWhen, username
                , ref updatedInventoryCount, ref ingestedRecordsCount);

            // Assert
            Assert.AreEqual(1, _getManifestDaypartByNameCalls.Count, "Invalid call count.");
            Assert.AreEqual(0, _deleteInventoryCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, _createInventoryCalls.Count, "Invalid call count.");

            Assert.AreEqual(1, ingestedRecordsCount, "Only one record should be ingested.");
            Assert.AreEqual(1, updatedInventoryCount, "Only one inventory item should be updated.");
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
