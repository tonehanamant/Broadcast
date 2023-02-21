using Common.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.Unposted;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.SpotExceptions
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionsUnpostedServiceUnitTestsV2
    {
        private SpotExceptionsUnpostedServiceV2 _SpotExceptionsUnpostedServiceV2;

        private Mock<ISpotExceptionsUnpostedRepositoryV2> _SpotExceptionsUnpostedRepositoryV2Mock;
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _SpotExceptionsUnpostedRepositoryV2Mock = new Mock<ISpotExceptionsUnpostedRepositoryV2>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();

            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();


            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsUnpostedRepositoryV2>())
                .Returns(_SpotExceptionsUnpostedRepositoryV2Mock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepositoryMock.Object);

            _SpotExceptionsUnpostedServiceV2 = new SpotExceptionsUnpostedServiceV2
                (
                    _DataRepositoryFactoryMock.Object,
                    _FeatureToggleMock.Object,
                    _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public void GetSpotExceptionsUnposted_Exist()
        {
            // Arrange
            OutOfSpecUnpostedRequestDto spotExceptionsUnpostedRequest = new OutOfSpecUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<OutOfSpecUnpostedNoPlanDto> unpostedNoPlan = GetOutOfSpecUnpostedNoPlan();
            List<OutOfSpecUnpostedNoReelRosterDto> unpostedNoReelRoster = GetOutOfSpecUnpostedNoReelRoster();

            _SpotExceptionsUnpostedRepositoryV2Mock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlan(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(unpostedNoPlan);

            _SpotExceptionsUnpostedRepositoryV2Mock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRoster(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(unpostedNoReelRoster);

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            // Act
            var result = _SpotExceptionsUnpostedServiceV2.GetOutOfSpecUnposted(spotExceptionsUnpostedRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(2, result.NoPlan.Count);
            Assert.AreEqual(2, result.NoReelRoster.Count);
        }

        [Test]
        public void GetSpotExceptionsUnposted_DoesNotExist()
        {
            // Arrange
            OutOfSpecUnpostedRequestDto spotExceptionsUnpostedRequest = new OutOfSpecUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<OutOfSpecUnpostedNoPlanDto> unpostedNoPlan = new List<OutOfSpecUnpostedNoPlanDto>();
            List<OutOfSpecUnpostedNoReelRosterDto> unpostedNoReelRoster = new List<OutOfSpecUnpostedNoReelRosterDto>(0);

            _SpotExceptionsUnpostedRepositoryV2Mock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlan(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(unpostedNoPlan);

            _SpotExceptionsUnpostedRepositoryV2Mock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRoster(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(unpostedNoReelRoster);

            // Act
            var result = _SpotExceptionsUnpostedServiceV2.GetOutOfSpecUnposted(spotExceptionsUnpostedRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(0, result.NoPlan.Count);
            Assert.AreEqual(0, result.NoReelRoster.Count);
        }

        [Test]
        public void GetSpotExceptionsUnposted_ThrowException()
        {
            // Arrange
            OutOfSpecUnpostedRequestDto spotExceptionsUnpostedRequest = new OutOfSpecUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<OutOfSpecUnpostedNoPlanDto> unpostedNoPlan = GetOutOfSpecUnpostedNoPlan();
            List<OutOfSpecUnpostedNoReelRosterDto> unpostedNoReelRoster = GetOutOfSpecUnpostedNoReelRoster();

            _SpotExceptionsUnpostedRepositoryV2Mock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlan(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(unpostedNoPlan);

            _SpotExceptionsUnpostedRepositoryV2Mock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRoster(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsUnpostedServiceV2.GetOutOfSpecUnposted(spotExceptionsUnpostedRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the Out Of Spec Unposted from the Database", result.Message);
        }

        private List<OutOfSpecUnpostedNoPlanDto> GetOutOfSpecUnpostedNoPlan()
        {
            return new List<OutOfSpecUnpostedNoPlanDto>()
            {
                new OutOfSpecUnpostedNoPlanDto
                {
                    HouseIsci = "YB82TXT2H",
                    ClientIsci = "AB82VR589",
                    ClientSpotLength = 60,
                    Count = 1,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateID = 191757,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    IngestMediaWeekId = 1,
                },
                new OutOfSpecUnpostedNoPlanDto
                {
                    HouseIsci = "YB82TXT2H",
                    ClientIsci = "AB82VR590",
                    ClientSpotLength = 15,
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateID = 191758,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    IngestMediaWeekId = 1
                }
            };
        }

        private List<OutOfSpecUnpostedNoReelRosterDto> GetOutOfSpecUnpostedNoReelRoster()
        {
            return new List<OutOfSpecUnpostedNoReelRosterDto>()
            {
                new OutOfSpecUnpostedNoReelRosterDto
                {
                    HouseIsci = "YB82TXT2M",
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateId = 191759,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    IngestedMediaWeekId = 1
                },
                new OutOfSpecUnpostedNoReelRosterDto
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
