using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
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
using Services.Broadcast.Entities.DTO.Program;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using CreativeLength = Services.Broadcast.Entities.CreativeLength;

namespace Services.Broadcast.IntegrationTests.UnitTests.PlanServices
{
    [TestFixture]
    [NUnit.Framework.Category("short_running")]
    public class WeeklyBreakdownEngineUnitTests
    {
        private WeeklyBreakdownEngine _WeeklyBreakdownEngine;
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

            _WeeklyBreakdownEngine = new WeeklyBreakdownEngine(
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
        public void PlanWeeklyGoalBreakdown_AddHiatusAfterClear()
        {
            //Arrange
            var request = new WeeklyBreakdownRequest
            {
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightStartDate = new DateTime(2021, 12, 6),
                FlightEndDate = new DateTime(2021, 12, 19),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2021,12,15)
                },
                TotalImpressions = 22000,
                TotalRatings = 18.2,
                TotalBudget = 50000,
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
                Equivalized = true,
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 937,
                        StartDate = new DateTime(2021, 12,6),
                        EndDate = new DateTime(2021,12,12),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0,
                        WeeklyImpressionsPercentage = 0,
                        WeeklyRatings = 0,
                        WeeklyBudget = 0,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        SpotLengthId = null,
                        SpotLengthDuration = null,
                        DaypartCodeId = null,
                        PercentageOfWeek = null,
                        IsUpdated = false,
                        UnitImpressions = 0,
                        WeeklyUnits = 0,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 938,
                        StartDate = new DateTime(2021, 12,13),
                        EndDate = new DateTime(2021,12,19),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0,
                        WeeklyImpressionsPercentage = 0,
                        WeeklyRatings = 0,
                        WeeklyBudget = 0,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        SpotLengthId = null,
                        SpotLengthDuration = null,
                        DaypartCodeId = null,
                        PercentageOfWeek = null,
                        IsUpdated = false,
                        UnitImpressions = 0,
                        WeeklyUnits = 0,
                        IsLocked = false
                    }
                },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength{SpotLengthId = 1, Weight = 100}
                },
                ImpressionsPerUnit = 10,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 12,
                        DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                        StartTimeSeconds = 32400,
                        IsStartTimeModified = false,
                        EndTimeSeconds = 57600,
                        IsEndTimeModified = false,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 1,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2021,12,1)
                            }
                        },
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                ShowTypes = new List<LookupDto>()
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Genres = new List<LookupDto> {new LookupDto{Id = 33, Display = "News"}}
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Programs = new List<ProgramDto>()
                            },
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Affiliates = new List<LookupDto>()
                            }
                        }

                    }
                },
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
            };

            var mockedListMediaWeeksByFlight = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek
                {
                    Id = 937,
                    IsHiatus = false,
                    MediaMonthId = 483,
                    Month = 12,
                    MonthEndDate = new DateTime(2021, 12, 26),
                    MonthStartDate = new DateTime(2021,11,29),
                    Week = 2,
                    WeekEndDate = new DateTime(2021,12,12),
                    WeekStartDate = new DateTime(2021,12,6),
                    Year = 2021
                },
                new DisplayMediaWeek
                {
                    Id = 938,
                    IsHiatus = false,
                    MediaMonthId = 483,
                    Month = 12,
                    MonthEndDate = new DateTime(2021, 12, 26),
                    MonthStartDate = new DateTime(2021,11,29),
                    Week = 3,
                    WeekEndDate = new DateTime(2021,12,19),
                    WeekStartDate = new DateTime(2021,12,13),
                    Year = 2021
                }
            };

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
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
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
                    new CreativeLength {SpotLengthId = 1,Weight = 0}
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
                .Returns(new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 100 } });

            //Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_WithWeeks_DaypartCodeIdisZero_SingleDaypartisAdded()
        {
            //Arrange
            var request = new WeeklyBreakdownRequest
            {
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightStartDate = new DateTime(2020, 09, 28, 0, 0, 0),
                FlightEndDate = new DateTime(2020, 12, 27, 23, 59, 59),
                FlightHiatusDays = new List<DateTime>(),             
                TotalImpressions = 70000,
                TotalRatings = 58,
                TotalBudget = 1070000,
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart,
                Equivalized = true,
                Weeks = new List<WeeklyBreakdownWeek> {
                    new WeeklyBreakdownWeek
                    {
                        IsLocked=false,
                        DaypartCodeId=0,
                        StartDate=new DateTime(2020, 9, 28),
                        EndDate=new DateTime(2020, 10, 4),
                        WeekNumber=1,
                        MediaWeekId=875,
                        NumberOfActiveDays = 7
                    },
                    new WeeklyBreakdownWeek
                    {
                        IsLocked=false,
                        DaypartCodeId=0,
                        StartDate=new DateTime(2020, 10, 5),
                        EndDate=new DateTime(2020, 10, 11),
                        WeekNumber=2,
                        MediaWeekId=876,
                        NumberOfActiveDays = 7
                    },
                    new WeeklyBreakdownWeek
                    {
                        IsLocked=false,
                        DaypartCodeId=0,
                        StartDate=new DateTime(2020, 10, 12),
                        EndDate=new DateTime(2020, 10, 18),
                        WeekNumber=3,
                        MediaWeekId=877,
                        NumberOfActiveDays = 7
                    },
                    new WeeklyBreakdownWeek
                    {
                        IsLocked=false,
                        DaypartCodeId=0,
                        StartDate=new DateTime(2020, 10, 19),
                        EndDate=new DateTime(2020, 10, 25),
                        WeekNumber=4,
                        MediaWeekId=878,
                        NumberOfActiveDays = 7
                    },
                },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1,Weight = 60}
                },
                ImpressionsPerUnit = 1,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypart { DaypartCodeId = 12,WeekdaysWeighting = 70, WeekendWeighting = 30}
                },
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            };
            var mockedListMediaWeeksByFlight = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek { Id = 875, Week = 1, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 9, 28), WeekEndDate = new DateTime(2020, 10, 4), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
                new DisplayMediaWeek { Id = 876, Week = 2, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 10, 5), WeekEndDate = new DateTime(2020, 10, 11), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
                new DisplayMediaWeek { Id = 877, Week = 3, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 10, 12), WeekEndDate = new DateTime(2020, 10, 18), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
                new DisplayMediaWeek { Id = 878, Week = 4, MediaMonthId = 469, Year = 2020, Month = 10, WeekStartDate = new DateTime(2020, 10, 19), WeekEndDate = new DateTime(2020, 10, 25), MonthStartDate = new DateTime(2020, 9, 28), MonthEndDate = new DateTime(2020, 10, 25) },
            };

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<List<CreativeLength>>()))
                .Returns(new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 100 } });

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
                IsUpdated = true,               
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
        public void PlanWeeklyGoalBreakdown_ClearAll_Success_Test()
        {
            //Arrange
            var request = _GetWeeklyBreakDownRequest();
            request.DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            //Act
            var result = _WeeklyBreakdownEngine.ClearPlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PlanWeeklyGoalBreakdown_ClearAll_When_SomeWeeks_AreLocked_Test()
        {
            //Arrange
            var sixtySpotWeight = 40;
            var fifteenSpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var sixtySpotId = spotLengthsDict[60];
            var fifteenSpotId = spotLengthsDict[15];
            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);
            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = sixtySpotId, Weight = sixtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
            };
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliers())
                .Returns(_SpotLengthMultiplier);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object, featureToggleHelper);
            var request = new WeeklyBreakdownRequest
            {   CreativeLengths= creativeLengths,
                FlightStartDate = new DateTime(2020, 2, 24),
                FlightEndDate = new DateTime(2020, 3, 29),
                FlightHiatusDays = new List<DateTime>(),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypart { DaypartCodeId = 0}
                },
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
                        WeeklyUnits = 1,
                        IsLocked=true

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
                        WeeklyUnits = 1,
                        IsLocked=true
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
                        WeeklyUnits = 1,
                        IsLocked=false
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
                        WeeklyUnits = 1,
                        IsLocked=false
                    }
                },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 }
            };
            request.DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            //Act
            var result = weeklyBreakdownEngine.ClearPlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        /// <summary>
        /// The previous call 
        /// - resulted in decimal weekly percentages 
        /// - summed to a total percentage of 100
        /// - then stripped the decimals by flooring it (8.67 becomes 8) for display
        /// 
        /// Then this call was summing to 99 because it's missing those decimals
        /// from the original total summation.
        /// 
        /// This test verifies that during the Clear we can now resolve to an accurate
        /// total percentage of 100.
        /// 
        /// This should not change those locked weekly precentages.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PlanWeeklyGoalBreakdown_ClearAll_WithLocked_DecimalPercentageSumsTo100()
        {
            //Arrange
            var sixtySpotWeight = 40;
            var fifteenSpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var sixtySpotId = spotLengthsDict[60];
            var fifteenSpotId = spotLengthsDict[15];
            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);
            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = sixtySpotId, Weight = sixtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
            };
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliers())
                .Returns(_SpotLengthMultiplier);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object, featureToggleHelper);
            var request = new WeeklyBreakdownRequest
            {
                CreativeLengths = creativeLengths,
                FlightStartDate = new DateTime(2020, 2, 24),
                FlightEndDate = new DateTime(2020, 3, 8),
                FlightHiatusDays = new List<DateTime>(),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypart { DaypartCodeId = 0}
                },
                TotalImpressions = 10000,
                ImpressionsPerUnit = 5,
                TotalRatings = 28.041170045861335,
                TotalBudget = 10000,
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

                        WeeklyImpressions = 866,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 5.608234009172268,
                        WeeklyBudget = 60000,
                        WeeklyAdu = 0,
                        WeeklyUnits = 1,
                        IsLocked=true

                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 845,
                        StartDate = new DateTime(2020, 3, 2),
                        EndDate = new DateTime(2020, 3, 8),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",

                        WeeklyImpressions = 9134,
                        WeeklyImpressionsPercentage = 91,
                        WeeklyRatings = 5.608234009172268,
                        WeeklyBudget = 60000,
                        WeeklyAdu = 0,
                        WeeklyUnits = 1,
                        IsLocked=true
                    }                    
                },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 }
            };
            request.DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery;

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            //Act
            var result = weeklyBreakdownEngine.ClearPlanWeeklyGoalBreakdown(request);

            //Assert
            Assert.AreEqual(100, result.TotalImpressionsPercentage);
            // verify the weekly impressions didn't change.
            Assert.AreEqual(866, result.Weeks[0].WeeklyImpressions);
            Assert.AreEqual(9134, result.Weeks[1].WeeklyImpressions);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownByWeek()
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliers())
                .Returns(_SpotLengthMultiplier);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object, featureToggleHelper);
            var result = weeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(_WeeklyBreakdown, _ImpressionsPerUnit, _CreativeLengths);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownByWeekBySpotLength()
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliers())
                .Returns(_SpotLengthMultiplier);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object, featureToggleHelper);
            var result = weeklyBreakdownEngine.GroupWeeklyBreakdownByWeekBySpotLength(_WeeklyBreakdown, _ImpressionsPerUnit, true);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupWeeklyBreakdownByWeekByDaypart()
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);

            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            spotLengthEngine.Setup(x => x.GetDeliveryMultipliers())
                .Returns(_SpotLengthMultiplier);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);
            var weeklyBreakdownEngine = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                spotLengthEngine.Object,
                dataRepositoryFactory.Object, featureToggleHelper);

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

        /// <summary>
        /// Days are Monday(1) through Sunday(7)
        /// </summary>
        [Test]
        [TestCase(1, 7, true, true, 7, "M-Su")]
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
            var allDayparts = DaypartsTestData.GetAllStandardDaypartsWithFullData();

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
            var result = ((WeeklyBreakdownEngine)_WeeklyBreakdownEngine)._CalculateActiveDays(weekStartDate, weekEndDate, flightDays, hiatusDays, dayparts, out resultActiveDayString);

            // Assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedResultActiveDayString, resultActiveDayString);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekByAdLengthDeliveryType_OnPlanSave()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.IsAduEnabled = true;
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2 });
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 3 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 50,
                WeeklyImpressionsPercentage = 50,
                WeeklyRatings = 25,
                WeeklyBudget = 50,
                WeeklyAdu = 2,
                SpotLengthId = 1,
                WeeklyUnits = 2.5
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 2,
                WeeklyUnits = 1.25
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 3,
                WeeklyUnits = 0
            });

            // Act
            var results = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan, 110, 120);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekByAdLengthDeliveryType_OnPlanSave_UnEquivalized()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.Equivalized = false;
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.IsAduEnabled = true;
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2 });
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 3 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 50,
                WeeklyImpressionsPercentage = 50,
                WeeklyRatings = 25,
                WeeklyBudget = 50,
                WeeklyAdu = 2,
                SpotLengthId = 1,
                WeeklyUnits = 2.5
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 2,
                WeeklyUnits = 1.25
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 3,
                WeeklyUnits = 0
            });

            // Act
            var results = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan, 110, 120);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength;
            plan.TargetImpressions *= 1000;
            plan.WeeklyBreakdownWeeks.AddRange(_GetWeeklyBreakdownWeeks());

            // Act
            var result = _WeeklyBreakdownEngine.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType_UnEquivalized()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.Equivalized = false;
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength;
            plan.TargetImpressions *= 1000;

            plan.WeeklyBreakdownWeeks.AddRange(_GetWeeklyBreakdownWeeks());

            // Act
            var result = _WeeklyBreakdownEngine.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekDeliveryType_OnPlanSave()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.IsAduEnabled = true;
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2, Weight = 10 });
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 3, Weight = 40 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 100,
                WeeklyImpressionsPercentage = 100,
                WeeklyRatings = 50,
                WeeklyBudget = 100,
                WeeklyAdu = 6,
                WeeklyUnits = 5
            });

            // Act
            var results = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekByDaypartDeliveryType_OnPlanSave()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.FlightStartDate = new DateTime(2020, 2, 24);
            plan.FlightEndDate = new DateTime(2020, 3, 29);
            plan.FlightHiatusDays = new List<DateTime> { };
            plan.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2, Weight = 50 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.TargetImpressions = 5000;
            plan.ImpressionsPerUnit = 100;
            plan.TargetRatingPoints = 4.1;
            plan.TargetCPM = 0.1m;
            plan.Budget = 500;
            plan.WeeklyBreakdownTotals = new WeeklyBreakdownTotals
            {
                TotalActiveDays = 35,
                TotalBudget = 500,
                TotalImpressions = 5000,
                TotalImpressionsPercentage = 100,
                TotalRatingPoints = 4.1,
                TotalUnits = 50
            };
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 5
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 5
                    },
                };

            // Act
            var results = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        #region AdLengthTests

        private IWeeklyBreakdownEngine GetWeeklyBreakdownEngineForAdLengthTests(List<CreativeLength> creativeLengths, DateTime flightStartDate, DateTime flightEndDate)
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);

            _SpotLengthEngineMock.Setup(x => x.GetDeliveryMultipliers())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);

            _SpotLengthEngineMock.Setup(x => x.GetCostMultipliers(false))
                .Returns(SpotLengthTestData.GetCostMultipliersBySpotLengthId(applyInventoryPremium: false));

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<DisplayMediaWeek> { new DisplayMediaWeek { WeekStartDate = flightStartDate, WeekEndDate = flightEndDate } });

            _CreativeLengthEngineMock.Setup(s => s.DistributeWeight(It.IsAny<IEnumerable<CreativeLength>>()))
                .Returns(creativeLengths);

            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);

            var testClass = new WeeklyBreakdownEngine(
                _PlanValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _CreativeLengthEngineMock.Object,
                _SpotLengthEngineMock.Object,
                dataRepositoryFactory.Object, featureToggleHelper);

            return testClass;
        }

        private WeeklyBreakdownRequest GetBreakdownRequestForAdLengthTests(
            bool equivalized, List<CreativeLength> creativeLengths,
            DateTime flightStartDate, DateTime flightEndDate)
        {
            var request = new WeeklyBreakdownRequest
            {
                Equivalized = equivalized,
                CreativeLengths = creativeLengths,
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength,
                TotalBudget = 500000,
                TotalImpressions = 20000,
                ImpressionsPerUnit = 10,
                TotalRatings = 16.6,
                FlightStartDate = flightStartDate,
                FlightEndDate = flightEndDate,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>(),
                Weeks = new List<WeeklyBreakdownWeek>(),
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0 }
                },
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            };
            return request;
        }

        [Test]
        public void CalculateBreakdown_ByAdLength_TwoEq()
        {
            // Arrange
            var equivalized = true;
            var thirtySpotWeight = 40;
            var fifteenSpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var thirtySpotId = spotLengthsDict[30];
            var fifteenSpotId = spotLengthsDict[15];
            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = thirtySpotId, Weight = thirtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
            };

            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);

            var request = GetBreakdownRequestForAdLengthTests(equivalized, creativeLengths, flightStartDate, flightEndDate);
            var testClass = GetWeeklyBreakdownEngineForAdLengthTests(creativeLengths, flightStartDate, flightEndDate);

            // Act
            var result = testClass.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Assert.AreEqual(2, result.Weeks.Count);
            var thirtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == thirtySpotId);
            Assert.AreEqual(8000, thirtyRow.WeeklyImpressions);
            Assert.AreEqual(800, thirtyRow.WeeklyUnits);
            Assert.AreEqual(200000, thirtyRow.WeeklyBudget);

            var fifteenRow = result.Weeks.Single(w => w.SpotLengthId.Value == fifteenSpotId);
            Assert.AreEqual(12000, fifteenRow.WeeklyImpressions);
            Assert.AreEqual(2400, fifteenRow.WeeklyUnits);
            Assert.AreEqual(300000, fifteenRow.WeeklyBudget);
        }

        [Test]
        public void CalculateBreakdown_ByAdLength_TwoEqNo30()
        {
            // Arrange
            var equivalized = true;
            var sixtySpotWeight = 40;
            var fifteenSpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var sixtySpotId = spotLengthsDict[60];
            var fifteenSpotId = spotLengthsDict[15];
            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);
            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = sixtySpotId, Weight = sixtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
            };

            var request = GetBreakdownRequestForAdLengthTests(equivalized, creativeLengths, flightStartDate, flightEndDate);
            var testClass = GetWeeklyBreakdownEngineForAdLengthTests(creativeLengths, flightStartDate, flightEndDate);

            // Act
            var result = testClass.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Assert.AreEqual(2, result.Weeks.Count);
            var sixtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == sixtySpotId);
            Assert.AreEqual(8000, sixtyRow.WeeklyImpressions);
            Assert.AreEqual(400, sixtyRow.WeeklyUnits);
            Assert.AreEqual(200000, sixtyRow.WeeklyBudget);

            var fifteenRow = result.Weeks.Single(w => w.SpotLengthId.Value == fifteenSpotId);
            Assert.AreEqual(12000, fifteenRow.WeeklyImpressions);
            Assert.AreEqual(2400, fifteenRow.WeeklyUnits);
            Assert.AreEqual(300000, fifteenRow.WeeklyBudget);
        }

        [Test]
        public void CalculateBreakdown_ByAdLength_ThreeEq()
        {
            // Arrange
            var equivalized = true;
            var thirtySpotWeight = 20;
            var fifteenSpotWeight = 20;
            var sixtySpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var thirtySpotId = spotLengthsDict[30];
            var fifteenSpotId = spotLengthsDict[15];
            var sixtySpotId = spotLengthsDict[60];

            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = thirtySpotId, Weight = thirtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
                new CreativeLength {SpotLengthId = sixtySpotId, Weight = sixtySpotWeight},
            };

            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);

            var request = GetBreakdownRequestForAdLengthTests(equivalized, creativeLengths, flightStartDate, flightEndDate);
            var testClass = GetWeeklyBreakdownEngineForAdLengthTests(creativeLengths, flightStartDate, flightEndDate);

            // Act
            var result = testClass.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Assert.AreEqual(3, result.Weeks.Count);

            var thirtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == thirtySpotId);
            Assert.AreEqual(4000, thirtyRow.WeeklyImpressions);
            Assert.AreEqual(400, thirtyRow.WeeklyUnits);
            Assert.AreEqual(100000, thirtyRow.WeeklyBudget);

            var fifteenRow = result.Weeks.Single(w => w.SpotLengthId.Value == fifteenSpotId);
            Assert.AreEqual(4000, fifteenRow.WeeklyImpressions);
            Assert.AreEqual(800, fifteenRow.WeeklyUnits);
            Assert.AreEqual(100000, fifteenRow.WeeklyBudget);

            var sixtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == sixtySpotId);
            Assert.AreEqual(12000, sixtyRow.WeeklyImpressions);
            Assert.AreEqual(600, sixtyRow.WeeklyUnits);
            Assert.AreEqual(300000, sixtyRow.WeeklyBudget);
        }

        [Test]
        public void CalculateBreakdown_ByAdLength_TwoUnEq()
        {
            // Arrange
            var equivalized = false;
            var thirtySpotWeight = 40;
            var fifteenSpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var thirtySpotId = spotLengthsDict[30];
            var fifteenSpotId = spotLengthsDict[15];
            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = thirtySpotId, Weight = thirtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
            };

            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);

            var request = GetBreakdownRequestForAdLengthTests(equivalized, creativeLengths, flightStartDate, flightEndDate);
            var testClass = GetWeeklyBreakdownEngineForAdLengthTests(creativeLengths, flightStartDate, flightEndDate);

            // Act
            var result = testClass.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Assert.AreEqual(2, result.Weeks.Count);
            var thirtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == thirtySpotId);
            Assert.AreEqual(8000, thirtyRow.WeeklyImpressions);
            Assert.AreEqual(800, thirtyRow.WeeklyUnits);
            Assert.AreEqual(285714.29, Math.Round(thirtyRow.WeeklyBudget, 2));

            var fifteenRow = result.Weeks.Single(w => w.SpotLengthId.Value == fifteenSpotId);
            Assert.AreEqual(12000, fifteenRow.WeeklyImpressions);
            Assert.AreEqual(1200, fifteenRow.WeeklyUnits);
            Assert.AreEqual(214285.71, Math.Round(fifteenRow.WeeklyBudget, 2));
        }

        [Test]
        public void CalculateBreakdown_ByAdLength_TwoUnEqNo30()
        {
            // Arrange
            var equivalized = false;
            var sixtySpotWeight = 40;
            var fifteenSpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var sixtySpotId = spotLengthsDict[60];
            var fifteenSpotId = spotLengthsDict[15];
            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);
            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = sixtySpotId, Weight = sixtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
            };

            var request = GetBreakdownRequestForAdLengthTests(equivalized, creativeLengths, flightStartDate, flightEndDate);
            var testClass = GetWeeklyBreakdownEngineForAdLengthTests(creativeLengths, flightStartDate, flightEndDate);

            // Act
            var result = testClass.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Assert.AreEqual(2, result.Weeks.Count);
            var sixtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == sixtySpotId);
            Assert.AreEqual(8000, sixtyRow.WeeklyImpressions);
            Assert.AreEqual(800, sixtyRow.WeeklyUnits);
            Assert.AreEqual(363636.36, Math.Round(sixtyRow.WeeklyBudget, 2));

            var fifteenRow = result.Weeks.Single(w => w.SpotLengthId.Value == fifteenSpotId);
            Assert.AreEqual(12000, fifteenRow.WeeklyImpressions);
            Assert.AreEqual(1200, fifteenRow.WeeklyUnits);
            Assert.AreEqual(136363.64, Math.Round(fifteenRow.WeeklyBudget, 2));
        }

        [Test]
        public void CalculateBreakdown_ByAdLength_ThreeUnEq()
        {
            // Arrange
            var equivalized = false;
            var thirtySpotWeight = 20;
            var fifteenSpotWeight = 20;
            var sixtySpotWeight = 60;

            var spotLengthsDict = SpotLengthTestData.GetSpotLengthIdsByDuration();
            var thirtySpotId = spotLengthsDict[30];
            var fifteenSpotId = spotLengthsDict[15];
            var sixtySpotId = spotLengthsDict[60];

            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength {SpotLengthId = thirtySpotId, Weight = thirtySpotWeight},
                new CreativeLength {SpotLengthId = fifteenSpotId, Weight = fifteenSpotWeight},
                new CreativeLength {SpotLengthId = sixtySpotId, Weight = sixtySpotWeight},
            };

            var flightStartDate = new DateTime(2021, 08, 02);
            var flightEndDate = new DateTime(2021, 08, 08);

            var request = GetBreakdownRequestForAdLengthTests(equivalized, creativeLengths, flightStartDate, flightEndDate);
            var testClass = GetWeeklyBreakdownEngineForAdLengthTests(creativeLengths, flightStartDate, flightEndDate);

            // Act
            var result = testClass.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Assert.AreEqual(3, result.Weeks.Count);

            var thirtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == thirtySpotId);
            Assert.AreEqual(4000, thirtyRow.WeeklyImpressions);
            Assert.AreEqual(400, thirtyRow.WeeklyUnits);
            Assert.AreEqual(66666.67, Math.Round(thirtyRow.WeeklyBudget, 2));

            var fifteenRow = result.Weeks.Single(w => w.SpotLengthId.Value == fifteenSpotId);
            Assert.AreEqual(4000, fifteenRow.WeeklyImpressions);
            Assert.AreEqual(400, fifteenRow.WeeklyUnits);
            Assert.AreEqual(33333.33, Math.Round(fifteenRow.WeeklyBudget, 2));

            var sixtyRow = result.Weeks.Single(w => w.SpotLengthId.Value == sixtySpotId);
            Assert.AreEqual(12000, sixtyRow.WeeklyImpressions);
            Assert.AreEqual(1200, sixtyRow.WeeklyUnits);
            Assert.AreEqual(400000.00, Math.Round(sixtyRow.WeeklyBudget, 2));
        }

        #endregion AdLengthTests

        private static List<WeeklyBreakdownWeek> _GetWeeklyBreakdownWeeks()
        {
            List<WeeklyBreakdownWeek> result = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 600000,
                    DaypartCodeId = 2,
                    PercentageOfWeek = 30,
                    SpotLengthId = 1,
                    WeeklyBudget = 30,
                    WeeklyImpressions = 30000,
                    WeeklyImpressionsPercentage = 30,
                    WeeklyRatings = 15,
                    WeeklyUnits = 1500,
                    UnitImpressions = 20
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 400000,
                    DaypartCodeId = 11,
                    PercentageOfWeek = 20,
                    SpotLengthId = 1,
                    WeeklyBudget = 20,
                    WeeklyImpressions = 20000,
                    WeeklyImpressionsPercentage = 20,
                    WeeklyRatings = 10,
                    WeeklyUnits = 1000,
                    UnitImpressions = 20
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 600000,
                    DaypartCodeId = 2,
                    PercentageOfWeek = 15,
                    SpotLengthId = 2,
                    WeeklyBudget = 15,
                    WeeklyImpressions = 15000,
                    WeeklyImpressionsPercentage = 15,
                    WeeklyRatings = 7.5,
                    WeeklyUnits = 750,
                    UnitImpressions = 20
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 400000,
                    DaypartCodeId = 11,
                    PercentageOfWeek = 10,
                    SpotLengthId = 2,
                    WeeklyBudget = 10,
                    WeeklyImpressions = 10000,
                    WeeklyImpressionsPercentage = 10,
                    WeeklyRatings = 5,
                    WeeklyUnits = 500,
                    UnitImpressions = 20
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 600000,
                    DaypartCodeId = 2,
                    PercentageOfWeek = 15,
                    SpotLengthId = 3,
                    WeeklyBudget = 15,
                    WeeklyImpressions = 15000,
                    WeeklyImpressionsPercentage = 15,
                    WeeklyRatings = 7.5,
                    WeeklyUnits = 750,
                    UnitImpressions = 20
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 400000,
                    DaypartCodeId = 11,
                    PercentageOfWeek = 10,
                    SpotLengthId = 3,
                    WeeklyBudget = 10,
                    WeeklyImpressions = 10000,
                    WeeklyImpressionsPercentage = 10,
                    WeeklyRatings = 5,
                    WeeklyUnits = 500,
                    UnitImpressions = 20
                }
            };
            return result;
        }

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Internal sample notes",
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,4,15)
                },
                AudienceId = 31,        //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 436,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                TargetCPM = 12m,
                TargetImpressions = 100d,
                CoverageGoalPercent = 80.5,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn"},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York"}
                },
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto {MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto {MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 2000,
                        WeightingGoalPercent = 28.0,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.5,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 11,
                        StartTimeSeconds = 1500,
                        EndTimeSeconds = 2788,
                        WeightingGoalPercent = 33.2,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.5,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        }
                    }
                },
                Vpvh = 0.234543,
                TargetRatingPoints = 50,
                TargetCPP = 50,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                ImpressionsPerUnit = 20
            };
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
            { 1,1}, { 2, 2 }, { 3, 3 }
        };

        private readonly double _ImpressionsPerUnit = 250;

        private WeeklyBreakdownRequest _GetWeeklyBreakdownRequest_CustomByWeekByDaypart()
        {
            return new WeeklyBreakdownRequest
            {
                FlightStartDate = new DateTime(2020, 2, 25),
                FlightEndDate = new DateTime(2020, 3, 29),
                FlightDays = new List<int> { 1, 2, 3, 4, 5 },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1, Weight = 60 }
                },
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
            // spotLengthId should be null as this is not for ByAdLengthDeliveryType
           
            return new WeeklyBreakdownRequest
            {
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1, Weight = 60}
                },
                FlightStartDate = new DateTime(2020, 2, 24),
                FlightEndDate = new DateTime(2020, 3, 29),
                FlightHiatusDays = new List<DateTime>(),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypart { DaypartCodeId = 0}
                },
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
                        WeeklyUnits = 1,
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
                        WeeklyUnits = 1,
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
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypart { DaypartCodeId = 0}
                },
                TotalImpressions = 20,
                ImpressionsPerUnit = 10,
                TotalRatings = 0.00928868473742818,
                TotalBudget = 400,
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                Weeks = new List<WeeklyBreakdownWeek>(),
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1, Weight = 60 }
                },
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_ForEvenDelivery_WithIsLocked()
        {
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
                WeeklyUnits = 1,
                IsLocked = true
            });

            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;
            request.DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery;

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
        public void CalculatePlanWeeklyGoalBreakdown_ForCustomByWeek_WithIsLocked()
        {
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
                WeeklyUnits = 1,
                IsLocked = true
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
        public void InitialPlanWeeklyGoalBreakdown_ForEvenDelivery_WithIsLocked()
        {
            var request = _GetWeeklyBreakDownRequest();
            request.Weeks.Clear();
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;
            request.DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery;

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
        public void InitialPlanWeeklyGoalBreakdown_ForCustomByWeek_WithIsLocked()
        {
            var request = _GetWeeklyBreakDownRequest();
            request.Weeks.Clear();
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
        public void CalculatePlanWeeklyGoalBreakdown_ModifyDayparts_WithIsLocked()
        {
            var request = _GetWeeklyBreakDownRequestForDayparts();
            request.Dayparts.RemoveAt(0);
            request.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1 });

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));

        }
        private WeeklyBreakdownRequest _GetWeeklyBreakDownRequestForDayparts()
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
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1, Weight = 60}
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
                        WeekNumber = 1,
                        IsLocked = true
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
                        WeekNumber = 1,
                        IsLocked = true
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
                        WeekNumber = 2,
                        IsLocked = false
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
                        WeekNumber = 2,
                        IsLocked = false
                    }
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekByAdLengthDeliveryType_OnPlanSave_WithIsLocked()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.IsAduEnabled = true;
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2 });
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 3 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 50,
                WeeklyImpressionsPercentage = 50,
                WeeklyRatings = 25,
                WeeklyBudget = 50,
                WeeklyAdu = 2,
                SpotLengthId = 1,
                WeeklyUnits = 2.5,
                IsLocked = false
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 2,
                WeeklyUnits = 1.25,
                IsLocked = false
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 3,
                WeeklyUnits = 0,
                IsLocked = true
            });

            // Act
            var results = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan, 110, 120);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekDeliveryType_OnPlanSave_WithIsLocked()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.IsAduEnabled = true;
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2, Weight = 10 });
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 3, Weight = 40 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 100,
                WeeklyImpressionsPercentage = 100,
                WeeklyRatings = 50,
                WeeklyBudget = 100,
                WeeklyAdu = 6,
                WeeklyUnits = 5,
                IsLocked = false
            });

            // Act
            var results = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekByDaypartDeliveryType_OnPlanSave_WithIsLocked()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.FlightStartDate = new DateTime(2020, 2, 24);
            plan.FlightEndDate = new DateTime(2020, 3, 29);
            plan.FlightHiatusDays = new List<DateTime> { };
            plan.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2, Weight = 50 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.TargetImpressions = 5000;
            plan.ImpressionsPerUnit = 100;
            plan.TargetRatingPoints = 4.1;
            plan.TargetCPM = 0.1m;
            plan.Budget = 500;
            plan.WeeklyBreakdownTotals = new WeeklyBreakdownTotals
            {
                TotalActiveDays = 35,
                TotalBudget = 500,
                TotalImpressions = 5000,
                TotalImpressionsPercentage = 100,
                TotalRatingPoints = 4.1,
                TotalUnits = 50
            };
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 1,
                        IsLocked = true
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 1,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 2,
                        IsLocked = true
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 2,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 3,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 3,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 4,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 4,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 5,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 5,
                        IsLocked = false
                    },
                };

            // Act
            var results = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void WeeklyGoalBreakdown_AddDaypart()
        {
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByDaypart();
            request.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1 });

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void WeeklyGoalBreakdown_RemoveDaypart()
        {
            var request = _GetWeeklyBreakdownRequest_CustomByWeekByDaypart();
            request.Dayparts.RemoveAt(0);

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetDisplayMediaWeeks());

            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
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
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_WEEKLY_BREAKDOWN_ENGINE_V2] = true;
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
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_WEEKLY_BREAKDOWN_ENGINE_V2] = true;
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

        [Test]
        public void CalculatePlanWeeklyGoalBreakdown_AduPlan()
        {
            // Arrange
            // spot length delivery multipliers are setup in the test class constructor
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_WEEKLY_BREAKDOWN_ENGINE_V2] = true;
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = true;

            var weightedCreatives = new List<CreativeLength>
                    {
                        new CreativeLength { SpotLengthId = 1, Weight = 100 }
                    };

            _CreativeLengthEngineMock.Setup(s => s.DistributeWeight(It.IsAny<IEnumerable<CreativeLength>>()))
                .Returns(weightedCreatives);

            var request = new WeeklyBreakdownRequest
            { 
                    FlightDays = new List<int> { 1,2,3,4,5,6,7 },
                    FlightStartDate = new DateTime(2023, 03, 27),
                    FlightEndDate = new DateTime(2023, 4, 9, 23, 59 ,59),
                    FlightHiatusDays = new List<DateTime>(),
                    // TotalImpressions = null,
                    DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                    Equivalized = true,
                    Weeks = new List<WeeklyBreakdownWeek>(),
                    CreativeLengths = weightedCreatives,
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto { DaypartCodeId = 1 }
                    },
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                    IsAduOnly = true,
                    ImpressionsPerUnit = 0,
                    TotalBudget = 0,
                    TotalImpressions = 0,
                    TotalRatings = 0
            };

            var cachedWeeks = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek
                {
                    Id = 1005,
                    Week = 1,
                    MediaMonthId = 499,
                    Year = 2023,
                    Month = 4,
                    WeekStartDate = new DateTime(2023, 3, 27),
                    WeekEndDate = new DateTime(2023,4, 2),
                    MonthStartDate = new DateTime(2023, 3, 27),
                    MonthEndDate = new DateTime(2023, 4, 30)
                },
                new DisplayMediaWeek
                {
                    Id = 1006,
                    Week = 2,
                    MediaMonthId = 499,
                    Year = 2023,
                    Month = 4,
                    WeekStartDate = new DateTime(2023, 4, 3),
                    WeekEndDate = new DateTime(2023,4, 9),
                    MonthStartDate = new DateTime(2023, 3, 27),
                    MonthEndDate = new DateTime(2023, 4, 30)
                }
            };

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(cachedWeeks);

            // Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Weeks.Count);
            Assert.AreEqual(2, result.RawWeeklyBreakdownWeeks.Count);
            Assert.AreEqual(14, result.TotalActiveDays);
            Assert.AreEqual(0, result.TotalShareOfVoice);
            Assert.AreEqual(0, result.TotalImpressions);
            Assert.AreEqual(0, result.TotalRatingPoints);
            Assert.AreEqual(0, result.TotalImpressionsPercentage);
            Assert.AreEqual(0, result.TotalBudget);
            Assert.AreEqual(0, result.TotalUnits);
        }

        #region Handling Delivery Type Change

        [Test]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void IsDeliveryTypeChange(bool hasDaypart, bool hasSpotLength, bool expectedResult)
        {
            // arrange
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart,
                CreativeLengths = new List<CreativeLength>()
                {
                    new CreativeLength { SpotLengthId = 1, Weight = 100}
                },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 24,
                        WeightingGoalPercent = 40
                    }
                },
                TotalBudget = 100000,
                Equivalized = true,
                ImpressionsPerUnit = 1,
                TotalImpressions = 10000,
                TotalRatings = 8.3,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightStartDate = new DateTime(2020, 2, 24),
                FlightEndDate = new DateTime(2020, 3, 1),
                FlightHiatusDays = new List<DateTime>(),
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                Weeks = new List<WeeklyBreakdownWeek>()
            };
            var weekNumber = 0;
            if (hasDaypart)
            {
                request.Weeks.Add(new WeeklyBreakdownWeek { DaypartCodeId = 24, WeekNumber = weekNumber++ });
            }
            if (hasSpotLength)
            {
                request.Weeks.Add(new WeeklyBreakdownWeek{SpotLengthId = 1, WeekNumber = weekNumber++ });
            }
            if (!hasDaypart && !hasSpotLength)
            {
                request.Weeks.Add(new WeeklyBreakdownWeek { WeekNumber = weekNumber++ });
            }

            var result = _WeeklyBreakdownEngine._IsDeliveryTypeChange(request);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChange_ToByDaypart()
        {
            // arrange
            var request = GetRequestFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests(PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart);

            var cachedWeeks = GetCachedWeeksFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests();
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(cachedWeeks);

            // Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChange_ToByAdLength()
        {
            // arrange
            var request = GetRequestFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests(PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength);

            var cachedWeeks = GetCachedWeeksFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests();
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(cachedWeeks);

            // Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChange_ToByWeek()
        {
            // arrange
            var request = GetRequestFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests(PlanGoalBreakdownTypeEnum.CustomByWeek);

            var cachedWeeks = GetCachedWeeksFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests();
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(cachedWeeks);

            // Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChange_ToEven()
        {
            // arrange
            var request = GetRequestFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests(PlanGoalBreakdownTypeEnum.EvenDelivery);

            var cachedWeeks = GetCachedWeeksFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests();
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(cachedWeeks);

            // Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private WeeklyBreakdownRequest GetRequestFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests(PlanGoalBreakdownTypeEnum deliveryType)
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = deliveryType,
                CreativeLengths = new List<CreativeLength>()
                {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 3, Weight = 50 }
                },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 12,
                        WeightingGoalPercent = 50
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 6,
                        WeightingGoalPercent = 50
                    }
                },
                TotalBudget = 100000,
                Equivalized = true,
                ImpressionsPerUnit = 1,
                TotalImpressions = 10000,
                TotalRatings = 8.3,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightStartDate = new DateTime(2020, 2, 24),
                FlightEndDate = new DateTime(2020, 3, 1),
                FlightHiatusDays = new List<DateTime>(),
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 844,
                        StartDate =  new DateTime(2020, 2, 24),
                        EndDate =  new DateTime(2020, 3, 1),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 2000,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1.6600000000000002,
                        WeeklyBudget = 20000,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        SpotLengthId = 1,
                        SpotLengthDuration = 30,
                        DaypartCodeId = 12,
                        PlanDaypartId = 1,
                        PercentageOfWeek = 20,
                        IsUpdated = false,
                        UnitImpressions = 0.5,
                        WeeklyUnits = 2000,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 844,
                        StartDate =  new DateTime(2020, 2, 24),
                        EndDate =  new DateTime(2020, 3, 1),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 2000,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 1.6600000000000002,
                        WeeklyBudget = 20000,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        SpotLengthId = 1,
                        SpotLengthDuration = 30,
                        DaypartCodeId = 6,
                        PlanDaypartId = 2,
                        PercentageOfWeek = 20,
                        IsUpdated = false,
                        UnitImpressions = 0.5,
                        WeeklyUnits = 2000,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 844,
                        StartDate =  new DateTime(2020, 2, 24),
                        EndDate =  new DateTime(2020, 3, 1),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 3000,
                        WeeklyImpressionsPercentage = 30,
                        WeeklyRatings = 2.49,
                        WeeklyBudget = 30000,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        SpotLengthId = 3,
                        SpotLengthDuration = 15,
                        DaypartCodeId = 12,
                        PlanDaypartId = 1,
                        PercentageOfWeek = 30,
                        IsUpdated = false,
                        UnitImpressions = 0.25,
                        WeeklyUnits = 6000,
                        IsLocked = false
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 844,
                        StartDate =  new DateTime(2020, 2, 24),
                        EndDate =  new DateTime(2020, 3, 1),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 3000,
                        WeeklyImpressionsPercentage = 30,
                        WeeklyRatings = 2.49,
                        WeeklyBudget = 30000,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        SpotLengthId = 3,
                        SpotLengthDuration = 15,
                        DaypartCodeId = 6,
                        PlanDaypartId = 2,
                        PercentageOfWeek = 30,
                        IsUpdated = false,
                        UnitImpressions =0.25,
                        WeeklyUnits = 6000,
                        IsLocked = false
                    }
                }
            };
            return request;
        }

        private List<DisplayMediaWeek> GetCachedWeeksFor_CalculatePlanWeeklyGoalBreakdown_DeliveryTypeChangeTests()
        {
            var cachedWeeks = new List<DisplayMediaWeek>
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
                }
            };
            return cachedWeeks;
        }

        #endregion // #region Handling Delivery Type Change

        #region Handling Hiatus Day Change

        [Test]
        public void CalculatePlanWeeklyGoalBreakdown_HiatusDayChange()
        {
            // arrange
            var request = GetRequestFor_CalculatePlanWeeklyGoalBreakdown_HiatusDayChangeTests();

            var cachedWeeks = GetCachedWeeksFor_CalculatePlanWeeklyGoalBreakdown_HiatusDayChangeTest();
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(cachedWeeks);

            // Act
            var result = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            // Assert
            var lockedWeek = result.Weeks.Single(w => w.IsLocked);
            Assert.AreEqual(6, lockedWeek.NumberOfActiveDays);
            Assert.AreEqual("M-W,F-Su", lockedWeek.ActiveDays);
        }

        private WeeklyBreakdownRequest GetRequestFor_CalculatePlanWeeklyGoalBreakdown_HiatusDayChangeTests()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
                CreativeLengths = new List<CreativeLength>()
                {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 3, Weight = 50 }
                },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 12,
                        WeightingGoalPercent = 50
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 6,
                        WeightingGoalPercent = 50
                    }
                },
                TotalBudget = 100000,
                Equivalized = true,
                ImpressionsPerUnit = 1,
                TotalImpressions = 10000,
                TotalRatings = 8.3,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightStartDate = new DateTime(2020, 2, 24),
                FlightEndDate = new DateTime(2020, 3, 1),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2020, 2, 27)
                },
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 844,
                        StartDate =  new DateTime(2020, 2, 24),
                        EndDate =  new DateTime(2020, 3, 1),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 10000,
                        WeeklyImpressionsPercentage = 100,
                        WeeklyBudget = 100000,
                        WeeklyAdu = 0,
                        AduImpressions = 0,
                        PercentageOfWeek = 100,
                        IsUpdated = false,
                        UnitImpressions = 0,
                        WeeklyUnits = 10000,
                        IsLocked = true
                    }
                }
            };
            return request;
        }

        private List<DisplayMediaWeek> GetCachedWeeksFor_CalculatePlanWeeklyGoalBreakdown_HiatusDayChangeTest()
        {
            var cachedWeeks = new List<DisplayMediaWeek>
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
                }
            };
            return cachedWeeks;
        }

        #endregion // #region Handling Hiatus Day Change        
    }
}
