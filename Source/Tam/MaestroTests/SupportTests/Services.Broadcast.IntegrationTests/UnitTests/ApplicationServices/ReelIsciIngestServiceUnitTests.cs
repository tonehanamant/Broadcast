using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.ReelRosterIscis;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]    
    public class ReelIsciIngestServiceUnitTests
    {
        private ReelIsciIngestService _ReelIsciIngestService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IReelIsciApiClient> _ReelIsciApiClientMock;
        private Mock<IReelIsciIngestJobsRepository> _ReelIsciIngestJobsRepositoryMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _ReelIsciApiClientMock = new Mock<IReelIsciApiClient>();
            _ReelIsciIngestJobsRepositoryMock = new Mock<IReelIsciIngestJobsRepository>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IReelIsciIngestJobsRepository>())
                .Returns(_ReelIsciIngestJobsRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepositoryMock.Object);

            _ReelIsciIngestService = new ReelIsciIngestService(_ReelIsciApiClientMock.Object, _DataRepositoryFactoryMock.Object, _DateTimeEngineMock.Object);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_NoReelIsciReceive()
        {
            //Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            int numberOfDays = 6;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>());

            _ReelIsciIngestJobsRepositoryMock
                .Setup(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(0);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(startDate, numberOfDays);

            //Assert
            _ReelIsciIngestJobsRepositoryMock.Verify(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            _ReelIsciIngestJobsRepositoryMock.Verify(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()), Times.Never);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_DeleteReelIscis()
        {
            //Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            int numberOfDays = 6;
            DateTime endDate = startDate.AddDays(numberOfDays);
            int expectedDeleteCount = 2, deletedCount = 0;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>());

            _ReelIsciIngestJobsRepositoryMock
                .Setup(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    deletedCount = 2;
                })
                .Returns(deletedCount);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(startDate, numberOfDays);

            //Assert
            _ReelIsciIngestJobsRepositoryMock.Verify(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            Assert.AreEqual(expectedDeleteCount, deletedCount);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_AddReelIscis()
        {
            //Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            int numberOfDays = 6;
            int expectedAddCount = 2, addedCount = 0;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>()
                {
                    new ReelRosterIsciDto
                    {
                        Isci = "OKWF1701H",
                        SpotLengthDuration = 15,
                        StartDate = new DateTime(2021,01,01),
                        EndDate = new DateTime(2021,01,17),
                        AdvertiserNames = new List<string> { "Colgate EM", "Nature's Bounty" }
                    },
                    new ReelRosterIsciDto
                    {
                        Isci = "OKWL1702H",
                        SpotLengthDuration = 30,
                        StartDate = new DateTime(2021,01,01),
                        EndDate = new DateTime(2021,01,17),
                        AdvertiserNames = new List<string> { "O'Keeffes" }
                    }
                });

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());

            _ReelIsciIngestJobsRepositoryMock
                .Setup(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()))
                .Callback(() =>
                {
                    addedCount = 2;
                })
                .Returns(addedCount);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(startDate, numberOfDays);

            //Assert
            _ReelIsciIngestJobsRepositoryMock.Verify(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()), Times.Once);
            Assert.AreEqual(expectedAddCount, addedCount);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_ThrowsException()
        {
            //Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            int numberOfDays = 6;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>()
                {
                    new ReelRosterIsciDto
                    {
                        Isci = "OKWF1701H",
                        SpotLengthDuration = 15,
                        StartDate = new DateTime(2021,01,01),
                        EndDate = new DateTime(2021,01,17),
                        AdvertiserNames = new List<string> { "Colgate EM", "Nature's Bounty" }
                    },
                    new ReelRosterIsciDto
                    {
                        Isci = "OKWL1702H",
                        SpotLengthDuration = 30,
                        StartDate = new DateTime(2021,01,01),
                        EndDate = new DateTime(2021,01,17),
                        AdvertiserNames = new List<string> { "O'Keeffes" }
                    }
                });

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());

            _ReelIsciIngestJobsRepositoryMock
                .Setup(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            //Act
            var result = Assert.Throws<Exception>(() => _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(startDate, numberOfDays));

            //Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }
    }
}
