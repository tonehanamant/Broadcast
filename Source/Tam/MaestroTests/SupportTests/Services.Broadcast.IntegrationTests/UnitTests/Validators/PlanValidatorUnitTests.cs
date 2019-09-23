using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    [TestFixture]
    public class PlanValidatorUnitTest
    {
        private PlanValidator _planValidator;
        private Mock<IRatingForecastService> _ratingForecastServiceMock;
        private Mock<ISpotLengthEngine> _spotLengthEngineMock;
        private Mock<IBroadcastAudiencesCache> _broadcastAudiencesCacheMock;
        private Mock<ITrafficApiClient> _traffiApiClientMock;

        private const int HUT_BOOK_ID = 55;
        private const int SHARE_BOOK_ID = 79;

        [SetUp]
        public void Init()
        {
            _spotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _ratingForecastServiceMock = new Mock<IRatingForecastService>();
            _broadcastAudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _traffiApiClientMock = new Mock<ITrafficApiClient>();
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
            _planValidator = new PlanValidator(_spotLengthEngineMock.Object, _broadcastAudiencesCacheMock.Object,
                _ratingForecastServiceMock.Object, _traffiApiClientMock.Object);
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
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.ProductId = 0;

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
            plan.CPM = candidate;

            Assert.That(() => _planValidator.ValidatePlan(plan),
                Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid CPM."));
        }

        [Test]
        public void ValidatePlan_WithLowButValidGoalValues()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Budget = 0.0000001m;
            plan.CPP = 0.0000001m;
            plan.CPM = 0.0000001m;
            plan.DeliveryImpressions = 0.0000001;

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
            plan.CPP = candidate;

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
            plan.DeliveryImpressions = candidate;

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
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 50000,
                    WeightingGoalPercent = 80
                },
                new PlanDaypartDto
                {
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 50000,
                    WeightingGoalPercent = 90
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
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 50000,
                    WeightingGoalPercent = 20
                },
                new PlanDaypartDto
                {
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 50000,
                },
                new PlanDaypartDto
                {
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 50000,
                    WeightingGoalPercent = 25
                },
                new PlanDaypartDto
                {
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 50000,
                    WeightingGoalPercent = 50
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
            plan.Vpvh = 2;

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
                    AudienceId = 11, Vpvh = 1.5
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
            plan.DeliveryImpressions = 100;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek {Impressions = 20},
                new WeeklyBreakdownWeek {Impressions = 30}
            };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("The impressions count is different between the delivery and the weekly breakdown"));
        }

        [Test]
        public void ValidatePlan_WeeklyBreakdown_RoundImpressions()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.DeliveryImpressions = 100;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek {Impressions = 50, ShareOfVoice=50},
                new WeeklyBreakdownWeek {Impressions = 49.999999, ShareOfVoice = 50}
            };

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        public void ValidatePlan_WeeklyBreakdown_RoundImpressions2()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.DeliveryImpressions = 90.123;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek {Impressions = 90, ShareOfVoice=50},
                new WeeklyBreakdownWeek {Impressions = 0.1229999, ShareOfVoice = 50}
            };

            Assert.DoesNotThrow(() => _planValidator.ValidatePlan(plan));
        }

        [Test]
        public void ValidatePlan_SumOfShareOfVoiceDifferentFrom100()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.DeliveryImpressions = 50;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek {Impressions = 20, ShareOfVoice = 20},
                new WeeklyBreakdownWeek {Impressions = 30, ShareOfVoice = 20}
            };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("The share of voice count is not equal to 100%"));
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
        public void ValidateWeeklyBreakdown_InvalidWeeksForcustomDelivery()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(2019, 8, 1),
                FlightStartDate = new DateTime(2019, 7, 1),
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.Custom
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("For custom delivery you have to provide the weeks values"));
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
                        WeightingGoalPercent = 50
                    }
                },
                ShareBookId = SHARE_BOOK_ID,
                HUTBookId = HUT_BOOK_ID,
                Vpvh = 0.35,
                SpotLengthId = 1,
                Equivalized = true,
                Status = PlanStatusEnum.Working,
                ModifiedBy = "UnitTestUser",
                ModifiedDate = new DateTime(2019, 9, 16, 12, 31, 33),
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 09, 01),
                FlightNotes = "Notes for this flight.",
                AudienceType = AudienceTypeEnum.Nielsen,
                PostingType = PostingTypeEnum.NSI,
                AudienceId = 31,
                Budget = 100.00m,
                DeliveryImpressions = 100,
                CPM = 12.00m,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.Custom,
                DeliveryRatingPoints = 6,
                CPP = 200951583.9999m,
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
