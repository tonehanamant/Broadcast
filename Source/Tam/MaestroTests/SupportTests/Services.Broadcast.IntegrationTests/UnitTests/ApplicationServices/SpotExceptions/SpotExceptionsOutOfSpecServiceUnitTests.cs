﻿using ApprovalTests;
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

            var outOfSpecToDo = _GetOutOfSpecToDoData();
            var outOfSpecDone = _GetOutOfSpecDoneData();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecsPlansAsync(spotExceptionsOutofSpecsPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 1);
            Assert.AreEqual(result.Completed.Count, 2);
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

            var outOfSpecToDo = _GetOutOfSpecToDoData();
            List<SpotExceptionsOutOfSpecsDoneDto> outOfSpecDone = null;

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecsPlansAsync(spotExceptionsOutofSpecsPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 1);
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

            List<SpotExceptionsOutOfSpecsToDoDto> outOfSpecToDo = null;
            var outOfSpecDone = _GetOutOfSpecDoneData();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecsPlansAsync(spotExceptionsOutofSpecsPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 2);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecPlans_NeitherExist()
        {
            // Arrange
            List<SpotExceptionsOutOfSpecsToDoDto> outOfSpecToDo = null;
            List<SpotExceptionsOutOfSpecsDoneDto> outOfSpecDone = null;

            SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutofSpecsPlansRequest = new SpotExceptionsOutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecsPlansAsync(spotExceptionsOutofSpecsPlansRequest);

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

            var outOfSpecToDo = _GetOutOfSpecToDoData();

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecsPlansAsync(spotExceptionsOutofSpecsPlansRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
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

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsToDoAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryMock
                .Setup(x => x.GetOutOfSpecSpotsDoneAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

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
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
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
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
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
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsOutOfSpecPrograms()
        {
            // Arrange
            string programNameQuery = "WNRUSH";
            _SpotExceptionsOutOfSpecRepositoryMock.Setup(s => s.FindProgramFromProgramsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "#HTOWNRUSH",
                           GenreId=  33
                        }
                    }
                ));

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 1,
                    Display = "Genre"
                });

            // Act           
            var result = await _SpotExceptionsOutOfSpecService.GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
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
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
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
                    InventorySourceName = "TVB"
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
                    InventorySourceName = "TVB"
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
                    InventorySourceName = "TVB"
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
                    InventorySourceName = "TVB"
                }
            };
        }
    }
}