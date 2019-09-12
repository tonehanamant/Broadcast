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
            _ratingForecastServiceMock.Setup(r => r.GetMediaMonthCrunchStatuses()).Returns(new List<Entities.MediaMonthCrunchStatus> {
                new Entities.MediaMonthCrunchStatus(new Entities.RatingsForecastStatus{ UniverseMarkets = 10, MediaMonth = new Tam.Maestro.Data.Entities.MediaMonth {  Id = HUT_BOOK_ID, StartDate = new DateTime(2019,8,11) }, UsageMarkets = 10, ViewerMarkets = 10 }, 10),
                new Entities.MediaMonthCrunchStatus(new Entities.RatingsForecastStatus{ UniverseMarkets = 10, MediaMonth = new Tam.Maestro.Data.Entities.MediaMonth {  Id = SHARE_BOOK_ID, StartDate = new DateTime(2019,8,13) }, UsageMarkets = 10, ViewerMarkets = 10 }, 10),
                new Entities.MediaMonthCrunchStatus(new Entities.RatingsForecastStatus{ UniverseMarkets = 10, MediaMonth = new Tam.Maestro.Data.Entities.MediaMonth {  Id = 21, StartDate = new DateTime(2019,8,19) }, UsageMarkets = 10, ViewerMarkets = 10 }, 10),
            });
            _planValidator = new PlanValidator(_spotLengthEngineMock.Object, _broadcastAudiencesCacheMock.Object, _ratingForecastServiceMock.Object, _traffiApiClientMock.Object);
        }

        [Test]
        public void ValidatePlan_EmptyName()
        {
            var plan = _GetPlan();
            plan.Name = string.Empty;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid plan name"));
        }

        [Test]
        public void ValidatePlan_NameLargerThan255()
        {
            var plan = _GetPlan();
            plan.Name = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque blandit ex sed purus auctor rhoncus. Mauris lobortis nisi eget sollicitudin ultrices. Duis mollis blandit nisi id ultrices. Suspendisse molestie urna in enim egestas vehicula. Nulla facilisi.";

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid plan name"));
        }

        [Test]
        public void ValidatePlan_SpotLengthIdExistsReturnsFalse()
        {
            _spotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(false);

            var plan = _GetPlan();

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid spot length"));
        }

        [Test]
        public void ValidatePlan_InvalidProductId()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.ProductId = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid product"));
        }

        [Test]
        public void ValidatePlan_FlightStartBiggerThanFlightEnd()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.FlightStartDate = new DateTime(2019, 8, 1);
            plan.FlightEndDate = new DateTime(2019, 7, 1);

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight dates.  The end date cannot be before the start date."));
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
                new DateTime(2018,7,1)
            };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight hiatus day.  All days must be within the flight date range."));
        }

        [Test]
        public void ValidatePlan_WithoutDayparts()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.Dayparts = null;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("There should be at least one daypart selected."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart weighting goal."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart weighting goal."));
        }

        [Test]
        public void ValidatePlan_IsValidAudienceTrue()
        {
            _ConfigureSpotLenghtEngineMockToReturnTrue();

            var plan = _GetPlan();
            plan.AudienceId = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience"));
        }

        [Test]
        public void ValidatePlan_InvalidShareBookId()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.ShareBookId = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share book"));
        }

        [Test]
        public void ValidatePlan_HUTBookIdNegative()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.HUTBookId = -1;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid HUT book."));
        }

        [Test]
        public void ValidatePlan_HUTBookIdDoesntExistInPostingBooks()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.HUTBookId = 51;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid HUT book."));
        }

        [Test]
        public void ValidatePlan_HUTStartDateBiggerThanShareStartDate()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.HUTBookId = 21;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("HUT Book must be prior to Share Book"));
        }

        [Test]
        public void ValidatePlan_PrimaryAudienceVPVHLessThanMinimum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Vpvh = 0.0001;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
        }

        [Test]
        public void ValidatePlan_PrimaryAudienceVPVHBiggerThanMaximum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.Vpvh = 2;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience"));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("An audience cannot appear multiple times"));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
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

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid VPVH. The value must be between 0.001 and 1."));
        }

        [Test]
        public void ValidatePlan_CoverageGoalPercenteLessThanMinimum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.CoverageGoalPercent = 0;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
        }

        [Test]
        public void ValidatePlan_CoverageGoalPercenteBiggerThanMaximum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.CoverageGoalPercent = 230;

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
        }

        [Test]
        public void ValidatePlan_ShareOfVoicePercentLessThanMinimum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.AvailableMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto { ShareOfVoicePercent = 0 }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
        }

        [Test]
        public void ValidatePlan_ShareOfVoicePercentBiggerThanMaximum()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.AvailableMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto { ShareOfVoicePercent = 150 }
            };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
        }

        [Test]
        public void ValidatePlan_SumOfWeeklyBreakdownWeeksDifferentFromDeliveryImpressions()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.DeliveryImpressions = 100;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
            {
                    new WeeklyBreakdownWeek{ Impressions = 20},
                    new WeeklyBreakdownWeek{ Impressions = 30}
            };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("The impressions count is different betweek the delivery and the weekly breakdown"));
        }

        [Test]
        public void ValidatePlan_SumOfShareOfVoiceDifferentFrom100()
        {
            _ConfigureMocksToReturnTrue();

            var plan = _GetPlan();
            plan.DeliveryImpressions = 50;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek{ Impressions = 20, ShareOfVoice = 20},
                    new WeeklyBreakdownWeek{ Impressions = 30, ShareOfVoice = 20}
                };

            Assert.That(() => _planValidator.ValidatePlan(plan), Throws.TypeOf<Exception>().With.Message.EqualTo("The share of voice count is not equat to 100%"));
        }

        [Test]
        public void ValidateWeeklyBreakdown_RequestNull()
        {
            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(null), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid request"));
        }

        [Test]
        public void ValidateWeeklyBreakdown_EmptyFlightStartDate()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(2019, 8, 1),
                FlightStartDate = new DateTime()
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
        }

        [Test]
        public void ValidateWeeklyBreakdown_EmptyFlightEndDate()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(),
                FlightStartDate = new DateTime(2019, 8, 1)
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
        }

        [Test]
        public void ValidateWeeklyBreakdown_FlightStartBiggerThanFlightEnd()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(2019, 8, 1),
                FlightStartDate = new DateTime(2019, 9, 1)
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight dates.  The end date cannot be before the start date."));
        }

        [Test]
        public void ValidateWeeklyBreakdown_InvalidWeeksForcustomDelivery()
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightEndDate = new DateTime(2019, 8, 1),
                FlightStartDate = new DateTime(2019, 7, 1),
                DeliveryType = Entities.Enums.PlanGloalBreakdownTypeEnum.Custom
            };

            Assert.That(() => _planValidator.ValidateWeeklyBreakdown(request), Throws.TypeOf<Exception>().With.Message.EqualTo("For custom delivery you have to provide the weeks values"));
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
               Vpvh = 0.35
           };
    }
}
