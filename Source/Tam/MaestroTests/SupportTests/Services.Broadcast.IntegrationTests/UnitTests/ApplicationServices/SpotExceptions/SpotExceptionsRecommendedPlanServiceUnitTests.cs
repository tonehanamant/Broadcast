using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
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
    public class SpotExceptionsRecommendedPlanServiceUnitTests
    {
        private SpotExceptionsRecommendedPlanService _SpotExceptionsRecommendedPlanService;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionsRecommendedPlanRepository> _SpotExceptionsRecommendedPlanRepositoryMock;
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;

        private Mock<IDateTimeEngine> _DateTimeEngine;
        private Mock<IAabEngine> _AabEngine;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionsRecommendedPlanRepositoryMock = new Mock<ISpotExceptionsRecommendedPlanRepository>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();

            _DateTimeEngine = new Mock<IDateTimeEngine>();
            _AabEngine = new Mock<IAabEngine>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsRecommendedPlanRepository>())
                .Returns(_SpotExceptionsRecommendedPlanRepositoryMock.Object);
            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepositoryMock.Object);

            _SpotExceptionsRecommendedPlanService = new SpotExceptionsRecommendedPlanService(_DataRepositoryFactoryMock.Object,
                _DateTimeEngine.Object,
                _AabEngine.Object,
                _FeatureToggleMock.Object,
                _ConfigurationSettingsHelperMock.Object);
        }

        [Test]
        public async void GetRecommendedPlanGroupingAsync_BothExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlansToDo = _GetRecommendedPlanGroupingToDoData();
            var recommendedPlansDone = _GetRecommendedPlanGroupingDoneData();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanGroupingAsync(recommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 4);
            Assert.AreEqual(result.Completed.Count, 1);
        }

        [Test]
        public async void GetRecommendedPlanGroupingAsync_ToDoExistOnly()
        {
            // Arrange
            SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlansToDo = _GetRecommendedPlanGroupingToDoData();
            List<SpotExceptionsRecommendedPlanGroupingDto> recommendedPlansDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanGroupingAsync(recommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 4);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public async void GetRecommendedPlanGroupingAsync_DoneExistOnly()
        {
            // Arrange
            SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsRecommendedPlanGroupingDto> recommendedPlansToDo = null;
            var recommendedPlansDone = _GetRecommendedPlanGroupingDoneData();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanGroupingAsync(recommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 1);
        }

        [Test]
        public async void GetRecommendedPlanGroupingAsync_DoesNotExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsRecommendedPlanGroupingDto> recommendedPlansToDo = null;
            List<SpotExceptionsRecommendedPlanGroupingDto> recommendedPlansDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanGroupingAsync(recommendedPlansRequest);

            // Assert            
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public void GetRecommendedPlanGroupingAsync_ThrowsException()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);

            SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var advertiser = new AdvertiserDto
            {
                Id = 2,
                MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                Name = "Name2"
            };

            var recommendedPlansToDo = _GetRecommendedPlanGroupingToDoData();
            var recommendedPlansDone = _GetRecommendedPlanGroupingDoneData();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanGroupingDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanGroupingAsync(recommendedPlansRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plan Groupings", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_BothExist()
        {
            //Arrange
            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
            {
                PlanId = 332,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlanSpotsToDo = _GetRecommendedPlanSpotsToDoData();
            var recommendedPlanSpotsDone = _GetRecommendedPlanSpotsDoneData();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsSynced(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 1);
            Assert.AreEqual(result.Queued.Count, 0);
            Assert.AreEqual(result.Synced.Count, 1);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_ToDoExistOnly()
        {
            //Arrange
            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
            {
                PlanId = 332,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlanSpotsToDo = _GetRecommendedPlanSpotsToDoData();
            List<SpotExceptionsRecommendedPlanSpotsDto> recommendedPlanSpotsDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsQueued(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsSynced(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 1);
            Assert.AreEqual(result.Queued.Count, 0);
            Assert.AreEqual(result.Synced.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_DoneExistOnly()
        {
            //Arrange
            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
            {
                PlanId = 332,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsRecommendedPlanSpotsDto> recommendedPlanSpotsToDo = null;
            var recommendedPlanSpotsQueued = _GetRecommendedPlanSpotsDoneData();
            List<SpotExceptionsRecommendedPlanSpotsDto> recommendedPlanSpotsSynced = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsQueued(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsQueued));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsSynced(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsSynced));

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Queued.Count, 1);
            Assert.AreEqual(result.Synced.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_DoesNotExist()
        {
            //Arrange
            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
            {
                PlanId = 332,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<SpotExceptionsRecommendedPlanSpotsDto> recommendedPlanSpotsToDo = null;
            List<SpotExceptionsRecommendedPlanSpotsDto> recommendedPlanSpotsQueued = null;
            List<SpotExceptionsRecommendedPlanSpotsDto> recommendedPlanSpotsSynced = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsQueued(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsQueued));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsSynced(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsSynced));

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Queued.Count, 0);
            Assert.AreEqual(result.Synced.Count, 0);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanSpots_ThrowsException()
        {
            //Arrange
            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
            {
                PlanId = 332,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlanSpotsToDo = _GetRecommendedPlanSpotsToDoData();
            var recommendedPlanSpotsDone = _GetRecommendedPlanSpotsDoneData();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsQueued(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsSynced(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plan Spots", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanDetails_DoesNotExist()
        {
            //Arrange
            var recommendedPlanId = 772;

            var advertiser = new AdvertiserDto
            {
                Id = 2,
                MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                Name = "Name2"
            };

            SpotExceptionsRecommendedPlanSpotsToDoDto recommendedPlanDetailsToDo = null;
            SpotExceptionsRecommendedPlanSpotsDoneDto recommendedPlanDetailsDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsToDoById(It.IsAny<int>()))
                .Returns(Task.FromResult(recommendedPlanDetailsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsDoneById(It.IsAny<int>()))
                .Returns(Task.FromResult(recommendedPlanDetailsDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanDetailsAsync(recommendedPlanId);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanDetailsToDo_Exist()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";
            var recommendedPlanId = 332;

            var advertiser = new AdvertiserDto
            {
                Id = 2,
                MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                Name = "Name2"
            };

            var recommendedPlanDetailsToDo = _GetRecommendedDetailToDoData(ingestedBy, ingestedAt);
            var ecommendedPlanDetailsDone = _GetRecommendedDetailDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsToDoById(It.IsAny<int>()))
                .Returns(Task.FromResult(recommendedPlanDetailsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsDoneById(It.IsAny<int>()))
                .Returns(Task.FromResult(ecommendedPlanDetailsDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanDetailsAsync(recommendedPlanId);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanDetailsDone_Exist()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";
            var recommendedPlanId = 334;

            var advertiser = new AdvertiserDto
            {
                Id = 2,
                MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                Name = "Name2"
            };

            var recommendedPlanDetailsToDo = _GetRecommendedDetailToDoData(ingestedBy, ingestedAt);
            var ecommendedPlanDetailsDone = _GetRecommendedDetailDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsToDoById(It.IsAny<int>()))
                .Returns(Task.FromResult(recommendedPlanDetailsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsDoneById(It.IsAny<int>()))
                .Returns(Task.FromResult(ecommendedPlanDetailsDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanDetailsAsync(recommendedPlanId);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanDetails_ThrowsException()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";
            var recommendedPlanId = 334;

            var advertiser = new AdvertiserDto
            {
                Id = 2,
                MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                Name = "Name2"
            };

            var recommendedPlanDetailsToDo = _GetRecommendedDetailToDoData(ingestedBy, ingestedAt);
            var ecommendedPlanDetailsDone = _GetRecommendedDetailDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsToDoById(It.IsAny<int>()))
                .Returns(Task.FromResult(recommendedPlanDetailsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanDetailsDoneById(It.IsAny<int>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanDetailsAsync(recommendedPlanId));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plan Details", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanAdvertisers_Exist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto recommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C") };
            List<Guid> recommendedPlanDone = new List<Guid> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanAdvertisersAsync(recommendedPlanAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanAdvertisers_DuplicateExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto recommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };
            List<Guid> recommendedPlanDone = new List<Guid> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanAdvertisersAsync(recommendedPlanAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanAdvertisers_DoesNotExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto recommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid>();
            List<Guid> recommendedPlanDone = new List<Guid>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanAdvertisersAsync(recommendedPlanAdvertisersRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanAdvertisers_ThrowsException()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto recommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };
            List<Guid> recommendedPlanDone = new List<Guid> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanAdvertisersDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanAdvertisersAsync(recommendedPlanAdvertisersRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plan Advertisers", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanStations_Exist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationsRequestDto recommendedPlanStationsRequest = new SpotExceptionsRecommendedPlanStationsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string> { "KSTP" };
            List<string> recommendedPlanDone = new List<string> { "WDAY" };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanStationsAsync(recommendedPlanStationsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanStations_DuplicateExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationsRequestDto recommendedPlanStationsRequest = new SpotExceptionsRecommendedPlanStationsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string> { "KSTP", "KSTP", "WDAY" };
            List<string> recommendedPlanDone = new List<string> { "WDAY", "WDAY" };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanStationsAsync(recommendedPlanStationsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanStations_DoNotExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationsRequestDto recommendedPlanStationsRequest = new SpotExceptionsRecommendedPlanStationsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string>();
            List<string> recommendedPlanDone = new List<string>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanStationsAsync(recommendedPlanStationsRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanStations_ThrowsException()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationsRequestDto recommendedPlanStationsRequest = new SpotExceptionsRecommendedPlanStationsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string>();
            List<string> recommendedPlanDone = new List<string>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanStationsDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanStationsAsync(recommendedPlanStationsRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plan Stations", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanFilters_Exist()
        {
            //Arrange
            SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> marketFilters = new List<string>() { "Columbus, OH", "Paducah-Cape Girard-Harsbg", "Seattle-Tacoma" };
            List<string> legacyCallLettersFilters = new List<string>() { "KOMO", "WBNS", "WDKA" };
            List<string> inventorysourceFilters = new List<string>() { "Ference POD", "Sinclair Corp - Day Syn 10a-4p", "Tegna" };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanMarketFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(marketFilters));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanLegacyCallLetterFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(legacyCallLettersFilters));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanInventorySourceFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(inventorysourceFilters));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanFilters(recommendedPlanFiltersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Markets.Count, 3);
            Assert.AreEqual(result.Stations.Count, 3);
            Assert.AreEqual(result.InventorySources.Count, 3);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanFilters_DoesNotExist()
        {
            //Arrange
            SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> marketFilters = new List<string>();
            List<string> legacyCallLettersFilters = new List<string>();
            List<string> inventorysourceFilters = new List<string>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanMarketFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(marketFilters));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanLegacyCallLetterFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(legacyCallLettersFilters));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanInventorySourceFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(inventorysourceFilters));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanFilters(recommendedPlanFiltersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Markets.Count, 0);
            Assert.AreEqual(result.Stations.Count, 0);
            Assert.AreEqual(result.InventorySources.Count, 0);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanFilters_ThrowsException()
        {
            //Arrange
            SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> marketFilters = new List<string>() { "Columbus, OH", "Paducah-Cape Girard-Harsbg", "Seattle-Tacoma" };
            List<string> legacyCallLettersFilters = new List<string>() { "KOMO", "WBNS", "WDKA" };
            List<string> inventorysourceFilters = new List<string>() { "Ference POD", "Sinclair Corp - Day Syn 10a-4p", "Tegna" };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanMarketFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(marketFilters));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanLegacyCallLetterFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(legacyCallLettersFilters));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanInventorySourceFiltersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanFilters(recommendedPlanFiltersRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plan Filters", result.Message);
        }

        [Test]
        public async void SaveSpotExceptionsRecommendedPlanDecisions_PostOneToDoDecisionToIsRecommended()
        {
            // Arrange
            var recommendedPlanDecisionsSaveRequest = new SpotExceptionsRecommendedPlanSaveDecisionsRequestDto
            {
                SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
                {
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        TodoId = 14,
                        SelectedPlanId = 15
                    }
                }
            };

            var existingSpotExceptionRecommendedPlanToDo = new List<SpotExceptionsRecommendedPlanSpotsToDoDto>()
            {
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "221216164429TQcO66uETE",
                    EstimateId = 7523,
                    InventorySource = "Ference POD",
                    HouseIsci = "523NVE0005H",
                    ClientIsci = "RNVE0005000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    StationLegacyCallLetters = "WZBJ",
                    Affiliate = "IND",
                    MarketCode = 173,
                    MarketRank = 71,
                    ProgramName = "ToRemove",
                    ProgramGenre = null,
                    IngestedBy = "DatabricksDatalakeAcct",
                    IngestedAt = new DateTime(2022, 12, 12),
                    IngestedMediaWeekId = 989,
                    SpotLength =
                    {
                        Id = 2,
                        Length = 60
                    },
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            Id = 14,
                            SpotExceptionsRecommendedPlanId = 14,
                            RecommendedPlanId = 14,
                            ExecutionTraceId = 8854,
                            Rate = 0,
                            AudienceName = "Adults 25-54",
                            ContractedImpressions = 13692000.0,
                            DeliveredImpressions = 11528522.180647785,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "DAY",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 270.87375,
                            PlanTotalContractedImpressions = 126836000.0,
                            PlanTotalDeliveredImpressions = 70837755.548371226,
                            IngestedMediaWeekId = 989,
                            IngestedBy = "Test User",
                            IngestedAt = new DateTime(2022, 12, 12),
                            SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                            ExecutionIdExternal = "221216164429TQcO66uETE",
                            RecommendedPlanDetail =
                            {
                                Id = 14,
                                Name = "4Q'22 Abbvie Daytime Upfront",
                                FlightStartDate = new DateTime(2022, 12, 12),
                                FlightEndDate = new DateTime(2022, 12, 25),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 3,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 1,
                                        Length = 30
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 8,
                                        Length = 45
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 2,
                                        Length = 60
                                    }
                                },
                                AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                            }
                        },
                        {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                            {
                                Id = 15,
                                SpotExceptionsRecommendedPlanId = 14,
                                RecommendedPlanId = 14,
                                ExecutionTraceId = 8853,
                                Rate = 0,
                                AudienceName = "Adults 25-54",
                                ContractedImpressions = 6137000.0,
                                DeliveredImpressions = 4641773.9953086916,
                                IsRecommendedPlan = true,
                                PlanClearancePercentage = null,
                                DaypartCode = "EM",
                                StartTime = 14400,
                                EndTime = 39599,
                                Monday = 1,
                                Tuesday = 1,
                                Wednesday = 1,
                                Thursday = 1,
                                Friday = 1,
                                Saturday = 1,
                                Sunday = 1,
                                SpotDeliveredImpressions = 270.87375,
                                PlanTotalContractedImpressions = 70505000.0,
                                PlanTotalDeliveredImpressions = 38555869.184943005,
                                IngestedMediaWeekId = 989,
                                IngestedBy = "Test User",
                                IngestedAt = new DateTime(2022, 12, 12),
                                SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ = ",
                                ExecutionIdExternal = "221216164429TQcO66uETE",
                                RecommendedPlanDetail =
                                {
                                    Id = 14,
                                    Name = "4Q'22 Abbvie Early Morning Upfront",
                                    FlightStartDate = new DateTime(2022, 12, 12),
                                    FlightEndDate = new DateTime(2022, 12, 25),
                                    SpotLengths = new List<SpotLengthDto>
                                    {
                                        new SpotLengthDto
                                        {
                                            Id = 3,
                                            Length = 15
                                        },
                                        new SpotLengthDto
                                        {
                                            Id = 1,
                                            Length = 30
                                        },
                                        new SpotLengthDto
                                        {
                                            Id = 2,
                                            Length = 60
                                        }
                                    },
                                    AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                    ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                                }
                            }
                        }
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(s => s.GetRecommendedPlanSpotsToDoByIds(It.IsAny<List<int?>>()))
                .Returns(Task.FromResult(existingSpotExceptionRecommendedPlanToDo));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.HandleSaveRecommendedPlanDecisionsAsync(recommendedPlanDecisionsSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsRecommendedPlansDecisions_PostOneToDoDecisionToNotRecommended()
        {
            // Arrange
            var recommendedPlanDecisionsSaveRequest = new SpotExceptionsRecommendedPlanSaveDecisionsRequestDto
            {
                SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
                {
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        TodoId = 15,
                        SelectedPlanId = 14
                    }
                }
            };

            var existingSpotExceptionRecommendedPlanToDo = new List<SpotExceptionsRecommendedPlanSpotsToDoDto>()
            {
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "221216164429TQcO66uETE",
                    EstimateId = 7523,
                    InventorySource = "Ference POD",
                    HouseIsci = "523NVE0005H",
                    ClientIsci = "RNVE0005000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    StationLegacyCallLetters = "WZBJ",
                    Affiliate = "IND",
                    MarketCode = 173,
                    MarketRank = 71,
                    ProgramName = "ToRemove",
                    ProgramGenre = null,
                    IngestedBy = "DatabricksDatalakeAcct",
                    IngestedAt = new DateTime(2022, 12, 12),
                    IngestedMediaWeekId = 989,
                    SpotLength =
                    {
                        Id = 2,
                        Length = 60
                    },
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            Id = 14,
                            SpotExceptionsRecommendedPlanId = 14,
                            RecommendedPlanId = 14,
                            ExecutionTraceId = 8854,
                            Rate = 0,
                            AudienceName = "Adults 25-54",
                            ContractedImpressions = 13692000.0,
                            DeliveredImpressions = 11528522.180647785,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "DAY",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 270.87375,
                            PlanTotalContractedImpressions = 126836000.0,
                            PlanTotalDeliveredImpressions = 70837755.548371226,
                            IngestedMediaWeekId = 989,
                            IngestedBy = "Test User",
                            IngestedAt = new DateTime(2022, 12, 12),
                            SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                            ExecutionIdExternal = "221216164429TQcO66uETE",
                            RecommendedPlanDetail =
                            {
                                Id = 14,
                                Name = "4Q'22 Abbvie Daytime Upfront",
                                FlightStartDate = new DateTime(2022, 12, 12),
                                FlightEndDate = new DateTime(2022, 12, 25),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 3,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 1,
                                        Length = 30
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 8,
                                        Length = 45
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 2,
                                        Length = 60
                                    }
                                },
                                AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                            }
                        },
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            Id = 15,
                            SpotExceptionsRecommendedPlanId = 14,
                            RecommendedPlanId = 14,
                            ExecutionTraceId = 8853,
                            Rate = 0,
                            AudienceName = "Adults 25-54",
                            ContractedImpressions = 6137000.0,
                            DeliveredImpressions = 4641773.9953086916,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 14400,
                            EndTime = 39599,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 270.87375,
                            PlanTotalContractedImpressions = 70505000.0,
                            PlanTotalDeliveredImpressions = 38555869.184943005,
                            IngestedMediaWeekId = 989,
                            IngestedBy = "Test User",
                            IngestedAt = new DateTime(2022, 12, 12),
                            SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ = ",
                            ExecutionIdExternal = "221216164429TQcO66uETE",
                            RecommendedPlanDetail =
                            {
                                Id = 14,
                                Name = "4Q'22 Abbvie Early Morning Upfront",
                                FlightStartDate = new DateTime(2022, 12, 12),
                                FlightEndDate = new DateTime(2022, 12, 25),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 3,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 1,
                                        Length = 30
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 2,
                                        Length = 60
                                    }
                                },
                                AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                            }
                        }
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(s => s.GetRecommendedPlanSpotsToDoByIds(It.IsAny<List<int?>>()))
                .Returns(Task.FromResult(existingSpotExceptionRecommendedPlanToDo));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.HandleSaveRecommendedPlanDecisionsAsync(recommendedPlanDecisionsSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsOutOfSpecsDecisions_PostMultipleToDoDecisions()
        {
            // Arrange
            var recommendedPlanDecisionsSaveRequest = new SpotExceptionsRecommendedPlanSaveDecisionsRequestDto
            {
                SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
                {
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        TodoId = 14,
                        SelectedPlanId = 14
                    },
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        TodoId = 15,
                        SelectedPlanId = 15
                    }
                }
            };

            var existingSpotExceptionRecommendedPlanToDo = new List<SpotExceptionsRecommendedPlanSpotsToDoDto>()
            {
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    Id = 15,
                    SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "221216164429TQcO66uETE",
                    EstimateId = 7523,
                    InventorySource = "Ference POD",
                    HouseIsci = "523NVE0005H",
                    ClientIsci = "RNVE0005000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    StationLegacyCallLetters = "WZBJ",
                    Affiliate = "IND",
                    MarketCode = 173,
                    MarketRank = 71,
                    ProgramName = "ToRemove",
                    ProgramGenre = null,
                    IngestedBy = "DatabricksDatalakeAcct",
                    IngestedAt = new DateTime(2022, 12, 12),
                    IngestedMediaWeekId = 989,
                    SpotLength =
                    {
                        Id = 2,
                        Length = 60
                    },
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            Id = 407309,
                            SpotExceptionsRecommendedPlanId = 15,
                            RecommendedPlanId = 15,
                            ExecutionTraceId = 8854,
                            Rate = 0,
                            AudienceName = "Adults 25-54",
                            ContractedImpressions = 13692000.0,
                            DeliveredImpressions = 11528522.180647785,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "DAY",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 270.87375,
                            PlanTotalContractedImpressions = 126836000.0,
                            PlanTotalDeliveredImpressions = 70837755.548371226,
                            IngestedMediaWeekId = 989,
                            IngestedBy = "Test User",
                            IngestedAt = new DateTime(2022, 12, 12),
                            SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                            ExecutionIdExternal = "221216164429TQcO66uETE",
                            RecommendedPlanDetail =
                            {
                                Id = 15,
                                Name = "4Q'22 Abbvie Daytime Upfront",
                                FlightStartDate = new DateTime(2022, 12, 12),
                                FlightEndDate = new DateTime(2022, 12, 25),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 3,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 1,
                                        Length = 30
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 8,
                                        Length = 45
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 2,
                                        Length = 60
                                    }
                                },
                                AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                            }
                        },
                        {
                            new SpotExceptionsRecommendedPlanDetailsToDoDto
                            {
                                Id = 407310,
                                SpotExceptionsRecommendedPlanId = 15,
                                RecommendedPlanId = 15,
                                ExecutionTraceId = 8853,
                                Rate = 0,
                                AudienceName = "Adults 25-54",
                                ContractedImpressions = 6137000.0,
                                DeliveredImpressions = 4641773.9953086916,
                                IsRecommendedPlan = true,
                                PlanClearancePercentage = null,
                                DaypartCode = "EM",
                                StartTime = 14400,
                                EndTime = 39599,
                                Monday = 1,
                                Tuesday = 1,
                                Wednesday = 1,
                                Thursday = 1,
                                Friday = 1,
                                Saturday = 1,
                                Sunday = 1,
                                SpotDeliveredImpressions = 270.87375,
                                PlanTotalContractedImpressions = 70505000.0,
                                PlanTotalDeliveredImpressions = 38555869.184943005,
                                IngestedMediaWeekId = 989,
                                IngestedBy = "Test User",
                                IngestedAt = new DateTime(2022, 12, 12),
                                SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ = ",
                                ExecutionIdExternal = "221216164429TQcO66uETE",
                                RecommendedPlanDetail =
                                {
                                    Id = 15,
                                    Name = "4Q'22 Abbvie Early Morning Upfront",
                                    FlightStartDate = new DateTime(2022, 12, 12),
                                    FlightEndDate = new DateTime(2022, 12, 25),
                                    SpotLengths = new List<SpotLengthDto>
                                    {
                                        new SpotLengthDto
                                        {
                                            Id = 3,
                                            Length = 15
                                        },
                                        new SpotLengthDto
                                        {
                                            Id = 1,
                                            Length = 30
                                        },
                                        new SpotLengthDto
                                        {
                                            Id = 2,
                                            Length = 60
                                        }
                                    },
                                    AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                    ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                                }
                            }
                        }
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(s => s.GetRecommendedPlanSpotsToDoByIds(It.IsAny<List<int?>>()))
                .Returns(Task.FromResult(existingSpotExceptionRecommendedPlanToDo));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.HandleSaveRecommendedPlanDecisionsAsync(recommendedPlanDecisionsSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsRecommendedPlanDecisions_PostOneDoneDecisionToIsRecommended()
        {
            // Arrange
            var recommendedPlanDecisionsSaveRequest = new SpotExceptionsRecommendedPlanSaveDecisionsRequestDto
            {
                SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
                {
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        DoneId = 14,
                        SelectedPlanId = 15
                    }
                }
            };

            var existingSpotExceptionRecommendedPlanDone = new List<SpotExceptionsRecommendedPlanSpotsDoneDto>()
            {
                new SpotExceptionsRecommendedPlanSpotsDoneDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "221216164429TQcO66uETE",
                    EstimateId = 7523,
                    InventorySource = "Ference POD",
                    HouseIsci = "523NVE0005H",
                    ClientIsci = "RNVE0005000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    StationLegacyCallLetters = "WZBJ",
                    Affiliate = "IND",
                    MarketCode = 173,
                    MarketRank = 71,
                    ProgramName = "ToRemove",
                    ProgramGenre = null,
                    IngestedBy = "DatabricksDatalakeAcct",
                    IngestedAt = new DateTime(2022, 12, 12),
                    IngestedMediaWeekId = 989,
                    SpotLength =
                    {
                        Id = 2,
                        Length = 60
                    },
                    SpotExceptionsRecommendedPlanDetailsDone = new List<SpotExceptionsRecommendedPlanDetailsDoneDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDoneDto
                        {
                            Id = 15,
                            SpotExceptionsRecommendedPlanId = 14,
                            RecommendedPlanId = 15,
                            ExecutionTraceId = 8853,
                            Rate = 0,
                            AudienceName = "Adults 25-54",
                            ContractedImpressions = 6137000.0,
                            DeliveredImpressions = 4641773.9953086916,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 14400,
                            EndTime = 39599,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 270.87375,
                            PlanTotalContractedImpressions = 70505000.0,
                            PlanTotalDeliveredImpressions = 38555869.184943005,
                            IngestedMediaWeekId = 989,
                            IngestedBy = "Test User",
                            IngestedAt = new DateTime(2022, 12, 12),
                            SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ = ",
                            ExecutionIdExternal = "221216164429TQcO66uETE",
                            RecommendedPlanDetail =
                            {
                                Id = 15,
                                Name = "4Q'22 Abbvie Early Morning Upfront",
                                FlightStartDate = new DateTime(2022, 12, 12),
                                FlightEndDate = new DateTime(2022, 12, 25),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 3,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 1,
                                        Length = 30
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 2,
                                        Length = 60
                                    }
                                },
                                AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                            }
                        }
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(s => s.GetSpotExceptionPlanDetailsWithDecision(It.IsAny<List<int>>()))
                .Returns(Task.FromResult(existingSpotExceptionRecommendedPlanDone));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.HandleSaveRecommendedPlanDecisionsAsync(recommendedPlanDecisionsSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsRecommendedPlansDecisions_PostOneDoneDecisionToNotRecommended()
        {
            // Arrange
            var recommendedPlanDecisionsSaveRequest = new SpotExceptionsRecommendedPlanSaveDecisionsRequestDto
            {
                SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
                {
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        DoneId = 14,
                        SelectedPlanId = 14
                    }
                }
            };

            var existingSpotExceptionRecommendedPlanDone = new List<SpotExceptionsRecommendedPlanSpotsDoneDto>()
            {
                new SpotExceptionsRecommendedPlanSpotsDoneDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "221216164429TQcO66uETE",
                    EstimateId = 7523,
                    InventorySource = "Ference POD",
                    HouseIsci = "523NVE0005H",
                    ClientIsci = "RNVE0005000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    StationLegacyCallLetters = "WZBJ",
                    Affiliate = "IND",
                    MarketCode = 173,
                    MarketRank = 71,
                    ProgramName = "ToRemove",
                    ProgramGenre = null,
                    IngestedBy = "DatabricksDatalakeAcct",
                    IngestedAt = new DateTime(2022, 12, 12),
                    IngestedMediaWeekId = 989,
                    SpotLength =
                    {
                        Id = 2,
                        Length = 60
                    },
                    SpotExceptionsRecommendedPlanDetailsDone = new List<SpotExceptionsRecommendedPlanDetailsDoneDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDoneDto
                        {
                            Id = 14,
                            SpotExceptionsRecommendedPlanId = 14,
                            RecommendedPlanId = 14,
                            ExecutionTraceId = 8854,
                            Rate = 0,
                            AudienceName = "Adults 25-54",
                            ContractedImpressions = 13692000.0,
                            DeliveredImpressions = 11528522.180647785,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "DAY",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 270.87375,
                            PlanTotalContractedImpressions = 126836000.0,
                            PlanTotalDeliveredImpressions = 70837755.548371226,
                            IngestedMediaWeekId = 989,
                            IngestedBy = "Test User",
                            IngestedAt = new DateTime(2022, 12, 12),
                            SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                            ExecutionIdExternal = "221216164429TQcO66uETE",
                            RecommendedPlanDetail =
                            {
                                Id = 14,
                                Name = "4Q'22 Abbvie Daytime Upfront",
                                FlightStartDate = new DateTime(2022, 12, 12),
                                FlightEndDate = new DateTime(2022, 12, 25),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 3,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 1,
                                        Length = 30
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 8,
                                        Length = 45
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 2,
                                        Length = 60
                                    }
                                },
                                AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                            }
                        }
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(s => s.GetSpotExceptionPlanDetailsWithDecision(It.IsAny<List<int>>()))
                .Returns(Task.FromResult(existingSpotExceptionRecommendedPlanDone));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.HandleSaveRecommendedPlanDecisionsAsync(recommendedPlanDecisionsSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async void SaveSpotExceptionsRecommendedPlansDecisions_PostMultipleDoneDecisions()
        {
            // Arrange
            var recommendedPlanDecisionsSaveRequest = new SpotExceptionsRecommendedPlanSaveDecisionsRequestDto
            {
                SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
                {
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        DoneId = 14,
                        SelectedPlanId = 14
                    },
                    new SpotExceptionsRecommendedPlanSaveDto
                    {
                        DoneId = 15,
                        SelectedPlanId = 14
                    }
                }
            };

            var existingSpotExceptionRecommendedPlanDone = new List<SpotExceptionsRecommendedPlanSpotsDoneDto>()
            {
                new SpotExceptionsRecommendedPlanSpotsDoneDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "221216164429TQcO66uETE",
                    EstimateId = 7523,
                    InventorySource = "Ference POD",
                    HouseIsci = "523NVE0005H",
                    ClientIsci = "RNVE0005000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    StationLegacyCallLetters = "WZBJ",
                    Affiliate = "IND",
                    MarketCode = 173,
                    MarketRank = 71,
                    ProgramName = "ToRemove",
                    ProgramGenre = null,
                    IngestedBy = "DatabricksDatalakeAcct",
                    IngestedAt = new DateTime(2022, 12, 12),
                    IngestedMediaWeekId = 989,
                    SpotLength =
                    {
                        Id = 2,
                        Length = 60
                    },
                    SpotExceptionsRecommendedPlanDetailsDone = new List<SpotExceptionsRecommendedPlanDetailsDoneDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDoneDto
                        {
                            Id = 14,
                            SpotExceptionsRecommendedPlanId = 14,
                            RecommendedPlanId = 14,
                            ExecutionTraceId = 8854,
                            Rate = 0,
                            AudienceName = "Adults 25-54",
                            ContractedImpressions = 13692000.0,
                            DeliveredImpressions = 11528522.180647785,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "DAY",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 270.87375,
                            PlanTotalContractedImpressions = 126836000.0,
                            PlanTotalDeliveredImpressions = 70837755.548371226,
                            IngestedMediaWeekId = 989,
                            IngestedBy = "Test User",
                            IngestedAt = new DateTime(2022, 12, 12),
                            SpotUniqueHashExternal = "TE9DQUwtNDY2ODMxMjQ=",
                            ExecutionIdExternal = "221216164429TQcO66uETE",
                            RecommendedPlanDetail =
                            {
                                Id = 14,
                                Name = "4Q'22 Abbvie Daytime Upfront",
                                FlightStartDate = new DateTime(2022, 12, 12),
                                FlightEndDate = new DateTime(2022, 12, 25),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 3,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 1,
                                        Length = 30
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 8,
                                        Length = 45
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 2,
                                        Length = 60
                                    }
                                },
                                AdvertiserMasterId = new Guid("3a69600d-a8e8-4a11-8ed9-ff0e2054f492"),
                                ProductMasterId = new Guid("143de5d8-919a-451d-a38e-c3312444acd7")
                            }
                        }
                    }
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(s => s.GetSpotExceptionPlanDetailsWithDecision(It.IsAny<List<int>>()))
                .Returns(Task.FromResult(existingSpotExceptionRecommendedPlanDone));

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.HandleSaveRecommendedPlanDecisionsAsync(recommendedPlanDecisionsSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        private List<SpotExceptionsRecommendedPlanGroupingDto> _GetRecommendedPlanGroupingToDoData()
        {
            return new List<SpotExceptionsRecommendedPlanGroupingDto>()
            {
                new SpotExceptionsRecommendedPlanGroupingDto
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
                new SpotExceptionsRecommendedPlanGroupingDto
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
                new SpotExceptionsRecommendedPlanGroupingDto
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
                new SpotExceptionsRecommendedPlanGroupingDto
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

        private List<SpotExceptionsRecommendedPlanGroupingDto> _GetRecommendedPlanGroupingDoneData()
        {
            return new List<SpotExceptionsRecommendedPlanGroupingDto>()
            {
                new SpotExceptionsRecommendedPlanGroupingDto
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

        private List<SpotExceptionsRecommendedPlanSpotsDto> _GetRecommendedPlanSpotsToDoData()
        {
            return new List<SpotExceptionsRecommendedPlanSpotsDto>
            {
                new SpotExceptionsRecommendedPlanSpotsDto
                {
                    Id = 1,
                    EstimateId = 2009,
                    IsciName = "840T42AY13H",
                    ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
                    RecommendedPlanName = "4Q'21 Macy's EM",
                    PlanId = 332,
                    Impressions = 10000,
                    SpotLength = 15,
                    ProgramName = "CBS Mornings",
                    InventorySource = "Tegna",
                    Affiliate = "CBS",
                    MarketName = "Columbus, OH",
                    Station = "WBNS"
                }
            };
        }

        private List<SpotExceptionsRecommendedPlanSpotsDto> _GetRecommendedPlanSpotsDoneData()
        {
            return new List<SpotExceptionsRecommendedPlanSpotsDto>
            {
                new SpotExceptionsRecommendedPlanSpotsDto
                {
                    Id = 1,
                    EstimateId = 6840,
                    IsciName = "840T42AY13H",
                    ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
                    RecommendedPlanName = "4Q'21 Macy's EM",
                    PlanId = 332,
                    Impressions = 10000,
                    SpotLength = 15,
                    ProgramName = "CBS Mornings",
                    InventorySource = "Tegna",
                    Affiliate = "CBS",
                    MarketName = "Columbus, OH",
                    Station = "WBNS"
                }
            };
        }

        private SpotExceptionsRecommendedPlanSpotsToDoDto _GetRecommendedDetailToDoData(string ingestedBy, DateTime ingestedAt)
        {
            return new SpotExceptionsRecommendedPlanSpotsToDoDto
            {
                SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                AmbiguityCode = 1,
                ExecutionIdExternal = "220609090855BRt8EHXqSy",
                EstimateId = 6840,
                InventorySource = "Tegna",
                HouseIsci = "840T42AY13H",
                ClientIsci = "QMAY2913OS1H",
                SpotLengthId = 3,
                ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
                StationLegacyCallLetters = "WBNS",
                Affiliate = "CBS",
                MarketCode = 135,
                MarketRank = 33,
                ProgramName = "CBS Mornings",
                ProgramGenre = "INFORMATIONAL/NEWS",
                IngestedBy = ingestedBy,
                IngestedAt = ingestedAt,
                IngestedMediaWeekId = 1,
                SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                {
                    new SpotExceptionsRecommendedPlanDetailsToDoDto
                    {
                        RecommendedPlanId = 332,
                        ExecutionTraceId = 73,
                        Rate = 0.00m,
                        AudienceName = "Women 25-54",
                        ContractedImpressions = 100000,
                        DeliveredImpressions = 50000,
                        IsRecommendedPlan = true,
                        PlanClearancePercentage = null,
                        DaypartCode = "SYN",
                        StartTime = 28800,
                        EndTime = 7199,
                        Monday = 1,
                        Tuesday = 1,
                        Wednesday = 1,
                        Thursday = 1,
                        Friday = 1,
                        Saturday = 1,
                        Sunday = 1,
                        SpotDeliveredImpressions = 50,
                        PlanTotalContractedImpressions = 1000,
                        PlanTotalDeliveredImpressions = 50,
                        IngestedMediaWeekId = 1,
                        IngestedBy = ingestedBy,
                        IngestedAt = ingestedAt,
                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
                        RecommendedPlanDetail = new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            Name = "4Q'21 Macy's EM",
                            AdvertiserMasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                            SpotLengths = new List<SpotLengthDto>
                            {
                                new SpotLengthDto
                                {
                                    Id = 3,
                                    Length = 15
                                }
                            }
                        }
                    }
                }
            };
        }

        private SpotExceptionsRecommendedPlanSpotsDoneDto _GetRecommendedDetailDoneData(string ingestedBy, DateTime ingestedAt)
        {
            return new SpotExceptionsRecommendedPlanSpotsDoneDto
            {
                SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=F",
                AmbiguityCode = 1,
                ExecutionIdExternal = "220609090855BRt8EHXqSy",
                EstimateId = 6616,
                InventorySource = "Ference POD",
                HouseIsci = "616MAY2913H",
                ClientIsci = "QMAY2913OS1H",
                SpotLengthId = 3,
                ProgramAirTime = new DateTime(2022, 04, 10, 08, 41, 57),
                StationLegacyCallLetters = "WDKA",
                Affiliate = "IND",
                MarketCode = 232,
                MarketRank = 3873,
                ProgramName = "Mike & Molly",
                ProgramGenre = "COMEDY",
                IngestedBy = ingestedBy,
                IngestedAt = ingestedAt,
                IngestedMediaWeekId = 1,
                SpotExceptionsRecommendedPlanDetailsDone = new List<SpotExceptionsRecommendedPlanDetailsDoneDto>
                {
                    new SpotExceptionsRecommendedPlanDetailsDoneDto
                    {
                        RecommendedPlanId = 334,
                        ExecutionTraceId = 624,
                        Rate = 0.00m,
                        AudienceName = "Women 25-54",
                        ContractedImpressions = 100000,
                        DeliveredImpressions = 50000,
                        IsRecommendedPlan = true,
                        PlanClearancePercentage = null,
                        DaypartCode = "SYN",
                        StartTime = 28800,
                        EndTime = 7199,
                        Monday = 1,
                        Tuesday = 1,
                        Wednesday = 1,
                        Thursday = 1,
                        Friday = 1,
                        Saturday = 1,
                        Sunday = 1,
                        SpotDeliveredImpressions = 50,
                        PlanTotalContractedImpressions = 1000,
                        PlanTotalDeliveredImpressions = 50,
                        IngestedMediaWeekId = 1,
                        IngestedBy = ingestedBy,
                        IngestedAt = ingestedAt,
                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=F",
                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
                        SpotExceptionsRecommendedPlanDoneDecisions = new SpotExceptionsRecommendedPlanSpotDecisionsDoneDto
                        {
                            DecidedBy = ingestedBy,
                            DecidedAt = ingestedAt,
                            SyncedBy = null,
                            SyncedAt = null
                        },
                        RecommendedPlanDetail = new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            Name = "4Q'21 Macy's SYN",
                            AdvertiserMasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                            SpotLengths = new List<SpotLengthDto>
                            {
                                new SpotLengthDto
                                {
                                    Id = 3,
                                    Length = 15
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
