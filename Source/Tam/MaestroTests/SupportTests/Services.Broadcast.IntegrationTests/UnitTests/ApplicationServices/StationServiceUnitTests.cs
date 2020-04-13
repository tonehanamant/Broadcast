using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class StationServiceUnitTests
    {
        // If the import already ran then it should not have to run again.
        [Test]
        public void ImportStationsFromForecastDatabase_AlreadyRan()
        {
            /*** Arrange ***/
            const int nsiMediaMonth = 123;
            const int broadcastMediaMonth = 123;
            const string testUser = "UnitTestUser";
            var currentDate = new DateTime(2020, 4, 11, 1, 36, 26);

            var nsiStationRepo = new Mock<INsiStationRepository>();
            nsiStationRepo.Setup(s => s.GetLatestMediaMonthIdFromStationList())
                .Returns(nsiMediaMonth);
            var getNsiStationListByMediaMonthCalledCount = 0;
            nsiStationRepo.Setup(s => s.GetNsiStationListByMediaMonth(It.IsAny<int>()))
                .Callback(() => getNsiStationListByMediaMonthCalledCount++)
                .Returns((List<NsiStationDto>)null);

            var stationRepo = new Mock<IStationRepository>();
            stationRepo.Setup(s => s.GetLatestMediaMonthIdFromStationMonthDetailsList())
                .Returns(broadcastMediaMonth);

            var stationMappingService = new Mock<IStationMappingService>();

            var service = _GetService(nsiStationRepo.Object, stationRepo.Object, stationMappingService.Object);

            /*** Act ***/
            service.ImportStationsFromForecastDatabase(testUser, currentDate);

            /*** Assert ***/
            // since the dates match this should check that and bail without getting the stations list.
            Assert.AreEqual(0, getNsiStationListByMediaMonthCalledCount);
        }

        // Create unknown stations with month details
        [Test]
        public void ImportStationsFromForecastDatabase_UpdateUnratedStation()
        {
            /*** Arrange ***/
            const int nsiMediaMonth = 124;
            const int broadcastMediaMonth = 123;
            const string testUser = "UnitTestUser";
            var currentDate = new DateTime(2020, 4, 11, 1, 36, 26);

            var nsiStations = new List<NsiStationDto>
            {
                new NsiStationDto
                {
                    MediaMonthId = nsiMediaMonth, LegacyCallLetters = "TVTV", CallLetters = "TVTV 1.0",
                    PrimaryAffiliation = "ABC", MarketCode = 123, DistributorCode = 456, DistributorGroup = ""
                }
            };

            var nsiStationRepo = new Mock<INsiStationRepository>();
            nsiStationRepo.Setup(s => s.GetLatestMediaMonthIdFromStationList())
                .Returns(nsiMediaMonth);
            var getNsiStationListByMediaMonthCalledCount = 0;
            nsiStationRepo.Setup(s => s.GetNsiStationListByMediaMonth(It.IsAny<int>()))
                .Callback(() => getNsiStationListByMediaMonthCalledCount++)
                .Returns(nsiStations);

            var stationRepo = new Mock<IStationRepository>();
            stationRepo.Setup(s => s.GetLatestMediaMonthIdFromStationMonthDetailsList())
                .Returns(broadcastMediaMonth);
            stationRepo.Setup(s => s.ExistsStationWithCallLetter(It.IsAny<string>()))
                .Returns(true);
            stationRepo.Setup(s => s.GetBroadcastStationByLegacyCallLetters(It.IsAny<string>()))
                .Returns(new DisplayBroadcastStation
                {
                    Id = 4,
                    LegacyCallLetters = "TVTV",
                    CallLetters = "TVTV 1.0",
                    Code = null,
                    Affiliation = null,
                    MarketCode = null
                });
            var updatedStations = new List<Tuple<DisplayBroadcastStation, string, DateTime>>();
            stationRepo.Setup(s => s.UpdateStation(It.IsAny<DisplayBroadcastStation>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<DisplayBroadcastStation, string, DateTime>((s, u, d) => updatedStations.Add(new Tuple<DisplayBroadcastStation, string, DateTime>(s, u, d)));
            var saveStationMonthDetailsCalls = new List<StationMonthDetailDto>();
            stationRepo.Setup(s => s.SaveStationMonthDetails(It.IsAny<StationMonthDetailDto>()))
                .Callback<StationMonthDetailDto>((d) => saveStationMonthDetailsCalls.Add(d));
            var getUnratedBroadcastStationsCalledCount = 0;
            stationRepo.Setup(s => s.GetUnratedBroadcastStations())
                .Callback(() => getUnratedBroadcastStationsCalledCount++)
                .Returns(new List<DisplayBroadcastStation>());

            var stationMappingService = new Mock<IStationMappingService>();

            var service = _GetService(nsiStationRepo.Object, stationRepo.Object, stationMappingService.Object);

            /*** Act ***/
            service.ImportStationsFromForecastDatabase(testUser, currentDate);

            /*** Assert ***/
            Assert.AreEqual(1, getNsiStationListByMediaMonthCalledCount);
            // verify we made the unrated rated
            Assert.AreEqual(1, updatedStations.Count);
            Assert.AreEqual("ABC", updatedStations[0].Item1.Affiliation);
            Assert.AreEqual(123, updatedStations[0].Item1.MarketCode);
            Assert.AreEqual(456, updatedStations[0].Item1.Code);
            Assert.AreEqual(testUser, updatedStations[0].Item2);
            // saved the media month details
            Assert.AreEqual(1, saveStationMonthDetailsCalls.Count);
            Assert.AreEqual(4, saveStationMonthDetailsCalls[0].StationId);
            Assert.AreEqual(nsiMediaMonth, saveStationMonthDetailsCalls[0].MediaMonthId);
            Assert.AreEqual("ABC", saveStationMonthDetailsCalls[0].Affiliation);
            Assert.AreEqual(123, saveStationMonthDetailsCalls[0].MarketCode);
            Assert.AreEqual(456, saveStationMonthDetailsCalls[0].DistributorCode);
            // looked for unrated
            Assert.AreEqual(1, getUnratedBroadcastStationsCalledCount);
        }

        private StationService _GetService(
            INsiStationRepository nsiStationRepo, 
            IStationRepository stationRepo,
            IStationMappingService stationMappingService
            )
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();

            dataRepositoryFactory.Setup(s => s.GetDataRepository<INsiStationRepository>())
                .Returns(nsiStationRepo);
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStationRepository>())
                .Returns(stationRepo);

            var service = new StationService(dataRepositoryFactory.Object, stationMappingService);
            return service;
        }
    }
}