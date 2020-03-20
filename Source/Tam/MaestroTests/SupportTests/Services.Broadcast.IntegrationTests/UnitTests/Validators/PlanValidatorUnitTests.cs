using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    [TestFixture]
    [Category("short_running")]
    public class PlanValidatorUnitTest
    {
        private PlanValidatorUnitTestClass _planValidator;
        private Mock<IRatingForecastService> _ratingForecastServiceMock;
        private Mock<ISpotLengthEngine> _spotLengthEngineMock;
        private Mock<IBroadcastAudiencesCache> _broadcastAudiencesCacheMock;
        private Mock<IDataRepositoryFactory> _broadcastDataRepositoryFactoryMock;
        private Mock<IPlanRepository> _planRepositoryMock;

        private const int HUT_BOOK_ID = 55;
        private const int SHARE_BOOK_ID = 79;

        [SetUp]
        public void Init()
        {
            _spotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _ratingForecastServiceMock = new Mock<IRatingForecastService>();
            _broadcastAudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _ratingForecastServiceMock.Setup(r => r.GetMediaMonthCrunchStatuses()).Returns(
                new List<Entities.MediaMonthCrunchStatus>
                {
                    new Entities.MediaMonthCrunchStatus(
                        new Entities.RatingsForecastStatus
                        {
                            UniverseMarkets = 10,
                            MediaMonth = new Tam.Maestro.Data.Entities.MediaMonth
                                {Id = HUT_BOOK_ID, StartDate = new DateTime(2019, 8, 11)},
                            UsageMarkets = 10, ViewerMarkets = 10
                        }, 10),
                    new Entities.MediaMonthCrunchStatus(
                        new Entities.RatingsForecastStatus
                        {
                            UniverseMarkets = 10,
                            MediaMonth = new Tam.Maestro.Data.Entities.MediaMonth
                                {Id = SHARE_BOOK_ID, StartDate = new DateTime(2019, 8, 13)},
                            UsageMarkets = 10, ViewerMarkets = 10
                        }, 10),
                    new Entities.MediaMonthCrunchStatus(
                        new Entities.RatingsForecastStatus
                        {
                            UniverseMarkets = 10,
                            MediaMonth = new Tam.Maestro.Data.Entities.MediaMonth
                                {Id = 21, StartDate = new DateTime(2019, 8, 19)},
                            UsageMarkets = 10, ViewerMarkets = 10
                        }, 10),
                });
            _planRepositoryMock = new Mock<IPlanRepository>();
            _broadcastDataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _broadcastDataRepositoryFactoryMock.Setup(f => f.GetDataRepository<IPlanRepository>()).Returns(_planRepositoryMock.Object);
            _planValidator = new PlanValidatorUnitTestClass(_spotLengthEngineMock.Object, _broadcastAudiencesCacheMock.Object,
                _ratingForecastServiceMock.Object, new TrafficApiCache(new TrafficApiClientStub()), _broadcastDataRepositoryFactoryMock.Object);
        }

        [Test]
        public void ValidatePlan_EmptyName()
        {
            var plan = _GetPlan();
            plan.Name = string.Empty;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid plan name"));
        }

        [Test]
        public void ValidatePlan_CannotSaveDraftOnEmptyPlan()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.VersionId = 0;
            plan.IsDraft = true;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot create a new draft on a non existing plan"));
        }

        [Test]
        public void ValidatePlan_NameLargerThan255()
        {
            var plan = _GetPlan();
            plan.Name =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque blandit ex sed purus auctor rhoncus. Mauris lobortis nisi eget sollicitudin ultrices. Duis mollis blandit nisi id ultrices. Suspendisse molestie urna in enim egestas vehicula. Nulla facilisi.";

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid plan name"));
        }

        [Test]
        public void ValidatePlan_SpotLengthIdExistsReturnsFalse()
        {
            _spotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(false);

            var plan = _GetPlan();

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid spot length"));
        }

        [Test]
        public void ValidatePlan_InvalidProductId()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.ProductId = 666;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid product"));
        }

        [Test]
        public void ValidatePlan_FlightStartBiggerThanFlightEnd()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.FlightStartDate = new DateTime(2019, 8, 1);
            plan.FlightEndDate = new DateTime(2019, 7, 1);

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid flight dates.  The end date cannot be before the start date."));
        }

        [Test]
        public void ValidatePlan_WithEmptyFlightStartDate()
        {
            _ConfigureMocksToReturnTrue();
            var plan = _GetPlan();
            plan.FlightStartDate = null;

            Assert.That(() => _planValidator.ValidatePlan(plan)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
        }

        [Test]
        public void ValidatePlan_WithEmptyFlightEndDate()
        {
            _ConfigureMocksToReturnTrue();
            var plan = _GetPlan();
            plan.FlightEndDate = null;

            Assert.That(() => _planValidator.ValidatePlan(plan)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
        }

        [Test]
        public void ValidatePlan_InvalidFligthHiatusDay()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.FlightStartDate = new DateTime(2019, 8, 1);
            plan.FlightEndDate = new DateTime(2019, 9, 1);
            plan.FlightHiatusDays = new List<DateTime>
            {
                new DateTime(2018, 7, 1)
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid flight hiatus day.  All days must be within the flight date range."));
        }
        
        [Test]
        public void ValidatePlan_InvalidFligthCannotStartOnNonFlightDay()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.FlightStartDate = new DateTime(2020, 2, 1);
            plan.FlightEndDate = new DateTime(2020, 2, 29);
            plan.FlightDays = new List<int> { 1, 2, 3, 4, 5, 7 };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid flight dates. The flight cannot start or end, with non-flight days"));
        }

        [Test]
        public void ValidatePlan_InvalidFligthDaysThereShouldBeAtLeastOne()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.FlightStartDate = new DateTime(2020, 2, 1);
            plan.FlightEndDate = new DateTime(2020, 2, 29);
            plan.FlightDays = new List<int>();

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid flight days. The plan should have at least one flight day"));
        }

        [Test]
        public void ValidatePlan_InvalidFligthHiatusDaysCannotIntersectWithNonFlightDays()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.FlightStartDate = new DateTime(2020, 2, 1);
            plan.FlightEndDate = new DateTime(2020, 2, 29);
            plan.FlightHiatusDays = new List<DateTime>
            {
                new DateTime(2020, 2, 12)
            };
            plan.FlightDays = new List<int> { 1, 2, 4, 5, 7 };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid flight hiatus day. Hiatus day cannot be a non-flight day."));
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-25)]
        // have to use int? because the attribute requires a constant.
        public void ValidatePlan_WithInvalidBudget(int? candidate)
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Budget = candidate;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid budget."));
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-25)]
        // have to use int? because the attribute requires a constant.
        public void ValidatePlan_WithInvalidCpm(int? candidate)
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.TargetCPM = candidate;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid CPM."));
        }

        [Test]
        public void ValidatePlan_WithLowButValidGoalValues()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Budget = 0.0000001m;
            plan.TargetCPP = 0.0000001m;
            plan.TargetCPM = 0.0000001m;
            plan.TargetImpressions = 0.0000001;

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-25)]
        // have to use int? because the attribute requires a constant.
        public void ValidatePlan_WithInvalidCpp(int? candidate)
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.TargetCPP = candidate;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid CPP."));
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-25)]
        // have to use int? because the attribute requires a constant.
        public void ValidatePlan_WithInvalidDeliveryImpressions(int? candidate)
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.TargetImpressions = candidate;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid Delivery Impressions."));
        }

        [Test]
        public void ValidatePlan_WithoutDayparts()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts = null;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("There should be at least one daypart selected."));
        }

        [Test]
        public void ValidatePlan_WithWrongShowTypeRestrictionsContainType()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts[0].Restrictions.ShowTypeRestrictions.ContainType = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Contain type of the show types restrictions is not valid"));
        }

        [Test]
        public void ValidatePlan_WithWrongGenreRestrictionsContainType()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts[0].Restrictions.GenreRestrictions.ContainType = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Contain type of the genres restrictions is not valid"));
        }

        [Test]
        public void ValidatePlan_WithWrongProgramRestrictionsContainType()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts[0].Restrictions.ProgramRestrictions.ContainType = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Contain type of the program restrictions is not valid"));
        }

        [Test]
        public void ValidatePlan_WithWrongAffiliateRestrictionsContainType()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts[0].Restrictions.AffiliateRestrictions.ContainType = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Contain type of the affiliate restrictions is not valid"));
        }

        [Test]
        public void ValidatePlan_DuplicateDaypart()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartCodeId = 1
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 1
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid dayparts.  Each daypart can be entered only once."));
        }

        [Test]
        public void ValidatePlan_DayPartStartLessThanSecondsMinimum()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    StartTimeSeconds = -1
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
        }

        [Test]
        public void ValidatePlan_DayPartStartLargerThanSecondsMaximum()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    StartTimeSeconds = 86420
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
        }

        [Test]
        public void ValidatePlan_DayPartEndLessThanSecondsMinimum()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    StartTimeSeconds = 50,
                    EndTimeSeconds = -1
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
        }

        [Test]
        public void ValidatePlan_DayPartEndLargerThanSecondsMaximum()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    StartTimeSeconds = 50,
                    EndTimeSeconds = 86420
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
        }

        [Test]
        public void ValidatePlan_WeightingGoalPercentLessThanMinimum()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();
            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    StartTimeSeconds = 50,
                    EndTimeSeconds = 70,
                    WeightingGoalPercent = -1
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart weighting goal."));
        }

        [Test]
        public void ValidatePlan_WeightingGoalLargerThanMaximum()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();
            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    StartTimeSeconds = 50,
                    EndTimeSeconds = 70,
                    WeightingGoalPercent = 110
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart weighting goal."));
        }

        [Test]
        public void ValidatePlan_SumofWeightingGoalPercentsExceeds100()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();
            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartCodeId = 2,
                    StartTimeSeconds = 50000,
                    EndTimeSeconds = 54000,
                    WeightingGoalPercent = 80,
                    Restrictions = new PlanDaypartDto.RestrictionsDto
                    {
                        ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            ShowTypes = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        },
                        GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            Genres = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        }
                    }
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 1,
                    StartTimeSeconds = 50000,
                    EndTimeSeconds = 54000,
                    WeightingGoalPercent = 90,
                    Restrictions = new PlanDaypartDto.RestrictionsDto
                    {
                        ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            ShowTypes = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        },
                        GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            Genres = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        }
                    }
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Sum of weighting is greater than 100%"));
        }

        [Test]
        public void ValidatePlan_SumofWeightingGoalPercentsIsInBounds()
        {
            _ConfigureMocksToReturnTrue();
            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartCodeId = 1,
                    StartTimeSeconds = 50000,
                    EndTimeSeconds = 54000,
                    WeightingGoalPercent = 20,
                    Restrictions = new PlanDaypartDto.RestrictionsDto
                    {
                        ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            ShowTypes = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        },
                        GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            Genres = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        }
                    }
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 2,
                    StartTimeSeconds = 50000,
                    EndTimeSeconds = 54000,
                    Restrictions = new PlanDaypartDto.RestrictionsDto
                    {
                        ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            ShowTypes = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        },
                        GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            Genres = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        }
                    }
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 3,
                    StartTimeSeconds = 50000,
                    EndTimeSeconds = 54000,
                    WeightingGoalPercent = 25,
                    Restrictions = new PlanDaypartDto.RestrictionsDto
                    {
                        ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            ShowTypes = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        },
                        GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            Genres = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        }
                    }
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 4,
                    StartTimeSeconds = 50000,
                    EndTimeSeconds = 54000,
                    WeightingGoalPercent = 50,
                    Restrictions = new PlanDaypartDto.RestrictionsDto
                    {
                        ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            ShowTypes = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        },
                        GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            Genres = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                        }
                    }
                },
            };

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        public void ValidatePlan_IsValidAudienceTrue()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.AudienceId = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience"));
        }

        [Test]
        public void ValidatePlan_InvalidShareBookId()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.ShareBookId = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share book"));
        }

        [Test]
        public void ValidatePlan_HUTBookIdNegative()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.HUTBookId = -1;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid HUT book."));
        }

        [Test]
        public void ValidatePlan_HUTBookIdDoesntExistInPostingBooks()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.HUTBookId = 51;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid HUT book."));
        }

        [Test]
        public void ValidatePlan_HUTStartDateBiggerThanShareStartDate()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.HUTBookId = 21;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("HUT Book must be prior to Share Book"));
        }

        [Test]
        public void ValidatePlan_PrimaryAudienceVPVHWithDefault()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Vpvh = default;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
        }

        [Test]
        public void ValidatePlan_PrimaryAudienceVPVHLessThanMinimum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Vpvh = 0.0001;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
        }

        [Test]
        public void ValidatePlan_PrimaryAudienceVPVHBiggerThanMaximum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Vpvh = 12;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
        }

        [Test]
        public void ValidatePlan_InvalidSecondaryAudience()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();
            _broadcastAudiencesCacheMock.Setup(b => b.IsValidAudience(0)).Returns(true);
            _broadcastAudiencesCacheMock.Setup(b => b.IsValidAudience(21)).Returns(false);

            var plan = _GetPlan();
            plan.SecondaryAudiences = new List<PlanAudienceDto>
            {
                new PlanAudienceDto
                {
                    AudienceId = 11,
                    Vpvh = 0.35
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience"));
        }

        [Test]
        public void ValidatePlan_DuplicateSecondaryAudience()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.SecondaryAudiences = new List<PlanAudienceDto>
            {
                new PlanAudienceDto
                {
                    AudienceId = 11,
                    Vpvh = 0.18
                },
                new PlanAudienceDto
                {
                    AudienceId = 11,
                    Vpvh = 0.18
                }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("An audience cannot appear multiple times"));
        }

        [Test]
        public void ValidatePlan_SecondaryAudienceVPVHLessThanMinimum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.SecondaryAudiences = new List<PlanAudienceDto>
            {
                new PlanAudienceDto
                {
                    AudienceId = 11,
                    Vpvh = -1
                },
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
        }

        [Test]
        public void ValidatePlan_SecondaryAudienceVPVHBiggerThanMaximum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.SecondaryAudiences = new List<PlanAudienceDto>
            {
                new PlanAudienceDto
                {
                    AudienceId = 11, Vpvh = 10.5
                },
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-25)]
        [TestCase(230)]
        [TestCase(101)]
        // have to use int? because the attribute requires a constant.
        public void ValidatePlan_WithInvalidCoverageGoalPercentage(int? candidate)
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.CoverageGoalPercent = candidate;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
        }

        [Test]
        public void ValidatePlan_WithInvalidMarketCoverage()
        {
            _ConfigureMocksToReturnTrue();
            var plan = _GetPlan();
            plan.CoverageGoalPercent = 80.5;
            plan.AvailableMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto {PercentageOfUS = 50,}, new PlanAvailableMarketDto {PercentageOfUS = 20.5}
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid total market coverage."));
        }

        [Test]
        [TestCase(80.5)]
        [TestCase(70.5)]
        public void ValidatePlan_WithValidMarketCoverage(double candidate)
        {
            _ConfigureMocksToReturnTrue();
            var plan = _GetPlan();
            plan.CoverageGoalPercent = candidate;
            plan.AvailableMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto {PercentageOfUS = 50,}, new PlanAvailableMarketDto {PercentageOfUS = 30.5}
            };

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        public void ValidatePlan_WithValidMarketCoverage_DecimalSumValue()
        {
            _ConfigureMocksToReturnTrue();
            var plan = _GetPlan();
            plan.CoverageGoalPercent = 100;
            plan.AvailableMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto { PercentageOfUS = 99.999999999999943 }
            };

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        public void ValidatePlan_ShareOfVoicePercentLessThanMinimum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.AvailableMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto {ShareOfVoicePercent = 0}
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
        }

        [Test]
        public void ValidatePlan_ShareOfVoicePercentBiggerThanMaximum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.AvailableMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto {ShareOfVoicePercent = 150}
            };

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
        }

        [Test]
        public void ValidatePlan_SumOfWeeklyBreakdownWeeksDifferentFromDeliveryImpressions()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.TargetImpressions = 100;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek {WeeklyImpressions = 20},
                new WeeklyBreakdownWeek {WeeklyImpressions = 30}
            };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("The impressions count is different between the delivery and the weekly breakdown"));
        }

        [Test]
        public void ValidatePlan_WeeklyBreakdown_RoundImpressions()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.TargetImpressions = 100;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek {WeeklyImpressions = 50, WeeklyImpressionsPercentage=50, WeeklyBudget = 50},
                new WeeklyBreakdownWeek {WeeklyImpressions = 50.05, WeeklyImpressionsPercentage = 50, WeeklyBudget = 50}
            };

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        public void ValidatePlan_WeeklyBreakdown_RoundImpressions2()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.TargetImpressions = 90.123;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek {WeeklyImpressions = 90, WeeklyImpressionsPercentage=50, WeeklyBudget = 50},
                new WeeklyBreakdownWeek {WeeklyImpressions = 0.1229999, WeeklyImpressionsPercentage = 50, WeeklyBudget = 50}
            };

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        public void ValidatePlan_FlightNotesLongerThan1024()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.FlightNotes = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque porttitor tellus at ante tempus vehicula ac at sapien. Pellentesque lorem velit, sodales in ex quis, laoreet dictum risus. Quisque odio sapien, dignissim a lacus et, dignissim auctor urna. Vestibulum tempus dui tortor, nec fermentum massa pharetra sit amet. Morbi fermentum ornare scelerisque. Proin ut lectus in nisl vulputate mattis in in ex. Nam erat sem, convallis condimentum velit blandit, scelerisque condimentum dolor. Maecenas fermentum feugiat lectus. Phasellus et sem in velit hendrerit sodales. Suspendisse porta nec felis ac blandit. In eu nisi ut dui tristique mattis. Vivamus vulputate, elit sit amet porta molestie, justo mauris cursus ipsum, et rhoncus arcu odio id enim. Pellentesque elementum posuere nibh ac rutrum. Donec eget erat nec lorem feugiat ornare vel congue nibh. Nulla cursus bibendum sollicitudin. Quisque viverra ante massa, sed molestie augue rutrum sed. Aenean tempus vitae purus sed lobortis. Sed cursus tempor erat ac pulvinar.";

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Flight notes cannot be longer than 1024 characters."));
        }

        [Test]
        public void ValidateWeeklyBreakdown_RequestNull()
        {
            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(null),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid request"));
        }

        [Test]
        public void ValidateWeeklyBreakdown_EmptyFlightStartDate()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(2019, 8, 1),
                FlightStartDate = new DateTime()
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
        }

        [Test]
        public void ValidateWeeklyBreakdown_EmptyFlightEndDate()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(),
                FlightStartDate = new DateTime(2019, 8, 1)
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
        }

        [Test]
        public void ValidateWeeklyBreakdown_FlightStartBiggerThanFlightEnd()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(2019, 8, 1),
                FlightStartDate = new DateTime(2019, 9, 1)
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid flight dates.  The end date cannot be before the start date."));
        }

        [Test]
        public void ValidateWeeklyBreakdownWeeks_WithoutWeeklyBreakdown()
        {
            var plan = new PlanDto();

            _ValidateWeeklyBreakdownWeeks(plan);
        }

        [Test]
        [TestCase(12.121212121212123, 6.060606060606061, 6.060606060606061, false)]
        [TestCase(13.5, 6.060606060606061, 6.060606060606061, true)]
        public void ValidateWeeklyBreakdownWeeks_ImpressionTotal(double targetImpressions, double weeklyImpressionsOne, double weeklyImpressionsTwo, bool shouldThrow)
        {
            var plan = new PlanDto
            {
                TargetImpressions = targetImpressions,
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek { WeeklyImpressions = weeklyImpressionsOne, WeeklyImpressionsPercentage = 50 },
                    new WeeklyBreakdownWeek { WeeklyImpressions = weeklyImpressionsTwo, WeeklyImpressionsPercentage = 50 }
                }
            };

            _ValidateWeeklyBreakdownWeeks(plan, shouldThrow, PlanValidator.INVALID_IMPRESSIONS_COUNT);
        }

        private void _ValidateWeeklyBreakdownWeeks(PlanDto plan, bool shouldThrow = false, string errorMessageIfShouldThrow = null)
        {
            if (shouldThrow)
            {
                var e = Assert.Throws<Exception>(() => _planValidator.UT_ValidateWeeklyBreakdownWeeks(plan));
                Assert.AreEqual(errorMessageIfShouldThrow, e.Message);
            }
            else
            {
                Assert.DoesNotThrow(() => _planValidator.UT_ValidateWeeklyBreakdownWeeks(plan));
            }
        }

        private void _ConfigureSpotLenghtEngineMockToReturnTrue() =>
            _spotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(true);

        private void _ConfigureBroadcastAudiencesCacheMockToReturnTrue() =>
            _broadcastAudiencesCacheMock.Setup(b => b.IsValidAudience(It.IsAny<int>())).Returns(true);

        private void _ConfigureMocksToReturnTrue()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();
            _ConfigureBroadcastAudiencesCacheMockToReturnTrue();
        }

        private PlanDto _GetPlan() =>
            new PlanDto
            {
                Name = "Lorem ipsum",
                ProductId = 20,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        StartTimeSeconds = 50,
                        EndTimeSeconds = 70,
                        WeightingGoalPercent = 50,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                ShowTypes = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Genres = new List<LookupDto> { new LookupDto { Id = 1, Display = "Lorem" } }
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        ContentRating = "G",
                                        Genre = new LookupDto{ Id = 25},
                                        Name = "Pimp My Ride"
                                    }
                                }
                            },
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Affiliates = new List<LookupDto> { new LookupDto { Id = 1, Display = "NBC" } }
                            }
                        }
                    }
                },
                ShareBookId = SHARE_BOOK_ID,
                HUTBookId = HUT_BOOK_ID,
                Vpvh = 0.35,
                SpotLengthId = 1,
                Equivalized = true,
                Status = PlanStatusEnum.Scenario,
                ModifiedBy = "UnitTestUser",
                ModifiedDate = new DateTime(2019, 9, 16, 12, 31, 33),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 09, 01),
                FlightNotes = "Notes for this flight.",
                AudienceType = AudienceTypeEnum.Nielsen,
                PostingType = PostingTypeEnum.NSI,
                AudienceId = 31,
                Budget = 100.00m,
                TargetImpressions = 100,
                TargetCPM = 12.00m,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.Custom,
                TargetRatingPoints = 6,
                TargetCPP = 200951583.9999m,
                Currency = PlanCurrenciesEnum.Impressions,
                CoverageGoalPercent = 80.5,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { PercentageOfUS = 30 },
                    new PlanAvailableMarketDto { PercentageOfUS = 50.5 },
                }
            };
    }
}
