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
        private Mock<IDateTimeEngine> _DateTimeEngineMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionRepositoryMock = new Mock<ISpotExceptionRepository>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionRepository>())
                .Returns(_SpotExceptionRepositoryMock.Object);

            _SpotExceptionService = new SpotExceptionService(_DataRepositoryFactoryMock.Object, _FeatureToggleMock.Object, _ConfigurationSettingsHelperMock.Object, _DateTimeEngineMock.Object);
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
                        AdvertiserName="Beachbody",
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
                        AdvertiserName=null,
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
                        AdvertiserName=null,
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
            var spotExceptionsRecommendedPlanId = 1;
            SpotExceptionsRecommendedPlansDto spotExceptionsRecommendedPlans = null;

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
                .Returns(spotExceptionsRecommendedPlans);

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanDetails_RecommendedPlanDetails_Exist()
        {
            // Arrange
            var spotExceptionsRecommendedPlanId = 1;

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
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanDetails_ThrowsException()
        {
            // Arrange
            var spotExceptionsRecommendedPlanId = 1;

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlanById(It.IsAny<int>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanId));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void SaveSpotExceptionsRecommendedPlan_SaveSpotExceptionsRecommendedPlanDecision()
        {
            // Arrange
            var spotExceptionsRecommendedPlanSaveRequest = new SpotExceptionsRecommendedPlanSaveRequestDto
            {
                Id = 1,
                SelectedPlanId = 101
            };
            var userName = "Test User";
            var currentDateTime = new DateTime(2020, 10, 30, 12, 15, 23);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            bool isSpotExceptionsRecommendedPlanDecisionSaved = false;
            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsRecommendedPlanDecision(It.IsAny<SpotExceptionsRecommendedPlanDecisionDto>()))
                .Callback(() => isSpotExceptionsRecommendedPlanDecisionSaved = true)
                .Returns(isSpotExceptionsRecommendedPlanDecisionSaved);

            var expectedResult = true;

            // Act
            var result = _SpotExceptionService.SaveSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, isSpotExceptionsRecommendedPlanDecisionSaved);
        }

        [Test]
        public void SaveSpotExceptionsRecommendedPlan_UpdateRecommendedPlanOfSpotExceptionsRecommendedPlan()
        {
            // Arrange
            var spotExceptionsRecommendedPlanSaveRequest = new SpotExceptionsRecommendedPlanSaveRequestDto
            {
                Id = 1,
                SelectedPlanId = 101
            };
            var userName = "Test User";
            var currentDateTime = new DateTime(2020, 10, 30, 12, 15, 23);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsRecommendedPlanDecision(It.IsAny<SpotExceptionsRecommendedPlanDecisionDto>()))
                .Returns(true);

            bool isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated = false;
            _SpotExceptionRepositoryMock
                .Setup(s => s.UpdateRecommendedPlanOfSpotExceptionsRecommendedPlan(It.IsAny<SpotExceptionsRecommendedPlanDetailsDto>()))
                .Callback(() => isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated = true)
                .Returns(isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated);

            var expectedResult = true;

            // Act
            var result = _SpotExceptionService.SaveSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated);
        }

        [Test]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void SaveSpotExceptionsRecommendedPlan(bool isSpotExceptionsRecommendedPlanDecisionSaved, bool isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated, bool expectedResult)
        {
            // Arrange
            var spotExceptionsRecommendedPlanSaveRequest = new SpotExceptionsRecommendedPlanSaveRequestDto
            {
                Id = 1,
                SelectedPlanId = 101
            };
            var userName = "Test User";
            var currentDateTime = new DateTime(2020, 10, 30, 12, 15, 23);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsRecommendedPlanDecision(It.IsAny<SpotExceptionsRecommendedPlanDecisionDto>()))
                .Returns(isSpotExceptionsRecommendedPlanDecisionSaved);

            _SpotExceptionRepositoryMock
                .Setup(s => s.UpdateRecommendedPlanOfSpotExceptionsRecommendedPlan(It.IsAny<SpotExceptionsRecommendedPlanDetailsDto>()))
                .Returns(isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated);

            // Act
            var result = _SpotExceptionService.SaveSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanSaveRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveSpotExceptionsRecommendedPlan_ThrowsException()
        {
            // Arrange
            var spotExceptionsRecommendedPlanSaveRequest = new SpotExceptionsRecommendedPlanSaveRequestDto
            {
                Id = 1,
                SelectedPlanId = 101
            };
            var userName = "Test User";
            var currentDateTime = new DateTime(2020, 10, 30, 12, 15, 23);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsRecommendedPlanDecision(It.IsAny<SpotExceptionsRecommendedPlanDecisionDto>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.SaveSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanSaveRequest, userName));

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
        [Test]
        public void GetSpotExceptionOutofSpecsDetails_DoesNotExist()
        {
            // Arrange
            int spotExceptionsOutOfSpecId = 1;

            SpotExceptionsOutOfSpecsDto spotExceptionsOutOfSpecs = null;
            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecById(It.IsAny<int>()))
                .Returns(spotExceptionsOutOfSpecs);

            // Act
            var result = _SpotExceptionService.GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId);

            // Assert
            Assert.IsNull(result);
        }
        [Test]
        public void GetSpotExceptionOutofSpecsDetails_Exist()
        {
            // Arrange
            int spotExceptionsOutOfSpecId = 1;

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecById(It.IsAny<int>()))
                .Returns(new SpotExceptionsOutOfSpecsDto
                {
                    Id = 1,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 11728,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2020, 2, 1),
                    ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramFlightStartDate = new DateTime(2019, 12, 1),
                    ProgramFlightEndDate = new DateTime(2020, 2, 1),
                    ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    ProgramAirTime = new DateTime(2020, 1, 10, 23, 45, 00),
                    IngestedAt = new DateTime(2019, 1, 1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
                    {
                        SpotExceptionsOutOfSpecId = 1,
                        AcceptedAsInSpec = true,
                        DecisionNotes = "TestDecisionNotes",
                        UserName = "TestUser",
                        CreatedAt = new DateTime(2020, 2, 1)
                    },
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    }
                });
            // Act
            var result = _SpotExceptionService.GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));

        }
        [Test]
        public void GetSpotExceptionOutofSpecsDetails_ThrowsException()
        {
            // Arrange
            int spotExceptionsOutOfSpecId = 1;

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecById(It.IsAny<int>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId));

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
                    ReasonCodeMessage="",
                    EstimateId =191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 11728,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserName="MyBite",
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2020, 2, 1),
                    ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramFlightStartDate=new DateTime(2019, 12, 1),
                    ProgramFlightEndDate=new DateTime(2020, 2, 1),
                    ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
                   {
                       SpotExceptionsOutOfSpecId=1,
                       AcceptedAsInSpec=true,
                       DecisionNotes="TestDecisionNotes",
                       UserName = "TestUser",
                       CreatedAt = new DateTime(2020, 2, 1)
                   },
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 2,
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 11726,
                  RecommendedPlanName="4Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="Reynolds Foil @9",
                  AdvertiserName=null,
                  StationLegacyCallLetters="KSTP",
                  Affiliate = "NBC",
                  Market = "Phoenix (Prescott)",
                   SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70642,
                  ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                   ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
                   {
                       SpotExceptionsOutOfSpecId=2,
                       AcceptedAsInSpec=true,
                       DecisionNotes="",
                       UserName = "MockData",
                       CreatedAt = new DateTime(2020, 2, 1)
                   },
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 3,
                        ReasonCode = 2,
                        Reason = "genre content restriction",
                        Label = "Genre"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 3,
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 11725,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserName="MyBite",
                  StationLegacyCallLetters = "KSTP",
                  Affiliate = "ABC",
                  Market = "Lincoln & Hastings-Krny",
                   SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70643,
                  ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                   ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
                   {
                       SpotExceptionsOutOfSpecId=3,
                       AcceptedAsInSpec=true,
                       DecisionNotes="",
                       UserName = "MockData",
                       CreatedAt = new DateTime(2020, 2, 1)
                   },
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    }
                },
            };
        }

        private List<SpotExceptionsOutOfSpecsDto> _GetOutOfSpecPlansData()
        {
            return new List<SpotExceptionsOutOfSpecsDto>()
            {
                new SpotExceptionsOutOfSpecsDto
                {
                    Id = 1,
                    ReasonCodeMessage="",
                    EstimateId =191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserName="MyBite",
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
                    ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramFlightStartDate=new DateTime(2019, 12, 1),
                    ProgramFlightEndDate=new DateTime(2019, 12, 9),
                    ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsOutOfSpecDecision=null,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 2,
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 215,
                  RecommendedPlanName="4Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="Reynolds Foil @9",
                  AdvertiserName=null,
                  StationLegacyCallLetters="KSTP",
                  Affiliate = "NBC",
                  Market = "Phoenix (Prescott)",
                  PlanId = 215,
                   SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70642,
                  ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                   ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  SpotExceptionsOutOfSpecDecision =null,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 3,
                        ReasonCode = 2,
                        Reason = "genre content restriction",
                        Label = "Genre"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 3,
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 218,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserName="MyBite",
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
                  DaypartId= 70643,
                  ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                   ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  SpotExceptionsOutOfSpecDecision =  null,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 4,
                  ReasonCodeMessage="",
                  EstimateId= 191759,
                  IsciName="AB44NR59",
                  RecommendedPlanId= 11726,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserName="MyBite",
                  StationLegacyCallLetters = "KSTP",
                  Affiliate = "ABC",
                  Market = "Lincoln & Hastings-Krny",
                   SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70643,
                  ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                   ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  SpotExceptionsOutOfSpecDecision =  new SpotExceptionsOutOfSpecDecisionsDto
                   {
                       SpotExceptionsOutOfSpecId=4,
                       AcceptedAsInSpec=true,
                       DecisionNotes="",
                       UserName = "MockData",
                       CreatedAt = new DateTime(2020, 2, 1)
                   },
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    }
                },
            };
        }

        private List<SpotExceptionsOutOfSpecsDto> _GetOutOfSpecPlanSpotsData()
        {
            return new List<SpotExceptionsOutOfSpecsDto>()
            {
                new SpotExceptionsOutOfSpecsDto
                {
                    Id = 1,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserName = "MyBite",
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
                    ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramFlightStartDate = new DateTime(2019, 12, 1),
                    ProgramFlightEndDate = new DateTime(2019, 12, 9),
                    ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
                    {
                       SpotExceptionsOutOfSpecId = 1,
                       AcceptedAsInSpec = true,
                       DecisionNotes = "",
                       UserName = "MockData",
                       CreatedAt = new DateTime(2020, 2, 1),
                       SyncedBy = null,
                       SyncedAt = new DateTime(2020,1,10,23,45,00)
                    },
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                    Id = 2,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserName = "MyBite",
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
                    ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramFlightStartDate = new DateTime(2019, 12, 1),
                    ProgramFlightEndDate = new DateTime(2019, 12, 9),
                    ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
                    {
                       SpotExceptionsOutOfSpecId = 2,
                       AcceptedAsInSpec = true,
                       DecisionNotes = "",
                       UserName = "MockData",
                       CreatedAt = new DateTime(2020, 2, 1),
                       SyncedBy = null,
                       SyncedAt = null
                    },
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                    Id = 3,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserName = "MyBite",
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
                    ProgramDaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramFlightStartDate = new DateTime(2019, 12, 1),
                    ProgramFlightEndDate = new DateTime(2019, 12, 9),
                    ProgramAudience = new AudienceDto
                    {
                        Id = 427,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    SpotExceptionsOutOfSpecDecision = null,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    }
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

        [Test]
        public void GetSpotExceptionsRecommendedPlansAdvertisers_Advertisers_Exist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansAdvertisersRequest = new SpotExceptionsRecommendedPlansAdvertisersRequestDto
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
                        AdvertiserName="Beachbody",
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
                        AdvertiserName=null,
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
                        AdvertiserName=null,
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
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlansAdvertisers(spotExceptionsRecommendedPlansAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlansAdvertisers_Advertisers_DoesNotExist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansAdvertisersRequest = new SpotExceptionsRecommendedPlansAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsRecommendedPlansDto>());

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlansAdvertisers(spotExceptionsRecommendedPlansAdvertisersRequest);
            // Assert
            Assert.AreEqual(0, result.Count);
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetSpotExceptionsOutofSpecAdvertisers_Advertisers_Exist()
        {
            // Arrange
            SpotExceptionsOutofSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest = new SpotExceptionsOutofSpecAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)

            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetOutOfSpecData());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecAdvertisers(spotExceptionsOutofSpecAdvertisersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutofSpecAdvertisers_Advertisers_DoesNotExist()
        {
            // Arrange
            var spotExceptionsOutofSpecAdvertisersRequest = new SpotExceptionsOutofSpecAdvertisersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsOutOfSpecsDto>());

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecAdvertisers(spotExceptionsOutofSpecAdvertisersRequest);
            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecReasonCodes_ReasonCodes_DoNotExist()
        {
            // Arrange
            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodes())
                .Returns(new List<SpotExceptionsOutOfSpecReasonCodeDto>());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecReasonCodes();

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecReasonCodes_ReasonCodes_Exist()
        {
            // Arrange
            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodes())
                .Returns(new List<SpotExceptionsOutOfSpecReasonCodeDto>
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
                });

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecReasonCodes();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecReasonCodes_ThrowsException()
        {
            // Arrange
            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecReasonCodes())
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsOutOfSpecReasonCodes());

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlansStations_Stations_DoNotExist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansStationRequest = new SpotExceptionsRecommendedPlansStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsRecommendedPlansDto>());

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlansStations(spotExceptionsRecommendedPlansStationRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlansStations_Stations_Exist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansStationRequest = new SpotExceptionsRecommendedPlansStationRequestDto
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
                        StationLegacyCallLetters = "WDAY"
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 3,
                        StationLegacyCallLetters = "KSTP"
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 4,
                        StationLegacyCallLetters = null
                    }
                });

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlansStations(spotExceptionsRecommendedPlansStationRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutofSpecsStations_Stations_DoNotExist()
        {
            // Arrange
            var spotExceptionsOutofSpecsStationRequest = new SpotExceptionsOutofSpecsStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsOutOfSpecsDto>());

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecsStations(spotExceptionsOutofSpecsStationRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsOutofSpecsStations_Stations_Exist()
        {
            // Arrange
            var spotExceptionsOutofSpecsStationRequest = new SpotExceptionsOutofSpecsStationRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsOutOfSpecsDto>()
                {
                    new SpotExceptionsOutOfSpecsDto
                    {
                        Id = 1,
                        StationLegacyCallLetters = "WDAY"
                    },
                    new SpotExceptionsOutOfSpecsDto
                    {
                        Id = 2,
                        StationLegacyCallLetters = "KSTP"
                    },
                    new SpotExceptionsOutOfSpecsDto
                    {
                        Id = 3,
                        StationLegacyCallLetters = null
                    }
                });

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecsStations(spotExceptionsOutofSpecsStationRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecPlansExist()
        {
            // Arrange
            SpotExceptionsOutofSpecsPlansRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutofSpecsPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetOutOfSpecPlansData());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecsPlans(spotExceptionsOutOfSpecPostsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 2);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecPlans_DoesNotExist()
        {
            // Arrange
            List<SpotExceptionsOutOfSpecsDto> outofSpecData = null;
            SpotExceptionsOutofSpecsPlansRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutofSpecsPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outofSpecData);

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecsPlans(spotExceptionsOutOfSpecPostsRequest);

            // Assert            
            Assert.AreEqual(result.Active.Count, 0);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlans_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutofSpecsPlansRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutofSpecsPlansRequestDto
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
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsOutofSpecsPlans(spotExceptionsOutOfSpecPostsRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlans_OutOfSpecCompletedPlansExist()
        {
            // Arrange
            SpotExceptionsOutofSpecsPlansRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutofSpecsPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetOutOfSpecPlansData());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecsPlans(spotExceptionsOutOfSpecPostsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Completed.Count, 1);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlanSpots_OutOfSpecPlanSpots_Exist()
        {
            // Arrange
            SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest = new SpotExceptionsOutofSpecSpotsRequestDto
            {
                PlanId = 215,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetOutOfSpecPlanSpotsData());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecSpots(spotExceptionsOutofSpecSpotsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Active.Count, 1);
            Assert.AreEqual(result.Queued.Count, 1);
            Assert.AreEqual(result.Synced.Count, 1);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlanSpots_OutOfSpecPlanSpots_DoesNotExist()
        {
            // Arrange
            List<SpotExceptionsOutOfSpecsDto> outofSpecData = null;
            SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest = new SpotExceptionsOutofSpecSpotsRequestDto
            {
                PlanId = 215,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outofSpecData);

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsOutofSpecSpots(spotExceptionsOutofSpecSpotsRequest);

            // Assert            
            Assert.AreEqual(result.Active.Count, 0);
            Assert.AreEqual(result.Queued.Count, 0);
            Assert.AreEqual(result.Synced.Count, 0);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPlanSpots_ThrowsException()
        {
            // Arrange
            SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest = new SpotExceptionsOutofSpecSpotsRequestDto
            {
                PlanId = 215,
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
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsOutofSpecSpots(spotExceptionsOutofSpecSpotsRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }
    }
}