using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.SpotExceptions
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionsServiceUnitTests
    {
        private SpotExceptionsService _SpotExceptionsService;

        private Mock<ISpotExceptionsRepository> _SpotExceptionsRepositoryMock;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionsRepositoryMock = new Mock<ISpotExceptionsRepository>();

            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsRepository>())
                .Returns(_SpotExceptionsRepositoryMock.Object);

            _SpotExceptionsService = new SpotExceptionsService
                (
                    _DataRepositoryFactoryMock.Object,
                    _FeatureToggleMock.Object,
                    _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public async void TriggerDecisionSync_Exist()
        {
            // Arrange
            bool result = false;
            bool expectedResult = true;
            var spotExceptionSaveDecisionsPlansRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto();

            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto
            {
                UserName = "Test User"
            };

            var spotExceptionsOutOfSpecDecisionsPlansResult = new List<SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto>
            {
                new SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto
                {
                    Id = 21,
                    AcceptAsInSpec = true
                },
                new SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto
                {
                    Id = 22,
                    AcceptAsInSpec = true
                }
            };

            spotExceptionSaveDecisionsPlansRequest.Decisions.AddRange(spotExceptionsOutOfSpecDecisionsPlansResult);

            _SpotExceptionsRepositoryMock
                .Setup(s => s.SyncOutOfSpecDecisionsAsync(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(expectedResult));

            // Act
            result = await _SpotExceptionsService.TriggerDecisionSync(triggerDecisionSyncRequest);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void TriggerDecisionSync_DoesNotExist()
        {
            bool result = false;
            bool expectedResult = false;
            
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto
            {
                UserName = "Test User"
            };

            _SpotExceptionsRepositoryMock
                .Setup(s => s.SyncOutOfSpecDecisionsAsync(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(expectedResult));

            // Act
            result = await _SpotExceptionsService.TriggerDecisionSync(triggerDecisionSyncRequest);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TriggerDecisionSync_ThrowsException()
        {
            // Arrange
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto
            {
                UserName = "Test User"
            };

            _SpotExceptionsRepositoryMock
                .Setup(s => s.SyncOutOfSpecDecisionsAsync(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async() => await _SpotExceptionsService.TriggerDecisionSync(triggerDecisionSyncRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }

        [Test]
        public async void GetDecisionCount_Exist()
        {
            // Arrange
            int outOfSpecDecisioncount = 2;
            int recommendedPlanDecisioncount = 3;

            _SpotExceptionsRepositoryMock
                .Setup(s => s.GetOutOfSpecDecisionQueuedCountAsync())
                .Returns(Task.FromResult(outOfSpecDecisioncount));

            _SpotExceptionsRepositoryMock
                .Setup(s => s.GetRecommendedPlanDecisionQueuedCountAsync())
                .Returns(Task.FromResult(recommendedPlanDecisioncount));

            // Act
            var result = await _SpotExceptionsService.GetQueuedDecisionCount();

            // Assert
            Assert.AreEqual(result, 5);
        }

        [Test]
        public async void GetDecisionCount_DoesNotExist()
        {
            // Arrange
            int outOfSpecDecisioncount = 0;
            int recommendedPlanDecisioncount = 0;

            _SpotExceptionsRepositoryMock
                .Setup(s => s.GetOutOfSpecDecisionQueuedCountAsync())
                .Returns(Task.FromResult(outOfSpecDecisioncount));

            _SpotExceptionsRepositoryMock
                .Setup(s => s.GetRecommendedPlanDecisionQueuedCountAsync())
                .Returns(Task.FromResult(recommendedPlanDecisioncount));

            // Act
            var result = await _SpotExceptionsService.GetQueuedDecisionCount();

            // Assert
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void GetDecisionCount_ThrowsException()
        {
            // Arrange
            int outOfSpecDecisioncount = 0;

            _SpotExceptionsRepositoryMock
                .Setup(s => s.GetOutOfSpecDecisionQueuedCountAsync())
                .Returns(Task.FromResult(outOfSpecDecisioncount));

            _SpotExceptionsRepositoryMock
                .Setup(s => s.GetRecommendedPlanDecisionQueuedCountAsync())
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async() => await _SpotExceptionsService.GetQueuedDecisionCount());

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }
    }
}
