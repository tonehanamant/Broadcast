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
    public class SpotExceptionsOutOfSpecServiceUnitTests
    {
        private SpotExceptionsOutOfSpecService _SpotExceptionsOutOfSpecService;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionsOutOfSpecRepository> _SpotExceptionsOutOfSpecRepositoryMock;
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
              .Setup(x => x.GetDataRepository<IPlanRepository>())
              .Returns(_PlanRepositoryMock.Object);

            _SpotExceptionsOutOfSpecService = new SpotExceptionsOutOfSpecService
                (
                    _DataRepositoryFactoryMock.Object,
                    _DateTimeEngineMock.Object,
                    _AabEngine.Object,
                    _GenreCacheMock.Object,
                    _FeatureToggleMock.Object,
                    _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecPlans_BothExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutofSpecsPlansRequest = new SpotExceptionsOutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = _GetOutOfSpecGroupingToDoData();
            var outOfSpecDone = _GetOutOfSpecGroupingDoneData();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecGroupingAsync(spotExceptionsOutofSpecsPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 4);
            Assert.AreEqual(result.Completed.Count, 1);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecPlans_ToDoExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutofSpecsPlansRequest = new SpotExceptionsOutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = _GetOutOfSpecGroupingToDoData();
            List<SpotExceptionsOutOfSpecGroupingDto> outOfSpecDone = null;

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecGroupingAsync(spotExceptionsOutofSpecsPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 4);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecPlans_DoneExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutofSpecsPlansRequest = new SpotExceptionsOutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsOutOfSpecGroupingDto> outOfSpecToDo = null;
            var outOfSpecDone = _GetOutOfSpecGroupingDoneData();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecGroupingAsync(spotExceptionsOutofSpecsPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 1);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecPlans_NeitherExist()
        {
            // Arrange
            List<SpotExceptionsOutOfSpecGroupingDto> outOfSpecToDo = null;
            List<SpotExceptionsOutOfSpecGroupingDto> outOfSpecDone = null;

            SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutofSpecsPlansRequest = new SpotExceptionsOutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecGroupingAsync(spotExceptionsOutofSpecsPlansRequest);

            // Assert            
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlans_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutofSpecsPlansRequest = new SpotExceptionsOutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = _GetOutOfSpecGroupingToDoData();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecGroupingAsync(spotExceptionsOutofSpecsPlansRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Groupings", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecsPlanSpots_OutOfSpecPlanSpots_Exist()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 215,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();
            var outOfSpecDone = _GetOutOfSpecPlanDoneSpotsData();
            var timeZone = _GetTimeZones();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetMarketTimeZones())
                .Returns(timeZone);

            _PlanRepositoryMock
                .Setup(s => s.GetPlanDaypartsByPlanIds(It.IsAny<List<int>>()))
                .Returns(new List<PlanDaypartDetailsDto>
                    {
                        new PlanDaypartDetailsDto
                        {
                            PlanId=215,
                            Code="ROSP",
                            Name="ROS Programming"
                        },
                        new PlanDaypartDetailsDto
                        {
                            PlanId=215,
                            Code="TDNS",
                            Name="Total Day News and Syndication"
                        }
                    }
                );
            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotsAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Queued.Count, 1);
            Assert.AreEqual(result.Synced.Count, 1);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecsPlanSpots_OutOfSpecPlanSpots_DoesNotExist()
        {
            List<SpotExceptionsOutOfSpecsToDoDto> outOfSpecToDo = null;
            List<SpotExceptionsOutOfSpecsDoneDto> outOfSpecDone = null;

            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotsAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert            
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Queued.Count, 0);
            Assert.AreEqual(result.Synced.Count, 0);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlanSpots_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotsAsync(spotExceptionsOutOfSpecSpotsRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecInventorySources_Exist()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDo = new List<string> { "TBA" };
            List<string> outOfSpecDone = new List<string> { "TVB" };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecInventorySources_DuplicatesExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDo = new List<string> { "TBA", "TBA", "TVB" };
            List<string> outOfSpecDone = new List<string> { "TVB" };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecInventorySources_DoNotExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDo = new List<string> { };
            List<string> outOfSpecDone = new List<string>();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Assert.AreEqual(result.Count, 0);
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

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Inventory Sources", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecReasonCodes_ReasonCodes_DoNotExist()
        {
            // Arrange
            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodesAsync())
                .Returns(Task.FromResult(new List<SpotExceptionsOutOfSpecReasonCodeDto>()));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecReasonCodesAsync();

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecReasonCodes_ReasonCodes_Exist()
        {
            // Arrange
            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodesAsync())
                .Returns(Task.FromResult(new List<SpotExceptionsOutOfSpecReasonCodeDto>
                {
                    new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 3,
                        ReasonCode = 2,
                        Reason = "genre content restriction",
                        Label = "Genre"
                    },
                    new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    }
                }));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecReasonCodesAsync();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecReasonCodes_ThrowsException()
        {
            // Arrange
            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodesAsync())
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecReasonCodesAsync());

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Reason Codes", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecMarkets_DoNotExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDo = new List<string> { };
            List<string> outOfSpecDone = new List<string>();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecMarketsAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecMarkets_Exist()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 215,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDo = new List<string> { "Minot-Bsmrck-Dcknsn(Wlstn)" };
            List<string> outOfSpecDone = new List<string> { "Cincinnati" };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecMarketsAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecMarkets_DuplicateExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                PlanId = 215,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDo = new List<string> { "Minot-Bsmrck-Dcknsn(Wlstn)", "Minot-Bsmrck-Dcknsn(Wlstn)", "Cincinnati" };
            List<string> outOfSpecDone = new List<string> { "Cincinnati", "Cincinnati" };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecMarketsAsync(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecMarkets_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = new List<string> { "Minot-Bsmrck-Dcknsn(Wlstn)" };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneMarketsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecMarketsAsync(spotExceptionsOutOfSpecSpotsRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Markets", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecPrograms_ExistInProgramsOnly()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromProgramsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "WNRUSH",
                           GenreId=  33
                        }
                    }
                ));

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromSpotExceptionDecisionsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                {
                }
                ));

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 33,
                    Display = "Genre"
                });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecPrograms_ExistInDecisionsOnly()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromProgramsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                {
                }
                ));

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromSpotExceptionDecisionsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "#WNRUSH",
                           GenreId = 33
                        }
                    }
                ));

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 33,
                    Display = "Genre"
                });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecPrograms_ExistInProgramsOnly_NoGenre()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromProgramsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "WNRUSH",
                           GenreId = null
                        }
                    }
                ));

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromSpotExceptionDecisionsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                {
                }
                ));

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecPrograms_ExistInDecisionsOnly_NoGenre()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromProgramsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                {
                }
                ));

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromSpotExceptionDecisionsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "#WNRUSH",
                           GenreId = null
                        }
                    }
                ));

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecPrograms_DoesNotExist()
        {
            // Arrange
            string programNameQuery = "WNRUSH";
            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromProgramsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                {
                }
                ));

            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromSpotExceptionDecisionsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                {
                }
                ));

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 33,
                    Display = "Genre"
                });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecPrograms_ThrowsException()
        {
            // Arrange
            string programNameQuery = "WNRUSH";
            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromProgramsAsync(It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 1,
                    Display = "Genre"
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, "TestsUser"));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Programs", result.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void GetSpotExceptionsOutOfSpecAdvertisers_Exist()
        {
            // Arrange
            SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest = new SpotExceptionsOutOfSpecAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid?> outOfSpecToDo = new List<Guid?> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C") };
            List<Guid?> outOfSpecDone = new List<Guid?> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void GetSpotExceptionsOutOfSpecAdvertisers_DoesNotExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest = new SpotExceptionsOutOfSpecAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid?> outOfSpecToDo = new List<Guid?>();
            List<Guid?> outOfSpecDone = new List<Guid?>();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void GetSpotExceptionsOutOfSpecAdvertisers_DuplicateExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest = new SpotExceptionsOutOfSpecAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid?> outOfSpecToDo = new List<Guid?> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };
            List<Guid?> outOfSpecDone = new List<Guid?> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecAdvertisers_Unknown()
        {
            // Arrange
            SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest = new SpotExceptionsOutOfSpecAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid?> outOfSpecToDo = new List<Guid?> { new Guid() };
            List<Guid?> outOfSpecDone = new List<Guid?> { new Guid() };


            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecAdvertisers_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest = new SpotExceptionsOutOfSpecAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid?> outOfSpecToDo = new List<Guid?> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };
            List<Guid?> outOfSpecDone = new List<Guid?> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Advertisers", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutofSpecsStations_Exist()
        {
            // Arrange
            SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest = new SpotExceptionsOutofSpecsStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDoStations = new List<string> { "WDAY" };
            List<string> outOfSpecDoneStations = new List<string> { "KSTP" };


            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDoStations));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDoStations));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecStationsAsync(spotExceptionsOutofSpecsStationRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutofSpecsStations_DuplicateExist()
        {
            // Arrange
            SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest = new SpotExceptionsOutofSpecsStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDoStations = new List<string> { "WDAY", "WDAY" };
            List<string> outOfSpecDoneStations = new List<string> { "KSTP", "KSTP", "KSTP" };


            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDoStations));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDoStations));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecStationsAsync(spotExceptionsOutofSpecsStationRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsOutofSpecsStations_DoNotExist()
        {
            // Arrange
            SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest = new SpotExceptionsOutofSpecsStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDoStations = new List<string>();
            List<string> outOfSpecDoneStations = new List<string>();


            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDoStations));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDoStations));

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecStationsAsync(spotExceptionsOutofSpecsStationRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecStations_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest = new SpotExceptionsOutofSpecsStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> outOfSpecToDoStations = new List<string> { "WDAY" };
            List<string> outOfSpecDoneStations = new List<string> { "KSTP" };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDoStations));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecStationsAsync(spotExceptionsOutofSpecsStationRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_SaveOneToDoComment()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        TodoId = 14,
                        Comments = "Comment Saved By Unittests"
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.SaveOutOfSpecCommentsAsync(It.IsAny<List<SpotExceptionOutOfSpecCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(expectedResult));
            _SpotExceptionsOutOfSpecRepositoryMock
              .Setup(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int?>>()))
              .Returns(Task.FromResult(_GetOutOfSpecToDoData()));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_SaveMultipleToDoComments()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        TodoId = 14,
                        Comments = "Comment Saved By Unittests"
                    },
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        TodoId = 15,
                        Comments = "Comment Saved By Unittests"
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.SaveOutOfSpecCommentsAsync(It.IsAny<List<SpotExceptionOutOfSpecCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(expectedResult));
            _SpotExceptionsOutOfSpecRepositoryMock
            .Setup(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int?>>()))
            .Returns(Task.FromResult(_GetOutOfSpecToDoData()));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_SaveOneDoneComment()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        DoneId = 14,
                        Comments = "Comment Saved By Unittests"
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryMock
            .Setup(s => s.SaveOutOfSpecCommentsAsync(It.IsAny<List<SpotExceptionOutOfSpecCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Task.FromResult(expectedResult));
            _SpotExceptionsOutOfSpecRepositoryMock
            .Setup(s => s.GetOutOfSpecSpotsDoneByIds(It.IsAny<List<int?>>()))
            .Returns(Task.FromResult(_GetOutOfSpecDoneData()));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_SaveMultipleDoneComments()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        DoneId = 14,
                        Comments = "Comment Saved By Unittests"
                    },
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        DoneId = 15,
                        Comments = "Comment Saved By Unittests"
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryMock
            .Setup(s => s.SaveOutOfSpecCommentsAsync(It.IsAny<List<SpotExceptionOutOfSpecCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Task.FromResult(expectedResult));
            _SpotExceptionsOutOfSpecRepositoryMock
            .Setup(s => s.GetOutOfSpecSpotsDoneByIds(It.IsAny<List<int?>>()))
            .Returns(Task.FromResult(_GetOutOfSpecDoneData()));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_AcceptInSpecOneToDoDecision()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        TodoId = 1,
                        AcceptAsInSpec = true
                    }
                }
            };
            var getOutOfSpecToDoData = new List<SpotExceptionsOutOfSpecsToDoDto>()
            {
                new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = 1,
                    ReasonCodeMessage="",
                    EstimateId =191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    DaypartCode="PT",
                    GenreName="Horror",
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70642,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
                  HouseIsci = "289J76GN16H"
                }               
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int?>>()))
                .Returns(Task.FromResult(getOutOfSpecToDoData));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_AcceptOutOfSpecOneToDoDecision()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        TodoId = 14,
                        AcceptAsInSpec = false
                    }
                }
            };

            var existingSpotExceptionOutOfSpecToDo = new List<SpotExceptionsOutOfSpecsToDoDto>()
            {
                new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "ET",
                    DMA = 58,
                    Comments = null,
                    InventorySourceName = "Business First AM"
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int?>>()))
                .Returns(Task.FromResult(existingSpotExceptionOutOfSpecToDo));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_AcceptMultipleToDoDecisions()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        TodoId = 14,
                        AcceptAsInSpec = true
                    },
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        TodoId = 15,
                        AcceptAsInSpec = true
                    }
                }
            };

            var existingSpotExceptionOutOfSpecToDo = new List<SpotExceptionsOutOfSpecsToDoDto>()
            {
                new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "CT",
                    DMA = 58,
                    Comments = null,
                    InventorySourceName = "Business First AM"
                },
                new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = 15,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "MT",
                    DMA = 58,
                    Comments = null,
                    InventorySourceName = "Business First AM"
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int?>>()))
                .Returns(Task.FromResult(existingSpotExceptionOutOfSpecToDo));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_AcceptInSpecOneDoneDecision()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        DoneId = 14,
                        AcceptAsInSpec = true
                    }
                }
            };

            var isSpotExceptionsOutOfSpecDoneDecisionSaved = true;

            string userName = "Test User";
            bool expectedResult = true;

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2023, 01, 01));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(It.IsAny<List<SpotExceptionsOutOfSpecDoneDecisionsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(isSpotExceptionsOutOfSpecDoneDecisionSaved));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_AcceptOutOfSpecOneDoneDecision()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        DoneId = 14,
                        AcceptAsInSpec = false
                    }
                }
            };

            var isSpotExceptionsOutOfSpecDoneDecisionSaved = true;

            string userName = "Test User";
            bool expectedResult = true;

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2023, 01, 01));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(It.IsAny<List<SpotExceptionsOutOfSpecDoneDecisionsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(isSpotExceptionsOutOfSpecDoneDecisionSaved));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_AcceptMultipleDoneDecisions()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecSaveDecisionsRequestDto
            {
                Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>
                {
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        DoneId = 14,
                        AcceptAsInSpec = true
                    },
                    new SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
                    {
                        DoneId = 15,
                        AcceptAsInSpec = true
                    }
                }
            };

            var isSpotExceptionsOutOfSpecDoneDecisionSaved = true;

            string userName = "Test User";
            bool expectedResult = true;

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2023, 01, 01));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(It.IsAny<List<SpotExceptionsOutOfSpecDoneDecisionsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(isSpotExceptionsOutOfSpecDoneDecisionSaved));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase("ActionAdventure", 1, "ActionAdventure", "", "")]
        [TestCase("Action Adventure", 1, "Action Adventure", "", "")]
        [TestCase("Action/Adventure", 3, "Action", "Action/Adventure", "Adventure")]
        [TestCase("Adventure/Action", 3, "Action", "Adventure", "Adventure/Action")]
        [TestCase("Adventure / Action", 3, "Action", "Adventure", "Adventure / Action")]
        public void HandleFlexGenres(string genre, int expectedResultCount, string resultFirstEntry, string resultSecondEntry, string resultThirdEntry)
        {
            // Act
            var result = SpotExceptionsOutOfSpecService.HandleFlexGenres(genre);

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            if (expectedResultCount > 1)
            {
                Assert.AreEqual(resultFirstEntry, result[0]);
                Assert.AreEqual(resultSecondEntry, result[1]);
                Assert.AreEqual(resultThirdEntry, result[2]);
            }
        }

        private List<SpotExceptionsOutOfSpecGroupingDto> _GetOutOfSpecGroupingToDoData()
        {
            return new List<SpotExceptionsOutOfSpecGroupingDto>()
            {
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    PlanId = 322,
                    AdvertiserMasterId = new Guid("C56972B6-67F2-4986-A2BE-4B1F199A1420"),
                    PlanName = "4Q'21 Dupixent Early Morning News",
                    AffectedSpotsCount = 1,
                    Impressions = 55,
                    FlightStartDate = new DateTime(2021, 10, 4),
                    FlightEndDate = new DateTime(2021, 12, 19),
                    SpotLengths = new List<SpotLengthDto>
                    {
                        new SpotLengthDto
                        {
                            Length = 15,
                        }
                    },
                    AudienceName = "Women 25-54"
                },
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    PlanId = 323,
                    AdvertiserMasterId = new Guid("C56972B6-67F2-4986-A2BE-4B1F199A1420"),
                    PlanName = "4Q'21 Dupixent Daytime",
                    AffectedSpotsCount = 1,
                    Impressions = 5,
                    FlightStartDate = new DateTime(2021, 10, 4),
                    FlightEndDate = new DateTime(2021, 12, 19),
                    SpotLengths = new List<SpotLengthDto>
                    {
                        new SpotLengthDto
                        {
                            Length = 15,
                        }
                    },
                    AudienceName = "Women 25-54"
                },
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    PlanId = 332,
                    AdvertiserMasterId = new Guid( "B67C3C7B-E4F9-42AD-8EA5-CF18C83C61C0" ),
                    PlanName = "4Q'21 Macy's EM",
                    AffectedSpotsCount = 6,
                    Impressions = 217,
                    FlightStartDate = new DateTime(2021, 10, 4),
                    FlightEndDate = new DateTime(2021, 12, 19),
                    SpotLengths = new List<SpotLengthDto>
                    {
                        new SpotLengthDto
                        {
                            Length = 15,
                        }
                    },
                    AudienceName = "Women 25-54"
                },
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    PlanId = 334,
                    AdvertiserMasterId = new Guid( "C56972B6-67F2-4986-A2BE-4B1F199A1420" ),
                    PlanName = "4Q'21 Macy's SYN",
                    AffectedSpotsCount = 6,
                    Impressions = 237,
                    FlightStartDate = new DateTime(2021, 9, 28),
                    FlightEndDate = new DateTime(2021, 12, 06),
                    SpotLengths = new List<SpotLengthDto>
                    {
                        new SpotLengthDto
                        {
                            Length = 15,
                        }
                    },
                    AudienceName = "Women 25-54"
                }
            };
        }

        private List<SpotExceptionsOutOfSpecGroupingDto> _GetOutOfSpecGroupingDoneData()
        {
            return new List<SpotExceptionsOutOfSpecGroupingDto>()
            {
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    PlanId = 334,
                    AdvertiserMasterId = new Guid( "C56972B6-67F2-4986-A2BE-4B1F199A1420" ),
                    PlanName = "4Q'21 Macy's SYN",
                    AffectedSpotsCount = 1,
                    Impressions = 10,
                    FlightStartDate = new DateTime(2021, 9, 28),
                    FlightEndDate = new DateTime(2021, 12, 06),
                    SpotLengths = new List<SpotLengthDto>
                    {
                        new SpotLengthDto
                        {
                            Length = 15,
                        }
                    },
                    AudienceName = "Women 25-54"
                }
            };
        }

        private List<SpotExceptionsOutOfSpecsToDoDto> _GetOutOfSpecToDoData()
        {
            return new List<SpotExceptionsOutOfSpecsToDoDto>()
            {
                new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = 1,
                    ReasonCodeMessage="",
                    EstimateId =191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    DaypartCode="PT",
                    GenreName="Horror",
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70642,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
                  HouseIsci = "289J76GN16H"
                },
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  Id = 2,
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 215,
                  RecommendedPlanName="4Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="Reynolds Foil @9",
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                  StationLegacyCallLetters="KSTP",
                  Affiliate = "NBC",
                  Market = "Phoenix (Prescott)",
                  PlanId = 215,
                   SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                  AudienceId= 426,
                  Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70642,
                        Code = "CUS"
                    },
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartCode="PT",
                  GenreName="Horror",
                  ProgramNetwork = "",
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  IngestedMediaWeekId = 1,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 3,
                        ReasonCode = 2,
                        Reason = "genre content restriction",
                        Label = "Genre"
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                  HouseIsci = "289J76GN16H"
                }
            };
        }

        private List<SpotExceptionsOutOfSpecsDoneDto> _GetOutOfSpecDoneData()
        {
            return new List<SpotExceptionsOutOfSpecsDoneDto>()
            {
                new SpotExceptionsOutOfSpecsDoneDto
                {
                  Id = 3,
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 218,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                  StationLegacyCallLetters = "KSTP",
                  Affiliate = "ABC",
                  Market = "Lincoln & Hastings-Krny",
                  PlanId = 218,
                   SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                  AudienceId= 426,
                  Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70642,
                        Code = "CUS"
                    },
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartCode="PT",
                  GenreName="Horror",
                  ProgramNetwork = "",
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  IngestedMediaWeekId = 1,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
                  HouseIsci = "289J76GN16H"
                },
                new SpotExceptionsOutOfSpecsDoneDto
                {
                  Id = 4,
                  ReasonCodeMessage="",
                  EstimateId= 191759,
                  IsciName="AB44NR59",
                  RecommendedPlanId= 11726,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                  StationLegacyCallLetters = "KSTP",
                  Affiliate = "ABC",
                  Market = "Lincoln & Hastings-Krny",
                  PlanId = 218,
                  SpotLength = new SpotLengthDto
                  {
                    Id = 16,
                    Length = 45
                  },
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartCode="PT",
                  GenreName="Horror",
                  Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70642,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  IngestedMediaWeekId = 1,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxOTY3MA==",
                    HouseIsci = "289J76GN16H"
                }
            };
        }

        private List<SpotExceptionsOutOfSpecsToDoDto> _GetOutOfSpecPlanSpotsData()
        {
            return new List<SpotExceptionsOutOfSpecsToDoDto>()
            {
                new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = 3,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                    HouseIsci = "289J76GN16H",
                    Comments = "test Comment",
                    GenreName="Comedy",
                    DaypartCode="ROSP",
                    InventorySourceName = "TVB",
                    TimeZone="ET",
                    MarketCode=351,
                    MarketRank = 1
                },
                new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = 4,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Cincinnati",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
                    HouseIsci = "289J76GN16H",
                    Comments = "test Comment",
                    InventorySourceName = "TVB",
                    TimeZone="ET",
                    MarketCode=352,
                    MarketRank = 2
                }               
            };
        }

        private List<SpotExceptionsOutOfSpecsDoneDto> _GetOutOfSpecPlanDoneSpotsData()
        {
            return new List<SpotExceptionsOutOfSpecsDoneDto>()
            {
                new SpotExceptionsOutOfSpecsDoneDto
                {
                    Id = 1,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                        SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    SpotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsDto
                    {
                        SpotExceptionsOutOfSpecDoneId = 1,
                        AcceptedAsInSpec = true,
                        DecisionNotes = "",
                        DecidedBy = "MockData",
                        DecidedAt = new DateTime(2020, 2, 1),
                        SyncedBy = "MockData",
                        SyncedAt = new DateTime(2020,1,10,23,45,00)
                    },
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDYzNg==",
                    HouseIsci = "289J76GN16H",
                    Comments = "test Comment",
                    InventorySourceName = "TVB",
                    TimeZone="ET",
                    MarketCode=351,
                    MarketRank = 2
                },
                new SpotExceptionsOutOfSpecsDoneDto
                {
                    Id = 2,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                        SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    SpotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsDto
                    {
                        SpotExceptionsOutOfSpecDoneId = 2,
                        AcceptedAsInSpec = true,
                        DecisionNotes = "",
                        DecidedBy = "MockData",
                        DecidedAt = new DateTime(2020, 2, 1),
                        SyncedBy = null,
                        SyncedAt = null
                    },
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
                    HouseIsci = "289J76GN16H",
                    Comments = "test Comment",
                    InventorySourceName = "TVB",
                    TimeZone="CT",
                    MarketCode=352,
                    MarketRank = 4
                }
            };
        }

        private List<MarketTimeZoneDto> _GetTimeZones()
        {
            return new List<MarketTimeZoneDto>()
            {
                new MarketTimeZoneDto
                {
                    Name="Central",
                    MarketCode=351,
                    Code="CT"
                },
                new MarketTimeZoneDto
                {
                    Name="Eastern",
                    MarketCode=352,
                    Code="ET"
                },
                new MarketTimeZoneDto
                {
                    Name="Pacific",
                    MarketCode=353,
                    Code="PT"
                }
            };
        }

    }
}
