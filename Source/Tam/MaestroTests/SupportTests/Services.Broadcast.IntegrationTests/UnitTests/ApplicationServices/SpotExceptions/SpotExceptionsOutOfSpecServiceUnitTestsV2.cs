using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.SpotExceptions
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionsOutOfSpecServiceUnitTestsV2
    {
        private SpotExceptionsOutOfSpecServiceV2 _SpotExceptionsOutOfSpecService;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionsOutOfSpecRepository> _SpotExceptionsOutOfSpecRepositoryMock;
        private Mock<ISpotExceptionsOutOfSpecRepositoryV2> _SpotExceptionsOutOfSpecRepositoryV2Mock;
        private Mock<ISpotExceptionsOutOfSpecServiceV2> _SpotExceptionsOutOfSpecServiceV2Mock;
        private Mock<IPlanRepository> _PlanRepositoryMock;

        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IAabEngine> _AabEngine;
        private Mock<IGenreCache> _GenreCacheMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionsOutOfSpecRepositoryMock = new Mock<ISpotExceptionsOutOfSpecRepository>();
            _SpotExceptionsOutOfSpecRepositoryV2Mock = new Mock<ISpotExceptionsOutOfSpecRepositoryV2>();
            _SpotExceptionsOutOfSpecServiceV2Mock = new Mock<ISpotExceptionsOutOfSpecServiceV2>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();

            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _AabEngine = new Mock<IAabEngine>();
            _GenreCacheMock = new Mock<IGenreCache>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsOutOfSpecRepository>())
                .Returns(_SpotExceptionsOutOfSpecRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>())
                .Returns(_SpotExceptionsOutOfSpecRepositoryV2Mock.Object);


            _DataRepositoryFactoryMock
              .Setup(x => x.GetDataRepository<IPlanRepository>())
              .Returns(_PlanRepositoryMock.Object);

            _SpotExceptionsOutOfSpecService = new SpotExceptionsOutOfSpecServiceV2
                (
                    _DataRepositoryFactoryMock.Object,
                    _FeatureToggleMock.Object,
                    _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecReasonCodes_V2_ReasonCodes_DoNotExist()
        {
            // Arrange
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodesV2(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(new List<SpotExceptionsOutOfSpecReasonCodeDtoV2>()));
            var request = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(request);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecReasonCodes_V2_ReasonCodes_Exist()
        {
            // Arrange
            var request = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodesV2(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(new List<SpotExceptionsOutOfSpecReasonCodeDtoV2>
                {
                    new SpotExceptionsOutOfSpecReasonCodeDtoV2
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart",
                        Count = 10
                    },
                    new SpotExceptionsOutOfSpecReasonCodeDtoV2
                    {
                        Id = 3,
                        ReasonCode = 2,
                        Reason = "genre content restriction",
                        Label = "Genre",
                        Count = 20
                    },
                    new SpotExceptionsOutOfSpecReasonCodeDtoV2
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate",
                        Count = 30
                    }
                }));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecReasonCodes_V2_ThrowsException()
        {
            // Arrange
            var request = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodesV2(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(request));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Spot Reason Codes V2", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecInventorySources_V2_InventorySource_Exist()
        {
            // Arrange
            var request = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { "TBA" , "Ference Media" };
            List<string> outOfSpecDone = new List<string> { "TVB" , "Ference Media" };

            int expectedFerenceMediaCount = 2;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoInventorySourcesV2Async(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsDoneInventorySourcesV2Async(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(request);

            // Assert
            Assert.AreEqual(result[0].Count,expectedFerenceMediaCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecInventorySources_V2_InventorySource_DoesNotExist()
        {
            // Arrange
            var request = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { };
            List<string> outOfSpecDone = new List<string>();

            int expectedCount = 0;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoInventorySourcesV2Async(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsDoneInventorySourcesV2Async(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(request);

            // Assert
            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecInventorySources_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = new List<string> { "TBA" };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoInventorySourcesV2Async(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsDoneInventorySourcesV2Async(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Inventory Sources V2", result.Message);
        }
    }
}
