using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Hangfire;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class ProgramMappingServiceUnitTests
    {
        private ProgramMappingService _ProgramMappingService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IBackgroundJobClient> _BackgroundJobClientMock;
        private Mock<IProgramMappingRepository> _ProgramMappingRepositoryMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IShowTypeRepository> _ShowTypeRepositoryMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;
        private Mock<IProgramNameExceptionsRepository> _ProgramNameExceptionRepositoryMock;
        private Mock<IProgramMappingCleanupEngine> _ProgramMappingCleanupEngine;
        private Mock<IProgramNameMappingKeywordRepository> _ProgramNameMappingKeywordRepositoryMock;
        private Mock<IMasterProgramListImporter> _MasterListImporterMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;

        private Mock<IGenreCache> _GenreCache;
        private IShowTypeCache _ShowTypeCacheStub;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
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
            _GenreCache = new Mock<IGenreCache>();
            _ShowTypeCacheStub = new ShowTypeCacheStub();
            _ProgramNameExceptionRepositoryMock = new Mock<IProgramNameExceptionsRepository>();
            _ProgramMappingCleanupEngine = new Mock<IProgramMappingCleanupEngine>();
            _ProgramNameMappingKeywordRepositoryMock = new Mock<IProgramNameMappingKeywordRepository>();
            _MasterListImporterMock = new Mock<IMasterProgramListImporter>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            // Setup common mocks
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IProgramMappingRepository>())
                .Returns(_ProgramMappingRepositoryMock.Object);
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IProgramNameExceptionsRepository>())
                .Returns(_ProgramNameExceptionRepositoryMock.Object);
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);
            _DataRepositoryFactoryMock.Setup(s => s.GetDataRepository<IProgramNameMappingKeywordRepository>())
                .Returns(_ProgramNameMappingKeywordRepositoryMock.Object);

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
                .Setup(s => s.GetMaestroShowTypeByName(It.IsAny<string>()))
                .Returns((string showTypeName) =>
                {
                    return new ShowTypeDto
                    {
                        Id = 1,
                        Name = showTypeName
                    };
                });
            _ShowTypeCacheStub.GetMaestroShowTypeLookupDtoByName("Mini-Movie");

            _ProgramNameMappingKeywordRepositoryMock.Setup(k => k.GetProgramNameMappingKeywords()).Returns(new List<ProgramNameMappingKeyword>
            {
                new ProgramNameMappingKeyword{
                     Genre = new LookupDto(44, "Sports"),
                     ShowType = new LookupDto(44, "Sports"),
                     ProgramName = "Golf",
                     Keyword = "golf"
                },
                new ProgramNameMappingKeyword{
                     Genre = new LookupDto(44, "Sports"),
                     ShowType = new LookupDto(44, "Sports"),
                     ProgramName = "NFL",
                     Keyword = "NFL"
                }
            });

            _LaunchDarklyClientStub = new LaunchDarklyClientStub();

            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

            // Setup the actual Program Mapping Service
            _ProgramMappingService = new ProgramMappingService(
                _BackgroundJobClientMock.Object,
                _DataRepositoryFactoryMock.Object,
                _SharedFolderServiceMock.Object,
                null,
                _GenreCache.Object,
                _ShowTypeCacheStub,
                _ProgramMappingCleanupEngine.Object,
                _MasterListImporterMock.Object,
                _DateTimeEngineMock.Object,
                featureToggleHelper, _ConfigurationSettingsHelperMock.Object);
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
            var programExceptions = new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                    CustomProgramName = "10 NEWS", ShowTypeName = "News", GenreName = "News"
                }
            };
            _ProgramNameExceptionRepositoryMock
                .Setup(s => s.GetProgramExceptions())
                .Returns(programExceptions);
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
            _ProgramMappingService._ProcessProgramMappings(programMappings, modifiedWhen, username);

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
            var programExceptions = new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                    CustomProgramName = "10 NEWS", ShowTypeName = "News", GenreName = "News"
                }
            };
            _ProgramNameExceptionRepositoryMock
                .Setup(s => s.GetProgramExceptions())
                .Returns(programExceptions);
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
            _ProgramMappingService._ProcessProgramMappings(programMappings, modifiedWhen, username);

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
            var programExceptions = new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                    CustomProgramName = "10 NEWS", ShowTypeName = "News", GenreName = "News"
                }
            };
            _ProgramNameExceptionRepositoryMock
                .Setup(s => s.GetProgramExceptions())
                .Returns(programExceptions);
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
            _ProgramMappingService._ProcessProgramMappings(programMappings, modifiedWhen, username);

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

            var sut = new ProgramMappingService(null, broadcastDataRepositoryFactory.Object, null, null, null, null, null, null, null, null, _ConfigurationSettingsHelperMock.Object);

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
        [Test]
        public void LoadShowTypeTest_ExceptionPopulateShowType()
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto
                {
                    OriginalProgramName = "10 NEWS 6PM", OfficialProgramName = "10 NEWS", OfficialGenre = "News",
                    OfficialShowType = "NA"
                }
            };
            var programExceptions = new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                    CustomProgramName = "10 NEWS", ShowTypeName = "NewsException", GenreName = "News"
                }
            };
            _ProgramNameExceptionRepositoryMock
                .Setup(s => s.GetProgramExceptions())
                .Returns(programExceptions);

            // Act
            _ProgramMappingService._LoadShowTypes(programMappings);


            // Assert
            Assert.IsNotNull(programMappings);
            Assert.AreEqual("NewsException", programMappings.FirstOrDefault().OfficialShowType);
        }

        [TestCase("ABC", "FileShowType", "FileShowType")]
        [TestCase("ABC", "", "Miscellaneous")]
        public void LoadShowTypeTest_EnableInternalProgramSearch_PopulateShowType_FromFile(string officialProgramName, string officialShowType, string expected)
        {
            // Arrange
            var programMappings = new List<ProgramMappingsFileRequestDto>
            {
                new ProgramMappingsFileRequestDto
                {
                    OfficialProgramName = officialProgramName, OfficialGenre = "News", OfficialShowType = officialShowType
                }
            };

            var programExceptions = new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                    CustomProgramName = "10 NEWS", ShowTypeName = "News", GenreName = "News"
                }
            };

            _ProgramNameExceptionRepositoryMock
                .Setup(s => s.GetProgramExceptions())
                .Returns(programExceptions);

            // Act
            _ProgramMappingService._LoadShowTypes(programMappings);


            // Assert
            Assert.IsNotNull(programMappings);
            Assert.AreEqual(expected, programMappings.FirstOrDefault().OfficialShowType);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunProgramMappingsProcessingJob()
        {
            // Get mapping programs result
            var programsMapped = new List<ProgramMappingsDto>();
            _ProgramMappingRepositoryMock.Setup(p => p.UpdateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            _ProgramMappingRepositoryMock.Setup(p => p.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            // Import file
            var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsShowType.xlsx", FileMode.Open);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = Path.GetTempPath(),
                FileNameWithExtension = "ProgramMappings.xlsx",
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.ProgramLineup,
                CreatedDate = new DateTime(2020, 8, 28),
                CreatedBy = "IntegrationTestUser",
                FileContent = fileStream
            };
            _SharedFolderServiceMock.Setup(s => s.GetFile(It.IsAny<Guid>())).Returns(sharedFolderFile);

            _ProgramMappingCleanupEngine.Setup(s => s.InvertPrepositions(It.IsAny<string>())).Returns((string x) => { return x; });

            // Master list
            _MasterListImporterMock.Setup(m => m.ImportMasterProgramList()).Returns(new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Sports" },
                     OfficialShowType = new ShowTypeDto{ Name = "Sports"},
                     OfficialProgramName = "Good Morning NFL"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Series"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Special"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre{Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{Name = "Event"},
                    OfficialProgramName = "America Undercover"
                }
            });

            _ProgramNameExceptionRepositoryMock.Setup(e => e.GetProgramExceptions()).Returns(new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "The Boys",
                     GenreName = "Action",
                     ShowTypeName = "Series"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Mini-Movie"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Special"
                }
            });

            _GenreCache.Setup(s => s.GetMaestroGenreByName(It.IsAny<string>())).Returns<string>(s => new Genre { Id = 1, Name = s, ProgramSourceId = 1 });
            _ProgramMappingRepositoryMock.Setup(r => r.GetProgramMappingsByOriginalProgramNames(It.IsAny<IEnumerable<string>>())).Returns(new List<ProgramMappingsDto>());

            _ProgramMappingService.RunProgramMappingsProcessingJob(Guid.NewGuid(), "Unit Tests", DateTime.Now);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsMapped, _GetJsonSettings()));
            fileStream.Close();
        }      

        [Test]
        public void ProgramNotFound()
        {
            // Get mapping programs result
            var programsMapped = new List<ProgramMappingsDto>();
            var Genre = new Genre { Id = 1, Name = "Drama", ProgramSourceId = 1 };

            _ProgramMappingRepositoryMock.Setup(p => p.UpdateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            _ProgramMappingRepositoryMock.Setup(p => p.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            // Import file
            var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsShowType.xlsx", FileMode.Open);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = Path.GetTempPath(),
                FileNameWithExtension = "ProgramMappings.xlsx",
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.ProgramLineup,
                CreatedDate = new DateTime(2020, 8, 28),
                CreatedBy = "IntegrationTestUser",
                FileContent = fileStream
            };
            _SharedFolderServiceMock.Setup(s => s.GetFile(It.IsAny<Guid>())).Returns(sharedFolderFile);

            _ProgramMappingCleanupEngine.Setup(s => s.InvertPrepositions(It.IsAny<string>())).Returns((string x) => { return x; });

            // Master list
            _MasterListImporterMock.Setup(m => m.ImportMasterProgramList()).Returns(new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Series"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Special"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre{Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{Name = "Event"},
                    OfficialProgramName = "America Undercover"
                }
            });

            _ProgramNameExceptionRepositoryMock.Setup(e => e.GetProgramExceptions()).Returns(new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "The Boys",
                     GenreName = "Action",
                     ShowTypeName = "Series"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Mini-Movie"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Special"
                }
            });

            _GenreCache.Setup(s => s.GetMaestroGenreByName(It.IsAny<string>())).Returns<string>(s => new Genre { Name = s });
            _ProgramMappingRepositoryMock.Setup(r => r.GetProgramMappingsByOriginalProgramNames(It.IsAny<IEnumerable<string>>())).Returns(new List<ProgramMappingsDto>());

            var exception = Assert.Throws<InvalidOperationException>(() => _ProgramMappingService.RunProgramMappingsProcessingJob(Guid.NewGuid(), "Unit Tests", DateTime.Now));
            Assert.That(exception.Message, Is.EqualTo("Error parsing program 'Good Morning NFL': Mapping Program not found in master list or exception list.; MetaData=Good Morning NFL|Good Morning NFL|Sports;\r\n"));
            fileStream.Close();
        }        


        private List<SearchProgramDativaResponseDto> _Programs = new List<SearchProgramDativaResponseDto>
        {
            new SearchProgramDativaResponseDto
            {
                ProgramId = "1",
                ProgramName = "Star Trek Series",
                Genre = "Non-News",
                ShowType = "Series"

            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "2",
                ProgramName = "Star Trek",
                Genre = "Non-News",
                ShowType = "Movies"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "3",
                ProgramName = "10",
                GenreId = "33",
                Genre = "News",
                ShowType = "APIExactMatch"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "4",
                ProgramName = "ABC",
                Genre = "Crime",
                ShowType = "Movie"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "5",
                ProgramName = "1917",
                GenreId = "3",
                Genre = "Drama",
                ShowType = "Movie",
                MpaaRating = "R",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "6",
                ProgramName = "Mr. Jones",
                GenreId = "3",
                Genre = "Drama",
                ShowType = "Movie",
                MpaaRating = "R",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "7",
                ProgramName = "black-ish",
                GenreId = "8",
                Genre = "comedy",
                ShowType = "Series",
                MpaaRating = "",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "8",
                ProgramName = "black-ish",
                GenreId = "8",
                Genre = "comedy",
                ShowType = "Series",
                MpaaRating = "",
                SyndicationType = "Non-Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "9",
                ProgramName = "Star",
                GenreId = "33",
                Genre = "Non-News",
                ShowType = "APIExactMatch"
            },
        };
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadProgramsFromBroadcastOps()
        {
            // Get mapping programs result
            var masterlist = new List<MasterProgramsDto>
            {
                new MasterProgramsDto
                {
                    Name = "Good Morning NFL",
                    ShowTypeId = 1,
                    GenreId = 8
                },
                new MasterProgramsDto
                {
                    Name = "Breaking Bad",
                    ShowTypeId = 1,
                    GenreId = 8
                }
            };
            var programsMapped = new List<ProgramMappingsDto>();
            _ProgramMappingRepositoryMock.Setup(p => p.GetMasterPrograms())
                .Returns(masterlist);

            _ProgramMappingRepositoryMock.Setup(p => p.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            // Import file
            var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsShowType.xlsx", FileMode.Open);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = Path.GetTempPath(),
                FileNameWithExtension = "ProgramMappings.xlsx",
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.ProgramLineup,
                CreatedDate = new DateTime(2020, 8, 28),
                CreatedBy = "IntegrationTestUser",
                FileContent = fileStream
            };
            _SharedFolderServiceMock.Setup(s => s.GetFile(It.IsAny<Guid>())).Returns(sharedFolderFile);

            _ProgramMappingCleanupEngine.Setup(s => s.InvertPrepositions(It.IsAny<string>())).Returns((string x) => { return x; });
            var distinctMasterList =
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre { Name = "Drama" },
                    OfficialShowType = new ShowTypeDto { Name = "Event" },
                    OfficialProgramName = "America Undercover"
                };

            // Master list
            _MasterListImporterMock.Setup(m => m.UploadMasterProgramList(fileStream)).Returns(new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Sports" },
                     OfficialShowType = new ShowTypeDto{ Name = "Sports"},
                     OfficialProgramName = "Good Morning NFL"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Series"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Special"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre{Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{Name = "Event"},
                    OfficialProgramName = "America Undercover"
                }
            });

            _ProgramNameExceptionRepositoryMock.Setup(e => e.GetProgramExceptions()).Returns(new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "The Boys",
                     GenreName = "Action",
                     ShowTypeName = "Series"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Mini-Movie"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Special"
                }
            });
            _ProgramMappingRepositoryMock.Setup(e => e.UploadMasterProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()));
            _GenreCache.Setup(s => s.GetMaestroGenreByName(It.IsAny<string>())).Returns<string>(s => new Genre { Id = 1, Name = s, ProgramSourceId = 1 });
            _ProgramMappingRepositoryMock.Setup(r => r.GetProgramMappingsByOriginalProgramNames(It.IsAny<IEnumerable<string>>())).Returns(new List<ProgramMappingsDto>());

            _ProgramMappingService.UploadProgramsFromBroadcastOps(fileStream, "Unit Tests", "Test User", DateTime.Now);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsMapped, _GetJsonSettings()));
            fileStream.Close();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadProgramsFromBroadcastOps_ToggleOn()
        {
            // Get mapping programs result
            var masterlist = new List<MasterProgramsDto>
            {
                new MasterProgramsDto
                {
                    Name = "Good Morning NFL",
                    ShowTypeId = 1,
                    GenreId = 8
                },
                new MasterProgramsDto
                {
                    Name = "Breaking Bad",
                    ShowTypeId = 1,
                    GenreId = 8
                }
            };
            var programsMapped = new List<ProgramMappingsDto>();
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PROGRAM_GENRE_RELATION_V_2] = true;
            _ProgramMappingRepositoryMock.Setup(p => p.GetMasterPrograms())
                .Returns(masterlist);

            _ProgramMappingRepositoryMock.Setup(p => p.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            // Import file
            var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsShowType.xlsx", FileMode.Open);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = Path.GetTempPath(),
                FileNameWithExtension = "ProgramMappings.xlsx",
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.ProgramLineup,
                CreatedDate = new DateTime(2020, 8, 28),
                CreatedBy = "IntegrationTestUser",
                FileContent = fileStream
            };
            _SharedFolderServiceMock.Setup(s => s.GetFile(It.IsAny<Guid>())).Returns(sharedFolderFile);

            _ProgramMappingCleanupEngine.Setup(s => s.InvertPrepositions(It.IsAny<string>())).Returns((string x) => { return x; });
            var distinctMasterList =
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre { Name = "Drama" },
                    OfficialShowType = new ShowTypeDto { Name = "Event" },
                    OfficialProgramName = "America Undercover"
                };

            // Master list
            _MasterListImporterMock.Setup(m => m.UploadMasterProgramList(fileStream)).Returns(new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Sports" },
                     OfficialShowType = new ShowTypeDto{ Name = "Sports"},
                     OfficialProgramName = "Good Morning NFL"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Series"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Special"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre{Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{Name = "Event"},
                    OfficialProgramName = "America Undercover"
                }
            });

            _ProgramNameExceptionRepositoryMock.Setup(e => e.GetProgramExceptions()).Returns(new List<ProgramNameExceptionDto>
            {
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "The Boys",
                     GenreName = "Action",
                     ShowTypeName = "Series"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Mini-Movie"
                },
                new ProgramNameExceptionDto
                {
                     CustomProgramName = "Community",
                     GenreName = "Comedy",
                     ShowTypeName = "Special"
                }
            });
            _ProgramMappingRepositoryMock.Setup(e => e.UploadMasterProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()));
            _GenreCache.Setup(s => s.GetMaestroGenreByName(It.IsAny<string>())).Returns<string>(s => new Genre { Id = 1, Name = s, ProgramSourceId = 1 });
            _ProgramMappingRepositoryMock.Setup(r => r.GetProgramMappingsByOriginalProgramNames(It.IsAny<IEnumerable<string>>())).Returns(new List<ProgramMappingsDto>());

            _ProgramMappingService.UploadProgramsFromBroadcastOps(fileStream, "Unit Tests", "Test User", DateTime.Now);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsMapped, _GetJsonSettings()));
            fileStream.Close();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadPrograms()
        {
            // Get mapping programs result
            var masterlist = new List<MasterProgramsDto>
            {
                new MasterProgramsDto
                {
                    Name = "Good Morning NFL",
                    ShowTypeId = 1,
                    GenreId = 8
                },
                new MasterProgramsDto
                {
                    Name = "Breaking Bad",
                    ShowTypeId = 1,
                    GenreId = 8
                }
            };
            var programsMapped = new List<ProgramMappingsDto>();
            _ProgramMappingRepositoryMock.Setup(p => p.GetMasterPrograms())
                .Returns(masterlist);

            _ProgramMappingRepositoryMock.Setup(p => p.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            // Import file
            var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsShowType.xlsx", FileMode.Open);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = Path.GetTempPath(),
                FileNameWithExtension = "ProgramMappings.xlsx",
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.ProgramLineup,
                CreatedDate = new DateTime(2020, 8, 28),
                CreatedBy = "IntegrationTestUser",
                FileContent = fileStream
            };
            _SharedFolderServiceMock.Setup(s => s.GetFile(It.IsAny<Guid>())).Returns(sharedFolderFile);          

            // Master list
            _MasterListImporterMock.Setup(m => m.ParseProgramGenresExcelFile(fileStream)).Returns(new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Sports" },
                     OfficialShowType = new ShowTypeDto{ Name = "Sports"},
                     OfficialProgramName = "Good Morning NFL"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Series"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Special"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre{Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{Name = "Event"},
                    OfficialProgramName = "America Undercover"
                }
            });
            
            _ProgramMappingRepositoryMock.Setup(e => e.UploadMasterProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()));          

            _ProgramMappingService.UploadPrograms(fileStream, "Unit Tests", "Test User", DateTime.Now); 

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsMapped, _GetJsonSettings()));
            fileStream.Close();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadPrograms_ToggleOn()
        {
            // Get mapping programs result
            var masterlist = new List<MasterProgramsDto>
            {
                new MasterProgramsDto
                {
                    Name = "Good Morning NFL",
                    ShowTypeId = 1,
                    GenreId = 8
                },
                new MasterProgramsDto
                {
                    Name = "Breaking Bad",
                    ShowTypeId = 1,
                    GenreId = 8
                }
            };
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PROGRAM_GENRE_RELATION_V_2] = true;
            var programsMapped = new List<ProgramMappingsDto>();
            _ProgramMappingRepositoryMock.Setup(p => p.GetMasterPrograms())
                .Returns(masterlist);

            _ProgramMappingRepositoryMock.Setup(p => p.CreateProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<IEnumerable<ProgramMappingsDto>, string, DateTime>((p, s, d) => programsMapped.AddRange(p));

            // Import file
            var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsShowType.xlsx", FileMode.Open);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = Path.GetTempPath(),
                FileNameWithExtension = "ProgramMappings.xlsx",
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.ProgramLineup,
                CreatedDate = new DateTime(2020, 8, 28),
                CreatedBy = "IntegrationTestUser",
                FileContent = fileStream
            };
            _SharedFolderServiceMock.Setup(s => s.GetFile(It.IsAny<Guid>())).Returns(sharedFolderFile);

            // Master list
            _MasterListImporterMock.Setup(m => m.ParseProgramGenresExcelFile(fileStream)).Returns(new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Sports" },
                     OfficialShowType = new ShowTypeDto{ Name = "Sports"},
                     OfficialProgramName = "Good Morning NFL"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Series"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Drama" },
                     OfficialShowType = new ShowTypeDto{ Name = "Special"},
                     OfficialProgramName = "Breaking Bad"
                },
                new ProgramMappingsDto
                {
                    OfficialGenre = new Genre{Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{Name = "Event"},
                    OfficialProgramName = "America Undercover"
                }
            });

            _ProgramMappingRepositoryMock.Setup(e => e.UploadMasterProgramMappings(It.IsAny<IEnumerable<ProgramMappingsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()));

            _ProgramMappingService.UploadPrograms(fileStream, "Unit Tests", "Test User", DateTime.Now);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsMapped, _GetJsonSettings()));
            fileStream.Close();
        }
    }
}

