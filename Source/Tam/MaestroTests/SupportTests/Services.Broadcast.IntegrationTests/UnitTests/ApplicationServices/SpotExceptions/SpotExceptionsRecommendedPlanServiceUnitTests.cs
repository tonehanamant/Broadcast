using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;
using System.Threading.Tasks;
using System;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Repositories.SpotExceptions;
using System.Collections.Generic;
using Services.Broadcast.Repositories;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.ApplicationServices;

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
        public async void GetSpotExceptionsRecommendedPlans_BothExist()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
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

            var recommendedPlansToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            var recommendedPlansDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionRecommendedPlansAsync(spotExceptionsRecommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Completed.Count, 2);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlans_ToDoExist()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
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

            var recommendedPlansToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            List<SpotExceptionsRecommendedPlansDoneDto> recommendedPlansDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionRecommendedPlansAsync(spotExceptionsRecommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlans_DoneExist()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
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

            List<SpotExceptionsRecommendedPlansToDoDto> recommendedPlansToDo = null;
            var recommendedPlansDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionRecommendedPlansAsync(spotExceptionsRecommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 2);
        }

        [Test]
        public async void GetSpotExceptionRecommendedPlans_DoesNotExist()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
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

            List<SpotExceptionsRecommendedPlansToDoDto> recommendedPlansToDo = null;
            List<SpotExceptionsRecommendedPlansDoneDto> recommendedPlansDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            // Act
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionRecommendedPlansAsync(spotExceptionsRecommendedPlansRequest);

            // Assert            
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public void GetSpotExceptionRecommendedPlans_ThrowsException()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
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

            var recommendedPlansToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            var recommendedPlansDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetSpotExceptionRecommendedPlansAsync(spotExceptionsRecommendedPlansRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlansAdvertisers_Exist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto spotExceptionsRecommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C") };
            List<Guid> recommendedPlanDone = new List<Guid> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlanAdvertisersAsync(spotExceptionsRecommendedPlanAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlansAdvertisers_DuplicateExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto spotExceptionsRecommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };
            List<Guid> recommendedPlanDone = new List<Guid> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlanAdvertisersAsync(spotExceptionsRecommendedPlanAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlansAdvertisers_DoesNotExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto spotExceptionsRecommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid>();
            List<Guid> recommendedPlanDone = new List<Guid>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlanAdvertisersAsync(spotExceptionsRecommendedPlanAdvertisersRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlansAdvertisers_ThrowsException()
        {
            // Arrange
            SpotExceptionsRecommendedPlanAdvertisersRequestDto spotExceptionsRecommendedPlanAdvertisersRequest = new SpotExceptionsRecommendedPlanAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<Guid> recommendedPlanToDo = new List<Guid> { new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };
            List<Guid> recommendedPlanDone = new List<Guid> { new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7"), new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7") };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDoAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDoneAdvertisersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlanAdvertisersAsync(spotExceptionsRecommendedPlanAdvertisersRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlansStations_Exist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationRequestDto spotExceptionsRecommendedPlansStationRequest = new SpotExceptionsRecommendedPlanStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string> { "KSTP" };
            List<string> recommendedPlanDone = new List<string> { "WDAY" };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlansStationsAsync(spotExceptionsRecommendedPlansStationRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlansStations_DuplicateExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationRequestDto spotExceptionsRecommendedPlansStationRequest = new SpotExceptionsRecommendedPlanStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string> { "KSTP", "KSTP", "WDAY" };
            List<string> recommendedPlanDone = new List<string> { "WDAY", "WDAY" };

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlansStationsAsync(spotExceptionsRecommendedPlansStationRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlansStations_DoNotExist()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationRequestDto spotExceptionsRecommendedPlansStationRequest = new SpotExceptionsRecommendedPlanStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string>();
            List<string> recommendedPlanDone = new List<string>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanDone));

            // Act           
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlansStationsAsync(spotExceptionsRecommendedPlansStationRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanStations_ThrowsException()
        {
            // Arrange
            SpotExceptionsRecommendedPlanStationRequestDto spotExceptionsRecommendedPlansStationRequest = new SpotExceptionsRecommendedPlanStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            List<string> recommendedPlanToDo = new List<string>();
            List<string> recommendedPlanDone = new List<string>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanToDoStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionsRecommendedPlanDoneStationsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlansStationsAsync(spotExceptionsRecommendedPlansStationRequest));

            // Assert
            Assert.AreEqual("Could not retrieve the data from the Database", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_Exist()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            RecomendedPlansRequestDto recommendedPlansRequest = new RecomendedPlansRequestDto
            {
                PlanId = 332,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var advertiser = new AdvertiserDto
            {
                Id = 2,
                MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"),
                Name = "Name2"
            };

            var recommendedPlansToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            var recommendedPlansDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionRecommendedPlanToDoSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetSpotExceptionRecommendedPlanDoneSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlansDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetSpotExceptionsRecommendedPlanSpotsAsync(recommendedPlansRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Queued.Count, 1);
            Assert.AreEqual(result.Synced.Count, 1);
        }

        //[Test]
        //public void GetSpotExceptionRecommendedPlanSpots_DoesNotExist()
        //{
        //    // Arrange
        //    List<SpotExceptionsRecommendedPlansDto> recommendedPlanData = null;
        //    RecomendedPlansRequestDto spotExceptionsRecommendedRequest = new RecomendedPlansRequestDto
        //    {
        //        PlanId = 215,
        //        WeekStartDate = new DateTime(2021, 01, 04),
        //        WeekEndDate = new DateTime(2021, 01, 10)
        //    };
        //    _SpotExceptionRepositoryMock
        //        .Setup(x => x.GetSpotExceptionRecommendedPlanSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //        .Returns(recommendedPlanData);

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanSpots(spotExceptionsRecommendedRequest);

        //    // Assert            
        //    Assert.AreEqual(result.Active.Count, 0);
        //    Assert.AreEqual(result.Queued.Count, 0);
        //    Assert.AreEqual(result.Synced.Count, 0);
        //}

        //[Test]
        //public void GetSpotExceptionRecommendedPlanSpots_ThrowsException()
        //{
        //    // Arrange
        //    RecomendedPlansRequestDto spotExceptionsRecommendedRequest = new RecomendedPlansRequestDto
        //    {
        //        PlanId = 215,
        //        WeekStartDate = new DateTime(2021, 01, 04),
        //        WeekEndDate = new DateTime(2021, 01, 10)
        //    };
        //    _SpotExceptionRepositoryMock
        //        .Setup(x => x.GetSpotExceptionRecommendedPlanSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //        .Callback(() =>
        //        {
        //            throw new Exception("Throwing a test exception.");
        //        });

        //    // Act           
        //    var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsRecommendedPlanSpots(spotExceptionsRecommendedRequest));

        //    // Assert
        //    Assert.AreEqual("Throwing a test exception.", result.Message);
        //}

        //[Test]
        //public void GetSpotExceptionsRecommendedPlanSpotsWithPacing_Exist()
        //{
        //    // Arrange
        //    // Arrange
        //    var ingestedDateTime = new DateTime(2010, 10, 12);
        //    var ingestedBy = "Repository Test User";

        //    RecomendedPlansRequestDto spotExceptionsRecommendedRequest = new RecomendedPlansRequestDto
        //    {
        //        PlanId = 215,
        //        WeekStartDate = new DateTime(2021, 01, 04),
        //        WeekEndDate = new DateTime(2021, 01, 10)
        //    };
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionRecommendedPlanSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //        .Returns(new List<SpotExceptionsRecommendedPlansDto>
        //        {
        //            new SpotExceptionsRecommendedPlansDto
        //            {
        //                Id = 1,
        //                SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                AmbiguityCode = 1,
        //                ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                EstimateId = 6840,
        //                InventorySource = "Tegna",
        //                HouseIsci = "840T42AY13H",
        //                ClientIsci = "QMAY2913OS1H",
        //                SpotLengthId = 3,
        //                ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
        //                StationLegacyCallLetters = "WBNS",
        //                Affiliate = "CBS",
        //                MarketCode = 135,
        //                MarketRank = 33,
        //                ProgramName = "CBS Mornings",
        //                ProgramGenre = "INFORMATIONAL/NEWS",
        //                IngestedBy = ingestedBy,
        //                IngestedAt = ingestedDateTime,
        //                IngestedMediaWeekId = 1,
        //                SpotLength = new SpotLengthDto
        //                {
        //                    Id = 16,
        //                    Length = 45
        //                },
        //                SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
        //                {
        //                    new SpotExceptionsRecommendedPlanDetailsDto
        //                    {
        //                        Id = 102,
        //                        SpotExceptionsRecommendedPlanId = 1,
        //                        RecommendedPlanId = 301,
        //                        ExecutionTraceId = 73,
        //                        Rate = 0.00m,
        //                        AudienceName = "Women 25-54",
        //                        ContractedImpressions = 100000,
        //                        DeliveredImpressions = 50000,
        //                        IsRecommendedPlan = false,
        //                        PlanClearancePercentage = null,
        //                        DaypartCode = "SYN",
        //                        StartTime = 28800,
        //                        EndTime = 7199,
        //                        Monday = 1,
        //                        Tuesday = 1,
        //                        Wednesday = 1,
        //                        Thursday = 1,
        //                        Friday = 1,
        //                        Saturday = 1,
        //                        Sunday = 1,
        //                        SpotDeliveredImpressions = 50,
        //                        PlanTotalContractedImpressions = 1000,
        //                        PlanTotalDeliveredImpressions = 50,
        //                        IngestedMediaWeekId = 1,
        //                        IngestedBy = ingestedBy,
        //                        IngestedAt = ingestedDateTime,
        //                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                        RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                        {
        //                            Id = 301,
        //                            Name = "2Q' 21 Reynolds Foil TDN and SYN Upfront",
        //                            FlightStartDate = new DateTime(2019, 12, 1),
        //                            FlightEndDate = new DateTime(2020, 2, 1),
        //                            SpotLengths = new List<SpotLengthDto>
        //                            {
        //                                new SpotLengthDto
        //                                {
        //                                    Id = 16,
        //                                    Length = 45
        //                                }
        //                            }
        //                        },
        //                        SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
        //                        {
        //                            Id = 202,
        //                            SpotExceptionsRecommendedPlanDetailId = 102,
        //                            UserName = "Test User",
        //                            CreatedAt = new DateTime(2020,10,25)
        //                        }
        //                    },
        //                    new SpotExceptionsRecommendedPlanDetailsDto
        //                    {
        //                        Id = 103,
        //                        SpotExceptionsRecommendedPlanId = 1,
        //                        RecommendedPlanId = 302,
        //                        ExecutionTraceId = 75,
        //                        Rate = 0.00m,
        //                        AudienceName = "Women 25-54",
        //                        ContractedImpressions = 100000,
        //                        DeliveredImpressions = 50000,
        //                        IsRecommendedPlan = true,
        //                        PlanClearancePercentage = null,
        //                        DaypartCode = "EM",
        //                        StartTime = 18000,
        //                        EndTime = 35999,
        //                        Monday = 1,
        //                        Tuesday = 1,
        //                        Wednesday = 1,
        //                        Thursday = 1,
        //                        Friday = 1,
        //                        Saturday = 1,
        //                        Sunday = 1,
        //                        SpotDeliveredImpressions = 50,
        //                        PlanTotalContractedImpressions = 1000,
        //                        PlanTotalDeliveredImpressions = 50,
        //                        IngestedMediaWeekId = 1,
        //                        IngestedBy = ingestedBy,
        //                        IngestedAt = ingestedDateTime,
        //                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                        RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                        {
        //                            Id = 302,
        //                            Name = "2Q' 21 Reynolds",
        //                            FlightStartDate = new DateTime(2019, 12, 1),
        //                            FlightEndDate = new DateTime(2020, 2, 1),
        //                            SpotLengths = new List<SpotLengthDto>
        //                            {
        //                                new SpotLengthDto
        //                                {
        //                                    Id = 14,
        //                                    Length = 15
        //                                },
        //                                new SpotLengthDto
        //                                {
        //                                    Id = 15,
        //                                    Length = 30
        //                                }
        //                            }
        //                        },
        //                        SpotExceptionsRecommendedPlanDecision = null
        //                    }
        //                }
        //            },
        //            new SpotExceptionsRecommendedPlansDto
        //            {
        //                Id = 2,
        //                SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                AmbiguityCode = 1,
        //                ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                EstimateId = 6840,
        //                InventorySource = "Tegna",
        //                HouseIsci = "840T42AY13H",
        //                ClientIsci = "QMAY2913OS1H",
        //                SpotLengthId = 3,
        //                ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
        //                StationLegacyCallLetters = "WBNS",
        //                Affiliate = "CBS",
        //                MarketCode = 135,
        //                MarketRank = 33,
        //                ProgramName = "CBS Mornings",
        //                ProgramGenre = "INFORMATIONAL/NEWS",
        //                IngestedBy = ingestedBy,
        //                IngestedAt = ingestedDateTime,
        //                IngestedMediaWeekId = 1,
        //                SpotLength = new SpotLengthDto
        //                {
        //                    Id = 16,
        //                    Length = 45
        //                },
        //                SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
        //                {
        //                    new SpotExceptionsRecommendedPlanDetailsDto
        //                    {
        //                        Id = 104,
        //                        SpotExceptionsRecommendedPlanId = 2,
        //                        RecommendedPlanId = 302,
        //                        ExecutionTraceId = 75,
        //                        Rate = 0.00m,
        //                        AudienceName = "Women 25-54",
        //                        ContractedImpressions = 100000,
        //                        DeliveredImpressions = 50000,
        //                        IsRecommendedPlan = true,
        //                        PlanClearancePercentage = null,
        //                        DaypartCode = "EM",
        //                        StartTime = 18000,
        //                        EndTime = 35999,
        //                        Monday = 1,
        //                        Tuesday = 1,
        //                        Wednesday = 1,
        //                        Thursday = 1,
        //                        Friday = 1,
        //                        Saturday = 1,
        //                        Sunday = 1,
        //                        SpotDeliveredImpressions = 50,
        //                        PlanTotalContractedImpressions = 1000,
        //                        PlanTotalDeliveredImpressions = 50,
        //                        IngestedMediaWeekId = 1,
        //                        IngestedBy = ingestedBy,
        //                        IngestedAt = ingestedDateTime,
        //                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                        RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                        {
        //                            Id = 302,
        //                            Name = "2Q' 21 Reynolds",
        //                            FlightStartDate = new DateTime(2019, 12, 1),
        //                            FlightEndDate = new DateTime(2020, 2, 1),
        //                            SpotLengths = new List<SpotLengthDto>
        //                            {
        //                                new SpotLengthDto
        //                                {
        //                                    Id = 14,
        //                                    Length = 15
        //                                },
        //                                new SpotLengthDto
        //                                {
        //                                    Id = 15,
        //                                    Length = 30
        //                                }
        //                            }
        //                        },
        //                        SpotExceptionsRecommendedPlanDecision = null
        //                    }
        //                }
        //            },
        //            new SpotExceptionsRecommendedPlansDto
        //            {
        //                Id = 3,
        //                SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                AmbiguityCode = 1,
        //                ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                EstimateId = 6840,
        //                InventorySource = "Tegna",
        //                HouseIsci = "840T42AY13H",
        //                ClientIsci = "QMAY2913OS1H",
        //                SpotLengthId = 3,
        //                ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
        //                StationLegacyCallLetters = "WBNS",
        //                Affiliate = "CBS",
        //                MarketCode = 135,
        //                MarketRank = 33,
        //                ProgramName = "CBS Mornings",
        //                ProgramGenre = "INFORMATIONAL/NEWS",
        //                IngestedBy = ingestedBy,
        //                IngestedAt = ingestedDateTime,
        //                IngestedMediaWeekId = 1,
        //                SpotLength = new SpotLengthDto
        //                {
        //                    Id = 16,
        //                    Length = 45
        //                },
        //                SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
        //                {
        //                    new SpotExceptionsRecommendedPlanDetailsDto
        //                    {
        //                        Id = 105,
        //                        SpotExceptionsRecommendedPlanId = 3,
        //                        RecommendedPlanId = 302,
        //                        ExecutionTraceId = 75,
        //                        Rate = 0.00m,
        //                        AudienceName = "Women 25-54",
        //                        ContractedImpressions = 100000,
        //                        DeliveredImpressions = 50000,
        //                        IsRecommendedPlan = true,
        //                        PlanClearancePercentage = null,
        //                        DaypartCode = "EM",
        //                        StartTime = 18000,
        //                        EndTime = 35999,
        //                        Monday = 1,
        //                        Tuesday = 1,
        //                        Wednesday = 1,
        //                        Thursday = 1,
        //                        Friday = 1,
        //                        Saturday = 1,
        //                        Sunday = 1,
        //                        SpotDeliveredImpressions = 50,
        //                        PlanTotalContractedImpressions = 1000,
        //                        PlanTotalDeliveredImpressions = 50,
        //                        IngestedMediaWeekId = 1,
        //                        IngestedBy = ingestedBy,
        //                        IngestedAt = ingestedDateTime,
        //                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                        RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                        {
        //                            Id = 302,
        //                            Name = "2Q' 21 Reynolds",
        //                            FlightStartDate = new DateTime(2019, 12, 1),
        //                            FlightEndDate = new DateTime(2020, 2, 1),
        //                            SpotLengths = new List<SpotLengthDto>
        //                            {
        //                                new SpotLengthDto
        //                                {
        //                                    Id = 14,
        //                                    Length = 15
        //                                },
        //                                new SpotLengthDto
        //                                {
        //                                    Id = 15,
        //                                    Length = 30
        //                                }
        //                            }
        //                        },
        //                        SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
        //                        {
        //                            Id = 100,
        //                            SpotExceptionsRecommendedPlanDetailId = 105,
        //                            UserName = "Test User",
        //                            CreatedAt = new DateTime(2020,10,25),
        //                            SyncedAt = ingestedDateTime,
        //                            SyncedBy = ingestedBy
        //                        }
        //                    }
        //                }
        //            }
        //        });

        //    var expectedResult = 1;

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanSpots(spotExceptionsRecommendedRequest);

        //    // Assert
        //    Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        //    Assert.AreEqual(expectedResult, result.Active.Count);
        //    Assert.AreEqual(expectedResult, result.Queued.Count);
        //    Assert.AreEqual(expectedResult, result.Synced.Count);
        //}

        //[Test]
        //public void GetSpotExceptionsRecommendedPlanSpotsWithPacing_DoesNotExist()
        //{
        //    // Arrange
        //    var ingestedDateTime = new DateTime(2010, 10, 12);
        //    var ingestedBy = "Repository Test User";
        //    RecomendedPlansRequestDto spotExceptionsRecommendedRequest = new RecomendedPlansRequestDto
        //    {
        //        PlanId = 215,
        //        WeekStartDate = new DateTime(2021, 01, 04),
        //        WeekEndDate = new DateTime(2021, 01, 10)
        //    };
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionRecommendedPlanSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //        .Returns(new List<SpotExceptionsRecommendedPlansDto>
        //        {
        //            new SpotExceptionsRecommendedPlansDto
        //            {
        //                Id = 3,
        //                SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                AmbiguityCode = 1,
        //                ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                EstimateId = 6840,
        //                InventorySource = "Tegna",
        //                HouseIsci = "840T42AY13H",
        //                ClientIsci = "QMAY2913OS1H",
        //                SpotLengthId = 3,
        //                ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
        //                StationLegacyCallLetters = "WBNS",
        //                Affiliate = "CBS",
        //                MarketCode = 135,
        //                MarketRank = 33,
        //                ProgramName = "CBS Mornings",
        //                ProgramGenre = "INFORMATIONAL/NEWS",
        //                IngestedBy= ingestedBy,
        //                IngestedAt= ingestedDateTime,
        //                IngestedMediaWeekId = 1,
        //                SpotLength = new SpotLengthDto
        //                {
        //                    Id = 15,
        //                    Length = 30
        //                },
        //                SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
        //                {
        //                    new SpotExceptionsRecommendedPlanDetailsDto
        //                    {
        //                        RecommendedPlanId = 334,
        //                        ExecutionTraceId = 73,
        //                        Rate = 0.00m,
        //                        AudienceName = "Women 25-54",
        //                        ContractedImpressions = 100000,
        //                        DeliveredImpressions = 50000,
        //                        IsRecommendedPlan = false,
        //                        PlanClearancePercentage = null,
        //                        DaypartCode = "SYN",
        //                        StartTime = 28800,
        //                        EndTime = 7199,
        //                        Monday = 1,
        //                        Tuesday = 1,
        //                        Wednesday = 1,
        //                        Thursday = 1,
        //                        Friday = 1,
        //                        Saturday = 1,
        //                        Sunday = 1,
        //                        SpotDeliveredImpressions = 50,
        //                        PlanTotalContractedImpressions = 1000,
        //                        PlanTotalDeliveredImpressions = 50,
        //                        IngestedMediaWeekId = 1,
        //                        IngestedBy = ingestedBy,
        //                        IngestedAt = ingestedDateTime,
        //                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                        RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                        {
        //                            Name = "4Q' 21 Reynolds Foil TDN and SYN Upfront"
        //                        },
        //                        SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
        //                        {
        //                            UserName = ingestedBy,
        //                            CreatedAt = ingestedDateTime,
        //                            AcceptedAsInSpec = false
        //                        }
        //                    },
        //                    new SpotExceptionsRecommendedPlanDetailsDto
        //                    {
        //                        RecommendedPlanId = 332,
        //                        ExecutionTraceId = 75,
        //                        Rate = 0.00m,
        //                        AudienceName = "Women 25-54",
        //                        ContractedImpressions = 100000,
        //                        DeliveredImpressions = 50000,
        //                        IsRecommendedPlan = true,
        //                        PlanClearancePercentage = null,
        //                        DaypartCode = "EM",
        //                        StartTime = 18000,
        //                        EndTime = 35999,
        //                        Monday = 1,
        //                        Tuesday = 1,
        //                        Wednesday = 1,
        //                        Thursday = 1,
        //                        Friday = 1,
        //                        Saturday = 1,
        //                        Sunday = 1,
        //                        SpotDeliveredImpressions = 50,
        //                        PlanTotalContractedImpressions = 1000,
        //                        PlanTotalDeliveredImpressions = 50,
        //                        IngestedMediaWeekId = 1,
        //                        IngestedBy = ingestedBy,
        //                        IngestedAt = ingestedDateTime,
        //                        SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //                        ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //                        RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                        {
        //                            Name = "3Q' 21 Reynolds Foil TDN and SYN Upfront"
        //                        },
        //                        SpotExceptionsRecommendedPlanDecision = null
        //                    }
        //                }
        //            }
        //        });

        //    var expectedResult = 1;

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanSpots(spotExceptionsRecommendedRequest);

        //    // Assert
        //    Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        //    Assert.AreEqual(expectedResult, result.Queued.Count);
        //}

        private List<SpotExceptionsRecommendedPlansToDoDto> _GetRecommendedToDoData(string ingestedBy, DateTime ingestedAt)
        {
            return new List<SpotExceptionsRecommendedPlansToDoDto>
            {
                new SpotExceptionsRecommendedPlansToDoDto
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
                            RecommendedPlanDetail = new RecommendedPlanDetailDto
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
                },
                new SpotExceptionsRecommendedPlansToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6289,
                    InventorySource = "Sinclair Corp - Day Syn 10a-4p",
                    HouseIsci = "289IT2Y3P2H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 09, 58, 55),
                    StationLegacyCallLetters = "KOMO",
                    Affiliate = "ABC",
                    MarketCode = 419,
                    MarketRank = 11,
                    ProgramName = "LIVE with Kelly and Ryan",
                    ProgramGenre = "TALK",
                    IngestedBy = ingestedBy,
                    IngestedAt = ingestedAt,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 1923,
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
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=F",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy",
                            RecommendedPlanDetail = new RecommendedPlanDetailDto
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
                }
            };
        }

        private List<SpotExceptionsRecommendedPlansDoneDto> _GetRecommendedDoneData(string ingestedBy, DateTime ingestedAt)
        {
            return new List<SpotExceptionsRecommendedPlansDoneDto>
            {
                new SpotExceptionsRecommendedPlansDoneDto
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
                            SpotExceptionsRecommendedPlanDoneDecisions = new SpotExceptionsRecommendedPlanDoneDecisionsDto
                            {
                                DecidedBy = ingestedBy,
                                DecidedAt = ingestedAt,
                                SyncedBy = null,
                                SyncedAt = null
                            },
                            RecommendedPlanDetail = new RecommendedPlanDetailDto
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
                },
                new SpotExceptionsRecommendedPlansDoneDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6289,
                    InventorySource = "Sinclair Corp - Day Syn 10a-4p",
                    HouseIsci = "289IT2Y3P2H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 09, 58, 55),
                    StationLegacyCallLetters = "KOMO",
                    Affiliate = "ABC",
                    MarketCode = 419,
                    MarketRank = 11,
                    ProgramName = "LIVE with Kelly and Ryan",
                    ProgramGenre = "TALK",
                    IngestedBy = ingestedBy,
                    IngestedAt = ingestedAt,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsDone = new List<SpotExceptionsRecommendedPlanDetailsDoneDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDoneDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 1824,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
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
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=F",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotExceptionsRecommendedPlanDoneDecisions = new SpotExceptionsRecommendedPlanDoneDecisionsDto
                            {
                                DecidedBy = ingestedBy,
                                DecidedAt = ingestedAt,
                                SyncedBy = ingestedBy,
                                SyncedAt = ingestedAt
                            },
                            RecommendedPlanDetail = new RecommendedPlanDetailDto
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
                }
            };
        }
    }
}
