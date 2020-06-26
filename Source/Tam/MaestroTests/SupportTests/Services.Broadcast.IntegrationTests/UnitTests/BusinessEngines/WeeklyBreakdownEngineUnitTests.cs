using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.PlanServices
{
    [TestFixture]
    [Category("short_running")]
    public class WeeklyBreakdownEngineUnitTests
    {
        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private readonly Mock<IPlanValidator> _PlanValidatorMock;
        private readonly Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private readonly Mock<ICreativeLengthEngine> _CreativeLengthEngineMock;
        private readonly Mock<ISpotLengthEngine> _SpotLengthEngine;

        public WeeklyBreakdownEngineUnitTests()
        {
            _PlanValidatorMock = new Mock<IPlanValidator>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _CreativeLengthEngineMock = new Mock<ICreativeLengthEngine>();
            _SpotLengthEngine = new Mock<ISpotLengthEngine>();

            _WeeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object, _SpotLengthEngine.Object);
            _SpotLengthEngine.Setup(x => x.GetSpotLengthMultipliers())
                .Returns(_SpotLengthMultiplier);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PlanWeeklyGoalBreakdown_Even_Success_Test()
        {//CalculatePlanWeeklyGoalBreakdown_Even_Success_Test
            //Arrange
            var request = _GetWeeklyBreakDownEvenRequest();
            var mockedListMediaWeeksByFlight = _GetDisplayMediaWeeks_Even();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void WeeklyBreakdown_UpdatedImpressions_Success()
        {//CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Impressions_Success_Test
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
                IsUpdated = true,
                WeeklyUnits = 1
            });
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;
            request.DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek;

            var mockedListMediaWeeksByFlight = _GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Updated_Impres_Percentage_Success()
        {//CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Impressions_Percentage_Success_Test
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
                IsUpdated = true,
                WeeklyUnits = 1
            });
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Percentage;
            request.DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek;

            var mockedListMediaWeeksByFlight = _GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PlanWeeklyBreakdown_UpdatedRatingsSuccess()
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

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InitialWeeklyBreakdown_ForCustomByWeekByAdLength()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.Clear();

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupCreativeLengthEngineMock();

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RecalculatesWeeklyBreakdown_CustomByAdLength()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.RemoveAll(x => x.SpotLengthId == 3); // remove spot length 3 to check how the calculator adds them back

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupCreativeLengthEngineMock();

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Recalc_ImpsChange_CustWeekAdLen()
        {//Recalculate_ImpressionsChange_ForCustomByWeekByAdLengthDeliveryType
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.First().IsUpdated = true;
            request.Weeks.First().WeeklyImpressions = 50;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupCreativeLengthEngineMock();

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Recalc_RatingsChange_CustomWeekAdLength()
        {//RecalculatesPlanWeeklyBreakdown_RatingsChange_ForCustomByWeekByAdLengthDeliveryType
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.First().IsUpdated = true;
            request.Weeks.First().WeeklyRatings = 4;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Ratings;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupCreativeLengthEngineMock();

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void WeeklyBreakdown_UpdatedFirstWeek()
        {
            //Arrange
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByAdLengthDeliveryType();
            request.Weeks.First().IsUpdated = true;
            request.Weeks.First().WeeklyImpressionsPercentage = 50;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Percentage;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            _SetupCreativeLengthEngineMock();

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownByWeek()
        {
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetSpotLengthMultipliers())
                .Returns(_SpotLengthMultiplier);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object, spotLengthEngine.Object);
            var result = weeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(_WeeklyBreakdown, _ImpressionsPerUnit, _CreativeLengths);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownByWeekBySpotLength()
        {
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetSpotLengthMultipliers())
                .Returns(_SpotLengthMultiplier);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object, spotLengthEngine.Object);
            var result = weeklyBreakdownEngine.GroupWeeklyBreakdownByWeekBySpotLength(_WeeklyBreakdown, _ImpressionsPerUnit, true);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownByStandardDaypart()
        {
            var result = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByStandardDaypart(_WeeklyBreakdown);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsWeekNumberByMediaWeekDictionary()
        {
            var result = _WeeklyBreakdownEngine.GetWeekNumberByMediaWeekDictionary(_WeeklyBreakdown);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private IEnumerable<WeeklyBreakdownWeek> _WeeklyBreakdown = new List<WeeklyBreakdownWeek>
        {
            new WeeklyBreakdownWeek
            {
                WeekNumber = 3,
                MediaWeekId = 502,
                StartDate = new DateTime(2020, 5, 4),
                EndDate = new DateTime(2020, 5, 10),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 1000,
                WeeklyBudget = 100,
                SpotLengthId = 1,
                DaypartCodeId = 1,
                AduImpressions = 1000
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 3,
                MediaWeekId = 502,
                StartDate = new DateTime(2020, 5, 4),
                EndDate = new DateTime(2020, 5, 10),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 1500,
                WeeklyBudget = 200,
                SpotLengthId = 2,
                DaypartCodeId = 1,
                AduImpressions = 1000
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 2,
                MediaWeekId = 501,
                StartDate = new DateTime(2020, 4, 27),
                EndDate = new DateTime(2020, 5, 3),
                NumberOfActiveDays = 4,
                ActiveDays = "M,Tu,W,Th",
                WeeklyImpressions = 800,
                WeeklyBudget = 70,
                SpotLengthId = 1,
                DaypartCodeId = 1,
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 500,
                StartDate = new DateTime(2020, 4, 20),
                EndDate = new DateTime(2020, 4, 26),
                NumberOfActiveDays = 3,
                ActiveDays = "M,Tu,W",
                WeeklyImpressions = 500,
                WeeklyBudget = 30,
                SpotLengthId = 1,
                DaypartCodeId = 1,
                AduImpressions = 1000
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 500,
                StartDate = new DateTime(2020, 4, 20),
                EndDate = new DateTime(2020, 4, 26),
                NumberOfActiveDays = 3,
                ActiveDays = "M,Tu,W",
                WeeklyImpressions = 500,
                WeeklyBudget = 30,
                SpotLengthId = 1,
                DaypartCodeId = 2,
                AduImpressions = 1000
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 500,
                StartDate = new DateTime(2020, 4, 20),
                EndDate = new DateTime(2020, 4, 26),
                NumberOfActiveDays = 3,
                ActiveDays = "M,Tu,W",
                WeeklyImpressions = 500,
                WeeklyBudget = 30,
                SpotLengthId = 2,
                DaypartCodeId = 2,
                AduImpressions = 1000
            }
        };

        private void _SetupCreativeLengthEngineMock()
        {
            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<List<CreativeLength>>()))
                .Returns(_CreativeLengths);
        }

        private static List<CreativeLength> _CreativeLengths = new List<CreativeLength>
        {
            new CreativeLength
            {
                SpotLengthId = 1,
                Weight = 55
            },
            new CreativeLength
            {
                SpotLengthId = 2,
                Weight = 45
            }
        };


        private Dictionary<int, double> _SpotLengthMultiplier = new Dictionary<int, double>
        {
            { 1,1}, { 2, 2}
        };

        private readonly double _ImpressionsPerUnit = 250;

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
                ImpressionsPerUnit = 20,
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                        IsUpdated = false,
                        WeeklyUnits = 2
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
                ImpressionsPerUnit = 5000,
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
                        WeeklyAdu = 0,
                        WeeklyUnits = 1
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
                        WeeklyAdu = 0,
                        WeeklyUnits = 1
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
                        WeeklyAdu = 0,
                        WeeklyUnits = 1
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
                        WeeklyAdu = 0,
                        WeeklyUnits = 1
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
                ImpressionsPerUnit = 10,
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
