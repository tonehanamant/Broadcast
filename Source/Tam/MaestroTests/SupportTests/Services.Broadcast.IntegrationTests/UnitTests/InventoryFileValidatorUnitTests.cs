using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    class InventoryFileValidatorUnitTests
    {

        [Test]
        public void ValidateInventoryFile()
        {
            var _mockStationRepository = new Mock<IStationRepository>();
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WABC"))
                .Returns(new DisplayBroadcastStation());
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WDEF"))
                .Returns(new DisplayBroadcastStation());

            var _mockSpotLengthRepository = new Mock<ISpotLengthRepository>();
            _mockSpotLengthRepository.Setup(a => a.GetSpotLengthAndIds())
                .Returns(new Dictionary<int, int>());

            //var _mockRatesRepository = new Mock<IStationProgramRepository>();
            //_mockRatesRepository.Setup(
            //    a =>
            //        a.GetStationProgramsByNameDaypartFlight(
            //            It.IsAny<string>(),
            //            It.IsAny<int>(),
            //            It.IsAny<DateTime>(),
            //            It.IsAny<DateTime>())).Returns(new List<StationProgram>());

            //var _mockDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationRepository>())
            //    .Returns(_mockStationRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationProgramRepository>())
            //    .Returns(_mockRatesRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<ISpotLengthRepository>())
            //    .Returns(_mockSpotLengthRepository.Object);

            //var _ratesFileValidator = new InventoryFileValidator(_mockDataRepositoryFactory.Object);

            //var incomingInventoryFile = new InventoryFile();

            //var stationProgram1 = new StationProgram()
            //{
            //    StationLegacyCallLetters = "WABC",
            //    ProgramName = "News at 7",
            //    Daypart = new DisplayDaypart() { Id = 1 }
            //};
            //var program1Flight1 = new StationProgramFlightWeek()
            //{
            //    FlightWeek = new DisplayMediaWeek(){Id = 1},
            //    Rate15s = 10,
            //};
            //stationProgram1.FlightWeeks.Add(program1Flight1);
            //incomingInventoryFile.StationPrograms.Add(stationProgram1);
            //var stationProgram2 = new StationProgram()
            //{
            //    StationLegacyCallLetters = "WDEF",
            //    ProgramName = "Wheel of Fortune",
            //    Daypart = new DisplayDaypart() { Id = 1 }
            //};
            //var program2Flight1 = new StationProgramFlightWeek()
            //{
            //    FlightWeek = new DisplayMediaWeek(){Id = 1},
            //    Rate30s = 15
            //};
            //stationProgram2.FlightWeeks.Add(program2Flight1);
            //incomingInventoryFile.StationPrograms.Add(stationProgram2);
            //var fileProblems = new List<InventoryFileProblem>();
            //var validationResult =_ratesFileValidator.ValidateInventoryFile(incomingInventoryFile);
            //fileProblems.AddRange(validationResult.InventoryFileProblems);

        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryFileValidationException), ExpectedMessage = "unknown stations", MatchType = MessageMatch.Contains)]
        public void ValidateInventoryFileWithUnknownStation()
        {
            var _mockStationRepository = new Mock<IStationRepository>();
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WABC"))
                .Returns(new DisplayBroadcastStation());
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WDEF"))
                .Returns(new DisplayBroadcastStation());
            var _mockSpotLengthRepository = new Mock<ISpotLengthRepository>();
            _mockSpotLengthRepository.Setup(a => a.GetSpotLengthAndIds())
                .Returns(new Dictionary<int, int>());

            //var _mockRatesRepository = new Mock<IStationProgramRepository>();
            //_mockRatesRepository.Setup(
            //    a =>
            //        a.GetStationProgramsByNameDaypartFlight(
            //            It.IsAny<string>(),
            //            It.IsAny<int>(),
            //            It.IsAny<DateTime>(),
            //            It.IsAny<DateTime>())).Returns(new List<StationProgram>());

            //var _mockDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationRepository>())
            //    .Returns(_mockStationRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationProgramRepository>())
            //    .Returns(_mockRatesRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<ISpotLengthRepository>())
            //    .Returns(_mockSpotLengthRepository.Object);

            //var _ratesFileValidator = new InventoryFileValidator(_mockDataRepositoryFactory.Object);

            //var incomingInventoryFile = new InventoryFile();

            //var stationProgram1 = new StationProgram()
            //{
            //    StationLegacyCallLetters = "UKNW",
            //};
            //incomingInventoryFile.StationPrograms.Add(stationProgram1);
            //var fileProblems = new List<InventoryFileProblem>();
            //var validationResult =_ratesFileValidator.ValidateInventoryFile(incomingInventoryFile);
            //fileProblems.AddRange(validationResult.InventoryFileProblems);

        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryFileValidationException), ExpectedMessage = "Invalid rates file dates", MatchType = MessageMatch.Contains)]
        public void ValidateInventoryFileWithInvalidDates()
        {
            var _mockSpotLengthRepository = new Mock<ISpotLengthRepository>();
            _mockSpotLengthRepository.Setup(a => a.GetSpotLengthAndIds())
                .Returns(new Dictionary<int, int>());

            var _mockDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            _mockDataRepositoryFactory.Setup(a => a.GetDataRepository<ISpotLengthRepository>())
                .Returns(_mockSpotLengthRepository.Object);

            var _ratesFileValidator = new InventoryFileValidator(null);

            var incomingInventoryFile = new InventoryFile()
            {
                StartDate = DateTime.Now.AddDays(6),
                EndDate = DateTime.Now
            };
            var fileProblems = new List<InventoryFileProblem>();
            var validationResult = _ratesFileValidator.ValidateInventoryFile(incomingInventoryFile);
            fileProblems.AddRange(validationResult.InventoryFileProblems);

        }

        [Test]
        //[ExpectedException(typeof(BroadcastInventoryFileValidationException), ExpectedMessage = "programs with zero or null rates", MatchType = MessageMatch.Contains)]
        public void ValidateInventoryFileWithInvalidRates()
        {
            var _mockStationRepository = new Mock<IStationRepository>();
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WABC"))
                .Returns(new DisplayBroadcastStation());
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WDEF"))
                .Returns(new DisplayBroadcastStation());
            var _mockSpotLengthRepository = new Mock<ISpotLengthRepository>();
            _mockSpotLengthRepository.Setup(a => a.GetSpotLengthAndIds())
                .Returns(new Dictionary<int, int>());

            //var _mockRatesRepository = new Mock<IStationProgramRepository>();
            //_mockRatesRepository.Setup(
            //    a =>
            //        a.GetStationProgramsByNameDaypartFlight(
            //            It.IsAny<string>(),
            //            It.IsAny<int>(),
            //            It.IsAny<DateTime>(),
            //            It.IsAny<DateTime>())).Returns(new List<StationProgram>());

            //var _mockDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationRepository>())
            //    .Returns(_mockStationRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationProgramRepository>())
            //    .Returns(_mockRatesRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<ISpotLengthRepository>())
            //    .Returns(_mockSpotLengthRepository.Object);

            //var _ratesFileValidator = new InventoryFileValidator(_mockDataRepositoryFactory.Object);

            //var incomingInventoryFile = new InventoryFile();

            //var stationProgram1 = new StationProgram()
            //{
            //    StationLegacyCallLetters = "WABC",
            //    ProgramName = "News at 7",
            //    Daypart = new DisplayDaypart() { Id = 1 }
            //};
            //var program1Flight1 = new StationProgramFlightWeek()
            //{
            //    Rate15s = 10,
            //    FlightWeek = new DisplayMediaWeek() { Id = 1 }
            //};
            //stationProgram1.FlightWeeks.Add(program1Flight1);
            //incomingInventoryFile.StationPrograms.Add(stationProgram1);
            //var stationProgram2 = new StationProgram()
            //{
            //    StationLegacyCallLetters = "WDEF",
            //    ProgramName = "Wheel of Fortune",
            //    Daypart = new DisplayDaypart() { Id = 1 }
            //};
            //var program2Flight1 = new StationProgramFlightWeek()
            //{
            //    Rate30s = 0,
            //    FlightWeek = new DisplayMediaWeek() { Id = 1 }
            //};
            //var program2Flight2 = new StationProgramFlightWeek()
            //{
            //    Rate30s = 10,
            //    FlightWeek = new DisplayMediaWeek() { Id = 2 }
            //};
            //stationProgram2.FlightWeeks.Add(program2Flight1);
            //stationProgram2.FlightWeeks.Add(program2Flight2);
            //incomingInventoryFile.StationPrograms.Add(stationProgram2);
            //var fileProblems = new List<InventoryFileProblem>();
            //var validationResult = _ratesFileValidator.ValidateInventoryFile(incomingInventoryFile);
            //fileProblems.AddRange(validationResult.InventoryFileProblems);
            //Assert.AreEqual(1, validationResult.InvalidRates.Count);

        }
        [Ignore]
        [Test]
        public void ValidateInventoryFileWithDuplicateRates()
        {
            var _mockStationRepository = new Mock<IStationRepository>();
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WABC"))
                .Returns(new DisplayBroadcastStation());
            _mockStationRepository.Setup(a => a.GetBroadcastStationByLegacyCallLetters("WDEF"))
                .Returns(new DisplayBroadcastStation());
            var _mockSpotLengthRepository = new Mock<ISpotLengthRepository>();
            _mockSpotLengthRepository.Setup(a => a.GetSpotLengthAndIds())
                .Returns(new Dictionary<int, int>());

            //var existingPrograms = new List<StationProgram>();
            //var existingProgram = new StationProgram()
            //{
            //    StationLegacyCallLetters = "WABC",
            //    ProgramName = "News at 7",
            //    Daypart = new DisplayDaypart() { Id = 1 },
            //    StartDate = DateTime.Parse("2016-01-01"),
            //    EndDate = DateTime.Parse("2016-01-31")                
            //};
            //var existingProgramFlight = new StationProgramFlightWeek()
            //{
            //    Rate15s = 10,
            //    Audiences = new List<StationProgramFlightWeekAudience>(),
            //    FlightWeek = new DisplayMediaWeek() { Id = 1 }
            //};
            //var existingFlightAudience = new StationProgramFlightWeekAudience()
            //{
            //    Audience = new DisplayAudience(1, "HH"),
            //    Impressions = 10,
            //    Rating = 0.5f
            //};
            //existingProgramFlight.Audiences.Add(existingFlightAudience);
            //existingProgram.FlightWeeks.Add(existingProgramFlight);
            //existingPrograms.Add(existingProgram);
            //var _mockRatesRepository = new Mock<IStationProgramRepository>();
            //_mockRatesRepository.Setup(
            //    a =>
            //        a.GetStationProgramsByNameDaypartFlight(
            //            "News at 7",
            //            It.IsAny<int>(),
            //            It.IsAny<DateTime>(),
            //            It.IsAny<DateTime>())).Returns(existingPrograms);

            //_mockRatesRepository.Setup(
            //    a =>
            //        a.GetStationProgramsByNameDaypartFlight(
            //            "Wheel of Fortune",
            //            It.IsAny<int>(),
            //            It.IsAny<DateTime>(),
            //            It.IsAny<DateTime>())).Returns(new List<StationProgram>());

            //var _mockDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationRepository>())
            //    .Returns(_mockStationRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<IStationProgramRepository>())
            //    .Returns(_mockRatesRepository.Object);
            //_mockDataRepositoryFactory.Setup(a => a.GetDataRepository<ISpotLengthRepository>())
            //    .Returns(_mockSpotLengthRepository.Object);

            //var _ratesFileValidator = new InventoryFileValidator(_mockDataRepositoryFactory.Object);            

            //var incomingInventoryFile = new InventoryFile();

            //var stationProgram1 = new StationProgram()
            //{
            //    StationLegacyCallLetters = "WABC",
            //    ProgramName = "News at 7",
            //    Daypart = new DisplayDaypart() { Id = 1 },
            //    StartDate = DateTime.Parse("2016-01-01"),
            //    EndDate = DateTime.Parse("2016-01-31")
            //};
            //var program1Flight1 = new StationProgramFlightWeek()
            //{
            //    Rate15s = 10,
            //    Audiences = new List<StationProgramFlightWeekAudience>(),
            //    FlightWeek = new DisplayMediaWeek() { Id = 1 }
            //};
            //var audience1 = new StationProgramFlightWeekAudience()
            //{
            //    Audience = new DisplayAudience(1, "HH"),
            //    Impressions = 10,
            //    Rating = 0.5f
            //};
            //program1Flight1.Audiences.Add(audience1);
            //stationProgram1.FlightWeeks.Add(program1Flight1);
            //incomingInventoryFile.StationPrograms.Add(stationProgram1);
            //var stationProgram2 = new StationProgram()
            //{
            //    StationLegacyCallLetters = "WDEF",
            //    ProgramName = "Wheel of Fortune",
            //    Daypart = new DisplayDaypart() { Id = 1 },
            //    StartDate = DateTime.Parse("2016-01-01"),
            //    EndDate = DateTime.Parse("2016-01-31")
            //};
            //var program2Flight1 = new StationProgramFlightWeek()
            //{
            //    Rate30s = 15,
            //    Audiences = new List<StationProgramFlightWeekAudience>(),
            //    FlightWeek = new DisplayMediaWeek() { Id = 1}
            //};
            //var audience2 = new StationProgramFlightWeekAudience()
            //{
            //    Audience = new DisplayAudience(1, "HH"),
            //    Impressions = 12,
            //    Rating = 0.7f
            //};
            //program2Flight1.Audiences.Add(audience2);
            //stationProgram2.FlightWeeks.Add(program2Flight1);
            //incomingInventoryFile.StationPrograms.Add(stationProgram2);

            //Assert.IsTrue(incomingInventoryFile.StationPrograms.Count == 2, "Should have two programs initially");
            //var fileProblems = new List<InventoryFileProblem>();
            //var validationResult = _ratesFileValidator.ValidateInventoryFile(incomingInventoryFile);
            //fileProblems.AddRange(validationResult.InventoryFileProblems);

            //Assert.IsTrue(fileProblems.Count == 1, "Should have found one duplicate program");

        }
    }
}
