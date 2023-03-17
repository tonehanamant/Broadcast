using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [Category("short_running")]
    public class WeeklyBreakdownEngineV2UnitTests
    {
        private WeeklyBreakdownEngineV2 _WeeklyBreakdownEngine;
        private Mock<IPlanValidator> _PlanValidatorMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<ICreativeLengthEngine> _CreativeLengthEngineMock;
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;

        private LaunchDarklyClientStub _LaunchDarklyClientStub;

        [SetUp]
        public void Init()
        {
            _PlanValidatorMock = new Mock<IPlanValidator>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _CreativeLengthEngineMock = new Mock<ICreativeLengthEngine>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();

            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);

            _SpotLengthEngineMock.Setup(x => x.GetDeliveryMultipliers())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);

            _SpotLengthEngineMock.Setup(x => x.GetCostMultipliers(It.IsAny<bool>()))
                .Returns<bool>(SpotLengthTestData.GetCostMultipliersBySpotLengthId);

            _SpotLengthEngineMock.Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns<int>(SpotLengthTestData.GetSpotLengthValueById);

            _LaunchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

            _WeeklyBreakdownEngine = new WeeklyBreakdownEngineV2(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                _SpotLengthEngineMock.Object,
                dataRepositoryFactory.Object,
                featureToggleHelper);
        }

        private Mock<IStandardDaypartRepository> _GetMockStandardDaypartRepository()
        {
            var standardDaypartRepository = new Mock<IStandardDaypartRepository>();

            standardDaypartRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);

            standardDaypartRepository.Setup(s => s.GetAllStandardDaypartsWithAllData())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithFullData);

            var testDefaultDays = DaypartsTestData.GetDayIdsFromStandardDayparts();
            standardDaypartRepository.Setup(s => s.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>()))
                .Returns<List<int>>((ids) =>
                {
                    var items = new List<int>();
                    foreach (var id in ids)
                    {
                        items.AddRange(testDefaultDays[id]);
                    }
                    return items.Distinct().ToList();
                });

            return standardDaypartRepository;
        }

        // this tests that the ADU Only calculation retains the 
        // standard data, allowing the user to toggle between
        // standard and ADU only.
        //
        // We expect the same results for Standard and ADU only.
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void CalculateWeeklyBreakdown_ChangeAdu(bool setAduOnly)
        {
            // Arrange
            var request = _GetCalculateWeek2Request_WeekAduChange(aduOnly: setAduOnly);
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = true;

            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>((s, e) => MediaMonthAndWeekTestData.GetDisplayMediaWeekByFlight(s, e));

            _CreativeLengthEngineMock.Setup(s => s.DistributeWeight(It.IsAny<IEnumerable<CreativeLength>>()))
                .Returns<List<CreativeLength>>((c) => c);

            // Act
            var response = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            // Verify that the goal info is the same
            Assert.AreEqual(21, response.TotalActiveDays);
            Assert.AreEqual(100, response.TotalShareOfVoice);
            Assert.AreEqual(50000, response.TotalImpressions);
            Assert.AreEqual(41.5, response.TotalRatingPoints);
            Assert.AreEqual(100, response.TotalImpressionsPercentage);
            Assert.AreEqual(25000, response.TotalBudget);
            Assert.AreEqual(25000, response.TotalUnits);
            Assert.AreEqual(12000, response.TotalAduImpressions);

            // verify the collections are populated
            Assert.AreEqual(3, response.Weeks.Count);
            Assert.AreEqual(3, response.RawWeeklyBreakdownWeeks.Count);

            // Verify the updated week hasn't changed
            var updatedWeek = response.Weeks[1];
            Assert.AreEqual(16666, updatedWeek.WeeklyImpressions);
            Assert.AreEqual(33, updatedWeek.WeeklyImpressionsPercentage);
            Assert.AreEqual(13.83, updatedWeek.WeeklyRatings);
            Assert.AreEqual(8333, updatedWeek.WeeklyBudget);            
            Assert.AreEqual(12000, updatedWeek.WeeklyAdu);
            Assert.AreEqual(12000, updatedWeek.AduImpressions);
            Assert.AreEqual(8333, updatedWeek.WeeklyUnits);
            Assert.AreEqual(0.5, updatedWeek.WeeklyCpm);
        }

        private WeeklyBreakdownRequest _GetCalculateWeek2Request_WeekAduChange(bool aduOnly = false)
        {
            var request = new WeeklyBreakdownRequest
            {
                FlightDays = new List<int> { 1,2,3,4,5,6,7 },
                // These flight dates align with the MediaMonthAndWeekTestData
                FlightStartDate = new DateTime(2020, 11, 23),
                FlightEndDate = new DateTime(2020, 12, 13),
                FlightHiatusDays = new List<DateTime>(),
                TotalImpressions = 50000,
                TotalRatings = 41.5,
                TotalBudget = 25000,
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
                Equivalized = true,
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 883,
                        StartDate = new DateTime(2020, 11, 23),
                        EndDate = new DateTime(2020,11,29),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 16668,
                        WeeklyImpressionsPercentage = 33,
                        WeeklyRatings = 13.83,
                        WeeklyBudget = 8334,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        PlanDaypartId = 0,
                        PercentageOfWeek = 100,
                        IsUpdated = false,
                        UnitImpressions = 0,
                        WeeklyUnits = 8334,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 884,
                        StartDate = new DateTime(2020, 11, 30),
                        EndDate = new DateTime(2020,12,6),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 16666,
                        WeeklyImpressionsPercentage = 33,
                        WeeklyRatings = 13.83,
                        WeeklyBudget = 8333,
                        WeeklyAdu = 12000,
                        AduImpressions = 0,
                        PlanDaypartId = 0,
                        PercentageOfWeek = 100,
                        IsUpdated = true, // *** This is the week that is updated
                        UnitImpressions = 0,
                        WeeklyUnits = 8333,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 885,
                        StartDate = new DateTime(2020, 12, 7),
                        EndDate = new DateTime(2020,12, 13),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 16666,
                        WeeklyImpressionsPercentage = 33,
                        WeeklyRatings = 13.83,
                        WeeklyBudget = 8333,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        PlanDaypartId = 0,
                        PercentageOfWeek = 100,
                        IsUpdated = false,
                        UnitImpressions = 0,
                        WeeklyUnits = 8333,
                        IsLocked = false
                    }
                },
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 100 } },
                ImpressionsPerUnit = 2,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        PlanDaypartId = 1,
                        DaypartTypeId = DaypartTypeEnum.News,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Genres = new List<LookupDto> { new LookupDto { Id = 33, Display = "News" } }
                            }
                        },
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId= 31, 
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                Vpvh = 1, 
                                StartingPoint = new DateTime(2023,3,15,15,59,42)
                            }
                        }
                    }
                },
                IsAduOnly = aduOnly,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.ADUImpressions
            };

            return request;
        }

        [Test]
        [TestCase(false, true, true, 16000)]
        [TestCase(false, true, false, 8888.8888888888887d)]
        [TestCase(false, false, true, 8000)]
        [TestCase(false, false, false, 8000)]
        [TestCase(true, true, true, 8000)]
        [TestCase(true, true, false, 4444.444444444444d)]
        [TestCase(true, false, true, 4000)]
        [TestCase(true, false, false, 4000)]
        public void CalculateWeeklyADUImpressions_TwoCreatives(bool aduForPlanningV2Enabled,
            bool equivalized, bool weekHasSpotLengthId, double expectedResult)
        {
            // Arrange
            // spot length delivery multipliers are setup in the test class constructor
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = aduForPlanningV2Enabled;

            var weeklyBreakdownWeek = new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 937,
                StartDate = new DateTime(2021, 12, 6),
                EndDate = new DateTime(2021, 12, 12),
                NumberOfActiveDays = 7,
                ActiveDays = "M-Su",
                WeeklyImpressions = 0,
                WeeklyImpressionsPercentage = 0,
                WeeklyRatings = 0,
                WeeklyBudget = 0,
                WeeklyAdu = 4000,
                AduImpressions = 0,
                SpotLengthId = weekHasSpotLengthId ? (int?)2 : null,
                SpotLengthDuration = null,
                DaypartCodeId = null,
                PercentageOfWeek = null,
                IsUpdated = false,
                UnitImpressions = 0,
                WeeklyUnits = 0,
                IsLocked = false
            };

            var creatives = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1,Weight = 80},
                    new CreativeLength {SpotLengthId = 2,Weight = 20}
                };

            // Act
            var result = _WeeklyBreakdownEngine.CalculateWeeklyADUImpressions(weeklyBreakdownWeek, equivalized: equivalized, impressionsPerUnit: 2, creatives);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(false, true, true, 16000)]
        [TestCase(false, true, false, 16000)]
        [TestCase(false, false, true, 8000)]
        [TestCase(false, false, false, 8000)]
        [TestCase(true, true, true, 8000)]
        [TestCase(true, true, false, 8000)]
        [TestCase(true, false, true, 4000)]
        [TestCase(true, false, false, 4000)]
        public void CalculateWeeklyADUImpressions_SingleCreative(bool aduForPlanningV2Enabled,
            bool equivalized, bool weekHasSpotLengthId, double expectedResult)
        {
            // Arrange
            // spot length delivery multipliers are setup in the test class constructor
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = aduForPlanningV2Enabled;

            var weeklyBreakdownWeek = new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 937,
                StartDate = new DateTime(2021, 12, 6),
                EndDate = new DateTime(2021, 12, 12),
                NumberOfActiveDays = 7,
                ActiveDays = "M-Su",
                WeeklyImpressions = 0,
                WeeklyImpressionsPercentage = 0,
                WeeklyRatings = 0,
                WeeklyBudget = 0,
                WeeklyAdu = 4000,
                AduImpressions = 0,
                SpotLengthId = weekHasSpotLengthId ? (int?)2 : null,
                SpotLengthDuration = null,
                DaypartCodeId = null,
                PercentageOfWeek = null,
                IsUpdated = false,
                UnitImpressions = 0,
                WeeklyUnits = 0,
                IsLocked = false
            };

            var creatives = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 2,Weight = 100}
                };

            // Act
            var result = _WeeklyBreakdownEngine.CalculateWeeklyADUImpressions(weeklyBreakdownWeek, equivalized: equivalized, impressionsPerUnit: 2, creatives);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        // This tests an action that happens on Plan Save
        // This verifies that the result is calcualted as expected.
        [Test]
        public void DistributeGoalsByWeeksEtc_Standard()
        {
            // Arrange
            var plan = _GetPlanForDistributeGoal_StandardWithAdu();
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = true;

            // Act
            var weeks = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);

            // Assert
            Assert.AreEqual(6, weeks.Count);

            var weekOneSpotLengthOneDaypartOne = weeks.Single(w => w.WeekNumber == 1 && w.SpotLengthId == 1 && w.DaypartCodeId == 1);
            Assert.AreEqual(50, weekOneSpotLengthOneDaypartOne.PercentageOfWeek);
            Assert.AreEqual(666.66666666666663, weekOneSpotLengthOneDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.AduImpressions);
            Assert.AreEqual(4167, weekOneSpotLengthOneDaypartOne.WeeklyBudget);
            Assert.AreEqual(0.0005000, weekOneSpotLengthOneDaypartOne.WeeklyCpm);
            Assert.AreEqual(8334000, weekOneSpotLengthOneDaypartOne.WeeklyImpressions);
            Assert.AreEqual(17, weekOneSpotLengthOneDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(6.9172199999999995, weekOneSpotLengthOneDaypartOne.WeeklyRatings);

            var weekOneSpotLengthThreeDaypartOne = weeks.Single(w => w.WeekNumber == 1 && w.SpotLengthId == 3 && w.DaypartCodeId == 1);
            Assert.AreEqual(50, weekOneSpotLengthThreeDaypartOne.PercentageOfWeek);
            Assert.AreEqual(666.66666666666663, weekOneSpotLengthThreeDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.AduImpressions);
            Assert.AreEqual(4167, weekOneSpotLengthThreeDaypartOne.WeeklyBudget);
            Assert.AreEqual(0.0005000, weekOneSpotLengthThreeDaypartOne.WeeklyCpm);
            Assert.AreEqual(8334000, weekOneSpotLengthThreeDaypartOne.WeeklyImpressions);
            Assert.AreEqual(17, weekOneSpotLengthThreeDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(6.9172199999999995, weekOneSpotLengthThreeDaypartOne.WeeklyRatings);

            var weekTwoSpotLengthOneDaypartOne = weeks.Single(w => w.WeekNumber == 2 && w.SpotLengthId == 1 && w.DaypartCodeId == 1);
            Assert.AreEqual(50, weekTwoSpotLengthOneDaypartOne.PercentageOfWeek);
            Assert.AreEqual(666.66666666666663, weekTwoSpotLengthOneDaypartOne.UnitImpressions);
            Assert.AreEqual(4000000, weekTwoSpotLengthOneDaypartOne.AduImpressions);
            Assert.AreEqual(4166.5, weekTwoSpotLengthOneDaypartOne.WeeklyBudget);
            Assert.AreEqual(0.0005000, weekTwoSpotLengthOneDaypartOne.WeeklyCpm);
            Assert.AreEqual(8333000, weekTwoSpotLengthOneDaypartOne.WeeklyImpressions);
            Assert.AreEqual(17, weekTwoSpotLengthOneDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(6.91639, weekTwoSpotLengthOneDaypartOne.WeeklyRatings);

            var weekTwoSpotLengthThreeDaypartOne = weeks.Single(w => w.WeekNumber == 2 && w.SpotLengthId == 3 && w.DaypartCodeId == 1);
            Assert.AreEqual(50, weekTwoSpotLengthThreeDaypartOne.PercentageOfWeek);
            Assert.AreEqual(666.66666666666663, weekTwoSpotLengthThreeDaypartOne.UnitImpressions);
            Assert.AreEqual(4000000, weekTwoSpotLengthThreeDaypartOne.AduImpressions);
            Assert.AreEqual(4166.5, weekTwoSpotLengthThreeDaypartOne.WeeklyBudget);
            Assert.AreEqual(0.0005000, weekTwoSpotLengthThreeDaypartOne.WeeklyCpm);
            Assert.AreEqual(8333000, weekTwoSpotLengthThreeDaypartOne.WeeklyImpressions);
            Assert.AreEqual(17, weekTwoSpotLengthThreeDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(6.91639, weekTwoSpotLengthThreeDaypartOne.WeeklyRatings);

            var weekThreeSpotLengthOneDaypartOne = weeks.Single(w => w.WeekNumber == 3 && w.SpotLengthId == 1 && w.DaypartCodeId == 1);
            Assert.AreEqual(50, weekThreeSpotLengthOneDaypartOne.PercentageOfWeek);
            Assert.AreEqual(666.66666666666663, weekThreeSpotLengthOneDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.AduImpressions);
            Assert.AreEqual(4166.5, weekThreeSpotLengthOneDaypartOne.WeeklyBudget);
            Assert.AreEqual(0.0005000, weekThreeSpotLengthOneDaypartOne.WeeklyCpm);
            Assert.AreEqual(8333000, weekThreeSpotLengthOneDaypartOne.WeeklyImpressions);
            Assert.AreEqual(17, weekThreeSpotLengthOneDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(6.91639, weekThreeSpotLengthOneDaypartOne.WeeklyRatings);

            var weekThreeSpotLengthThreeDaypartOne = weeks.Single(w => w.WeekNumber == 3 && w.SpotLengthId == 3 && w.DaypartCodeId == 1);
            Assert.AreEqual(50, weekThreeSpotLengthThreeDaypartOne.PercentageOfWeek);
            Assert.AreEqual(666.66666666666663, weekThreeSpotLengthThreeDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.AduImpressions);
            Assert.AreEqual(4166.5, weekThreeSpotLengthThreeDaypartOne.WeeklyBudget);
            Assert.AreEqual(0.0005000, weekThreeSpotLengthThreeDaypartOne.WeeklyCpm);
            Assert.AreEqual(8333000, weekThreeSpotLengthThreeDaypartOne.WeeklyImpressions);
            Assert.AreEqual(17, weekThreeSpotLengthThreeDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(6.91639, weekThreeSpotLengthThreeDaypartOne.WeeklyRatings);
        }

        // This test verifies that the method works for an ADU Only plan.
        [Test]
        public void DistributeGoalsByWeeksEtc_AduOnly()
        {
            // Arrange
            var plan = _GetPlanForDistributeGoal_AduOnly();
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = true;

            // Act
            var weeks = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);

            // Assert
            Assert.AreEqual(6, weeks.Count);

            var weekOneSpotLengthOneDaypartOne = weeks.Single(w => w.WeekNumber == 1 && w.SpotLengthId == 1 && w.DaypartCodeId == 1);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.PercentageOfWeek);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.AduImpressions);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.WeeklyBudget);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.WeeklyCpm);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.WeeklyImpressions);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(0, weekOneSpotLengthOneDaypartOne.WeeklyRatings);

            var weekOneSpotLengthThreeDaypartOne = weeks.Single(w => w.WeekNumber == 1 && w.SpotLengthId == 3 && w.DaypartCodeId == 1);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.PercentageOfWeek);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.AduImpressions);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.WeeklyBudget);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.WeeklyCpm);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.WeeklyImpressions);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(0, weekOneSpotLengthThreeDaypartOne.WeeklyRatings);

            var weekTwoSpotLengthOneDaypartOne = weeks.Single(w => w.WeekNumber == 2 && w.SpotLengthId == 1 && w.DaypartCodeId == 1);
            Assert.AreEqual(0, weekTwoSpotLengthOneDaypartOne.PercentageOfWeek);
            Assert.AreEqual(0, weekTwoSpotLengthOneDaypartOne.UnitImpressions);
            Assert.AreEqual(4000000, weekTwoSpotLengthOneDaypartOne.AduImpressions);
            Assert.AreEqual(0, weekTwoSpotLengthOneDaypartOne.WeeklyBudget);
            Assert.AreEqual(0, weekTwoSpotLengthOneDaypartOne.WeeklyCpm);
            Assert.AreEqual(0, weekTwoSpotLengthOneDaypartOne.WeeklyImpressions);
            Assert.AreEqual(0, weekTwoSpotLengthOneDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(0, weekTwoSpotLengthOneDaypartOne.WeeklyRatings);

            var weekTwoSpotLengthThreeDaypartOne = weeks.Single(w => w.WeekNumber == 2 && w.SpotLengthId == 3 && w.DaypartCodeId == 1);
            Assert.AreEqual(0, weekTwoSpotLengthThreeDaypartOne.PercentageOfWeek);
            Assert.AreEqual(0, weekTwoSpotLengthThreeDaypartOne.UnitImpressions);
            Assert.AreEqual(4000000, weekTwoSpotLengthThreeDaypartOne.AduImpressions);
            Assert.AreEqual(0, weekTwoSpotLengthThreeDaypartOne.WeeklyBudget);
            Assert.AreEqual(0, weekTwoSpotLengthThreeDaypartOne.WeeklyCpm);
            Assert.AreEqual(0, weekTwoSpotLengthThreeDaypartOne.WeeklyImpressions);
            Assert.AreEqual(0, weekTwoSpotLengthThreeDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(0, weekTwoSpotLengthThreeDaypartOne.WeeklyRatings);

            var weekThreeSpotLengthOneDaypartOne = weeks.Single(w => w.WeekNumber == 3 && w.SpotLengthId == 1 && w.DaypartCodeId == 1);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.PercentageOfWeek);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.AduImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.WeeklyBudget);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.WeeklyCpm);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.WeeklyImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(0, weekThreeSpotLengthOneDaypartOne.WeeklyRatings);

            var weekThreeSpotLengthThreeDaypartOne = weeks.Single(w => w.WeekNumber == 3 && w.SpotLengthId == 3 && w.DaypartCodeId == 1);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.PercentageOfWeek);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.UnitImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.AduImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.WeeklyBudget);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.WeeklyCpm);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.WeeklyImpressions);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.WeeklyImpressionsPercentage);
            Assert.AreEqual(0, weekThreeSpotLengthThreeDaypartOne.WeeklyRatings);
        }

        private PlanDto _GetPlanForDistributeGoal_StandardWithAdu()
        {
            var plan = new PlanDto
            {
                IsAduPlan = false,
                AudienceId = 31, // HH
                AudienceType = AudienceTypeEnum.Nielsen,
                AvailableMarkets = new List<PlanAvailableMarketDto>(), // not relevant to these tests
                Budget = 25000,
                CampaignId = 1,
                CoverageGoalPercent = 80,
                CreativeLengths = new List<CreativeLength> 
                { 
                    new CreativeLength {SpotLengthId = 1,Weight = 50},
                    new CreativeLength {SpotLengthId = 3,Weight = 50}
                }, 
                Currency = PlanCurrenciesEnum.Impressions,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 1,
                        DaypartTypeId = DaypartTypeEnum.News,
                        EndTimeSeconds = 35999,
                        PlanDaypartId = 1,
                        Restrictions = new PlanDaypartDto.RestrictionsDto(),
                        StartTimeSeconds = 14400,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 1,
                                VpvhType = VpvhTypeEnum.FourBookAverage
                            }
                        }
                    }
                },
                Equivalized = true,
                FlightDays = new List<int> { 1,2,3,4,5,6,7 },
                // aligns with the test data
                FlightStartDate = new DateTime(202, 11, 23),
                FlightEndDate = new DateTime(2020, 12, 13),
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek,
                HUTBookId = 458,
                ImpressionsPerUnit = 2,
                TargetCPM = 0.5m,
                TargetCPP = 602.4096m,
                TargetImpressions = 50000000,
                TargetRatingPoints = 41.5,
                TargetUniverse = 120600000,
                WeeklyBreakdownTotals = new WeeklyBreakdownTotals
                {
                    TotalActiveDays = 21,
                    TotalAduImpressions = 12000,
                    TotalBudget = 25000,
                    TotalImpressions = 50000,
                    TotalImpressionsPercentage = 100,
                    TotalRatingPoints = 41.5,
                    TotalUnits = 25000
                },
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        AduImpressions = 0.0,
                        CustomName = null,
                        DaypartCodeId = null,
                        DaypartOrganizationId = null,
                        DaypartOrganizationName = null,
                        EndDate = new DateTime(2020, 11, 29),
                        IsLocked = false,
                        IsUpdated = false,
                        MediaWeekId = 883,
                        NumberOfActiveDays = 7,
                        PercentageOfWeek = 100.0,
                        PlanDaypartId = 0,
                        SpotLengthDuration = null,
                        SpotLengthId = null,
                        StartDate = new DateTime(2020, 11, 23),
                        UnitImpressions = 0.0,
                        WeeklyAdu = 0,
                        WeeklyBudget = 8334.0m,
                        WeeklyImpressions = 16668000,
                        WeeklyImpressionsPercentage = 33.0,
                        WeeklyRatings = 13.83,
                        WeeklyUnits = 12501,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        AduImpressions = 12000000,
                        CustomName = null,
                        DaypartCodeId = null,
                        DaypartOrganizationId = null,
                        DaypartOrganizationName = null,
                        EndDate = new DateTime(2020, 12, 06),
                        IsLocked = false,
                        IsUpdated = false,
                        MediaWeekId = 884,
                        NumberOfActiveDays = 7,
                        PercentageOfWeek = 100.0,
                        PlanDaypartId = 0,
                        SpotLengthDuration = null,
                        SpotLengthId = null,
                        StartDate = new DateTime(2020, 11, 30),
                        UnitImpressions = 0.0,
                        WeeklyAdu = 12000000,
                        WeeklyBudget = 8333.0m,
                        WeeklyImpressions = 16666000.0,
                        WeeklyImpressionsPercentage = 33.0,
                        WeeklyRatings = 13.83,
                        WeeklyUnits = 12499.5,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        AduImpressions = 0,
                        CustomName = null,
                        DaypartCodeId = null,
                        DaypartOrganizationId = null,
                        DaypartOrganizationName = null,
                        EndDate = new DateTime(2020, 12, 13),
                        IsLocked = false,
                        IsUpdated = false,
                        MediaWeekId = 885,
                        NumberOfActiveDays = 7,
                        PercentageOfWeek = 100.0,
                        PlanDaypartId = 0,
                        SpotLengthDuration = null,
                        SpotLengthId = null,
                        StartDate = new DateTime(2020, 12, 7),
                        UnitImpressions = 0.0,
                        WeeklyAdu = 0,
                        WeeklyBudget = 8333.0m,
                        WeeklyImpressions = 16666000.0,
                        WeeklyImpressionsPercentage = 33.0,
                        WeeklyRatings = 13.83,
                        WeeklyUnits = 12499.5,
                        WeekNumber = 3
                    }
                }
            };

            return plan;
        }

        private PlanDto _GetPlanForDistributeGoal_AduOnly()
        {
            var plan = new PlanDto
            {
                IsAduPlan = true,
                AudienceId = 31, // HH
                AudienceType = AudienceTypeEnum.Nielsen,
                AvailableMarkets = new List<PlanAvailableMarketDto>(), // not relevant to these tests                
                CampaignId = 1,
                CoverageGoalPercent = 80,
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1,Weight = 50},
                    new CreativeLength {SpotLengthId = 3,Weight = 50}
                },
                Currency = PlanCurrenciesEnum.Impressions,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 1,
                        DaypartTypeId = DaypartTypeEnum.News,
                        EndTimeSeconds = 35999,
                        PlanDaypartId = 1,
                        Restrictions = new PlanDaypartDto.RestrictionsDto(),
                        StartTimeSeconds = 14400,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 1,
                                VpvhType = VpvhTypeEnum.FourBookAverage
                            }
                        }
                    }
                },
                Equivalized = true,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                // aligns with the test data
                FlightStartDate = new DateTime(202, 11, 23),
                FlightEndDate = new DateTime(2020, 12, 13),
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek,
                HUTBookId = 458,
                Budget = 0,
                ImpressionsPerUnit = 2,
                TargetCPM = 0m,
                TargetCPP = 0m,
                TargetImpressions = 0,
                TargetRatingPoints = 0,
                TargetUniverse = 120600000,
                WeeklyBreakdownTotals = new WeeklyBreakdownTotals
                {
                    TotalActiveDays = 21,
                    TotalAduImpressions = 12000,
                    TotalBudget = 25000,
                    TotalImpressions = 50000,
                    TotalImpressionsPercentage = 100,
                    TotalRatingPoints = 41.5,
                    TotalUnits = 25000
                },
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        AduImpressions = 0.0,
                        CustomName = null,
                        DaypartCodeId = null,
                        DaypartOrganizationId = null,
                        DaypartOrganizationName = null,
                        EndDate = new DateTime(2020, 11, 29),
                        IsLocked = false,
                        IsUpdated = false,
                        MediaWeekId = 883,
                        NumberOfActiveDays = 7,
                        PercentageOfWeek = 100.0,
                        PlanDaypartId = 0,
                        SpotLengthDuration = null,
                        SpotLengthId = null,
                        StartDate = new DateTime(2020, 11, 23),
                        UnitImpressions = 0.0,
                        WeeklyAdu = 0,
                        WeeklyBudget = 0m,
                        WeeklyImpressions = 0,
                        WeeklyImpressionsPercentage = 0,
                        WeeklyRatings = 0,
                        WeeklyUnits = 0,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        AduImpressions = 12000000,
                        CustomName = null,
                        DaypartCodeId = null,
                        DaypartOrganizationId = null,
                        DaypartOrganizationName = null,
                        EndDate = new DateTime(2020, 12, 06),
                        IsLocked = false,
                        IsUpdated = false,
                        MediaWeekId = 884,
                        NumberOfActiveDays = 7,
                        PercentageOfWeek = 100.0,
                        PlanDaypartId = 0,
                        SpotLengthDuration = null,
                        SpotLengthId = null,
                        StartDate = new DateTime(2020, 11, 30),
                        UnitImpressions = 0.0,
                        WeeklyAdu = 12000000,
                        WeeklyBudget = 0m,
                        WeeklyImpressions = 0,
                        WeeklyImpressionsPercentage = 0,
                        WeeklyRatings = 0,
                        WeeklyUnits = 0,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        AduImpressions = 0,
                        CustomName = null,
                        DaypartCodeId = null,
                        DaypartOrganizationId = null,
                        DaypartOrganizationName = null,
                        EndDate = new DateTime(2020, 12, 13),
                        IsLocked = false,
                        IsUpdated = false,
                        MediaWeekId = 885,
                        NumberOfActiveDays = 7,
                        PercentageOfWeek = 100.0,
                        PlanDaypartId = 0,
                        SpotLengthDuration = null,
                        SpotLengthId = null,
                        StartDate = new DateTime(2020, 12, 7),
                        UnitImpressions = 0.0,
                        WeeklyAdu = 0,
                        WeeklyBudget = 0m,
                        WeeklyImpressions = 0,
                        WeeklyImpressionsPercentage = 0,
                        WeeklyRatings = 0,
                        WeeklyUnits = 0,
                        WeekNumber = 3
                    }
                }
            };

            return plan;
        }

        // This test verifies that the calculations work given one spot length.
        // It considers delivery modifiers.
        [Test]
        [TestCase(true, true, 1, 12000)]
        [TestCase(true, false, 1, 12000)]
        [TestCase(true, true, 2, 6000)]
        [TestCase(true, false, 2, 12000)]
        [TestCase(false, true, 1, 6000)]
        [TestCase(false, false, 1, 6000)]
        [TestCase(false, true, 2, 3000)]
        [TestCase(false, false, 2, 6000)]
        public void CalculateADU_SingleSpotLength(bool aduv2Enabled, bool equivalized, int spotLengthId,
                int expectedResult)
        {
            var impressionsPerUnit = 2;
            var aduImpressions = 12000;

            // Arrange
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = aduv2Enabled;

            // Act
            var result = _WeeklyBreakdownEngine._CalculateADU(impressionsPerUnit, aduImpressions, equivalized, spotLengthId, creativeLengths:null);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        // This test verifies that the calculations work given many spot lengths.
        // It considers delivery modifiers.
        [Test]
        [TestCase(true, true, 18000)]
        [TestCase(true, false, 12000)]
        [TestCase(false, true, 9000)]
        [TestCase(false, false, 6000)]
        public void CalucalateADU_MultipleSpotLength(bool aduv2Enabled, bool equivalized, int expectedResult)
        {
            // Arrange
            var impressionsPerUnit = 2;
            var aduImpressions = 12000;
            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength { SpotLengthId = 1, Weight = 50 },
                new CreativeLength { SpotLengthId = 3, Weight = 50 }
            };
            
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = aduv2Enabled;

            // Act
            var result = _WeeklyBreakdownEngine._CalculateADU(impressionsPerUnit, aduImpressions, equivalized, spotLengthId: null, creativeLengths);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}
