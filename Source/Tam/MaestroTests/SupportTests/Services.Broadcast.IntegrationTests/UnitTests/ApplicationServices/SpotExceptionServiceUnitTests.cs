using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
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
        private Mock<IAabEngine> _AabEngine;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _AabEngine = new Mock<IAabEngine>();
            _SpotExceptionRepositoryMock = new Mock<ISpotExceptionRepository>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionRepository>())
                .Returns(_SpotExceptionRepositoryMock.Object);

            _SpotExceptionService = new SpotExceptionService(_DataRepositoryFactoryMock.Object,
                _AabEngine.Object,
                _FeatureToggleMock.Object,
                _ConfigurationSettingsHelperMock.Object, _DateTimeEngineMock.Object);
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

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

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
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2020, 2, 1),
                    ProgramNetwork = "ABC",
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
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 34, Name = "News", ProgramSourceId = 1 }
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
                    AdvertiserMasterId = new Guid("D04797DB-870C-42D1-BC6C-CAF823D7A4D9"),
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
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDMwOA==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 14, Name = "Drama", ProgramSourceId = 1}
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
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
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
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDMwOA==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 9, Name = "Comedy", ProgramSourceId = 1}
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
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
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
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 14, Name = "Drama", ProgramSourceId = 1}
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
                    DaypartId= 70642,
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
                    SpotExceptionsOutOfSpecDecision=null,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
                  HouseIsci = "289J76GN16H",
                  ProgramGenre = new Entities.Genre { Id = 9, Name = "Comedy", ProgramSourceId = 1},
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
                  DaypartId= 70642,
                  ProgramNetwork = "",
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
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                  HouseIsci = "289J76GN16H",
                  ProgramGenre = new Entities.Genre { Id = 14, Name = "Drama", ProgramSourceId = 1},
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
                  DaypartId= 70643,
                  ProgramNetwork = "",
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
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
                  HouseIsci = "289J76GN16H",
                  ProgramGenre = new Entities.Genre { Id = 34, Name = "News", ProgramSourceId = 1}
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
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
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
                  DaypartId= 70642,
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
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxOTY3MA==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 14, Name = "Drama", ProgramSourceId = 1}
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
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDYzNg==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 14, Name = "Drama", ProgramSourceId = 1},
                    Comments = "test Comment"
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
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 34, Name = "News", ProgramSourceId = 1},
                    Comments = "test Comment"
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
                    SpotExceptionsOutOfSpecDecision = null,
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                    HouseIsci = "289J76GN16H",
                    ProgramGenre = new Entities.Genre { Id = 9, Name = "Comedy", ProgramSourceId = 1},
                    Comments = "test Comment"
                },
            };
        }

        private List<SpotExceptionUnpostedNoPlanDto> _GetSpotExceptionsUnpostedNoPlanData()
        {
            return new List<SpotExceptionUnpostedNoPlanDto>()
            {
                new SpotExceptionUnpostedNoPlanDto
                {
                    HouseIsci = "YB82TXT2H",
                    ClientIsci = "AB82VR589",
                    ClientSpotLengthId = 12,
                    Count = 1,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateID = 191757,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    CreatedBy = "Mock Data",
                    CreatedAt = new DateTime(2020,1,10,23,45,00),
                    ModifiedBy = "Mock Data",
                    ModifiedAt = new DateTime(2020,1,10,23,45,00)
                },
                new SpotExceptionUnpostedNoPlanDto
                {
                    HouseIsci = "YB82TXT2H",
                    ClientIsci = "AB82VR590",
                    ClientSpotLengthId = 13,
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateID = 191758,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    CreatedBy = "Mock Data",
                    CreatedAt = new DateTime(2020,1,10,23,45,00),
                    ModifiedBy = "Mock Data",
                    ModifiedAt = new DateTime(2020,1,10,23,45,00)
                }
            };
        }

        private List<SpotExceptionUnpostedNoReelRosterDto> _GetSpotExceptionsUnpostedNoReelRosterData()
        {
            return new List<SpotExceptionUnpostedNoReelRosterDto>()
            {
                new SpotExceptionUnpostedNoReelRosterDto
                {
                    HouseIsci = "YB82TXT2M",
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateId = 191759,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    CreatedBy = "Mock Data",
                    CreatedAt = new DateTime(2020,1,10,23,45,00),
                    ModifiedBy = "Mock Data",
                    ModifiedAt = new DateTime(2020,1,10,23,45,00)
                },
                new SpotExceptionUnpostedNoReelRosterDto
                {
                    HouseIsci = "YB82TXT2M",
                    Count = 2,
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    EstimateId = 191760,
                    IngestedBy = "Mock Data",
                    IngestedAt = new DateTime(2020,1,10,23,45,00),
                    CreatedBy = "Mock Data",
                    CreatedAt = new DateTime(2020,1,10,23,45,00),
                    ModifiedBy = "Mock Data",
                    ModifiedAt = new DateTime(2020,1,10,23,45,00)
                }
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

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

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

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

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

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

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
                .Setup(s => s.GetSpotExceptionsOutOfSpecPlanSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
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
                .Setup(s => s.GetSpotExceptionsOutOfSpecPlanSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsOutofSpecSpots(spotExceptionsOutofSpecSpotsRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecMarkets_Markets_DoNotExist()
        {
            // Arrange
            var spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutofSpecSpotsRequestDto
            {
                PlanId = 215,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecPosts(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsOutOfSpecsDto>());

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecMarkets(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecMarkets_Markets_Exist()
        {
            // Arrange
            var spotExceptionsOutOfSpecSpotsRequest = new SpotExceptionsOutofSpecSpotsRequestDto
            {
                PlanId = 215,
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsOutOfSpecPlanSpots(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetOutOfSpecPlanSpotsData());

            // Act           
            var result = _SpotExceptionService.GetSpotExceptionsOutOfSpecMarkets(spotExceptionsOutOfSpecSpotsRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsUnposted_Unposted_Exist()
        {
            // Arrange
            SpotExceptionOutOfSpecUnpostedRequestDto spotExceptionOutOfSpecUnpostedRequest = new SpotExceptionOutOfSpecUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlan(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetSpotExceptionsUnpostedNoPlanData());

            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRoster(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetSpotExceptionsUnpostedNoReelRosterData());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsUnposted(spotExceptionOutOfSpecUnpostedRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.NoPlan.Count, 2);
            Assert.AreEqual(result.NoReelRoster.Count, 2);
        }

        [Test]
        public void GetSpotExceptionsUnposted_Unposted_ThrowException()
        {
            // Arrange
            SpotExceptionOutOfSpecUnpostedRequestDto spotExceptionOutOfSpecUnpostedRequest = new SpotExceptionOutOfSpecUnpostedRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };
            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoPlan(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            _SpotExceptionRepositoryMock
                .Setup(x => x.GetSpotExceptionUnpostedNoReelRoster(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsUnposted(spotExceptionOutOfSpecUnpostedRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void SaveSpotExceptionsOutOfSpecsDecisionsPlans()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPlansResult = new List<SpotExceptionsOutOfSpecDecisionsPlansDto>
            {
                new SpotExceptionsOutOfSpecDecisionsPlansDto
                {
                    Id = 21,
                    AcceptAsInSpec = true
                },
                new SpotExceptionsOutOfSpecDecisionsPlansDto
                {
                    Id = 22,
                    AcceptAsInSpec = true
                }
            };

            var spotExceptionSaveDecisionsPlansRequest = new SpotExceptionSaveDecisionsPlansRequestDto();
            spotExceptionSaveDecisionsPlansRequest.Decisions.AddRange(spotExceptionsOutOfSpecDecisionsPlansResult);

            string userName = "Test User";
            bool result = false;
            bool expectedResult = true;
            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisionsPlans(It.IsAny<SpotExceptionSaveDecisionsPlansRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            // Act
            result = _SpotExceptionService.SaveOutofSpecDecisionsPlans(spotExceptionSaveDecisionsPlansRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveSpotExceptionsOutOfSpecsDecisionsPlans_ThrowsException()
        {
            // Arrange
            var spotExceptionsDecisionsPlansResult = new List<SpotExceptionsOutOfSpecDecisionsPlansDto>
            {
                new SpotExceptionsOutOfSpecDecisionsPlansDto
                {
                    Id = 21,
                    AcceptAsInSpec = true
                },
                new SpotExceptionsOutOfSpecDecisionsPlansDto
                {
                    Id = 22,
                    AcceptAsInSpec = true
                }
            };

            var spotExceptionOutOfSpecSaveDecisionsPlansRequest = new SpotExceptionSaveDecisionsPlansRequestDto();
            spotExceptionOutOfSpecSaveDecisionsPlansRequest.Decisions.AddRange(spotExceptionsDecisionsPlansResult);
            string userName = "Test User";
            _SpotExceptionRepositoryMock
                .Setup(s => s.SaveSpotExceptionsOutOfSpecsDecisionsPlans(It.IsAny<SpotExceptionSaveDecisionsPlansRequestDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.SaveOutofSpecDecisionsPlans(spotExceptionOutOfSpecSaveDecisionsPlansRequest, userName));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void TriggerDecisionSync_Exist()
        {
            // Arrange
            var spotExceptionsOutOfSpecDecisionsPlansResult = new List<SpotExceptionsOutOfSpecDecisionsPlansDto>
            {
                new SpotExceptionsOutOfSpecDecisionsPlansDto
                {
                    Id = 21,
                    AcceptAsInSpec = true
                },
                new SpotExceptionsOutOfSpecDecisionsPlansDto
                {
                    Id = 22,
                    AcceptAsInSpec = true
                }
            };

            var spotExceptionSaveDecisionsPlansRequest = new SpotExceptionSaveDecisionsPlansRequestDto();
            spotExceptionSaveDecisionsPlansRequest.Decisions.AddRange(spotExceptionsOutOfSpecDecisionsPlansResult);

            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto();
            triggerDecisionSyncRequest.UserName = "Test User";
            bool result = false;
            bool expectedResult = true;
            _SpotExceptionRepositoryMock
                .Setup(s => s.SyncOutOfSpecDecision(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            // Act
            result = _SpotExceptionService.TriggerDecisionSync(triggerDecisionSyncRequest);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TriggerDecisionSync_DoesNotExist()
        {
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto();
            triggerDecisionSyncRequest.UserName = "Test User";
            bool result = false;
            bool expectedResult = false;
            _SpotExceptionRepositoryMock
                .Setup(s => s.SyncOutOfSpecDecision(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            // Act
            result = _SpotExceptionService.TriggerDecisionSync(triggerDecisionSyncRequest);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TriggerDecisionSync_ThrowsException()
        {
            // Arrange
            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto();
            triggerDecisionSyncRequest.UserName = "Test User";
            _SpotExceptionRepositoryMock
                .Setup(s => s.SyncOutOfSpecDecision(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.TriggerDecisionSync(triggerDecisionSyncRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void GetDecisionCount_Exist()
        {
            // Arrange
            int outOfSpecDecisioncount = 2;
            int recommandedPlanDecisioncount = 3;

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetDecisionQueuedCount())
                .Returns(outOfSpecDecisioncount);

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetRecommandedPlanDecisionQueuedCount())
                .Returns(recommandedPlanDecisioncount);

            // Act
            var result = _SpotExceptionService.GetQueuedDecisionCount();

            // Assert
            Assert.AreEqual(result, 5);
        }

        [Test]
        public void GetDecisionCount_DoesNotExist()
        {
            // Arrange
            int outOfSpecDecisioncount = 0;
            int recommandedPlanDecisioncount = 0;

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetDecisionQueuedCount())
                .Returns(outOfSpecDecisioncount);

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetRecommandedPlanDecisionQueuedCount())
                .Returns(recommandedPlanDecisioncount);

            // Act
            var result = _SpotExceptionService.GetQueuedDecisionCount();

            // Assert
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void GetDecisionCount_ThrowsException()
        {
            // Arrange
            _SpotExceptionRepositoryMock
                .Setup(s => s.GetDecisionQueuedCount())
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetQueuedDecisionCount());

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void TriggerRecommandedPlanDecisionSync_Exist()
        {
            // Arrange

            var recommandedPlanDecision = new List<SpotExceptionsRecommendedPlanDecisionDto>()
            {
                new SpotExceptionsRecommendedPlanDecisionDto()
                {
                    Id = 1,
                    SpotExceptionsRecommendedPlanDetailId = 1,
                    UserName = "Test User",
                    CreatedAt = new DateTime(2020,1,10,23,45,00)
                },
                new SpotExceptionsRecommendedPlanDecisionDto()
                {
                    Id = 2,
                    SpotExceptionsRecommendedPlanDetailId = 2,
                    UserName = "Test User",
                    CreatedAt = new DateTime(2020,1,10,23,45,00)
                }
            };

            var triggerDecisionSyncRequest = new TriggerDecisionSyncRequestDto
            {
                UserName = "Test User"
            };
            bool result = false;
            bool expectedResult = true;
            _SpotExceptionRepositoryMock
                .Setup(s => s.SyncRecommandedPlanDecision(It.IsAny<TriggerDecisionSyncRequestDto>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            // Act
            result = _SpotExceptionService.TriggerDecisionSync(triggerDecisionSyncRequest);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}