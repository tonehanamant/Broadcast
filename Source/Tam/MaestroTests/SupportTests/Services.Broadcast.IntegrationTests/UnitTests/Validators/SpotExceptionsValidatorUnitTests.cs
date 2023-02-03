using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.SpotExceptions.DecisionSync;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories.SpotExceptions;
using Services.Broadcast.Validators;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    [TestFixture]
    public class SpotExceptionsValidatorUnitTests
    {
        private SpotExceptionsValidator _SpotExceptionsValidator;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionsOutOfSpecRepositoryV2> _SpotExceptionsOutOfSpecRepositoryV2Mock;
        private Mock<ISpotExceptionsRecommendedPlanRepositoryV2> _SpotExceptionsRecommendedPlanRepositoryV2Mock;

        private Mock<ISpotExceptionsApiClient> _SpotExceptionsApiClientMock;

        [SetUp]
        public void Setup()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();

            _SpotExceptionsOutOfSpecRepositoryV2Mock = new Mock<ISpotExceptionsOutOfSpecRepositoryV2>();
            _SpotExceptionsRecommendedPlanRepositoryV2Mock = new Mock<ISpotExceptionsRecommendedPlanRepositoryV2>();

            _DataRepositoryFactoryMock.Setup(f => f.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>())
                .Returns(_SpotExceptionsOutOfSpecRepositoryV2Mock.Object);
            _DataRepositoryFactoryMock.Setup(f => f.GetDataRepository<ISpotExceptionsRecommendedPlanRepositoryV2>())
                .Returns(_SpotExceptionsRecommendedPlanRepositoryV2Mock.Object);

            _SpotExceptionsApiClientMock = new Mock<ISpotExceptionsApiClient>();

            _SpotExceptionsValidator = new SpotExceptionsValidator(_DataRepositoryFactoryMock.Object
                , _SpotExceptionsApiClientMock.Object);
        }

        [Test]
        public void ValidateDataAvailableForSync_NoData()
        {
            // Arrange
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.GetOutOfSpecDecisionQueuedCountAsync())
                .Returns(0);

            _SpotExceptionsRecommendedPlanRepositoryV2Mock.Setup(s => s.GetRecommendedPlanDecisionQueuedCountAsync())
                .Returns(0);

            // Act and Assert
            Assert.That(() => _SpotExceptionsValidator.ValidateDataAvailableForSync(),
                Throws.TypeOf<SpotExceptionsException>().With.Message.EqualTo("There are no decisions available for a sync. Any decisions previously submitted are being processed."));
        }

        [Test]
        public void ValidateSyncAlreadyRunning_Running()
        {
            // Arrange
            var response = new GetSyncStateResponseDto()
            {
                Result = new GetSyncStateResultDto()
                {
                    State = new GetSyncStateResponseStateDto()
                    {
                        State = "RUNNING",
                        StateMessage = "RUNNING FROM THE UNITTESTS",
                        UserCancelledOrTimeout = false
                    },
                    JobId = 1,
                    RunId = 1,
                    NumberInJobs = 1
                }
            };

            _SpotExceptionsApiClientMock.Setup(s => s.GetSyncStateAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(response));

            // Act and Assert
            Assert.That(async () => await _SpotExceptionsValidator.ValidateSyncAlreadyRunning(It.IsAny<int>()),
                Throws.TypeOf<SpotExceptionsException>().With.Message.EqualTo("A Sync process is currently running. Please try again later."));
        }
    }
}
