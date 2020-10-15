using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Configuration;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalUtilities.Reflection;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters.RateImport
{
    [TestFixture]
    public class BarterFileImporterUnitTests
    {
        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("WCBS-tv", "WCBS")]
        [TestCase("WCBS", "WCBS")]
        [TestCase("WCBS-TV", "WCBS")]
        [TestCase("WCBS-TV2", "WCBS")]
        [TestCase("WCBS-TV 123.4", "WCBS")]
        [TestCase("WCBS-DT", "WCBS")]
        [TestCase("WCBS-DT2", "WCBS")]
        [TestCase("WCBS-DT 123.4", "WCBS")]
        [TestCase("WCBS-LD", "WCBS")]
        [TestCase("WCBS-LD2", "WCBS")]
        [TestCase("WCBS-LD 123.4", "WCBS")]
        [TestCase("WCBS-CD", "WCBS")]
        [TestCase("WCBS-CD2", "WCBS")]
        [TestCase("WCBS-CD 123.4", "WCBS")]
        [TestCase("WCBS-LPTV", "WCBS")]
        [TestCase("WCBS-LPTV2", "WCBS")]
        [TestCase("WCBS-LPTV 123.4", "WCBS")]
        public void TransformStationCallsign(string rawCallsign, string expectedResult)
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();

            var fileImporter = new BarterFileImporter(
                dataRepositoryFactory.Object, null, null,
                null, null, null,
                null, null, null,
                null, null, null, null);

            var result = fileImporter._TransformStationCallsign(rawCallsign);

            if (expectedResult == null)
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.AreEqual(expectedResult, result);
            }
        }

        private readonly Mock<IDataRepositoryFactory> _BroadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
        private readonly Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCache = new Mock<IBroadcastAudiencesCache>();
        private readonly Mock<IInventoryDaypartParsingEngine> _InventoryDaypartParsingEngine = new Mock<IInventoryDaypartParsingEngine>();
        private readonly Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
        private readonly Mock<IStationProcessingEngine> _StationProcessingEngine = new Mock<IStationProcessingEngine>();
        private readonly Mock<ISpotLengthEngine> _SpotLengthEngine = new Mock<ISpotLengthEngine>();
        private readonly Mock<IFileService> _FileService = new Mock<IFileService>();
        private readonly Mock<IStationMappingService> _StationMappingService = new Mock<IStationMappingService>();
        private readonly Mock<IGenreCache> _GenreCache = new Mock<IGenreCache>();
        private readonly Mock<IDateTimeEngine> _DateTimeEngine = new Mock<IDateTimeEngine>();
        private readonly Mock<IStandardDaypartRepository> _StandardDaypartRepository = new Mock<IStandardDaypartRepository>();
        private readonly Mock<IInventoryProprietaryDaypartRepository> _InventoryProprietaryDaypartRepository = new Mock<IInventoryProprietaryDaypartRepository>();
        private readonly Mock<IShowTypeRepository> _ShowTypeRepository = new Mock<IShowTypeRepository>();

        private BarterFileImporter GetImporter()
        {
            _BroadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_StandardDaypartRepository.Object);

            _BroadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryProprietaryDaypartRepository>())
                .Returns(_InventoryProprietaryDaypartRepository.Object);

            _BroadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IShowTypeRepository>())
                .Returns(_ShowTypeRepository.Object);

            return new BarterFileImporter(
                _BroadcastDataRepositoryFactory.Object,
                _BroadcastAudiencesCache.Object,
                _InventoryDaypartParsingEngine.Object,
                _MediaMonthAndWeekAggregateCache.Object,
                _StationProcessingEngine.Object,
                _SpotLengthEngine.Object,
                null, // IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine
                null, // IImpressionsService impressionsService
                _FileService.Object,
                _StationMappingService.Object,
                _GenreCache.Object, // IGenreCache genreCache
                null, // IShowTypeCache showTypeCache
                _DateTimeEngine.Object); // IDateTimeEngine dateTimeEngine
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAndValidateHeaderData()
        {
            // Arrange
            const string inventorySourceName = "CNN";
            const string fileName = @"Barter_CNN.xlsx";

            var workbook = _LoadFromFile(fileName);
            var workSheet = workbook.Worksheets[1];
            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetInventorySource(inventorySourceName)
            };

            const bool daypartIsFound = true;
            _StandardDaypartRepository.Setup(s => s.StandardDaypartExists(It.IsAny<string>()))
                .Returns(daypartIsFound);

            _StandardDaypartRepository.Setup(s => s.GetStandardDaypartByCode(It.IsAny<string>()))
                .Returns(new StandardDaypartDto { Id = 2, Code = "ELN", FullName = "Early Morning News", VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.AM_NEWS });

            _InventoryProprietaryDaypartRepository.Setup(s =>
                    s.GetInventoryProprietaryDaypartMappings(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new InventoryProprietaryDaypartDto{ InventorySourceId = 1, DefaultDaypartId = 2, ProgramName = "News", GenreId = 33, ShowTypeId = 2});

            _BroadcastAudiencesCache.Setup(s => s.GetBroadcastAudienceByCode(It.IsAny<string>()))
                .Returns(new BroadcastAudience {Name = "Households", Id = 31});
            var outDd = new List<DisplayDaypart>
            {
                DaypartsTestData.GetAllDisplayDayparts().Values.First()
            };
            var isValidDaypartText = true;
            _InventoryDaypartParsingEngine.Setup(s => s.TryParse(It.IsAny<string>(), out outDd))
                .Returns(isValidDaypartText);

            var mediaMonth = new MediaMonth {Id = 471, Year = 2020, Month = 10, StartDate = new DateTime(2020, 9,28), EndDate = new DateTime(2020, 10,25)};
            _MediaMonthAndWeekAggregateCache.Setup(s => s.GetMediaMonthByYearAndMonth(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(mediaMonth);

            var fileImporter = GetImporter();

            // Act
            fileImporter.LoadAndValidateHeaderData(workSheet, proprietaryFile);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(proprietaryFile));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAndValidateHeaderDataWithValidationError()
        {
            // Arrange
            const string inventorySourceName = "CNN";
            const string fileName = @"Barter_CNN_InvalidDaypart.xlsx";

            var workbook = _LoadFromFile(fileName);
            var workSheet = workbook.Worksheets[1];
            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetInventorySource(inventorySourceName)
            };

            const bool daypartIsFound = false;
            _StandardDaypartRepository.Setup(s => s.StandardDaypartExists(It.IsAny<string>()))
                .Returns(daypartIsFound);

            _StandardDaypartRepository.Setup(s => s.GetStandardDaypartByCode(It.IsAny<string>()))
                .Returns((StandardDaypartDto)null);

            _InventoryProprietaryDaypartRepository.Setup(s =>
                    s.GetInventoryProprietaryDaypartMappings(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new InventoryProprietaryDaypartDto { InventorySourceId = 1, DefaultDaypartId = 2, ProgramName = "News", GenreId = 33, ShowTypeId = 2 });

            _BroadcastAudiencesCache.Setup(s => s.GetBroadcastAudienceByCode(It.IsAny<string>()))
                .Returns((BroadcastAudience)null);
            var outDd = new List<DisplayDaypart>
            {
                DaypartsTestData.GetAllDisplayDayparts().Values.First()
            };
            var isValidDaypartText = false;
            _InventoryDaypartParsingEngine.Setup(s => s.TryParse(It.IsAny<string>(), out outDd))
                .Returns(isValidDaypartText);

            MediaMonth mediaMonth = new MediaMonth { Id = 471, Year = 2020, Month = 10, StartDate = new DateTime(2020, 9, 28), EndDate = new DateTime(2020, 10, 25) };
            _MediaMonthAndWeekAggregateCache.Setup(s => s.GetMediaMonthByYearAndMonth(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(mediaMonth);

            var fileImporter = GetImporter();

            // Act
            fileImporter.LoadAndValidateHeaderData(workSheet, proprietaryFile);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(proprietaryFile));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAndValidateHeaderDataWithInvalidSourceDaypart()
        {
            // Arrange
            const string inventorySourceName = "CNN";
            const string fileName = @"Barter_CNN_InvalidDaypart.xlsx";

            var workbook = _LoadFromFile(fileName);
            var workSheet = workbook.Worksheets[1];
            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetInventorySource(inventorySourceName)
            };

            const bool daypartIsFound = true;
            _StandardDaypartRepository.Setup(s => s.StandardDaypartExists(It.IsAny<string>()))
                .Returns(daypartIsFound);

            _StandardDaypartRepository.Setup(s => s.GetStandardDaypartByCode(It.IsAny<string>()))
                .Returns(new StandardDaypartDto { Id = 2, Code = "ELN", FullName = "Early Morning News", VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.AM_NEWS });

            _InventoryProprietaryDaypartRepository.Setup(s =>
                    s.GetInventoryProprietaryDaypartMappings(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((InventoryProprietaryDaypartDto) null);

            _BroadcastAudiencesCache.Setup(s => s.GetBroadcastAudienceByCode(It.IsAny<string>()))
                .Returns(new BroadcastAudience { Name = "Households", Id = 31 });
            var outDd = new List<DisplayDaypart>
            {
                DaypartsTestData.GetAllDisplayDayparts().Values.First()
            };
            var isValidDaypartText = true;
            _InventoryDaypartParsingEngine.Setup(s => s.TryParse(It.IsAny<string>(), out outDd))
                .Returns(isValidDaypartText);

            var mediaMonth = new MediaMonth { Id = 471, Year = 2020, Month = 10, StartDate = new DateTime(2020, 9, 28), EndDate = new DateTime(2020, 10, 25) };
            _MediaMonthAndWeekAggregateCache.Setup(s => s.GetMediaMonthByYearAndMonth(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(mediaMonth);

            var fileImporter = GetImporter();

            // Act
            fileImporter.LoadAndValidateHeaderData(workSheet, proprietaryFile);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(proprietaryFile));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PopulateManifests()
        {
            // Arrange
            const string inventorySourceName = "CNN";
            const string fileName = @"Barter_CNN.xlsx";

            var dataLines = _GetDataLines();

            var fileHeader = new ProprietaryInventoryHeader
            {
                DaypartCode = "ELM",
                EffectiveDate = new DateTime(2020,10,5),
                EndDate = new DateTime(2020,10,18),
                Audience = new BroadcastAudience { Id = 33, Name = "House Hold"},
                Cpm = 12.3m
            };

            var proprietaryFile = new ProprietaryInventoryFile
            {
                Id  = 21,
                FileName = fileName,
                InventorySource = _GetBarterInventorySource(inventorySourceName),
                Header = fileHeader,
                DataLines = dataLines
            };
            var stationsParam = new List<DisplayBroadcastStation>();

            StandardDaypartDto standardDaypart = new StandardDaypartDto
            {
                Id = 2,
                Code = "ELM",
                FullName = "Early Morning",
                VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS
            };
            _StandardDaypartRepository.Setup(s => s.GetStandardDaypartByCode(It.IsAny<string>()))
                .Returns(standardDaypart);

            InventoryProprietaryDaypartDto inventoryProprietaryDaypartDto = new InventoryProprietaryDaypartDto
            {
                ProgramName = "Early Morning",
                DefaultDaypartId = 2,
                InventorySourceId = 5,
                GenreId = 5,
                ShowTypeId = 1
            };

            _InventoryProprietaryDaypartRepository.Setup(s =>
                    s.GetInventoryProprietaryDaypartMappings(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(inventoryProprietaryDaypartDto);

            _GenreCache.Setup(s => s.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto {Id = 5, Display = "Genre5"});

            _GenreCache.Setup(s => s.GetSourceGenreLookupDtoByName(It.IsAny<string>(), It.IsAny<ProgramSourceEnum>()))
                .Returns(new LookupDto { Id = 6, Display = "Genre6" });

            _ShowTypeRepository.Setup(s => s.GetMaestroShowType(It.IsAny<int>()))
                .Returns(new ShowTypeDto { Id = 1, Name = "Series", ShowTypeSource = ProgramSourceEnum.Maestro});

            _StationMappingService.Setup(s => s.GetStationByCallLetters(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((c,t) => new DisplayBroadcastStation { LegacyCallLetters = c });

            _SpotLengthEngine.Setup(s => s.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns(3);

            _MediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksIntersecting(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaWeek> { new MediaWeek(), new MediaWeek() });

            _DateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(new DateTime(2020,10,17, 7, 34, 12));

            var fileImporter = GetImporter();

            // Act
            fileImporter.PopulateManifests(proprietaryFile, stationsParam);
            var result = proprietaryFile.InventoryGroups;

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void PopulateManifestsWithoutSourceDaypartProgramsMappings()
        {
            // Arrange
            const string inventorySourceName = "MLB";
            const string fileName = @"Barter_Template.xlsx";

            var dataLines = _GetDataLines();

            var fileHeader = new ProprietaryInventoryHeader
            {
                DaypartCode = "ELM",
                EffectiveDate = new DateTime(2020, 10, 5),
                EndDate = new DateTime(2020, 10, 18),
                Audience = new BroadcastAudience { Id = 33, Name = "House Hold" },
                Cpm = 12.3m
            };

            var proprietaryFile = new ProprietaryInventoryFile
            {
                Id = 21,
                FileName = fileName,
                InventorySource = _GetBarterInventorySource(inventorySourceName),
                Header = fileHeader,
                DataLines = dataLines
            };
            var stationsParam = new List<DisplayBroadcastStation>();

            StandardDaypartDto standardDaypart = new StandardDaypartDto
            {
                Id = 2,
                Code = "ELM",
                FullName = "Early Morning",
                VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS
            };
            _StandardDaypartRepository.Setup(s => s.GetStandardDaypartByCode(It.IsAny<string>()))
                .Returns(standardDaypart);

            InventoryProprietaryDaypartDto inventoryProprietaryDaypartDto = null;
            _InventoryProprietaryDaypartRepository.Setup(s =>
                    s.GetInventoryProprietaryDaypartMappings(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(inventoryProprietaryDaypartDto);

            _GenreCache.Setup(s => s.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto { Id = 5, Display = "Genre5" });

            _GenreCache.Setup(s => s.GetSourceGenreLookupDtoByName(It.IsAny<string>(), It.IsAny<ProgramSourceEnum>()))
                .Returns(new LookupDto { Id = 6, Display = "Genre6" });

            _ShowTypeRepository.Setup(s => s.GetMaestroShowType(It.IsAny<int>()))
                .Returns(new ShowTypeDto { Id = 1, Name = "Series", ShowTypeSource = ProgramSourceEnum.Maestro });

            _StationMappingService.Setup(s => s.GetStationByCallLetters(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((c, t) => new DisplayBroadcastStation { LegacyCallLetters = c });

            _SpotLengthEngine.Setup(s => s.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns(3);

            _MediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksIntersecting(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaWeek> { new MediaWeek(), new MediaWeek() });

            _DateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(new DateTime(2020, 10, 17, 7, 34, 12));

            var fileImporter = GetImporter();

            // Act AND Assert
            Assert.Throws<ApplicationException>(() => fileImporter.PopulateManifests(proprietaryFile, stationsParam));
        }

        private List<ProprietaryInventoryFile.ProprietaryInventoryDataLine> _GetDataLines()
        {
            var lines = new List<ProprietaryInventoryFile.ProprietaryInventoryDataLine>
            {
                new ProprietaryInventoryFile.ProprietaryInventoryDataLine
                {
                    Units = new List<ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit>
                    {
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM1",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM2",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM3",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM4",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM5",
                                SpotLength = 30
                            },
                            Spots = 1
                        }
                    },
                    Audiences = new List<ProprietaryInventoryFile.ProprietaryInventoryDataLine.LineAudience>(),
                    Dayparts = new List<DisplayDaypart>
                    {
                        new DisplayDaypart
                        {
                            Id = 6724,
                             StartTime = 32400, // 9AM
                             EndTime = 35999, // 10AM
                             Monday = true,
                             Tuesday = true,
                             Wednesday = true,
                             Thursday = true,
                             Friday = true,
                             Saturday = false,
                             Sunday = false
                        }
                    },
                    Station = "WWTV"
                },
                new ProprietaryInventoryFile.ProprietaryInventoryDataLine
                {
                    Units = new List<ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit>
                    {
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM1",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM2",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM3",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM4",
                                SpotLength = 30
                            },
                            Spots = 1
                        },
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = new ProprietaryInventoryUnit
                            {
                                Name = "AM5",
                                SpotLength = 30
                            },
                            Spots = 1
                        }
                    },
                    Audiences = new List<ProprietaryInventoryFile.ProprietaryInventoryDataLine.LineAudience>(),
                    Dayparts = new List<DisplayDaypart>
                    {
                        new DisplayDaypart
                        {
                            Id = 6724,
                             StartTime = 32400, // 9AM
                             EndTime = 35999, // 10AM
                             Monday = true,
                             Tuesday = true,
                             Wednesday = true,
                             Thursday = true,
                             Friday = true,
                             Saturday = false,
                             Sunday = false
                        }
                    },
                    Station = "EESH"
                }
            };
            return lines;
        }

        private InventorySource _GetBarterInventorySource(string name)
        {
            return new InventorySource
            {
                Id = 1,
                InventoryType = InventorySourceTypeEnum.Barter,
                Name = name,
                IsActive = true
            };
        }

        private ExcelWorkbook _LoadFromFile(string fileName)
        {
            var filePath = $@".\Files\ProprietaryDataFiles\{fileName}";
            var filePackage = new ExcelPackage(new FileInfo(filePath));
            var workbook = filePackage.Workbook;
            return workbook;
        }

        private InventorySource _GetInventorySource(string name)
        {
            return new InventorySource
            {
                Id = 1,
                InventoryType = InventorySourceTypeEnum.Diginet,
                Name = name,
                IsActive = true
            };
        }
    }
}