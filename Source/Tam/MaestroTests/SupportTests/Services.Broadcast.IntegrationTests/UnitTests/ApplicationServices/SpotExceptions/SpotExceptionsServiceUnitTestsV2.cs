using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.DecisionSync;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using Services.Broadcast.Validators;
using System;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.SpotExceptions
{
    [TestFixture]
    public class SpotExceptionsServiceUnitTestsV2
    {
        private SpotExceptionsServiceV2 _SpotExceptionsServiceV2;

        private Mock<ISpotExceptionsRepositoryV2> _SpotExceptionsRepositoryV2Mock;

        private Mock<ISpotExceptionsApiClient> _SpotExceptionsApiClientMock;

        private Mock<ISpotExceptionsValidator> _SpotExceptionsValidatorMock;

        private Mock<IDateTimeEngine> _DateTimeEngineMock;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionsRepositoryV2Mock = new Mock<ISpotExceptionsRepositoryV2>();

            _SpotExceptionsApiClientMock = new Mock<ISpotExceptionsApiClient>();

            _SpotExceptionsValidatorMock = new Mock<ISpotExceptionsValidator>();

            _DateTimeEngineMock = new Mock<IDateTimeEngine>();

            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();

            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsRepositoryV2>())
                .Returns(_SpotExceptionsRepositoryV2Mock.Object);

            _SpotExceptionsServiceV2 = new SpotExceptionsServiceV2
                (
                    _DataRepositoryFactoryMock.Object,
                    _SpotExceptionsApiClientMock.Object,
                    _SpotExceptionsValidatorMock.Object,
                    _DateTimeEngineMock.Object,
                    _FeatureToggleMock.Object,
                    _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public async Task TriggerDecisionSync_NoRunningSyncRecorded()
        {
            // Arrange
            var runningSyncRunId = 0;
            bool notifySucceeded = true;
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto { UserName = "Test User" };

            _SpotExceptionsRepositoryV2Mock.Setup(s => s.GetRunningSyncRunId())
                .Returns(Task.FromResult(runningSyncRunId));

            _SpotExceptionsApiClientMock.Setup(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()))
                .Returns(Task.FromResult(notifySucceeded));

            // Act
            var result = await _SpotExceptionsServiceV2.TriggerDecisionSyncAsync(triggerDecisionSyncRequest);

            // Assert
            _SpotExceptionsApiClientMock.Verify(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()), Times.Once);
            Assert.True(result);
        }

        [Test]
        public async Task TriggerDecisionSync_RunningSyncRecorded()
        {
            // Arrange
            var runningSyncRunId = 1;
            var response = new GetSyncStateResponseDto()
            {
                Result = new GetSyncStateResultDto()
                {
                    State = new GetSyncStateResponseStateDto()
                    {
                        State = "TERMINATED",
                        StateMessage = "TERMINATED FROM THE UNITTESTS",
                        UserCancelledOrTimeout = false
                    },
                    JobId = 1,
                    RunId = 1,
                    NumberInJobs = 1
                }
            };
            bool notifySucceeded = true;
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto { UserName = "Test User" };

            _SpotExceptionsRepositoryV2Mock.Setup(s => s.GetRunningSyncRunId())
                .Returns(Task.FromResult(runningSyncRunId));

            _SpotExceptionsValidatorMock.Setup(s => s.ValidateSyncAlreadyRunning(It.IsAny<int>()))
                .Returns(Task.FromResult(response));

            _SpotExceptionsApiClientMock.Setup(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()))
                .Returns(Task.FromResult(notifySucceeded));

            // Act
            var result = await _SpotExceptionsServiceV2.TriggerDecisionSyncAsync(triggerDecisionSyncRequest);

            // Assert
            _SpotExceptionsApiClientMock.Verify(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()), Times.Once);
            Assert.True(result);
        }

        [Test]
        public async Task TriggerDecisionSync_NoDataException()
        {
            // Arrange
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto { UserName = "Test User" };

            _SpotExceptionsValidatorMock.Setup(s => s.ValidateDataAvailableForSync())
                .Throws(new Exception("No decisions needing synced. Any decisions made so far are already being processed."));

            Exception caught = null;

            // Act
            try
            {
                await _SpotExceptionsServiceV2.TriggerDecisionSyncAsync(triggerDecisionSyncRequest);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.NotNull(caught);
            _SpotExceptionsApiClientMock.Verify(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()), Times.Never);
        }

        [Test]
        public async Task TriggerDecisionSync_SyncAlreadyRunningException()
        {
            // Arrange
            var runningSyncRunId = 1;
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto { UserName = "Test User" };

            _SpotExceptionsRepositoryV2Mock.Setup(s => s.GetRunningSyncRunId())
                .Returns(Task.FromResult(runningSyncRunId));

            _SpotExceptionsValidatorMock.Setup(s => s.ValidateSyncAlreadyRunning(It.IsAny<int>()))
                .Throws(new Exception("Sync is already running. Please try again later."));

            Exception caught = null;

            // Act
            try
            {
                await _SpotExceptionsServiceV2.TriggerDecisionSyncAsync(triggerDecisionSyncRequest);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.NotNull(caught);
            _SpotExceptionsApiClientMock.Verify(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()), Times.Never);
        }

        [Test]
        public async Task TriggerDecisionSync_ThrowsException()
        {
            // Arrange
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto { UserName = "Test User" };

            _SpotExceptionsApiClientMock.Setup(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()))
                .Throws(new Exception("Test Exception: TriggerDecisionSync_ThrowsException"));

            Exception caught = null;

            // Act
            try
            {
                await _SpotExceptionsServiceV2.TriggerDecisionSyncAsync(triggerDecisionSyncRequest);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.NotNull(caught);
            _SpotExceptionsApiClientMock.Verify(s => s.PublishSyncRequestAsync(It.IsAny<ResultsSyncRequest>()), Times.Once);
        }
    }
}
