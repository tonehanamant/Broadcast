using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.PlanServices
{
    [TestFixture]
    [Category("short_running")]
    public class PlanServiceTests
    {
        private readonly IPlanService planService;
        private readonly Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private readonly Mock<IPlanValidator> _PlanValidatorMock;
        private readonly Mock<IPlanBudgetDeliveryCalculator> _PlanBudgetDeliveryCalculatorMock;
        private readonly Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private readonly Mock<IPlanAggregator> _PlanAggregatorMock;
        private readonly Mock<ICampaignAggregationJobTrigger> _CampaignAggregationJobTriggerMock;
        private readonly Mock<INsiUniverseService> _NsiUniverseServiceMock;
        private readonly Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCacheMock;
        private readonly Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private readonly Mock<IBroadcastLockingManagerApplicationService> _BroadcastLockingManagerApplicationServiceMock;
        private readonly Mock<IPlanPricingService> _PlanPricingServiceMock;
        private readonly Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private readonly Mock<IDaypartDefaultService> _DaypartDefaultServiceMock;
        private readonly Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;
        private readonly Mock<ICreativeLengthEngine> _CreativeLengthEngineMock;

        public PlanServiceTests()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _PlanValidatorMock = new Mock<IPlanValidator>();
            _PlanBudgetDeliveryCalculatorMock = new Mock<IPlanBudgetDeliveryCalculator>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _PlanAggregatorMock = new Mock<IPlanAggregator>();
            _CampaignAggregationJobTriggerMock = new Mock<ICampaignAggregationJobTrigger>();
            _NsiUniverseServiceMock = new Mock<INsiUniverseService>();
            _BroadcastAudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _BroadcastLockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
            _PlanPricingServiceMock = new Mock<IPlanPricingService>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _DaypartDefaultServiceMock = new Mock<IDaypartDefaultService>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _CreativeLengthEngineMock = new Mock<ICreativeLengthEngine>();

            planService = new PlanService(
                _DataRepositoryFactoryMock.Object,
                _PlanValidatorMock.Object,
                _PlanBudgetDeliveryCalculatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _PlanAggregatorMock.Object,
                _CampaignAggregationJobTriggerMock.Object,
                _NsiUniverseServiceMock.Object,
                _BroadcastAudiencesCacheMock.Object,
                _SpotLengthEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object,
                _PlanPricingServiceMock.Object,
                _QuarterCalculationEngineMock.Object,
                _DaypartDefaultServiceMock.Object,
                _WeeklyBreakdownEngineMock.Object,
                _CreativeLengthEngineMock.Object
            );
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Even_Success_Test()
        {
            //Arrange
            var request = _GetWeeklyBreakDownEvenRequest();
            var mockedListMediaWeeksByFlight = _GetDisplayMediaWeeks_Even();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            _SetupWeeklyBreakdownEngineMock(request);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Impressions_Success_Test()
        {
            //Arrange
            var request = _GetWeeklyBreakDownRequest();

            request.Weeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 3,
                MediaWeekId = 846,
                StartDate = new DateTime(2020, 3, 9),
                EndDate = new DateTime(2020, 3, 15),
                NumberOfActiveDays = 7,
                ActiveDays = "M-Su",
                WeeklyImpressions = 3000,
                WeeklyImpressionsPercentage = 20,
                WeeklyRatings = 5.608234009172268,
                WeeklyBudget = 60000,
                WeeklyAdu = 0,
                IsUpdated = true
            });
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;
            request.DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek;

            var mockedListMediaWeeksByFlight = _GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            _SetupWeeklyBreakdownEngineMock(request);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Impressions_Percentage_Success_Test()
        {
            //Arrange
            var request = _GetWeeklyBreakDownRequest();
            request.Weeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 3,
                MediaWeekId = 846,
                StartDate = new DateTime(2020, 3, 9),
                EndDate = new DateTime(2020, 3, 15),
                NumberOfActiveDays = 7,
                ActiveDays = "M-Su",
                WeeklyImpressions = 6000,
                WeeklyImpressionsPercentage = 10,
                WeeklyRatings = 5.608234009172268,
                WeeklyBudget = 60000,
                WeeklyAdu = 0,
                IsUpdated = true
            });
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Percentage;
            request.DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            
            var mockedListMediaWeeksByFlight = _GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            _SetupWeeklyBreakdownEngineMock(request);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Ratings_Success_Test()
        {
            //Arrange
            var request = _GetWeeklyBreakDownRequest();
            request.Weeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 3,
                MediaWeekId = 846,
                StartDate = new DateTime(2020, 3, 9),
                EndDate = new DateTime(2020, 3, 15),
                NumberOfActiveDays = 7,
                ActiveDays = "M-Su",
                WeeklyImpressions = 6000,
                WeeklyImpressionsPercentage = 20,
                WeeklyRatings = 3.608234009172268,
                WeeklyBudget = 60000,
                WeeklyAdu = 0,
                IsUpdated = true
            });
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Ratings;
            request.DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek;

            var mockedListMediaWeeksByFlight = _GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            _SetupWeeklyBreakdownEngineMock(request);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatesInitialPlanWeeklyBreakdown_ForCustomByWeekByAdLengthDeliveryType()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.Clear();

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupWeeklyBreakdownEngineMock(request);
            _SetupCreativeLengthEngineMock();

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RecalculatesPlanWeeklyBreakdown_ForCustomByWeekByAdLengthDeliveryType()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.RemoveAll(x => x.SpotLengthId == 3); // remove spot length 3 to check how the calculator adds them back

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupWeeklyBreakdownEngineMock(request);
            _SetupCreativeLengthEngineMock();

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RecalculatesPlanWeeklyBreakdown_ImpressionsChange_ForCustomByWeekByAdLengthDeliveryType()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.First().IsUpdated = true;
            request.Weeks.First().WeeklyImpressions = 50;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupWeeklyBreakdownEngineMock(request);
            _SetupCreativeLengthEngineMock();

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RecalculatesPlanWeeklyBreakdown_RatingsChange_ForCustomByWeekByAdLengthDeliveryType()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.First().IsUpdated = true;
            request.Weeks.First().WeeklyRatings = 4;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Ratings;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupWeeklyBreakdownEngineMock(request);
            _SetupCreativeLengthEngineMock();

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RecalculatesPlanWeeklyBreakdown_WeeklyPercentageChange_ForCustomByWeekByAdLengthDeliveryType()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.First().IsUpdated = true;
            request.Weeks.First().WeeklyImpressionsPercentage = 50;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Percentage;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupWeeklyBreakdownEngineMock(request);
            _SetupCreativeLengthEngineMock();

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private void _SetupWeeklyBreakdownEngineMock(WeeklyBreakdownRequest request)
        {
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeekNumberByMediaWeekDictionary(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(_GetWeekNumberByMediaWeekDictionary());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<List<WeeklyBreakdownWeek>>()))
                .Returns<List<WeeklyBreakdownWeek>>(p => p
                    .GroupBy(x => x.MediaWeekId)
                    .Select(x => new WeeklyBreakdownByWeek
                    {
                        WeekNumber = x.First().WeekNumber,
                        MediaWeekId = x.First().MediaWeekId,
                        StartDate = x.First().StartDate,
                        EndDate = x.First().EndDate,
                        NumberOfActiveDays = x.First().NumberOfActiveDays,
                        ActiveDays = x.First().ActiveDays,
                        Impressions = x.Sum(i => i.WeeklyImpressions),
                        Budget = x.Sum(i => i.WeeklyBudget),
                        Adu = x.Sum(i => i.WeeklyAdu)
                    })
                    .ToList());
        }

        private void _SetupCreativeLengthEngineMock()
        {
            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<List<CreativeLength>>()))
                .Returns(new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = 30
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 2,
                        Weight = 13
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3,
                        Weight = 57
                    }
                });
        }

        private Dictionary<int, int> _GetWeekNumberByMediaWeekDictionary()
        {
            return new Dictionary<int, int>
            {
                { 844, 1 },
                { 845, 2 },
                { 846, 3 },
                { 847, 4 },
                { 848, 5 }
            };
        }

        private WeeklyBreakdownRequest _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType()
        {
            return new WeeklyBreakdownRequest
            {
                FlightStartDate = new DateTime(2020, 2, 25),
                FlightEndDate = new DateTime(2020, 3, 29),
                FlightDays = new List<int> { 1, 2, 3, 4, 5 },
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2020, 3, 2)
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength,
                TotalImpressions = 213,
                TotalRatings = 7,
                TotalBudget = 500,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = 30
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 2,
                        Weight = 13
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3,
                        Weight = null
                    }
                },
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    // week 1
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 845,
                        StartDate = new DateTime(2020, 3, 2),
                        EndDate = new DateTime(2020, 3, 8),
                        NumberOfActiveDays = 6,
                        ActiveDays = "M,Tu,W,F,Sa,Su",
                        WeeklyImpressions = 30,
                        WeeklyImpressionsPercentage = 60,
                        WeeklyRatings = 3,
                        WeeklyBudget = 60,
                        WeeklyAdu = 3,
                        SpotLengthId = 1,
                        PercentageOfWeek = 34,
                        IsUpdated = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 845,
                        StartDate = new DateTime(2020, 3, 2),
                        EndDate = new DateTime(2020, 3, 8),
                        NumberOfActiveDays = 6,
                        ActiveDays = "M,Tu,W,F,Sa,Su",
                        WeeklyImpressions = 30,
                        WeeklyImpressionsPercentage = 60,
                        WeeklyRatings = 3,
                        WeeklyBudget = 60,
                        WeeklyAdu = 3,
                        SpotLengthId = 2,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 845,
                        StartDate = new DateTime(2020, 3, 2),
                        EndDate = new DateTime(2020, 3, 8),
                        NumberOfActiveDays = 6,
                        ActiveDays = "M,Tu,W,F,Sa,Su",
                        WeeklyImpressions = 30,
                        WeeklyImpressionsPercentage = 60,
                        WeeklyRatings = 3,
                        WeeklyBudget = 60,
                        WeeklyAdu = 3,
                        SpotLengthId = 3,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    },

                    // week 2
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 846,
                        StartDate = new DateTime(2020, 3, 9),
                        EndDate = new DateTime(2020, 3, 15),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 1,
                        PercentageOfWeek = 34,
                        IsUpdated = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 846,
                        StartDate = new DateTime(2020, 3, 9),
                        EndDate = new DateTime(2020, 3, 15),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 2,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 846,
                        StartDate = new DateTime(2020, 3, 9),
                        EndDate = new DateTime(2020, 3, 15),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 3,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    },

                    // week 3
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 847,
                        StartDate = new DateTime(2020, 3, 16),
                        EndDate = new DateTime(2020, 3, 22),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 1,
                        PercentageOfWeek = 34,
                        IsUpdated = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 847,
                        StartDate = new DateTime(2020, 3, 16),
                        EndDate = new DateTime(2020, 3, 22),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 2,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 847,
                        StartDate = new DateTime(2020, 3, 16),
                        EndDate = new DateTime(2020, 3, 22),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 3,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    },
                    // week with not chosen spot length
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 847,
                        StartDate = new DateTime(2020, 3, 16),
                        EndDate = new DateTime(2020, 3, 22),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 4,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    },

                    // out of flight week
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4,
                        MediaWeekId = 888,
                        StartDate = new DateTime(2021, 3, 16),
                        EndDate = new DateTime(2020, 3, 22),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M,Tu,W,Th,F,Sa,Su",
                        WeeklyImpressions = 10,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1,
                        WeeklyBudget = 20,
                        WeeklyAdu = 1,
                        SpotLengthId = 3,
                        PercentageOfWeek = 33,
                        IsUpdated = false
                    }
                }
            };
        }
        
        private WeeklyBreakdownRequest _GetWeeklyBreakDownRequest()
        {
            return new WeeklyBreakdownRequest
            {
                FlightStartDate = new DateTime(2020, 2, 24),
                FlightEndDate = new DateTime(2020, 3, 29),
                FlightHiatusDays = new List<DateTime>(),
                TotalImpressions = 30000,
                TotalRatings = 28.041170045861335,
                TotalBudget = 300000,
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 844,
                        StartDate = new DateTime(2020, 2, 24),
                        EndDate = new DateTime(2020, 3, 1),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 6000,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 5.608234009172268,
                        WeeklyBudget = 60000,
                        WeeklyAdu = 0
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 845,
                        StartDate = new DateTime(2020, 3, 2),
                        EndDate = new DateTime(2020, 3, 8),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 6000,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 5.608234009172268,
                        WeeklyBudget = 60000,
                        WeeklyAdu = 0
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4,
                        MediaWeekId = 847,
                        StartDate = new DateTime(2020, 3, 16),
                        EndDate = new DateTime(2020, 3, 22),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 6000,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 5.608234009172268,
                        WeeklyBudget = 60000,
                        WeeklyAdu = 0
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 6,
                        MediaWeekId = 848,
                        StartDate = new DateTime(2020, 3, 23),
                        EndDate = new DateTime(2020, 3, 29),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 6000,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 5.608234009172268,
                        WeeklyBudget = 60000,
                        WeeklyAdu = 0
                    }
                },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 }
            };
        }

        private WeeklyBreakdownRequest _GetWeeklyBreakDownEvenRequest()
        {
            return new WeeklyBreakdownRequest
            {
                FlightStartDate = new DateTime(2020, 2, 25),
                FlightEndDate = new DateTime(2020, 3, 15),
                FlightHiatusDays = new List<DateTime>(),
                TotalImpressions = 20,
                TotalRatings = 0.00928868473742818,
                TotalBudget = 400,
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                Weeks = new List<WeeklyBreakdownWeek>(),
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 }
            };
        }

        private List<DisplayMediaWeek> _GetDisplayMediaWeeks()
        {
            return new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek
                {
                    Id = 844,
                    Week = 1,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 2, 24),
                    WeekEndDate = new DateTime(2020, 3, 1),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                },
                new DisplayMediaWeek
                {
                    Id = 845,
                    Week = 2,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 3, 2),
                    WeekEndDate = new DateTime(2020, 3, 8),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                },
                new DisplayMediaWeek
                {
                    Id = 846,
                    Week = 3,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 3, 9),
                    WeekEndDate = new DateTime(2020, 3, 15),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                },
                new DisplayMediaWeek
                {
                    Id = 847,
                    Week = 4,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 3, 16),
                    WeekEndDate = new DateTime(2020, 3, 22),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                },
                new DisplayMediaWeek
                {
                    Id = 848,
                    Week = 5,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 3, 23),
                    WeekEndDate = new DateTime(2020, 3, 29),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                }
            };
        }

        private List<DisplayMediaWeek> _GetDisplayMediaWeeks_Even()
        {
            return new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek
                {
                    Id = 844,
                    Week = 1,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 2, 24),
                    WeekEndDate = new DateTime(2020, 3, 1),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                },
                new DisplayMediaWeek
                {
                    Id = 845,
                    Week = 2,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 3, 2),
                    WeekEndDate = new DateTime(2020, 3, 8),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                },
                new DisplayMediaWeek
                {
                    Id = 846,
                    Week = 3,
                    MediaMonthId = 462,
                    Year = 2020,
                    Month = 3,
                    WeekStartDate = new DateTime(2020, 3, 9),
                    WeekEndDate = new DateTime(2020, 3, 15),
                    MonthStartDate = new DateTime(2020, 2, 24),
                    MonthEndDate = new DateTime(2020, 3, 29)
                }
            };
        }
    }
}
