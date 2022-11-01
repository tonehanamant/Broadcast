using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.Unposted;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.SpotExceptions
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionsUnpostedServiceUnitTests
    {
        private SpotExceptionsUnpostedService _SpotExceptionsUnpostedService;

        private Mock<ISpotExceptionsUnpostedRepository> _SpotExceptionsUnpostedRepositoryMock;
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _SpotExceptionsUnpostedRepositoryMock = new Mock<ISpotExceptionsUnpostedRepository>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();

            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();


            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsUnpostedRepository>())
                .Returns(_SpotExceptionsUnpostedRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepositoryMock.Object);

            _SpotExceptionsUnpostedService = new SpotExceptionsUnpostedService
                (
                    _DataRepositoryFactoryMock.Object,
                    _FeatureToggleMock.Object,
                    _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public async void GetSpotExceptionsUnposted_Exist()
        {
            // Arrange
            SpotExceptionsUnpostedRequestDto spotExceptionsUnpostedRequest = new SpotExceptionsUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsUnpostedNoPlanDto> unpostedNoPlan = GetSpotExceptionUnpostedNoPlan();
            List<SpotExceptionsUnpostedNoReelRosterDto> unpostedNoReelRoster = GetSpotExceptionUnpostedNoReelRoster();

            _SpotExceptionsUnpostedRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlanAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(unpostedNoPlan));

            _SpotExceptionsUnpostedRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRosterAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(unpostedNoReelRoster));

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            // Act
            var result = await _SpotExceptionsUnpostedService.GetSpotExceptionsUnposted(spotExceptionsUnpostedRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.NoPlan.Count, 2);
            Assert.AreEqual(result.NoReelRoster.Count, 2);
        }

        [Test]
        public async void GetSpotExceptionsUnposted_DoesNotExist()
        {
            // Arrange
            SpotExceptionsUnpostedRequestDto spotExceptionsUnpostedRequest = new SpotExceptionsUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsUnpostedNoPlanDto> unpostedNoPlan = new List<SpotExceptionsUnpostedNoPlanDto>();
            List<SpotExceptionsUnpostedNoReelRosterDto> unpostedNoReelRoster = new List<SpotExceptionsUnpostedNoReelRosterDto>(0);

            _SpotExceptionsUnpostedRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlanAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(unpostedNoPlan));

            _SpotExceptionsUnpostedRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRosterAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(unpostedNoReelRoster));

            // Act
            var result = await _SpotExceptionsUnpostedService.GetSpotExceptionsUnposted(spotExceptionsUnpostedRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.NoPlan.Count, 0);
            Assert.AreEqual(result.NoReelRoster.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsUnposted_ThrowException()
        {
            // Arrange
            SpotExceptionsUnpostedRequestDto spotExceptionsUnpostedRequest = new SpotExceptionsUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsUnpostedNoPlanDto> unpostedNoPlan = GetSpotExceptionUnpostedNoPlan();
            List<SpotExceptionsUnpostedNoReelRosterDto> unpostedNoReelRoster = GetSpotExceptionUnpostedNoReelRoster();

            _SpotExceptionsUnpostedRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlanAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(unpostedNoPlan));

            _SpotExceptionsUnpostedRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRosterAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async() => await _SpotExceptionsUnpostedService.GetSpotExceptionsUnposted(spotExceptionsUnpostedRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the Unposted No Plan from the Database", result.Message);
        }

        private List<SpotExceptionsUnpostedNoPlanDto> GetSpotExceptionUnpostedNoPlan()
        {
            return new List<SpotExceptionsUnpostedNoPlanDto>()
            {
                new SpotExceptionsUnpostedNoPlanDto
                {
                    HouseIsci = "YB82TXT2H",
                    ClientIsci = "AB82VR589",
                    ClientSpotLengthId = 2,
                    Count = 1,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateID = 191757,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    IngestMediaWeekId = 1,
                },
                new SpotExceptionsUnpostedNoPlanDto
                {
                    HouseIsci = "YB82TXT2H",
                    ClientIsci = "AB82VR590",
                    ClientSpotLengthId = 3,
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateID = 191758,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    IngestMediaWeekId = 1
                }
            };
        }

        private List<SpotExceptionsUnpostedNoReelRosterDto> GetSpotExceptionUnpostedNoReelRoster()
        {
            return new List<SpotExceptionsUnpostedNoReelRosterDto>()
            {
                new SpotExceptionsUnpostedNoReelRosterDto
                {
                    HouseIsci = "YB82TXT2M",
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateId = 191759,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    IngestedMediaWeekId = 1
                },
                new SpotExceptionsUnpostedNoReelRosterDto
                {
                    HouseIsci = "YB82TXT2M",
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateId = 191760,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    IngestedMediaWeekId = 1
                }
            };
        }
    }
}
