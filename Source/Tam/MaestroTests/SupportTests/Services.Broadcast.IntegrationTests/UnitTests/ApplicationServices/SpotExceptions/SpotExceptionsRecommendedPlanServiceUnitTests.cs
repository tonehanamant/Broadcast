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
        public async void GetSpotExceptionsRecommendedPlans_BothExist()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

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
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlansAsync(recommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Completed.Count, 2);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlans_ToDoExistOnly()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

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

            var recommendedPlansToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            List<SpotExceptionsRecommendedPlanSpotsDoneDto> recommendedPlansDone = null;

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
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlansAsync(recommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlans_DoneExistOnly()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

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

            List<SpotExceptionsRecommendedPlanSpotsToDoDto> recommendedPlansToDo = null;
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
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlansAsync(recommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 2);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlans_DoesNotExist()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

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

            List<SpotExceptionsRecommendedPlanSpotsToDoDto> recommendedPlansToDo = null;
            List<SpotExceptionsRecommendedPlanSpotsDoneDto> recommendedPlansDone = null;

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
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlansAsync(recommendedPlansRequest);

            // Assert            
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Completed.Count, 0);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_ThrowsException()
        {
            // Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

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
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlansAsync(recommendedPlansRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plans", result.Message);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_BothExist()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
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

            var recommendedPlanSpotsToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            var recommendedPlanSpotsDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDone(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Queued.Count, 1);
            Assert.AreEqual(result.Synced.Count, 1);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_ToDoExistOnly()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
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

            var recommendedPlanSpotsToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            List<SpotExceptionsRecommendedPlanSpotsDoneDto> recommendedPlanSpotsDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDone(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
            Assert.AreEqual(result.Queued.Count, 0);
            Assert.AreEqual(result.Synced.Count, 0);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_DoneExistOnly()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
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

            List<SpotExceptionsRecommendedPlanSpotsToDoDto> recommendedPlanSpotsToDo = null;
            var recommendedPlanSpotsDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDone(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Queued.Count, 1);
            Assert.AreEqual(result.Synced.Count, 1);
        }

        [Test]
        public async void GetSpotExceptionsRecommendedPlanSpots_DoesNotExist()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
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

            List<SpotExceptionsRecommendedPlanSpotsToDoDto> recommendedPlanSpotsToDo = null;
            List<SpotExceptionsRecommendedPlanSpotsDoneDto> recommendedPlanSpotsDone = null;

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDone(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsDone));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            _SpotLengthRepositoryMock.Setup(x => x.GetSpotLengthById(It.IsAny<int>()))
                .Returns(30);

            //Act
            var result = await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanSpotsAsync(recommendedPlanSpotRequest);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Queued.Count, 0);
            Assert.AreEqual(result.Synced.Count, 0);
        }

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

        [Test]
        public void GetSpotExceptionsRecommendedPlanSpots_ThrowsException()
        {
            //Arrange
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlanSpotsRequestDto recommendedPlanSpotRequest = new SpotExceptionsRecommendedPlanSpotsRequestDto
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

            var recommendedPlanSpotsToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            var recommendedPlanSpotsDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanSpotsToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlanSpotsDone(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
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
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";
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
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlanFiltersToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            var ecommendedPlanFiltersDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanFiltersToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(ecommendedPlanFiltersDone));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .SetupSequence(x => x.GetMarketName(It.IsAny<int>()))
                .Returns("Columbus, OH")
                .Returns("Paducah-Cape Girard-Harsbg")
                .Returns("Seattle-Tacoma");

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
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlanFiltersToDo = new List<SpotExceptionsRecommendedPlanSpotsToDoDto>();
            var ecommendedPlanFiltersDone = new List<SpotExceptionsRecommendedPlanSpotsDoneDto>();

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanFiltersToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(ecommendedPlanFiltersDone));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .SetupSequence(x => x.GetMarketName(It.IsAny<int>()))
                .Returns("Columbus, OH")
                .Returns("Paducah-Cape Girard-Harsbg")
                .Returns("Seattle-Tacoma")
                .Returns("Evansville");

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
            var ingestedAt = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var recommendedPlanFiltersToDo = _GetRecommendedToDoData(ingestedBy, ingestedAt);
            var ecommendedPlanFiltersDone = _GetRecommendedDoneData(ingestedBy, ingestedAt);

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansToDoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(recommendedPlanFiltersToDo));

            _SpotExceptionsRecommendedPlanRepositoryMock
                .Setup(x => x.GetRecommendedPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsRecommendedPlanService.GetRecommendedPlanFilters(recommendedPlanFiltersRequest));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Recommended Plan Filters", result.Message);
        }


        private List<SpotExceptionsRecommendedPlanSpotsToDoDto> _GetRecommendedToDoData(string ingestedBy, DateTime ingestedAt)
        {
            return new List<SpotExceptionsRecommendedPlanSpotsToDoDto>
            {
                new SpotExceptionsRecommendedPlanSpotsToDoDto
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
                new SpotExceptionsRecommendedPlanSpotsToDoDto
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

        private List<SpotExceptionsRecommendedPlanSpotsDoneDto> _GetRecommendedDoneData(string ingestedBy, DateTime ingestedAt)
        {
            return new List<SpotExceptionsRecommendedPlanSpotsDoneDto>
            {
                new SpotExceptionsRecommendedPlanSpotsDoneDto
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
                new SpotExceptionsRecommendedPlanSpotsDoneDto
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
                            SpotExceptionsRecommendedPlanDoneDecisions = new SpotExceptionsRecommendedPlanSpotDecisionsDoneDto
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
            };
        }
    }
}
