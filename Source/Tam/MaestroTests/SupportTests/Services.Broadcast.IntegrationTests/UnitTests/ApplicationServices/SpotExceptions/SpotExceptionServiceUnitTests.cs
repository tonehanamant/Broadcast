using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionServiceUnitTests
    {
        private SpotExceptionService _SpotExceptionService;
        private Mock<IProgramService> _ProgramService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IPlanRepository> _PlanRepositoryMock;
        private Mock<ISpotExceptionRepository> _SpotExceptionRepositoryMock;
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IAabEngine> _AabEngine;
        private Mock<IGenreCache> _GenreCacheMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _AabEngine = new Mock<IAabEngine>();
            _SpotExceptionRepositoryMock = new Mock<ISpotExceptionRepository>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _ProgramService = new Mock<IProgramService>();
            _GenreCacheMock = new Mock<IGenreCache>();
            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionRepository>())
                .Returns(_SpotExceptionRepositoryMock.Object);
            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepositoryMock.Object);
            _DataRepositoryFactoryMock
              .Setup(x => x.GetDataRepository<IPlanRepository>())
              .Returns(_PlanRepositoryMock.Object);
            _SpotExceptionService = new SpotExceptionService(_DataRepositoryFactoryMock.Object,
                _AabEngine.Object,
                _ProgramService.Object,
                _FeatureToggleMock.Object,
                _ConfigurationSettingsHelperMock.Object, _DateTimeEngineMock.Object,_GenreCacheMock.Object);
        }

        //[Test]
        //public void GetSpotExceptionsRecommendedPlanDetails_RecommendedPlanDetails_DoesNotExist()
        //{
        //    // Arrange
        //    var spotExceptionsRecommendedPlanId = 1;
        //    SpotExceptionsRecommendedPlansDto spotExceptionsRecommendedPlans = null;
        //    var spotExceptionRecommendedPlansDetails = new SpotExceptionsRecommendedPlanDetailsResultDto();

        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
        //        .Returns(spotExceptionsRecommendedPlans);

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanId);

        //    // Assert
        //    Assert.AreEqual(spotExceptionRecommendedPlansDetails.ToString(), result.ToString());
        //}

        //[Test]
        //public void GetSpotExceptionsRecommendedPlanDetails_RecommendedPlanDetails_Exist()
        //{
        //    // Arrange
        //    var ingestedDateTime = new DateTime(2010, 10, 12);
        //    var ingestedBy = "Repository Test User";
        //    var spotExceptionsRecommendedPlanId = 1;

        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
        //        .Returns(new SpotExceptionsRecommendedPlansDto
        //        {
        //            Id = 1,
        //            SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
        //            AmbiguityCode = 1,
        //            ExecutionIdExternal = "220609090855BRt8EHXqSy",
        //            EstimateId = 6840,
        //            InventorySource = "Tegna",
        //            HouseIsci = "840T42AY13H",
        //            ClientIsci = "QMAY2913OS1H",
        //            SpotLengthId = 3,
        //            ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
        //            StationLegacyCallLetters = "WBNS",
        //            Affiliate = "CBS",
        //            MarketCode = 135,
        //            MarketRank = 33,
        //            ProgramName = "CBS Mornings",
        //            ProgramGenre = "INFORMATIONAL/NEWS",
        //            IngestedBy = ingestedBy,
        //            IngestedAt = ingestedDateTime,
        //            IngestedMediaWeekId = 1,
        //            SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
        //            {
        //                new SpotExceptionsRecommendedPlanDetailsDto
        //                {   
        //                    Id = 102,
        //                    SpotExceptionsRecommendedPlanId = 1,
        //                    RecommendedPlanId = 301,
        //                    ExecutionTraceId = 73,
        //                    Rate = 0.00m,
        //                    AudienceName = "Women 25-54",
        //                    ContractedImpressions = 100000,
        //                    DeliveredImpressions = 50000,
        //                    IsRecommendedPlan = false,
        //                    PlanClearancePercentage = null,
        //                    DaypartCode = "SYN",
        //                    StartTime = 28800,
        //                    EndTime = 7199,
        //                    Monday = 1,
        //                    Tuesday = 1,
        //                    Wednesday = 1,
        //                    Thursday = 1,
        //                    Friday = 1,
        //                    Saturday = 1,
        //                    Sunday = 1,
        //                    SpotDeliveredImpressions = 0.0,
        //                    PlanTotalContractedImpressions = 5000,
        //                    PlanTotalDeliveredImpressions = 5000,
        //                    RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                    {
        //                        Id = 301,
        //                        Name = "2Q' 21 Reynolds Foil TDN and SYN Upfront",
        //                        FlightStartDate = new DateTime(2019, 12, 1),
        //                        FlightEndDate = new DateTime(2020, 2, 1),
        //                        SpotLengths = new List<SpotLengthDto>
        //                        {
        //                            new SpotLengthDto
        //                            {
        //                                Id = 16,
        //                                Length = 45
        //                            }
        //                        }
        //                    },
        //                    SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
        //                    {
        //                        Id = 202,
        //                        SpotExceptionsRecommendedPlanDetailId = 102,
        //                        UserName = "Test User",
        //                        CreatedAt = new DateTime(2020,10,25)
        //                    }
        //                },
        //                new SpotExceptionsRecommendedPlanDetailsDto
        //                {
        //                    Id = 103,
        //                    SpotExceptionsRecommendedPlanId = 1,
        //                    RecommendedPlanId = 302,
        //                    ExecutionTraceId = 75,
        //                    Rate = 0.00m,
        //                    AudienceName = "Women 25-54",
        //                    ContractedImpressions = 100000,
        //                    DeliveredImpressions = 50000,
        //                    IsRecommendedPlan = true,
        //                    PlanClearancePercentage = null,
        //                    DaypartCode = "EM",
        //                    StartTime = 18000,
        //                    EndTime = 35999,
        //                    Monday = 1,
        //                    Tuesday = 1,
        //                    Wednesday = 1,
        //                    Thursday = 1,
        //                    Friday = 1,
        //                    Saturday = 1,
        //                    Sunday = 1,
        //                    SpotDeliveredImpressions = 0.0,
        //                    PlanTotalDeliveredImpressions = 50000,
        //                    PlanTotalContractedImpressions = 5000,
        //                    RecommendedPlanDetail = new RecommendedPlanDetailDto
        //                    {
        //                        Id = 302,
        //                        Name = "2Q' 21 Reynolds",
        //                        FlightStartDate = new DateTime(2019, 12, 1),
        //                        FlightEndDate = new DateTime(2020, 2, 1),
        //                        SpotLengths = new List<SpotLengthDto>
        //                        {
        //                            new SpotLengthDto
        //                            {
        //                                Id = 14,
        //                                Length = 15
        //                            },
        //                            new SpotLengthDto
        //                            {
        //                                Id = 15,
        //                                Length = 30
        //                            }
        //                        }
        //                    },
        //                    SpotExceptionsRecommendedPlanDecision = null
        //                }
        //            }
        //        });

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanId);

        //    // Assert
        //    Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        //}

        //[Test]
        //public void GetSpotExceptionsRecommendedPlanDetails_ThrowsException()
        //{
        //    // Arrange
        //    var spotExceptionsRecommendedPlanId = 1;

        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
        //        .Callback(() =>
        //        {
        //            throw new Exception("Throwing a test exception.");
        //        });

        //    // Act
        //    var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanId));

        //    // Assert
        //    Assert.AreEqual("Throwing a test exception.", result.Message);
        //}

        //[Test]
        //public void SaveSpotExceptionsRecommendedPlan_SaveSpotExceptionsRecommendedPlanDecision()
        //{
        //    // Arrange
        //    var spotExceptionsRecommendedPlanSaveRequest = new SpotExceptionsRecommendedPlanSaveRequestDto
        //    {
        //        AcceptAsInSpec = true,
        //        SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
        //         {
        //             new SpotExceptionsRecommendedPlanSaveDto()
        //             {
        //                  Id = 261,
        //                  SelectedPlanId = 219
        //             },
        //             new SpotExceptionsRecommendedPlanSaveDto()
        //             {
        //                 Id = 262,
        //                 SelectedPlanId =220
        //             }
        //         }
        //    };
        //    var userName = "Test User";
        //    var currentDateTime = DateTime.Now;
        //    _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
        //        .Returns(currentDateTime);
        //    bool isSpotExceptionsRecommendedPlanDecisionSaved = false;
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SaveSpotExceptionsRecommendedPlanDecision(It.IsAny<SpotExceptionsRecommendedPlanDecisionDto>()))
        //        .Callback(() => isSpotExceptionsRecommendedPlanDecisionSaved = true)
        //        .Returns(isSpotExceptionsRecommendedPlanDecisionSaved);

        //    var expectedResult = true;

        //    // Act
        //    var result = _SpotExceptionService.SaveSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanSaveRequest, userName);

        //    // Assert
        //    Assert.AreEqual(expectedResult, isSpotExceptionsRecommendedPlanDecisionSaved);
        //}

       
        //[Test]
        //public void SaveSpotExceptionsRecommendedPlan_ThrowsException()
        //{
        //    // Arrange
        //    var spotExceptionsRecommendedPlanSaveRequest = new SpotExceptionsRecommendedPlanSaveRequestDto
        //    {
        //        AcceptAsInSpec = true,
        //        SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>
        //         {
        //             new SpotExceptionsRecommendedPlanSaveDto()
        //             {
        //                  Id = 261,
        //                  SelectedPlanId = 219
        //             },
        //             new SpotExceptionsRecommendedPlanSaveDto()
        //             {
        //                 Id = 262,
        //                 SelectedPlanId =220
        //             }
        //         }
        //    };
        //    var userName = "Test User";
        //    var currentDateTime = DateTime.Now;

        //    _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
        //        .Returns(currentDateTime);

        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SaveSpotExceptionsRecommendedPlanDecision(It.IsAny<SpotExceptionsRecommendedPlanDecisionDto>()))
        //        .Callback(() =>
        //        {
        //            throw new Exception("Throwing a test exception.");
        //        });

        //    // Act
        //    var result = Assert.Throws<Exception>(() => _SpotExceptionService.SaveSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanSaveRequest, userName));

        //    // Assert
        //    Assert.AreEqual("Throwing a test exception.", result.Message);
        //}

        //[Test]
        //public void GetSpotExceptionsOutOfSpecsPosts_OutOfSpecExist()
        //{
        //    // Arrange
        //    SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto
        //    {
        //        WeekStartDate = new DateTime(2021, 01, 04),
        //        WeekEndDate = new DateTime(2021, 01, 10)

        //    };
        //    _SpotExceptionRepositoryMock
        //        .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //        .Returns(_GetOutOfSpecData());

        //    _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
        //        .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest);

        //    // Assert
        //    Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        //    Assert.AreEqual(result.Count, 3);
        //}
        //[Test]
        //public void GetSpotExceptionsOutOfSpecsPosts_OutOfSpec_DoesNotExist()
        //{
        //    // Arrange
        //    List<SpotExceptionsOutOfSpecsDto> outofSpecData = null;
        //    SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto
        //    {
        //        WeekStartDate = new DateTime(2021, 01, 04),
        //        WeekEndDate = new DateTime(2021, 01, 10)

        //    };
        //    _SpotExceptionRepositoryMock
        //        .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //        .Returns(outofSpecData);

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest);
        //    // Assert            
        //    Assert.AreEqual(result.Count, 0);
        //}
        //[Test]
        //public void GetSpotExceptionsOutOfSpecsPosts_ThrowsException()
        //{
        //    // Arrange
        //    SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto
        //    {
        //        WeekStartDate = new DateTime(2021, 01, 04),
        //        WeekEndDate = new DateTime(2021, 01, 10)

        //    };
        //    _SpotExceptionRepositoryMock
        //        .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //        .Callback(() =>
        //        {
        //            throw new Exception("Throwing a test exception.");
        //        });
        //    // Act           
        //    var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest));
        //    // Assert
        //    Assert.AreEqual("Throwing a test exception.", result.Message);
        //}
        //[Test]
        //public void GetSpotExceptionOutofSpecsDetails_DoesNotExist()
        //{
        //    // Arrange
        //    int spotExceptionsOutOfSpecId = 1;

        //    SpotExceptionsOutOfSpecsDto spotExceptionsOutOfSpecs = null;
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionsOutOfSpecById(It.IsAny<int>()))
        //        .Returns(spotExceptionsOutOfSpecs);

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId);

        //    // Assert
        //    Assert.IsNull(result);
        //}
        //[Test]
        //public void GetSpotExceptionOutofSpecsDetails_Exist()
        //{
        //    // Arrange
        //    int spotExceptionsOutOfSpecId = 1;

        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionsOutOfSpecById(It.IsAny<int>()))
        //        .Returns(new SpotExceptionsOutOfSpecsDto
        //        {
        //            Id = 1,
        //            ReasonCodeMessage = "",
        //            EstimateId = 191760,
        //            IsciName = "CC44ZZPT4",
        //            RecommendedPlanId = 11728,
        //            RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
        //            ProgramName = "Reckitt HYHO",
        //            StationLegacyCallLetters = "KXMC",
        //            Affiliate = "CBS",
        //            Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
        //            SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            AudienceId = 426,
        //            Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            Product = "Nike",
        //            FlightStartDate = new DateTime(2019, 12, 1),
        //            FlightEndDate = new DateTime(2020, 2, 1),
        //            ProgramNetwork = "ABC",
        //            ProgramAirTime = new DateTime(2020, 1, 10, 23, 45, 00),
        //            IngestedAt = new DateTime(2019, 1, 1),
        //            IngestedBy = "Repository Test User",
        //            IngestedMediaWeekId = 1,
        //            SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
        //            {
        //                SpotExceptionsOutOfSpecId = 1,
        //                AcceptedAsInSpec = true,
        //                DecisionNotes = "TestDecisionNotes",
        //                UserName = "TestUser",
        //                CreatedAt = new DateTime(2020, 2, 1)
        //            },
        //            SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                ReasonCode = 1,
        //                Reason = "spot aired outside daypart",
        //                Label = "Daypart"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
        //            HouseIsci = "289J76GN16H",
        //            GenreName = "News"
        //        });

        //    // Act
        //    var result = _SpotExceptionService.GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId);

        //    // Assert
        //    Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));

        //}
        //[Test]
        //public void GetSpotExceptionOutofSpecsDetails_ThrowsException()
        //{
        //    // Arrange
        //    int spotExceptionsOutOfSpecId = 1;

        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.GetSpotExceptionsOutOfSpecById(It.IsAny<int>()))
        //        .Callback(() =>
        //        {
        //            throw new Exception("Throwing a test exception.");
        //        });

        //    // Act
        //    var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId));

        //    // Assert
        //    Assert.AreEqual("Throwing a test exception.", result.Message);
        //}
        //private List<SpotExceptionsOutOfSpecsDto> _GetOutOfSpecData()
        //{
        //    return new List<SpotExceptionsOutOfSpecsDto>()
        //    {
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //            Id = 1,
        //            ReasonCodeMessage="",
        //            EstimateId =191760,
        //            IsciName = "CC44ZZPT4",
        //            RecommendedPlanId = 11728,
        //            RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
        //            ProgramName = "Reckitt HYHO",
        //            StationLegacyCallLetters = "KXMC",
        //            AdvertiserMasterId = new Guid("D04797DB-870C-42D1-BC6C-CAF823D7A4D9"),
        //            Affiliate = "CBS",
        //            Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
        //             SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            AudienceId = 426,
        //            Product = "Nike",
        //            FlightStartDate = new DateTime(2019, 12, 1),
        //            FlightEndDate = new DateTime(2020, 2, 1),
        //            Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //            ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //            IngestedAt = new DateTime(2019,1,1),
        //            IngestedBy = "Repository Test User",
        //            IngestedMediaWeekId = 1,
        //            SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
        //           {
        //               SpotExceptionsOutOfSpecId=1,
        //               AcceptedAsInSpec=true,
        //               DecisionNotes="TestDecisionNotes",
        //               UserName = "TestUser",
        //               CreatedAt = new DateTime(2020, 2, 1)
        //           },
        //            SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 2,
        //                ReasonCode = 1,
        //                Reason = "spot aired outside daypart",
        //                Label = "Daypart"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDMwOA==",
        //            HouseIsci = "289J76GN16H",
        //            GenreName = "Drama"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //          Id = 2,
        //          ReasonCodeMessage="",
        //          EstimateId= 191757,
        //          IsciName="AB82VR58",
        //          RecommendedPlanId= 11726,
        //          RecommendedPlanName="4Q' 21 Reynolds Foil TDN and SYN Upfront",
        //          ProgramName="Reynolds Foil @9",
        //          AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //          StationLegacyCallLetters="KSTP",
        //          Affiliate = "NBC",
        //          Market = "Phoenix (Prescott)",
        //           SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //          AudienceId= 430,
        //          Product="Spotify",
        //          FlightStartDate =  new DateTime(2018, 7, 2),
        //          FlightEndDate = new DateTime(2018, 8, 2),
        //          DaypartCode="PT",
        //          GenreName="Horror",
        //          Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //          ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //          IngestedAt = new DateTime(2019,1,1),
        //          IngestedBy = "Repository Test User",
        //          IngestedMediaWeekId = 1,
        //          SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
        //           {
        //               SpotExceptionsOutOfSpecId=2,
        //               AcceptedAsInSpec=true,
        //               DecisionNotes="",
        //               UserName = "MockData",
        //               CreatedAt = new DateTime(2020, 2, 1)
        //           },
        //          SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 3,
        //                ReasonCode = 2,
        //                Reason = "genre content restriction",
        //                Label = "Genre"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDMwOA==",
        //            HouseIsci = "289J76GN16H"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //          Id = 3,
        //          ReasonCodeMessage="",
        //          EstimateId= 191758,
        //          IsciName="AB44NR58",
        //          RecommendedPlanId= 11725,
        //          RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
        //          ProgramName="TEN O'CLOCK NEWS",
        //          AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //          StationLegacyCallLetters = "KSTP",
        //          Affiliate = "ABC",
        //          Market = "Lincoln & Hastings-Krny",
        //           SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //          AudienceId= 430,
        //          Product="Spotify",
        //          FlightStartDate =  new DateTime(2018, 7, 2),
        //          FlightEndDate = new DateTime(2018, 8, 2),
        //          DaypartCode="PT",
        //          GenreName="Horror",
        //          Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //          ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //          IngestedAt = new DateTime(2019,1,1),
        //          IngestedBy = "Repository Test User",
        //          IngestedMediaWeekId = 1,
        //          SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
        //           {
        //               SpotExceptionsOutOfSpecId=3,
        //               AcceptedAsInSpec=true,
        //               DecisionNotes="",
        //               UserName = "MockData",
        //               CreatedAt = new DateTime(2020, 2, 1)
        //           },
        //          SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 4,
        //                ReasonCode = 3,
        //                Reason = "affiliate content restriction",
        //                Label = "Affiliate"
        //            },
        //          SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
        //            HouseIsci = "289J76GN16H"
        //        },
        //    };
        //}

        //private List<SpotExceptionsOutOfSpecsDto> _GetOutOfSpecPlansData()
        //{
        //    return new List<SpotExceptionsOutOfSpecsDto>()
        //    {
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //            Id = 1,
        //            ReasonCodeMessage="",
        //            EstimateId =191760,
        //            IsciName = "CC44ZZPT4",
        //            RecommendedPlanId = 215,
        //            RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
        //            ProgramName = "Reckitt HYHO",
        //            StationLegacyCallLetters = "KXMC",
        //            AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //            Affiliate = "CBS",
        //            Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
        //            PlanId = 215,
        //            SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            AudienceId = 426,
        //            Product = "Nike",
        //            FlightStartDate = new DateTime(2019, 12, 1),
        //            FlightEndDate = new DateTime(2019, 12, 9),
        //            DaypartCode="PT",
        //            GenreName="Horror",
        //            Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 70642,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //            ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //            IngestedAt = new DateTime(2019,1,1),
        //            IngestedBy = "Repository Test User",
        //            IngestedMediaWeekId = 1,
        //            SpotExceptionsOutOfSpecDecision=null,
        //            SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 2,
        //                ReasonCode = 1,
        //                Reason = "spot aired outside daypart",
        //                Label = "Daypart"
        //            },
        //          SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
        //          HouseIsci = "289J76GN16H"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //          Id = 2,
        //          ReasonCodeMessage="",
        //          EstimateId= 191757,
        //          IsciName="AB82VR58",
        //          RecommendedPlanId= 215,
        //          RecommendedPlanName="4Q' 21 Reynolds Foil TDN and SYN Upfront",
        //          ProgramName="Reynolds Foil @9",
        //          AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //          StationLegacyCallLetters="KSTP",
        //          Affiliate = "NBC",
        //          Market = "Phoenix (Prescott)",
        //          PlanId = 215,
        //           SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //          AudienceId= 426,
        //          Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 70642,
        //                Code = "CUS"
        //            },
        //          Product="Spotify",
        //          FlightStartDate =  new DateTime(2018, 7, 2),
        //          FlightEndDate = new DateTime(2018, 8, 2),
        //          DaypartCode="PT",
        //          GenreName="Horror",
        //          ProgramNetwork = "",
        //          ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //          IngestedAt = new DateTime(2019,1,1),
        //          IngestedBy = "Repository Test User",
        //          IngestedMediaWeekId = 1,
        //          SpotExceptionsOutOfSpecDecision =null,
        //          SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 3,
        //                ReasonCode = 2,
        //                Reason = "genre content restriction",
        //                Label = "Genre"
        //            },
        //          SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
        //          HouseIsci = "289J76GN16H"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //          Id = 3,
        //          ReasonCodeMessage="",
        //          EstimateId= 191758,
        //          IsciName="AB44NR58",
        //          RecommendedPlanId= 218,
        //          RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
        //          ProgramName="TEN O'CLOCK NEWS",
        //          AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //          StationLegacyCallLetters = "KSTP",
        //          Affiliate = "ABC",
        //          Market = "Lincoln & Hastings-Krny",
        //          PlanId = 218,
        //           SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //          AudienceId= 426,
        //          Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 70642,
        //                Code = "CUS"
        //            },
        //          Product="Spotify",
        //          FlightStartDate =  new DateTime(2018, 7, 2),
        //          FlightEndDate = new DateTime(2018, 8, 2),
        //          DaypartCode="PT",
        //          GenreName="Horror",
        //          ProgramNetwork = "",
        //          ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //          IngestedAt = new DateTime(2019,1,1),
        //          IngestedBy = "Repository Test User",
        //          IngestedMediaWeekId = 1,
        //          SpotExceptionsOutOfSpecDecision =  null,
        //          SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 4,
        //                ReasonCode = 3,
        //                Reason = "affiliate content restriction",
        //                Label = "Affiliate"
        //            },
        //          SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
        //          HouseIsci = "289J76GN16H"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //          Id = 4,
        //          ReasonCodeMessage="",
        //          EstimateId= 191759,
        //          IsciName="AB44NR59",
        //          RecommendedPlanId= 11726,
        //          RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
        //          ProgramName="TEN O'CLOCK NEWS",
        //          AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //          StationLegacyCallLetters = "KSTP",
        //          Affiliate = "ABC",
        //          Market = "Lincoln & Hastings-Krny",
        //           SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //          AudienceId= 430,
        //          Product="Spotify",
        //          FlightStartDate =  new DateTime(2018, 7, 2),
        //          FlightEndDate = new DateTime(2018, 8, 2),
        //          DaypartCode="PT",
        //          GenreName="Horror",
        //          Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 70642,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //          ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //          IngestedAt = new DateTime(2019,1,1),
        //          IngestedBy = "Repository Test User",
        //          IngestedMediaWeekId = 1,
        //          SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
        //           {
        //               SpotExceptionsOutOfSpecId=4,
        //               AcceptedAsInSpec=true,
        //               DecisionNotes="",
        //               UserName = "MockData",
        //               CreatedAt = new DateTime(2020, 2, 1)
        //           },
        //          SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 4,
        //                ReasonCode = 3,
        //                Reason = "affiliate content restriction",
        //                Label = "Affiliate"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxOTY3MA==",
        //            HouseIsci = "289J76GN16H"
        //        },
        //    };
        //}

        //private List<SpotExceptionsOutOfSpecsDto> _GetOutOfSpecPlanSpotsData()
        //{
        //    return new List<SpotExceptionsOutOfSpecsDto>()
        //    {
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //            Id = 1,
        //            ReasonCodeMessage = "",
        //            EstimateId = 191760,
        //            IsciName = "CC44ZZPT4",
        //            RecommendedPlanId = 215,
        //            RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
        //            ProgramName = "Reckitt HYHO",
        //            StationLegacyCallLetters = "KXMC",
        //            AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //            Affiliate = "CBS",
        //            Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
        //            PlanId = 215,
        //             SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            AudienceId = 426,
        //            Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //            Product = "Nike",
        //            FlightStartDate = new DateTime(2019, 12, 1),
        //            FlightEndDate = new DateTime(2019, 12, 9),
        //            ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //            IngestedAt = new DateTime(2019,1,1),
        //            IngestedBy = "Repository Test User",
        //            IngestedMediaWeekId = 1,
        //            SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
        //            {
        //               SpotExceptionsOutOfSpecId = 1,
        //               AcceptedAsInSpec = true,
        //               DecisionNotes = "",
        //               UserName = "MockData",
        //               CreatedAt = new DateTime(2020, 2, 1),
        //               SyncedBy = null,
        //               SyncedAt = new DateTime(2020,1,10,23,45,00)
        //            },
        //            SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 2,
        //                ReasonCode = 1,
        //                Reason = "spot aired outside daypart",
        //                Label = "Daypart"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDYzNg==",
        //            HouseIsci = "289J76GN16H",
        //            Comments = "test Comment",
        //            InventorySourceName = "TVB"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //            Id = 2,
        //            ReasonCodeMessage = "",
        //            EstimateId = 191760,
        //            IsciName = "CC44ZZPT4",
        //            RecommendedPlanId = 215,
        //            RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
        //            ProgramName = "Reckitt HYHO",
        //            StationLegacyCallLetters = "KXMC",
        //            AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //            Affiliate = "CBS",
        //            Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
        //            PlanId = 215,
        //             SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            AudienceId = 426,
        //            Product = "Nike",
        //            FlightStartDate = new DateTime(2019, 12, 1),
        //            FlightEndDate = new DateTime(2019, 12, 9),
        //            Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //            ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //            IngestedAt = new DateTime(2019,1,1),
        //            IngestedBy = "Repository Test User",
        //            IngestedMediaWeekId = 1,
        //            SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
        //            {
        //               SpotExceptionsOutOfSpecId = 2,
        //               AcceptedAsInSpec = true,
        //               DecisionNotes = "",
        //               UserName = "MockData",
        //               CreatedAt = new DateTime(2020, 2, 1),
        //               SyncedBy = null,
        //               SyncedAt = null
        //            },
        //            SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 2,
        //                ReasonCode = 1,
        //                Reason = "spot aired outside daypart",
        //                Label = "Daypart"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
        //            HouseIsci = "289J76GN16H",
        //            Comments = "test Comment",
        //            InventorySourceName = "TVB"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //            Id = 3,
        //            ReasonCodeMessage = "",
        //            EstimateId = 191760,
        //            IsciName = "CC44ZZPT4",
        //            RecommendedPlanId = 215,
        //            RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
        //            ProgramName = "Reckitt HYHO",
        //            StationLegacyCallLetters = "KXMC",
        //            AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //            Affiliate = "CBS",
        //            Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
        //            PlanId = 215,
        //             SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            AudienceId = 426,
        //            Product = "Nike",
        //            FlightStartDate = new DateTime(2019, 12, 1),
        //            FlightEndDate = new DateTime(2019, 12, 9),
        //            Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //            ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //            IngestedAt = new DateTime(2019,1,1),
        //            IngestedBy = "Repository Test User",
        //            IngestedMediaWeekId = 1,
        //            SpotExceptionsOutOfSpecDecision = null,
        //            SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 2,
        //                ReasonCode = 1,
        //                Reason = "spot aired outside daypart",
        //                Label = "Daypart"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
        //            HouseIsci = "289J76GN16H",
        //            Comments = "test Comment",
        //            GenreName="Comedy",
        //            DaypartCode="ROSP",
        //            InventorySourceName = "TVB"
        //        },
        //        new SpotExceptionsOutOfSpecsDto
        //        {
        //            Id = 4,
        //            ReasonCodeMessage = "",
        //            EstimateId = 191760,
        //            IsciName = "CC44ZZPT4",
        //            RecommendedPlanId = 215,
        //            RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
        //            ProgramName = "Reckitt HYHO",
        //            StationLegacyCallLetters = "KXMC",
        //            AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
        //            Affiliate = "CBS",
        //            Market = "Cincinnati",
        //            PlanId = 215,
        //             SpotLength = new SpotLengthDto
        //            {
        //                Id = 16,
        //                Length = 45
        //            },
        //            AudienceId = 426,
        //            Product = "Nike",
        //            FlightStartDate = new DateTime(2019, 12, 1),
        //            FlightEndDate = new DateTime(2019, 12, 9),
        //            Audience = new AudienceDto
        //            {
        //                Id = 426,
        //                Code = "M50-64",
        //                Name = "Men 50-64"
        //            },
        //            DaypartDetail = new DaypartDetailDto
        //            {
        //                Id = 71646,
        //                Code = "CUS"
        //            },
        //            ProgramNetwork = "ABC",
        //            ProgramAirTime = new DateTime(2020,1,10,23,45,00),
        //            IngestedAt = new DateTime(2019,1,1),
        //            IngestedBy = "Repository Test User",
        //            IngestedMediaWeekId = 1,
        //            SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
        //            {
        //               SpotExceptionsOutOfSpecId = 4,
        //               AcceptedAsInSpec = true,
        //               DecisionNotes = "",
        //               UserName = "MockData",
        //               CreatedAt = new DateTime(2020, 2, 1),
        //               SyncedBy = null,
        //               SyncedAt = null,
        //            },
        //            SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
        //            {
        //                Id = 2,
        //                ReasonCode = 1,
        //                Reason = "spot aired outside daypart",
        //                Label = "Daypart"
        //            },
        //            SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
        //            HouseIsci = "289J76GN16H",
        //            Comments = "test Comment",
        //            InventorySourceName = "TVB"
        //        },
        //    };
        //}

        

        //[Test]
        //public void SaveSpotExceptionsOutOfSpecsDecisions()
        //{
        //    // Arrange
        //    var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecDecisionsPostsRequestDto
        //    {
        //        Id = 14,
        //        AcceptAsInSpec = true,
        //        DecisionNotes = "Test Case execution"
        //    };
        //    string userName = "Test User";
        //    bool result = false;
        //    bool expectedResult = true;

        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisions(It.IsAny<SpotExceptionsOutOfSpecDecisionsPostsRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
        //        .Returns(expectedResult);

        //    // Act
        //    result = _SpotExceptionService.SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

        //    // Assert
        //    Assert.AreEqual(expectedResult, result);
        //}

        //[Test]
        //public void SaveSpotExceptionsOutOfSpecsDecisions_ThrowsException()
        //{
        //    // Arrange
        //    var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecDecisionsPostsRequestDto
        //    {
        //        Id = 14,
        //        AcceptAsInSpec = true,
        //        DecisionNotes = "Test Case execution"
        //    };
        //    string userName = "Test User";
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisions(It.IsAny<SpotExceptionsOutOfSpecDecisionsPostsRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
        //        .Callback(() =>
        //        {
        //            throw new Exception("Throwing a test exception.");
        //        });

        //    // Act
        //    var result = Assert.Throws<Exception>(() => _SpotExceptionService.SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName));

        //    // Assert
        //    Assert.AreEqual("Throwing a test exception.", result.Message);
        //}

        

        

        

        

        

        

        

        

        

        //[Test]
        //public void SaveSpotExceptionsOutOfSpecsDecisionsPlans()
        //{
        //    // Arrange
        //    var spotExceptionsOutOfSpecDecisionsPlansResult = new List<SpotExceptionsOutOfSpecDecisionsPlansDto>
        //    {
        //        new SpotExceptionsOutOfSpecDecisionsPlansDto
        //        {
        //            Id = 21,
        //            AcceptAsInSpec = true
        //        },
        //        new SpotExceptionsOutOfSpecDecisionsPlansDto
        //        {
        //            Id = 22,
        //            AcceptAsInSpec = true
        //        }
        //    };

        //    var spotExceptionSaveDecisionsPlansRequest = new SpotExceptionSaveDecisionsPlansRequestDto();
        //    spotExceptionSaveDecisionsPlansRequest.Decisions.AddRange(spotExceptionsOutOfSpecDecisionsPlansResult);

        //    string userName = "Test User";
        //    bool result = false;
        //    bool expectedResult = true;
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisionsPlans(It.IsAny<SpotExceptionSaveDecisionsPlansRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
        //        .Returns(expectedResult);

        //    // Act
        //    result = _SpotExceptionService.SaveOutofSpecDecisionsPlans(spotExceptionSaveDecisionsPlansRequest, userName);

        //    // Assert
        //    Assert.AreEqual(expectedResult, result);
        //}

        //[Test]
        //public void SaveSpotExceptionsOutOfSpecsDecisionsPlans_ThrowsException()
        //{
        //    // Arrange
        //    var spotExceptionsDecisionsPlansResult = new List<SpotExceptionsOutOfSpecDecisionsPlansDto>
        //    {
        //        new SpotExceptionsOutOfSpecDecisionsPlansDto
        //        {
        //            Id = 21,
        //            AcceptAsInSpec = true
        //        },
        //        new SpotExceptionsOutOfSpecDecisionsPlansDto
        //        {
        //            Id = 22,
        //            AcceptAsInSpec = true
        //        }
        //    };

        //    var spotExceptionOutOfSpecSaveDecisionsPlansRequest = new SpotExceptionSaveDecisionsPlansRequestDto();
        //    spotExceptionOutOfSpecSaveDecisionsPlansRequest.Decisions.AddRange(spotExceptionsDecisionsPlansResult);
        //    string userName = "Test User";
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisionsPlans(It.IsAny<SpotExceptionSaveDecisionsPlansRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
        //        .Callback(() =>
        //        {
        //            throw new Exception("Throwing a test exception.");
        //        });

        //    // Act
        //    var result = Assert.Throws<Exception>(() => _SpotExceptionService.SaveOutofSpecDecisionsPlans(spotExceptionOutOfSpecSaveDecisionsPlansRequest, userName));

        //    // Assert
        //    Assert.AreEqual("Throwing a test exception.", result.Message);
        //}

        

        

        //[Test]
        //public void TriggerRecommandedPlanDecisionSync_Exist()
        //{
        //    // Arrange

        //    var recommandedPlanDecision = new List<SpotExceptionsRecommendedPlanDecisionDto>()
        //    {
        //        new SpotExceptionsRecommendedPlanDecisionDto()
        //        {
        //            Id = 1,
        //            SpotExceptionsRecommendedPlanDetailId = 1,
        //            UserName = "Test User",
        //            CreatedAt = new DateTime(2020,1,10,23,45,00)
        //        },
        //        new SpotExceptionsRecommendedPlanDecisionDto()
        //        {
        //            Id = 2,
        //            SpotExceptionsRecommendedPlanDetailId = 2,
        //            UserName = "Test User",
        //            CreatedAt = new DateTime(2020,1,10,23,45,00)
        //        }
        //    };

        //    var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto
        //    {
        //        UserName = "Test User"
        //    };
        //    bool result = false;
        //    bool expectedResult = true;
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SyncRecommandedPlanDecision(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
        //        .Returns(expectedResult);

        //    // Act
        //    result = _SpotExceptionService.TriggerDecisionSync(triggerDecisionSyncRequest);

        //    // Assert
        //    Assert.AreEqual(expectedResult, result);
        //}

        

        //[Test]
        //public void GetSpotExceptionsOutOfSpecPrograms()
        //{
        //    // Arrange
        //    string programNameQuery = "WNRUSH";
        //    _SpotExceptionRepositoryMock.Setup(s => s.FindProgramFromPrograms(It.IsAny<string>()))
        //        .Returns(new List<ProgramNameDto>
        //            {
        //                new ProgramNameDto
        //                {
        //                   OfficialProgramName = "#HTOWNRUSH",
        //                   GenreId=  33
        //                }
        //            }
        //        );
        //    _GenreCacheMock
        //        .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
        //        .Returns(new LookupDto
        //        {
        //            Id = 1,
        //            Display = "Genre"
        //        });
        //    // Act           
        //    var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecPrograms(programNameQuery, "TestsUser");

        //    // Assert
        //    Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        //}

        //[Test]
        //public void SaveOutofSpecDecisionsPlans_WithProgramGenreAndDaypart()
        //{
        //    // Arrange
        //    var spotExceptionsOutOfSpecDecisionsPlansResult = new List<SpotExceptionsOutOfSpecDecisionsPlansDto>
        //    {
        //        new SpotExceptionsOutOfSpecDecisionsPlansDto
        //        {
        //            Id = 21,
        //            AcceptAsInSpec = true,
        //            ProgramName = "Program8",
        //            GenreName = "Horror",
        //            DaypartCode = "EMN"
        //        },
        //        new SpotExceptionsOutOfSpecDecisionsPlansDto
        //        {
        //            Id = 22,
        //            AcceptAsInSpec = false,
        //            ProgramName = "Program9",
        //            GenreName = "Comedy",
        //            DaypartCode = "EMN"
        //        }
        //    };

        //    var spotExceptionSaveDecisionsPlansRequest = new SpotExceptionSaveDecisionsPlansRequestDto();
        //    spotExceptionSaveDecisionsPlansRequest.Decisions.AddRange(spotExceptionsOutOfSpecDecisionsPlansResult);

        //    string userName = "Test User";
        //    bool result = false;
        //    bool expectedResult = true;
        //    _SpotExceptionRepositoryMock
        //        .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisionsPlans(It.IsAny<SpotExceptionSaveDecisionsPlansRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
        //        .Returns(expectedResult);

        //    // Act
        //    result = _SpotExceptionService.SaveOutofSpecDecisionsPlans(spotExceptionSaveDecisionsPlansRequest, userName);

        //    // Assert
        //    Assert.AreEqual(expectedResult, result);
        //} 
    }
}