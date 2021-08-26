using Common.Services.Repositories;
using Hangfire;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.ReelRosterIscis;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]    
    public class ReelIsciIngestServiceUnitTests
    {
        private const string _UserName = "TestUser";
        private ReelIsciIngestService _ReelIsciIngestService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IReelIsciApiClient> _ReelIsciApiClientMock;
        private Mock<IReelIsciIngestJobsRepository> _ReelIsciIngestJobsRepositoryMock;
        private Mock<IReelIsciRepository> _ReelIsciRepository;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;
        private Mock<IBackgroundJobClient> _BackgroundJobClientMock;
        private Mock<IReelIsciProductRepository> _ReelIsciProductRepositoryMock;
        private Mock<IPlanIsciRepository> _PlanIsciRepositoryMock;
        private Mock<IFeatureToggleHelper> _featureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _ReelIsciApiClientMock = new Mock<IReelIsciApiClient>();
            _ReelIsciIngestJobsRepositoryMock = new Mock<IReelIsciIngestJobsRepository>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();
            _BackgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _ReelIsciRepository = new Mock<IReelIsciRepository>();
            _ReelIsciProductRepositoryMock = new Mock<IReelIsciProductRepository>();
            _PlanIsciRepositoryMock = new Mock<IPlanIsciRepository>();
            _featureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IReelIsciIngestJobsRepository>())
                .Returns(_ReelIsciIngestJobsRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IReelIsciRepository>())
                .Returns(_ReelIsciRepository.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IReelIsciProductRepository>())
                .Returns(_ReelIsciProductRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanIsciRepository>())
                .Returns(_PlanIsciRepositoryMock.Object);

            _ReelIsciIngestService = new ReelIsciIngestService(_ReelIsciApiClientMock.Object, _DataRepositoryFactoryMock.Object, _DateTimeEngineMock.Object, _BackgroundJobClientMock.Object, _featureToggleMock.Object,_ConfigurationSettingsHelperMock.Object);
        }

        [Test]
        public void RunQueued_ConfiguredIngestNumberOfDays()
        {
            //Arrange
            var currentDateTime = new DateTime(2021, 08, 26, 14, 26, 36);
            var expectedIngestStartDate = new DateTime(2020, 08, 25);
            const int numberOfDays = 366;
            const int jobId = 12;

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            var passedIngestNumberOfDays = -1;
            var passedIngestStartDate = DateTime.MinValue;
            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Callback<DateTime, int>((st, nm) =>
                {
                    passedIngestStartDate = st;
                    passedIngestNumberOfDays = nm;
                })
                .Returns(new List<ReelRosterIsciDto>());

            _ReelIsciRepository
                .Setup(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(0);

            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<int>(ConfigKeys.ReelIsciIngestNumberOfDays))
                .Returns(numberOfDays);

            //Act
            _ReelIsciIngestService.RunQueued(jobId, _UserName);

            //Assert
            Assert.AreEqual(numberOfDays, passedIngestNumberOfDays);
            const string dtFormat = "yyyy/MM/dd";
            Assert.AreEqual(expectedIngestStartDate.ToString(dtFormat), passedIngestStartDate.ToString(dtFormat));
        }

        [Test]
        public void PerformReelIsciIngest_ConfiguredIngestNumberOfDays()
        {
            //Arrange
            var currentDateTime = new DateTime(2021, 08, 26, 14,26,36);
            var expectedIngestStartDate = new DateTime(2020, 08, 25);
            const int numberOfDays = 366;
            const int jobId = 12;

            _ReelIsciIngestJobsRepositoryMock.Setup(s => s.AddReelIsciIngestJob(It.IsAny<ReelIsciIngestJobDto>()))
                .Returns(jobId);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            var passedIngestNumberOfDays = -1;
            var passedIngestStartDate = DateTime.MinValue;
            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Callback<DateTime, int>((st, nm) =>
                {
                    passedIngestStartDate = st;
                    passedIngestNumberOfDays = nm;
                })
                .Returns(new List<ReelRosterIsciDto>());

            _ReelIsciRepository
                .Setup(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(0);

            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<int>(ConfigKeys.ReelIsciIngestNumberOfDays))
                .Returns(numberOfDays);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngest(_UserName);

            //Assert
            Assert.AreEqual(numberOfDays, passedIngestNumberOfDays);
            const string dtFormat = "yyyy/MM/dd";
            Assert.AreEqual(expectedIngestStartDate.ToString(dtFormat), passedIngestStartDate.ToString(dtFormat));
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_NoReelIsciReceive()
        {
            //Arrange
            var startDate = new DateTime(2021, 01, 01);
            const int numberOfDays = 6;
            const int jobId = 12;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>());

            _ReelIsciRepository
                .Setup(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(0);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, _UserName);

            //Assert
            _ReelIsciRepository.Verify(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            _ReelIsciRepository.Verify(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()), Times.Never);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_DeleteReelIscis()
        {
            //Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            const int numberOfDays = 6;
            DateTime endDate = startDate.AddDays(numberOfDays);
            int expectedDeleteCount = 2, deletedCount = 0;
            const int jobId = 12;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>());

            _ReelIsciRepository
                .Setup(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    deletedCount = 2;
                })
                .Returns(deletedCount);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, _UserName);

            //Assert
            _ReelIsciRepository.Verify(x => x.DeleteReelIscisBetweenRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            Assert.AreEqual(expectedDeleteCount, deletedCount);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_AddReelIscis()
        {
            //Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            int numberOfDays = 6;
            int expectedAddCount = 2, addedCount = 0;
            const int jobId = 12;

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

            _ReelIsciRepository
                .Setup(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()))
                .Callback(() =>
                {
                    addedCount = 2;
                })
                .Returns(addedCount);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, _UserName);

            //Assert
            _ReelIsciRepository.Verify(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()), Times.Once);
            Assert.AreEqual(expectedAddCount, addedCount);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_DeleteReelIsciProducts_NotExistInReelIsci()
        {
            //Arrange
            var startDate = new DateTime(2021, 01, 01);
            const int numberOfDays = 6;
            var expectedDeleteCount = 2;
            var deletedCount = 0;
            const int jobId = 12;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>());

            _ReelIsciProductRepositoryMock
                .Setup(x => x.DeleteReelIsciProductsNotExistInReelIsci())
                .Callback(() =>
                {
                    deletedCount = 2;
                })
                .Returns(deletedCount);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, _UserName);

            //Assert
            _ReelIsciProductRepositoryMock.Verify(x => x.DeleteReelIsciProductsNotExistInReelIsci(), Times.Once);
            Assert.AreEqual(expectedDeleteCount, deletedCount);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_DeletePlanIscis_NotExistInReelIsci()
        {
            //Arrange
            var startDate = new DateTime(2021, 01, 01);
            const int numberOfDays = 6;
            var expectedDeleteCount = 2;
            var deletedCount = 0;
            const int jobId = 12;

            _ReelIsciApiClientMock
                .Setup(x => x.GetReelRosterIscis(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<ReelRosterIsciDto>());

            _PlanIsciRepositoryMock
                .Setup(x => x.DeletePlanIscisNotExistInReelIsci(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() =>
                {
                    deletedCount = 2;
                })
                .Returns(deletedCount);

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, _UserName);

            //Assert
            _PlanIsciRepositoryMock.Verify(x => x.DeletePlanIscisNotExistInReelIsci(It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(expectedDeleteCount, deletedCount);
        }

        [Test]
        public void PerformReelIsciIngestBetweenRange_ThrowsException()
        {
            //Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            int numberOfDays = 6;
            const int jobId = 12;

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

            var errorMessage = "Throwing a test exception.";
            _ReelIsciRepository
                .Setup(x => x.AddReelIscis(It.IsAny<List<ReelIsciDto>>()))
                .Callback(() =>
                {
                    throw new Exception(errorMessage);
                });

            var savedJobs = new List<ReelIsciIngestJobDto>();
            _ReelIsciIngestJobsRepositoryMock.Setup(s => s.UpdateReelIsciIngestJob(It.IsAny<ReelIsciIngestJobDto>()))
                .Callback<ReelIsciIngestJobDto>((j) => savedJobs.Add(j));

            //Act
            _ReelIsciIngestService.PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, _UserName);

            //Assert
            Assert.AreEqual(1, savedJobs.Count);
            var job = savedJobs.First();
            Assert.AreEqual(BackgroundJobProcessingStatus.Failed, job.Status);
            Assert.IsNotNull(job.CompletedAt);
            Assert.IsTrue(job.ErrorMessage.Contains(errorMessage));
        }
    }
}
