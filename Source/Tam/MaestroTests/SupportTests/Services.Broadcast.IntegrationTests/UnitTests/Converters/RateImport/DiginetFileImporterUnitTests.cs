using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters.RateImport
{
    /// <summary>
    /// Test the functionality of the <see cref="DiginetFileImporter"/>
    /// </summary>
    [TestFixture]
    [Category("short_running")]
    public class DiginetFileImporterUnitTests
    {
        // worksheets are 1-indexed
        private const int SECOND_WORKSHEET_INDEX = 2;

        private readonly Mock<IDataRepositoryFactory> _BroadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
        private readonly Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCache = new Mock<IBroadcastAudiencesCache>();
        private readonly Mock<IInventoryDaypartParsingEngine> _InventoryDaypartParsingEngine = new Mock<IInventoryDaypartParsingEngine>();
        private readonly Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
        private readonly Mock<IStationProcessingEngine> _StationProcessingEngine = new Mock<IStationProcessingEngine>();
        private readonly Mock<ISpotLengthEngine> _SpotLengthEngine = new Mock<ISpotLengthEngine>();
        private readonly Mock<IImpressionAdjustmentEngine> _ImpressionAdjustmentEngine = new Mock<IImpressionAdjustmentEngine>();
        private readonly Mock<IFileService> _FileService = new Mock<IFileService>();

        private readonly Mock<IStandardDaypartRepository> _StandardDaypartRepository = new Mock<IStandardDaypartRepository>();
        private readonly Mock<IStationMappingService> _StationMappingService = new Mock<IStationMappingService>();

        private DiginetFileImporter _GetDiginetFileImporter()
        {
            _BroadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_StandardDaypartRepository.Object);

            return new DiginetFileImporter(
                _BroadcastDataRepositoryFactory.Object,
                _BroadcastAudiencesCache.Object,
                _InventoryDaypartParsingEngine.Object,
                _MediaMonthAndWeekAggregateCache.Object,
                _StationProcessingEngine.Object,
                _SpotLengthEngine.Object,
                _ImpressionAdjustmentEngine.Object,
                _FileService.Object,
                _StationMappingService.Object);
        }

        /// <summary>
        /// Test that the indicator is set correctly.
        /// </summary>
        /// <remarks>
        ///     Expected Result is True.
        /// </remarks>
        [Test]
        public void HasSecondWorksheet()
        {
            // Arrange
            var testClass = _GetDiginetFileImporter();

            // Act
            var result = testClass.HasSecondWorksheet;

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test the loading of the second sheet.
        /// </summary>
        /// <remarks>
        ///     Scenario : Loads some stations without issues.
        ///     Expected :
        ///         - 111 stations should be ingested for the inventory source.
        ///         - No validation problems.
        /// </remarks>
        [Test]
        public void LoadAndValidateSecondWorksheet()
        {
            // Arrange
            const string inventorySourceName = "HITV";
            const int expectedStationCount = 111; 

            const string fileName = @"Diginet_Template.xlsx";
            var templateWorkBook = _LoadFromFile(fileName);

            var secondWorksheet = templateWorkBook.Worksheets[SECOND_WORKSHEET_INDEX];
            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetDiginetInventorySource(inventorySourceName)
            };

            var importer = _GetDiginetFileImporter();
            importer.InventorySourceName = inventorySourceName;

            // Act
            importer.LoadAndValidateSecondWorksheet(secondWorksheet, proprietaryFile);

            // Assert
            Assert.AreEqual(expectedStationCount, importer.FileStationsCallsigns.Count);
            Assert.IsFalse(proprietaryFile.ValidationProblems.Any());
        }

        /// <summary>
        /// Test the loading of the second sheet.
        /// </summary>
        /// <remarks>
        ///     Scenario : The inventory source yields no stations.
        ///     Expected :
        ///         - 0 stations should be ingested for the inventory source.
        ///         - It should report stations weren't loaded
        /// </remarks>
        [Test]
        public void LoadAndValidateSecondWorksheet_NoStationsLoaded()
        {
            // Arrange
            const string inventorySourceName = "NotADiginetSource";
            const int expectedStationCount = 0;

            const string fileName = @"Diginet_Template.xlsx";
            var templateWorkBook = _LoadFromFile(fileName);

            var secondWorksheet = templateWorkBook.Worksheets[SECOND_WORKSHEET_INDEX];
            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetDiginetInventorySource(inventorySourceName)
            };

            var importer = _GetDiginetFileImporter();
            importer.InventorySourceName = inventorySourceName;

            // Act
            importer.LoadAndValidateSecondWorksheet(secondWorksheet, proprietaryFile);

            // Assert
            Assert.AreEqual(expectedStationCount, importer.FileStationsCallsigns.Count);
            Assert.AreEqual(1, proprietaryFile.ValidationProblems.Count(s => s.Contains("tab didn't produce station information")));
        }

        /// <summary>
        /// Test the loading of the second sheet.
        /// </summary>
        /// <remarks>
        ///     Scenario : The worksheet is not the stations worksheet.
        ///     Expected :
        ///         - 0 stations should be ingested
        ///         - It should be reported
        /// </remarks>
        [Test]
        public void LoadAndValidateSecondWorksheet_NotTheStationsWorksheet()
        {
            // Arrange
            const string inventorySourceName = "HITV";
            const string fileName = @"Diginet_Template.xlsx";
            var templateWorkBook = _LoadFromFile(fileName);
            
            var secondWorksheet = templateWorkBook.Worksheets[SECOND_WORKSHEET_INDEX];
            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetDiginetInventorySource(inventorySourceName)
            };

            var importer = _GetDiginetFileImporter();
            secondWorksheet.Name = "IncorrectName";

            // Act
            importer.LoadAndValidateSecondWorksheet(secondWorksheet, proprietaryFile);

            // Assert
            Assert.AreEqual(0, importer.FileStationsCallsigns.Count);
            Assert.AreEqual(1, proprietaryFile.ValidationProblems.Count(s => s.Contains("template that requires a second tab named 'Diginet Stations'")));
        }

        /// <summary>
        /// Test manifests are resolving the mapped stations correctly.
        /// </summary>
        /// <remarks>
        ///     Scenario : The stations are found.
        ///     Expected : The manifest should be populated
        /// </remarks>
        [Test]
        public void PopulateManifests_StationMappings()
        {
            /*** Arrange ***/
            const string inventorySourceName = "HITV";
            const string fileName = @"Diginet_Template.xlsx";

            var dataLines = _GetDataLines();

            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetDiginetInventorySource(inventorySourceName),
                Header = new ProprietaryInventoryHeader { NtiToNsiIncrease = 25.123m },
                DataLines = dataLines
            };
            var stationsParam = new List<DisplayBroadcastStation>();

            _SpotLengthEngine.Setup(s => s.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns(3);

            var standardDayparts = _GetStandardDayparts();
            _StandardDaypartRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(standardDayparts);

            _ImpressionAdjustmentEngine.Setup(s =>
                    s.ConvertNtiImpressionsToNsi(It.IsAny<double>(), It.IsAny<double>()))
                .Returns(1234.5d);

            _MediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksIntersecting(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaWeek> {new MediaWeek(), new MediaWeek()});

            var getMappedCallLettersCalls = new List<string>();
            _StationMappingService.Setup(s => s.GetStationByCallLetters(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>((mappedCallsign, throwsIfNotFound) => getMappedCallLettersCalls.Add(mappedCallsign))
                .Returns(new DisplayBroadcastStation { LegacyCallLetters = "CadentStationCallsign" });

            var importer = _GetDiginetFileImporter();

            /*** Act ***/
            importer.PopulateManifests(proprietaryFile, stationsParam);

            /*** Assert ***/
            Assert.AreEqual(1, proprietaryFile.InventoryManifests.Count);
            Assert.AreEqual("CadentStationCallsign", proprietaryFile.InventoryManifests[0].Station.LegacyCallLetters);
            Assert.AreEqual(1, getMappedCallLettersCalls.Count);
        }

        /// <summary>
        /// Test manifests are resolving the mapped stations correctly.
        /// </summary>
        /// <remarks>
        ///     Scenario : The station is not found.
        ///     Expected : An exception is thrown to the caller.
        /// </remarks>
        [Test]
        public void PopulateManifests_StationMappings_StationNotFound()
        {
            /*** Arrange ***/
            const string inventorySourceName = "HITV";
            const string fileName = @"Diginet_Template.xlsx";

            var dataLines = _GetDataLines();

            var proprietaryFile = new ProprietaryInventoryFile
            {
                FileName = fileName,
                InventorySource = _GetDiginetInventorySource(inventorySourceName),
                Header = new ProprietaryInventoryHeader { NtiToNsiIncrease = 25.123m },
                DataLines = dataLines
            };
            var stationsParam = new List<DisplayBroadcastStation>();

            _SpotLengthEngine.Setup(s => s.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns(3);

            var standardDayparts = _GetStandardDayparts();
            _StandardDaypartRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(standardDayparts);

            _ImpressionAdjustmentEngine.Setup(s =>
                    s.ConvertNtiImpressionsToNsi(It.IsAny<double>(), It.IsAny<double>()))
                .Returns(1234.5d);

            _MediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksIntersecting(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaWeek> { new MediaWeek(), new MediaWeek() });

            var getMappedCallLettersCalls = new List<string>();
            _StationMappingService.Setup(s => s.GetStationByCallLetters(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>((mappedCallsign, throwsIfNotFound) => getMappedCallLettersCalls.Add(mappedCallsign))
                .Throws(new Exception("Station not found test message."));

            var importer = _GetDiginetFileImporter();

            /*** Act ***/
            var caught = Assert.Throws<Exception>(() => importer.PopulateManifests(proprietaryFile, stationsParam));

            /*** Assert ***/
            Assert.IsNotNull(caught);
            Assert.IsTrue(caught.Message.Contains("Station not found test message."));
            Assert.AreEqual(1, getMappedCallLettersCalls.Count);
        }

        private List<ProprietaryInventoryFile.ProprietaryInventoryDataLine> _GetDataLines()
        {
            var lines = new List<ProprietaryInventoryFile.ProprietaryInventoryDataLine>
            {
                new ProprietaryInventoryFile.ProprietaryInventoryDataLine
                {
                    Audiences = new List<ProprietaryInventoryFile.ProprietaryInventoryDataLine.LineAudience>
                    {
                        new ProprietaryInventoryFile.ProprietaryInventoryDataLine.LineAudience
                        {
                            Audience = new DisplayAudience {Id = 31, AudienceString = "HouseHolds"},
                            Impressions = 250d,
                            Rating = .023
                        }
                    },
                    Dayparts = new List<DisplayDaypart>
                    {
                        new DisplayDaypart
                        {
                             StartTime = 3600 * 3,
                             EndTime = 3600 * 4
                        }
                    },
                    SpotCost = 1234.5m,
                    Station = "ExtendedStationCallsign"
                }
            };
            return lines;
        }

        private List<StandardDaypartDto> _GetStandardDayparts()
        {
            return new List<StandardDaypartDto>
            {
                new StandardDaypartDto { Id = 1, Code = "EMN", FullName = "Early Morning News"},
                new StandardDaypartDto { Id = 2, Code = "MDN", FullName = "Midday News"},
                new StandardDaypartDto { Id = 3, Code = "EN", FullName = "Evening News"},
                new StandardDaypartDto { Id = 4, Code = "LN", FullName = "Late News"},
                new StandardDaypartDto { Id = 5, Code = "ENLN", FullName = "Evening News/Late News"},
                new StandardDaypartDto { Id = 6, Code = "EF", FullName = "Early Fringe"},
                new StandardDaypartDto { Id = 7, Code = "PA", FullName = "Prime Access"},
                new StandardDaypartDto { Id = 8, Code = "PT", FullName = "Prime"},
                new StandardDaypartDto { Id = 9, Code = "LF", FullName = "Late Fringe"},
                new StandardDaypartDto { Id = 10, Code = "SYN", FullName = "Total Day Syndication"},
                new StandardDaypartDto { Id = 11, Code = "OVN", FullName = "Overnights"},
                new StandardDaypartDto { Id = 12, Code = "DAY", FullName = "Daytime"},
                new StandardDaypartDto { Id = 14, Code = "EM", FullName = "Early morning"},
                new StandardDaypartDto { Id = 15, Code = "AMN", FullName = "AM News"},
                new StandardDaypartDto { Id = 16, Code = "PMN", FullName = "PM News"},
                new StandardDaypartDto { Id = 17, Code = "TDN", FullName = "Total Day News"},
                new StandardDaypartDto { Id = 19, Code = "ROSS", FullName = "ROS Syndication"},
                new StandardDaypartDto { Id = 20, Code = "SPORTS", FullName = "ROS Sports"},
                new StandardDaypartDto { Id = 21, Code = "ROSP", FullName = "ROS Programming"},
                new StandardDaypartDto { Id = 22, Code = "TDNS", FullName = "Total Day News and Syndication"},
            };
        }

        private ExcelWorkbook _LoadFromFile(string fileName)
        {
            var filePath = $@".\Files\ProprietaryDataFiles\{fileName}";
            var filePackage = new ExcelPackage(new FileInfo(filePath));
            var workbook = filePackage.Workbook;
            return workbook;
        }

        private InventorySource _GetDiginetInventorySource(string name)
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