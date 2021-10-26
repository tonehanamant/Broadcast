using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionServiceUnitTests
    {
        private SpotExceptionService _SpotExceptionService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionRepository> _SpotExceptionRepositoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<IDateTimeEngine> _DateTimeEngine;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionRepositoryMock = new Mock<ISpotExceptionRepository>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _DateTimeEngine = new Mock<IDateTimeEngine>();
            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionRepository>())
                .Returns(_SpotExceptionRepositoryMock.Object);

            _SpotExceptionService = new SpotExceptionService(_DataRepositoryFactoryMock.Object, _FeatureToggleMock.Object, _ConfigurationSettingsHelperMock.Object, _DateTimeEngine.Object);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_RecommendedPlan_DoesNotExist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsRecommendedPlansDto>());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_RecommendedPlans_Exist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsRecommendedPlansDto>
                {
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 2,
                        EstimateId =191757,
                        IsciName = "BB82TXT4P",
                        RecommendedPlanId = 11725,
                        RecommendedPlanName = "2Q' 21 Reynolds Foil TDN and SYN Upfront",
                        ProgramName = "FOX 13 10:00 News",
                        ProgramAirTime = new DateTime(2020,1,4,8,7,15),
                        StationLegacyCallLetters = "KSTP",
                        Affiliate = "ABC",
                        Market = "Lincoln & Hastings-Krny",
                        Cost = 700,
                        Impressions = 1000,
                        SpotLength = new SpotLengthDto
                        {
                            Id = 14,
                            Length = 15
                        },
                        Audience = new AudienceDto
                        {
                            Id = 425,
                            Code = "M18-24",
                            Name = "Men 18-24"
                        },
                        Product = "Spotify",
                        FlightStartDate = new DateTime(2019, 12, 1),
                        FlightEndDate = new DateTime(2020, 2, 1),
                        DaypartDetail = new DaypartDetailDto
                        {
                            Id = 71657,
                            Code = "PMN"
                        },
                        IngestedAt = new DateTime(2019,1,1),
                        IngestedBy = "Repository Test User",
                        SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>()
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 3,
                        EstimateId =191758,
                        IsciName = "CC42TXT4P",
                        RecommendedPlanId = 11726,
                        RecommendedPlanName = "4Q' 21 Reynolds Foil TDN and SYN Upfront",
                        ProgramName = "Reynolds Foil @9",
                        ProgramAirTime = new DateTime(2020,1,6,11,15,30),
                        StationLegacyCallLetters = "WDAY",
                        Affiliate = "NBC",
                        Market = "Phoenix (Prescott)",
                        Cost = 800,
                        Impressions = 1500,
                        SpotLength = new SpotLengthDto
                        {
                            Id = 15,
                            Length = 30
                        },
                        Audience = new AudienceDto
                        {
                            Id = 426,
                            Code = "W18-24",
                            Name = "Women 18-24"
                        },
                        Product = "Spotify",
                        FlightStartDate = new DateTime(2019, 12, 1),
                        FlightEndDate = new DateTime(2020, 2, 1),
                        DaypartDetail = new DaypartDetailDto
                        {
                            Id = 71658,
                            Code = "EN"
                        },
                        IngestedAt = new DateTime(2019,1,1),
                        IngestedBy = "Repository Test User",
                        SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>()
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                Id = 101,
                                SpotExceptionsRecommendedPlanId = 3,
                                IsRecommendedPlan = true,
                                SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                                {
                                    Id = 201,
                                    SpotExceptionsRecommendedPlanDetailId = 101,
                                    UserName = "Test User",
                                    CreatedAt = new DateTime(2020,10,25)
                                }
                            }
                        }
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 4,
                        EstimateId =191760,
                        IsciName = "CC44ZZPT4",
                        RecommendedPlanId = 11728,
                        RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                        ProgramName = "Reckitt HYHO",
                        ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                        StationLegacyCallLetters = "KXMC",
                        Affiliate = "CBS",
                        Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                        Cost = 450,
                        Impressions = 1752,
                        SpotLength = new SpotLengthDto
                        {
                            Id = 16,
                            Length = 45
                        },
                        Audience = new AudienceDto
                        {
                            Id = 427,
                            Code = "M50-64",
                            Name = "Men 50-64"
                        },
                        Product = "Nike",
                        FlightStartDate = new DateTime(2019, 12, 1),
                        FlightEndDate = new DateTime(2020, 2, 1),
                        DaypartDetail = new DaypartDetailDto
                        {
                            Id = 71659,
                            Code = "PMN"
                        },
                        IngestedAt = new DateTime(2019,1,1),
                        IngestedBy = "Repository Test User",
                        SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                Id = 102,
                                SpotExceptionsRecommendedPlanId = 4,
                                IsRecommendedPlan = false,
                                SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                                {
                                    Id = 202,
                                    SpotExceptionsRecommendedPlanDetailId = 102,
                                    UserName = "Test User",
                                    CreatedAt = new DateTime(2020,10,25)
                                }
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                Id = 103,
                                SpotExceptionsRecommendedPlanId = 4,
                                IsRecommendedPlan = true,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    }
                });

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_ThrowsException()
        {
            // Arrange
            var spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanDetails_RecommendedPlanDetails_DoesNotExist()
        {
            // Arrange
            var spotExceptionsRecommendedPlanDetailsRequest = new SpotExceptionsRecommendedPlanDetailsRequestDto
            {
                Id = 1
            };

            SpotExceptionsRecommendedPlansDto spotExceptionsRecommendedPlans = null;
            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
                .Returns(spotExceptionsRecommendedPlans);

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanDetailsRequest);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanDetails_RecommendedPlanDetails_Exist()
        {
            // Arrange
            var spotExceptionsRecommendedPlanDetailsRequest = new SpotExceptionsRecommendedPlanDetailsRequestDto
            {
                Id = 1
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
                .Returns(new SpotExceptionsRecommendedPlansDto
                {
                    Id = 1,
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 11728,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    ProgramAirTime = new DateTime(2020, 1, 10, 23, 45, 00),
                    StationLegacyCallLetters = "KXMC",
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    Cost = 450,
                    Impressions = 1752,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    Audience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2020, 2, 1),
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71659,
                        Code = "PMN"
                    },
                    IngestedAt = new DateTime(2019, 1, 1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            Id = 102,
                            SpotExceptionsRecommendedPlanId = 1,
                            RecommendedPlanDetail = new RecommendedPlanDetailDto
                            { 
                                Id = 301,
                                Name = "2Q' 21 Reynolds Foil TDN and SYN Upfront",
                                FlightStartDate = new DateTime(2019, 12, 1),
                                FlightEndDate = new DateTime(2020, 2, 1),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 16,
                                        Length = 45
                                    }
                                }
                            },
                            IsRecommendedPlan = false,
                            SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                            {
                                Id = 202,
                                SpotExceptionsRecommendedPlanDetailId = 102,
                                UserName = "Test User",
                                CreatedAt = new DateTime(2020,10,25)
                            }
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            Id = 103,
                            SpotExceptionsRecommendedPlanId = 1,
                            RecommendedPlanDetail = new RecommendedPlanDetailDto
                            {
                                Id = 302,
                                Name = "2Q' 21 Reynolds",
                                FlightStartDate = new DateTime(2019, 12, 1),
                                FlightEndDate = new DateTime(2020, 2, 1),
                                SpotLengths = new List<SpotLengthDto>
                                {
                                    new SpotLengthDto
                                    {
                                        Id = 14,
                                        Length = 15
                                    },
                                    new SpotLengthDto
                                    {
                                        Id = 15,
                                        Length = 30
                                    }
                                }
                            },
                            IsRecommendedPlan = true,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                });

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanDetailsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanDetails_ThrowsException()
        {
            // Arrange
            var spotExceptionsRecommendedPlanDetailsRequest = new SpotExceptionsRecommendedPlanDetailsRequestDto
            {
                Id = 1
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanDetailsRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPosts_OutOfSpecExist()
        {
            // Arrange
            SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)

            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetOutOfSpecData());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Count, 3);
        }
        [Test]
        public void GetSpotExceptionsOutOfSpecsPosts_OutOfSpec_DoesNotExist()
        {
            // Arrange
            List<SpotExceptionsOutOfSpecsDto> outofSpecData = null;
            SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)

            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outofSpecData);

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest);
            // Assert            
            Assert.AreEqual(result.Count, 0);
        }
        [Test]
        public void GetSpotExceptionsOutOfSpecsPosts_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)

            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });
            // Act           
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest));
            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        private List<SpotExceptionsOutOfSpecsDto> _GetOutOfSpecData()
        {
            return new List<SpotExceptionsOutOfSpecsDto>()
            {
                new SpotExceptionsOutOfSpecsDto
                {
                    Id = 1,
                    ReasonCode="",
                    ReasonCodeMessage="",
                    EstimateId =191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 11728,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    SpotLengthId = 16,
                    SpotLengthString ="45",
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2020, 2, 1),
                    DaypartId = 71646,
                    DaypartCode="CUS",
                    ProgramFlightStartDate=new DateTime(2019, 12, 1),
                    ProgramFlightEndDate=new DateTime(2020, 2, 1),
                    ProgramAudienceId=426,
                    AudienceName = "Men 50-64",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsOutOfSpecId = 1
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 2,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 11726,
                  RecommendedPlanName="4Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="Reynolds Foil @9",
                  StationLegacyCallLetters="KSTP",
                  Affiliate = "NBC",
                  Market = "Phoenix (Prescott)",
                  SpotLengthId= 15,
                  SpotLengthString="30",
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70642,
                  ProgramDaypartId= 70613,
                  DaypartCode="CUS",
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                  ProgramAudienceId = 425,
                  AudienceName="Women 18-24",
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  SpotExceptionsOutOfSpecId=2
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 3,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 11725,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  StationLegacyCallLetters = "KSTP",
                  Affiliate = "ABC",
                  Market = "Lincoln & Hastings-Krny",
                  SpotLengthId = 14,
                  SpotLengthString ="15",
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70643,
                  ProgramDaypartId= 70614,
                  DaypartCode="CUS",
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                  ProgramAudienceId = 425,
                  AudienceName = "Men 18-24",
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  SpotExceptionsOutOfSpecId=null,
                },
            };
        }

        [Test]
        public void SaveSpotExceptionsOutOfSpecsDecisions()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecDecisionsPostsRequestDto
            {
                Id = 14,
                AcceptAsInSpec = true,
                DecisionNotes = "Test Case execution"
            };
            string userName = "Test User";
            bool result = false;
            bool expectedResult = true;
            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisions(It.IsAny<SpotExceptionsOutOfSpecDecisionsPostsRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            // Act
            result = _SpotExceptionService.SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveSpotExceptionsOutOfSpecsDecisions_ThrowsException()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPostsRequest = new SpotExceptionsOutOfSpecDecisionsPostsRequestDto
            {
                Id = 14,
                AcceptAsInSpec = true,
                DecisionNotes = "Test Case execution"
            };
            string userName = "Test User";
            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisions(It.IsAny<SpotExceptionsOutOfSpecDecisionsPostsRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }
    }
}
