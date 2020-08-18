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
using System.ServiceModel.Configuration;
using Castle.Components.DictionaryAdapter;
using Common.Services.Repositories;
using Services.Broadcast.IntegrationTests.TestData;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Repositories;

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
        private readonly Mock<ISpotLengthEngine> _SpotLengthEngineMock;

        public WeeklyBreakdownEngineUnitTests()
        {
            _PlanValidatorMock = new Mock<IPlanValidator>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _CreativeLengthEngineMock = new Mock<ICreativeLengthEngine>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();

            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(_GetMockDaypartDefaultRepository().Object);

            _SpotLengthEngineMock.Setup(x => x.GetDeliveryMultipliersBySpotLengthId())
                .Returns(_SpotLengthMultiplier);

            _WeeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                _SpotLengthEngineMock.Object,
                dataRepositoryFactory.Object);
        }

        private Mock<IDaypartDefaultRepository> _GetMockDaypartDefaultRepository()
        {
            var daypartDefaultRepository = new Mock<IDaypartDefaultRepository>();
            
            daypartDefaultRepository.Setup(s => s.GetAllDaypartDefaults())
                .Returns(DaypartsTestData.GetAllDaypartDefaultsWithBaseData);
            
            daypartDefaultRepository.Setup(s => s.GetAllDaypartDefaultsWithAllData())
                .Returns(DaypartsTestData.GetAllDaypartDefaultsWithFullData);

            var testDefaultDays = DaypartsTestData.GetDayIdsFromDaypartDefaults();
            daypartDefaultRepository.Setup(s => s.GetDayIdsFromDaypartDefaults(It.IsAny<List<int>>()))
                .Returns<List<int>>((ids) =>
                {
                    var items = new List<int>();
                    foreach (var id in ids)
                    {
                        items.AddRange(testDefaultDays[id]);
                    }
                    return items.Distinct().ToList();
                });

            return daypartDefaultRepository;
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
        public void PlanWeeklyGoalBreakdown_CreatePlan_WithoutDaypart()
        {
            //Arrange
            var request = new WeeklyBreakdownRequest
            {
                FlightDays = new List<int> { 1,2,3,4,5,6,7 },
                FlightStartDate = new DateTime(2020, 09, 28, 0, 0, 0),
                FlightEndDate = new DateTime(2020, 12, 27, 23, 59, 59),
                FlightHiatusDays = new List<DateTime>(),
                TotalImpressions = 70000,
                TotalRatings = 58,
                TotalBudget = 1070000,
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                Equivalized = true,
                Weeks = new List<WeeklyBreakdownWeek>(),
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1}
                },
                ImpressionsPerUnit = 1,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypart { WeekdaysWeighting = 70, WeekendWeighting = 30}
                }, 
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            };
            var mockedListMediaWeeksByFlight = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek { Id = 875, Week = 1, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 9, 28), WeekEndDate = new DateTime(2020, 10, 4), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
                new DisplayMediaWeek { Id = 876, Week = 2, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 10, 5), WeekEndDate = new DateTime(2020, 10, 11), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
                new DisplayMediaWeek { Id = 877, Week = 3, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 10, 12), WeekEndDate = new DateTime(2020, 10, 18), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
                new DisplayMediaWeek { Id = 878, Week = 4, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 10, 19), WeekEndDate = new DateTime(2020, 10, 25), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
                new DisplayMediaWeek { Id = 879, Week = 1, MediaMonthId = 470, Year = 2020, Month = 11, WeekStartDate = new DateTime(2020, 10, 26), WeekEndDate = new DateTime(2020, 11, 1), MonthStartDate = new DateTime(2020, 10, 26), MonthEndDate = new DateTime(2020, 11, 29) },
                new DisplayMediaWeek { Id = 880, Week = 2, MediaMonthId = 470, Year = 2020, Month = 11, WeekStartDate = new DateTime(2020, 11, 2), WeekEndDate = new DateTime(2020, 11, 8), MonthStartDate = new DateTime(2020, 10, 26), MonthEndDate = new DateTime(2020, 11, 29) },
                new DisplayMediaWeek { Id = 881, Week = 3, MediaMonthId = 470, Year = 2020, Month = 11, WeekStartDate = new DateTime(2020, 11, 9), WeekEndDate = new DateTime(2020, 11, 15), MonthStartDate = new DateTime(2020, 10, 26), MonthEndDate = new DateTime(2020, 11, 29) },
                new DisplayMediaWeek { Id = 882, Week = 4, MediaMonthId = 470, Year = 2020, Month = 11, WeekStartDate = new DateTime(2020, 11, 16), WeekEndDate = new DateTime(2020, 11, 22), MonthStartDate = new DateTime(2020, 10, 26), MonthEndDate = new DateTime(2020, 11, 29) },
                new DisplayMediaWeek { Id = 883, Week = 5, MediaMonthId = 470, Year = 2020, Month = 11, WeekStartDate = new DateTime(2020, 11, 23), WeekEndDate = new DateTime(2020, 11, 29), MonthStartDate = new DateTime(2020, 10, 26), MonthEndDate = new DateTime(2020, 11, 29) },
                new DisplayMediaWeek { Id = 884, Week = 1, MediaMonthId = 471, Year = 2020, Month = 12, WeekStartDate = new DateTime(2020, 11, 30), WeekEndDate = new DateTime(2020, 12, 6), MonthStartDate = new DateTime(2020, 11, 30), MonthEndDate = new DateTime(2020, 12, 27) },
                new DisplayMediaWeek { Id = 885, Week = 2, MediaMonthId = 471, Year = 2020, Month = 12, WeekStartDate = new DateTime(2020, 12, 7), WeekEndDate = new DateTime(2020, 12, 13), MonthStartDate = new DateTime(2020, 11, 30), MonthEndDate = new DateTime(2020, 12, 27) },
                new DisplayMediaWeek { Id = 886, Week = 3, MediaMonthId = 471, Year = 2020, Month = 12, WeekStartDate = new DateTime(2020, 12, 14), WeekEndDate = new DateTime(2020, 12, 20), MonthStartDate = new DateTime(2020, 11, 30), MonthEndDate = new DateTime(2020, 12, 27) },
                new DisplayMediaWeek { Id = 887, Week = 4, MediaMonthId = 471, Year = 2020, Month = 12, WeekStartDate = new DateTime(2020, 12, 21), WeekEndDate = new DateTime(2020, 12, 27), MonthStartDate = new DateTime(2020, 11, 30), MonthEndDate = new DateTime(2020, 12, 27) }
            };

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<List<CreativeLength>>()))
                .Returns(new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 100 }});

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
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(_GetMockDaypartDefaultRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliersBySpotLengthId())
                .Returns(_SpotLengthMultiplier);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object);
            var result = weeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(_WeeklyBreakdown, _ImpressionsPerUnit, _CreativeLengths);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownByWeekBySpotLength()
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(_GetMockDaypartDefaultRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliersBySpotLengthId())
                .Returns(_SpotLengthMultiplier);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object);
            var result = weeklyBreakdownEngine.GroupWeeklyBreakdownByWeekBySpotLength(_WeeklyBreakdown, _ImpressionsPerUnit, true);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupWeeklyBreakdownByWeekByDaypart()
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(_GetMockDaypartDefaultRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliersBySpotLengthId())
                .Returns(_SpotLengthMultiplier);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object);

            var result = weeklyBreakdownEngine.GroupWeeklyBreakdownByWeekByDaypart(_WeeklyBreakdown, _ImpressionsPerUnit, true, _CreativeLengths);

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void WeeklyBreakdown_ByDaypartInitialLoad()
        {
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByDaypart();
            request.Weeks.Clear();

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void WeeklyBreakdown_ByDaypartNoWeightingGoal()
        {//CalculatePlanWeeklyGoalBreakdown_ByWeekByDaypart_WithoutDaypartWeightingGoalPercent
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByDaypart();
            request.Weeks.Clear();
            request.Dayparts.ForEach(d => d.WeightingGoalPercent = null);

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void WeeklyGoalBreakdown_ByDaypartModifyDayparts()
        {//CalculatePlanWeeklyGoalBreakdown_ByWeekByDaypart_ModifyDayparts
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByDaypart();
            request.Dayparts.RemoveAt(0);
            request.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1 });

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        /// <summary>
        /// Days are Monday(1) through Sunday(7)
        /// </summary>
        [Test]
        [TestCase(1,7, true, true, 7, "M-Su")]
        [TestCase(1, 7, true, false, 7, "M-Su")]
        [TestCase(1, 7, false, true, 2, "Sa,Su")]
        public void CalculateActiveDays_AllDaysCoveredByAllDayparts(
            int weekStartDayId, int weekEndDayId, 
            bool hasFullWeekDaypart, bool hasWeekendOnlyDaypart,
            int expectedResult, string expectedResultActiveDayString)
        {
            // Arrange
            var testDateMonday = new DateTime(2020, 8, 10);
            var weekStartDate = testDateMonday.AddDays((weekStartDayId - 1));
            var weekEndDate = testDateMonday.AddDays((weekEndDayId - 1));

            // variance here is out of scope of this test.
            var flightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 }; // active DOWs for the flight
            var hiatusDays = new List<DateTime>();

            var dayparts = new List<PlanDaypartDto>();

            const string daypartCodeWeekend = "WKD";
            var allDayparts = DaypartsTestData.GetAllDaypartDefaultsWithFullData();

            if (hasFullWeekDaypart)
            {
                var foundDaypart = allDayparts.First(s => !s.Code.Equals(daypartCodeWeekend, StringComparison.OrdinalIgnoreCase));
                var planDaypart = new PlanDaypartDto
                {
                    DaypartCodeId = foundDaypart.Id,
                    DaypartTypeId = foundDaypart.DaypartType,
                    StartTimeSeconds = foundDaypart.DefaultStartTimeSeconds,
                    IsStartTimeModified = false,
                    EndTimeSeconds = foundDaypart.DefaultEndTimeSeconds,
                    IsEndTimeModified = false,
                };
                dayparts.Add(planDaypart);
            }

            if (hasWeekendOnlyDaypart)
            {
                var foundDaypart = allDayparts.First(s => s.Code.Equals(daypartCodeWeekend, StringComparison.OrdinalIgnoreCase));
                var planDaypart = new PlanDaypartDto
                {
                    DaypartCodeId = foundDaypart.Id,
                    DaypartTypeId = foundDaypart.DaypartType,
                    StartTimeSeconds = foundDaypart.DefaultStartTimeSeconds,
                    IsStartTimeModified = false,
                    EndTimeSeconds = foundDaypart.DefaultEndTimeSeconds,
                    IsEndTimeModified = false,
                };
                dayparts.Add(planDaypart);
            }

            // Act
            string resultActiveDayString;
            var result = ((WeeklyBreakdownEngine) _WeeklyBreakdownEngine)._CalculateActiveDays(weekStartDate, weekEndDate, flightDays, hiatusDays, dayparts, out resultActiveDayString);

            // Assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedResultActiveDayString, resultActiveDayString);
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
                WeeklyRatings = 5,
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
                WeeklyRatings = 5,
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
                WeeklyRatings = 5,
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
                WeeklyRatings = 5,
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
                WeeklyRatings = 5,
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
                WeeklyRatings = 5,
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

        private WeeklyBreakdownRequest _GetWeeklyBreakdownRequest_CustomByWeekByDaypart()
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
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart,
                TotalImpressions = 213,
                ImpressionsPerUnit = 20,
                TotalRatings = 7,
                TotalBudget = 500,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{ DaypartCodeId = 15, WeightingGoalPercent = 60 },
                    new PlanDaypartDto{ DaypartCodeId = 20, WeightingGoalPercent = 40 }
                },
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "Tu-F",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 4,
                        PercentageOfWeek = 60,
                        StartDate = new DateTime(2020,2,24),
                        WeeklyBudget = 63.380281690140845070422535210m,
                        WeeklyImpressions = 27,
                        WeeklyImpressionsPercentage = 12.6,
                        WeeklyRatings = 0.84,
                        WeeklyUnits = 1.35,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "Tu-F",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 4,
                        PercentageOfWeek = 40,
                        StartDate = new DateTime(2020,2,24),
                        WeeklyBudget = 42.253521126760563380281690140m,
                        WeeklyImpressions = 18,
                        WeeklyImpressionsPercentage = 8.4,
                        WeeklyRatings = 0.56,
                        WeeklyUnits = 0.9,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "Tu-F",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 60,
                        StartDate = new DateTime(2020,3,2),
                        WeeklyBudget = 59.154929577464788732394366196m,
                        WeeklyImpressions = 25.2,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.78,
                        WeeklyUnits = 1.26,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "Tu-F",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 40,
                        StartDate = new DateTime(2020,3,2),
                        WeeklyBudget = 39.436619718309859154929577464m,
                        WeeklyImpressions = 16.8,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.52,
                        WeeklyUnits = 0.84,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-F",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 60,
                        StartDate = new DateTime(2020,3,9),
                        WeeklyBudget = 59.154929577464788732394366196m,
                        WeeklyImpressions = 25.2,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.78,
                        WeeklyUnits = 1.26,
                        WeekNumber = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-F",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 40,
                        StartDate = new DateTime(2020,3,9),
                        WeeklyBudget = 39.436619718309859154929577464m,
                        WeeklyImpressions = 16.8,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.52,
                        WeeklyUnits = 0.84,
                        WeekNumber = 3
                    },

                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-F",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 60,
                        StartDate = new DateTime(2020,3,16),
                        WeeklyBudget = 59.154929577464788732394366196m,
                        WeeklyImpressions = 25.2,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.78,
                        WeeklyUnits = 1.26,
                        WeekNumber = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-F",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 40,
                        StartDate = new DateTime(2020,3,16),
                        WeeklyBudget = 39.436619718309859154929577464m,
                        WeeklyImpressions = 16.8,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.52,
                        WeeklyUnits = 0.84,
                        WeekNumber = 4
                    },

                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-F",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 60,
                        StartDate = new DateTime(2020,3,23),
                        WeeklyBudget = 59.154929577464788732394366196m,
                        WeeklyImpressions = 25.2,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.78,
                        WeeklyUnits = 1.26,
                        WeekNumber = 5
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-F",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 5,
                        PercentageOfWeek = 40,
                        StartDate = new DateTime(2020,3,23),
                        WeeklyBudget = 39.436619718309859154929577464m,
                        WeeklyImpressions = 16.8,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.52,
                        WeeklyUnits = 0.84,
                        WeekNumber = 5
                    },
                }
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
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{ DaypartCodeId = 15, WeightingGoalPercent = 50 },
                    new PlanDaypartDto{ DaypartCodeId = 20, WeightingGoalPercent = 50 }
                },
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
