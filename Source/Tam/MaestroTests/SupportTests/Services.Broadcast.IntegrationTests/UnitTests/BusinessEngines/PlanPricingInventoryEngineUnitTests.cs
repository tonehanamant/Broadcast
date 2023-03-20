using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using FizzWare.NBuilder;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanPricingInventoryEngineUnitTests
    {
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCache;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IImpressionsCalculationEngine> _ImpressionsCalculationEngineMock;
        private Mock<IDayRepository> _DayRepositoryMock;
        private Mock<INtiToNsiConversionRepository> _NtiToNsiConversionRepositoryMock;
        private Mock<IPlanPricingInventoryQuarterCalculatorEngine> _PlanPricingInventoryQuarterCalculatorEngineMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IStationProgramRepository> _StationProgramRepositoryMock;
        private Mock<IStationRepository> _StationRepositoryMock;
        private Mock<IDaypartCache> _DaypartCacheMock;
        private Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;
        private Mock<IStandardDaypartRepository> _StandardDaypartRepository;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelper;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _ImpressionsCalculationEngineMock = new Mock<IImpressionsCalculationEngine>();
            _DayRepositoryMock = new Mock<IDayRepository>();
            _NtiToNsiConversionRepositoryMock = new Mock<INtiToNsiConversionRepository>();
            _PlanPricingInventoryQuarterCalculatorEngineMock = new Mock<IPlanPricingInventoryQuarterCalculatorEngine>();
            _MediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();

            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _StationRepositoryMock = new Mock<IStationRepository>();
            _DaypartCacheMock = new Mock<IDaypartCache>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
            _StandardDaypartRepository = _GetMockStandardDaypartRepository();

            _SpotLengthEngineMock.Setup(s => s.GetSpotCostMultiplierBySpotLengthId(It.IsAny<int>()))
                .Returns<int>((i) => SpotLengthTestData.GetCostMultipliersBySpotLengthId()[i]);

            _DayRepositoryMock
                .Setup(x => x.GetDays())
                .Returns(_GetDays());

            _NtiToNsiConversionRepositoryMock
                .Setup(x => x.GetLatestNtiToNsiConversionRates())
                .Returns(ConversionTestData.GetNtiToNsiConversionRates());

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<INtiToNsiConversionRepository>())
                .Returns(_NtiToNsiConversionRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IDayRepository>())
                .Returns(_DayRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationProgramRepository>())
                .Returns(_StationProgramRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationRepository>())
                .Returns(_StationRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketCoverageRepository>())
                .Returns(_MarketCoverageRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_StandardDaypartRepository.Object);

            _MediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksIntersecting(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>((start, end) => MediaMonthAndWeekTestData.GetMediaWeeksIntersecting(start, end));

            _ConfigurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
        }

        private PlanPricingInventoryEngineTestClass _GetTestClass()
        {
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);

            var planPricingInventoryEngine = new PlanPricingInventoryEngineTestClass(
                _DataRepositoryFactoryMock.Object,
                _ImpressionsCalculationEngineMock.Object,
                _PlanPricingInventoryQuarterCalculatorEngineMock.Object,
                _MediaMonthAndWeekAggregateCache.Object,
                _DaypartCacheMock.Object,
                _QuarterCalculationEngineMock.Object,
                _SpotLengthEngineMock.Object, 
                featureToggleHelper,
                _ConfigurationSettingsHelper.Object);

            return planPricingInventoryEngine;
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
        [TestCase(null, null, 4)]
        [TestCase(12.00, null, 3)]
        [TestCase(null, 15.00, 3)]
        [TestCase(12.00, 15.00, 2)]
        public void ExcludeProgramsWithOutOfBoundsMinAndMaxCPM(double? minCPM, double? maxCPM, int expectedCount)
        {
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 350.00M,
                        }
                    },
                    ProjectedImpressions = 33615.275,
                    ProvidedImpressions = 29600.0
                },
                new PlanPricingInventoryProgram
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 400.00M,
                        }
                    },
                    ProjectedImpressions = 44998.25,
                    ProvidedImpressions = 30200.0
                },
                new PlanPricingInventoryProgram
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 375.00M,
                        }
                    },
                    ProjectedImpressions = 30683.0,
                    ProvidedImpressions = 21800.0
                },
                new PlanPricingInventoryProgram
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M,
                        }
                    },
                    ProjectedImpressions = 40034.6875,
                    ProvidedImpressions = 23400.0
                }
            };
            var testClass = _GetTestClass();

            var result = testClass.CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, (decimal?)minCPM, (decimal?)maxCPM);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatesProgramCPM()
        {
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 50.00M,
                        }
                    },
                    ProvidedImpressions = 10000
                }
            };
            var testClass = _GetTestClass();

            var result = testClass.CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, null, null);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatesProgramCPM_v3()
        {
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 2,
                            Cost = 50.00M,
                        },
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 25.00M,
                        }
                    },
                    ProvidedImpressions = 10000
                }
            };
            var testClass = _GetTestClass();

            var result = testClass.CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, null, null);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void CalculatesProgramCPM_v3_NoRateFor30()
        {
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 2,
                            Cost = 50.00M,
                        },
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 3,
                            Cost = 30.00M,
                        }
                    },
                    ProvidedImpressions = 10000
                }
            };

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotCostMultiplierBySpotLengthId(It.IsAny<int>()))
                .Returns(0.5m);

            var testClass = _GetTestClass();

            var result = testClass.CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, null, null);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(null, 300.0)]
        [TestCase(5.0, 315.0)]
        [TestCase(10.0, 330.0)]
        [TestCase(4.5, 313.5)]
        public void ApplyInflationToSpotCost_ProgramsWithPlanInventoryPricingQuarterType(double? inflationFactor, double expectedResult)
        {
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M
                        }
                    }
                }
            };
            var testClass = _GetTestClass();

            testClass.ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.Single().ManifestRates.Single().Cost);
        }

        [Test]
        [TestCase(null, 300.0)]
        [TestCase(5.0, 315.0)]
        [TestCase(10.0, 330.0)]
        [TestCase(4.5, 313.5)]
        public void ApplyInflationToSpotCost_ProgramsWithFallbackInventoryPricingQuarterType(double? inflationFactor, double expectedResult)
        {
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Fallback,
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M
                        }
                    }
                }
            };
            var testClass = _GetTestClass();

            testClass.ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.Single().ManifestRates.Single().Cost);
        }

        [Test]
        [TestCase(null, 300.0)]
        [TestCase(5.0, 315.0)]
        [TestCase(10.0, 330.0)]
        [TestCase(4.5, 313.5)]
        public void ApplyInflationToSpotCost_ProgramsWithMixedInventoryPricingQuarterType(double? inflationFactor, double expectedResult)
        {
            const decimal defaultSpotCost = 300;
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Fallback,
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = defaultSpotCost
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = defaultSpotCost
                        }
                    }
                }
            };
            var testClass = _GetTestClass();

            testClass.ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.FirstOrDefault().ManifestRates.Single().Cost);
            Assert.AreEqual(defaultSpotCost, programs.LastOrDefault().ManifestRates.Single().Cost);
        }

        [Test]
        [TestCase(null, null, null, 4)]
        [TestCase(null, null, 10.0, 4)]
        [TestCase(14.0, null, 10.0, 3)]
        [TestCase(null, 18.0, 10.0, 3)]
        [TestCase(14.00, 18.00, 10.0, 2)]
        public void ExcludeProgramsWithOutOfBoundsMinAndMaxCPMAfterInflationApplied(double? minCPM, double? maxCPM, double? inflationFactor, int expectedCount)
        {
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram // CPM 11.8243
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 350.00M
                        }
                    },
                    ProvidedImpressions = 29600.0
                },
                new PlanPricingInventoryProgram // CPM 13.2450
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 400.00M
                        }
                    },
                    ProvidedImpressions = 30200.0
                },
                new PlanPricingInventoryProgram // CPM 17.2018
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 375.00M
                        }
                    },
                    ProvidedImpressions = 21800.0
                },
                new PlanPricingInventoryProgram // CPM 12.8205
                {
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M
                        }
                    },
                    ProvidedImpressions = 23400.0
                }
            };
            var testClass = _GetTestClass();

            testClass.ApplyInflationFactorToSpotCost(programs, inflationFactor);
            var result = testClass.CalculateProgramCpmAndFilterByMinAndMaxCpm(programs, (decimal?)minCPM, (decimal?)maxCPM);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void ConvertPostingType_ConvertsInputInventoryToNti()
        {
            //Arrange
            var defaultInventory = Builder<PlanPricingInventoryProgram>.CreateListOfSize(2).Build()
                .ToList();
            var testClass = _GetTestClass();

            //Act
            testClass.ConvertPostingType(PostingTypeEnum.NTI, defaultInventory);

            //Assert
            Assert.AreEqual(2, defaultInventory.Count(x => x.PostingType == PostingTypeEnum.NTI)); 
        }

        [Test]
        public void GetsPlanDaypartDaysFromPlanFlight()
        {
            var expectedDaypart = new DisplayDaypart
            {
                Monday = true,
                Sunday = true
            };
            var planFlightDateRanges = _GetPlanFlightDateRanges();
            var flightDays = new List<int> { 1, 3, 5, 7 };
            var planDaypartDayIds = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            var testClass = _GetTestClass();

            var result = testClass._GetDaypartDaysFromFlight(flightDays, planFlightDateRanges, planDaypartDayIds);

            Assert.AreEqual(expectedDaypart, result);
        }

        [Test]
        public void GetsPlanDaypartDaysFromPlanFlightWeekendDaypart()
        {
            var expectedDaypart = new DisplayDaypart
            {
                Sunday = true
            };
            var planFlightDays = new List<int> { 1, 3, 5, 7 };
            var planFlightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2019, 12, 16), new DateTime(2019, 12, 17)), // Mon - Tue
                new DateRange(new DateTime(2019, 12, 21), new DateTime(2019, 12, 22)) // Sat - Sun
            };
            var planDaypartDayIds = new List<int> { 6, 7 };
            var testClass = _GetTestClass();

            var result = testClass._GetDaypartDaysFromFlight(planFlightDays, planFlightDateRanges, planDaypartDayIds);

            Assert.AreEqual(expectedDaypart, result);
        }

        [Test]
        public void DoesNotApplyNTIConversionToNSI_WhenPlanPostingType_IsNotNTI()
        {
            var plan = _GetPlan();
            plan.PostingType = PostingTypeEnum.NSI;
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ProjectedImpressions = 1000,
                    ProvidedImpressions = 2000
                }
            };
            var testClass = _GetTestClass();

            testClass.UT_ApplyNTIConversionToNSI(plan, programs);

            Assert.AreEqual(1000, programs.Single().ProjectedImpressions);
            Assert.AreEqual(2000, programs.Single().ProvidedImpressions);
        }

        [Test]
        public void AppliesNTIConversionToNSI_WhenPlanPostingType_IsNTI()
        {
            var plan = _GetPlan();
            plan.PostingType = PostingTypeEnum.NTI;
            plan.Dayparts.Add(new PlanDaypartDto
            {
                DaypartCodeId = 2,
                StartTimeSeconds = 32400, // 9am
                EndTimeSeconds = 35999 // 10am
            });
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    StandardDaypartId = 2,
                    ProjectedImpressions = 1000,
                    ProvidedImpressions = 2000,
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Monday = true,
                                StartTime = 28800, // 8am
                                EndTime = 37799 // 10:30am
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };
            var testClass = _GetTestClass();

            testClass.UT_ApplyNTIConversionToNSI(plan, programs);

            Assert.AreEqual(1000, programs.Single().ProjectedImpressions);
            Assert.AreEqual(2000, programs.Single().ProvidedImpressions);
        }

        /// <summary>
        /// Test that the stations fallback if necessary.
        /// </summary>
        /// <remarks>
        ///     Scenario : All stations are found in the plan quarter.
        ///     Expected : No stations fallback.
        /// </remarks>
        [Test]
        public void GetFullProgramsWhenAllStationsInPlanQuarter()
        {
            /*** Arrange ***/
            const int marketCount = 3;
            const int stationPerMarketCount = 2;
            const int spotLengthId = 1;
            var supportedInventorySourceTypes = new List<int> { 1 };
            var flightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2020, 05, 01),
                    new DateTime(2020, 05, 30))
            };
            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallbackQuarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto
                {
                    Quarter = 4,
                    Year = 2019,
                    StartDate = new DateTime(2020, 09, 25),
                    EndDate = new DateTime(2020, 12, 27)
                }
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };
            var availableMarkets = new List<short> { 1, 2, 3, 4, 5 };
            var availableStations = StationsTestData.GetStations(marketCount, stationPerMarketCount);

            var inventory = availableStations.Select(s =>
                   new PlanPricingInventoryProgram
                   {
                       Station = new DisplayBroadcastStation { Id = s.Id, LegacyCallLetters = s.LegacyCallLetters },
                       ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                   })
                .ToList();

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.Setup(s => s.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventory);
            var testClass = _GetTestClass();

            /*** Act ***/
            var result = testClass._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters, Guid.NewGuid());

            /*** Assert ***/
            Assert.AreEqual(6, result.Count);
            foreach (var station in availableStations)
            {
                Assert.AreEqual(1, result.Count(i => i.Station.LegacyCallLetters == station.LegacyCallLetters));
            }
        }

        /// <summary>
        /// Test that the stations fallback if necessary.
        /// </summary>
        /// <remarks>
        ///     Scenario : Some stations are found in the Plan Quarter and some in the Fallback Quarter.
        ///     Expected :
        ///     - 3 stations found in the Plan Quarter
        ///     - 3 stations found in the Fallback Quarter
        /// </remarks>
        [Test]
        public void GetFullProgramsWhenSomeStationsFallback()
        {
            /*** Arrange ***/
            const int marketCount = 3;
            const int stationPerMarketCount = 2;
            const int spotLengthId = 1;
            var supportedInventorySourceTypes = new List<int> { 1 };

            var flightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2020, 12, 01),
                    new DateTime(2020, 12, 25))
            };
            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallbackQuarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto
                {
                    Quarter = 4,
                    Year = 2019,
                    StartDate = new DateTime(2020, 09, 25),
                    EndDate = new DateTime(2020, 12, 27)
                }
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };

            var availableMarkets = new List<short> { 1, 2, 3, 4, 5 };
            var availableStations = StationsTestData.GetStations(marketCount, stationPerMarketCount);

            var inventoryOne = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 1, LegacyCallLetters = availableStations[0].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 2, LegacyCallLetters = availableStations[1].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 3, LegacyCallLetters = availableStations[2].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };
            var inventoryTwo = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 4, LegacyCallLetters = availableStations[3].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 5, LegacyCallLetters = availableStations[4].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 6, LegacyCallLetters = availableStations[5].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo);
            var testClass = _GetTestClass();

            /*** Act ***/
            var result = testClass._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters, Guid.NewGuid());

            /*** Assert ***/
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        /// <summary>
        /// Test that the stations fallback if necessary.
        /// </summary>
        /// <remarks>
        ///     Scenario : Some stations are found in the Plan Quarter and some in the Fallback Quarter over 2 date ranges.
        ///     Expected :
        ///     DateRange 1 : 
        ///     - 3 stations found in the Plan Quarter
        ///     - 3 stations found in the Fallback Quarter
        ///     DateRange 2 : 
        ///     - 3 stations found in the Plan Quarter
        ///     - 3 stations found in the Fallback Quarter
        /// </remarks>
        [Test]
        public void GetFullProgramsWhenSomeStationsFallback_WithHiatus()
        {
            /*** Arrange ***/
            const int marketCount = 3;
            const int stationPerMarketCount = 2;
            const int spotLengthId = 1;
            var supportedInventorySourceTypes = new List<int> { 1 };

            var flightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2020, 12, 01),
                    new DateTime(2020, 12, 08)),
                new DateRange(new DateTime(2020, 12, 12),
                    new DateTime(2020, 12, 25)),
            };
            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallbackQuarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto
                {
                    Quarter = 4,
                    Year = 2019,
                    StartDate = new DateTime(2020, 09, 25),
                    EndDate = new DateTime(2020, 12, 27)
                }
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };

            var availableMarkets = new List<short> { 1, 2, 3, 4, 5 };
            var availableStations = StationsTestData.GetStations(marketCount, stationPerMarketCount);

            var inventoryOne = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 1, LegacyCallLetters = availableStations[0].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 2, LegacyCallLetters = availableStations[1].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 3, LegacyCallLetters = availableStations[2].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };
            var inventoryTwo = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 4, LegacyCallLetters = availableStations[3].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 5, LegacyCallLetters = availableStations[4].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 6, LegacyCallLetters = availableStations[5].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(planQuarter);
            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo)
                .Returns(inventoryOne)
                .Returns(inventoryTwo);
            var testClass = _GetTestClass();

            /*** Act ***/
            var result = testClass._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters, Guid.NewGuid());

            /*** Assert ***/
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        /// <summary>
        /// Test that the stations fallback if necessary.
        /// </summary>
        /// <remarks>
        ///     Scenario : Some stations are found in the Plan Quarter and some in the Fallback Quarter, but some are not found in either.
        ///     Expected :
        ///     - 3 stations found in the Plan Quarter
        ///     - 2 stations found in the Fallback Quarter
        ///     - 1 station is not found
        ///     - no error is thrown.
        /// </remarks>
        [Test]
        public void GetFullProgramsWhenSomeStationsGetDropped()
        {
            /*** Arrange ***/
            const int marketCount = 3;
            const int stationPerMarketCount = 2;
            const int spotLengthId = 1;
            var supportedInventorySourceTypes = new List<int> { 1 };
            var flightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2020, 12, 01),
                    new DateTime(2020, 12, 25))
            };
            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallbackQuarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto
                {
                    Quarter = 4,
                    Year = 2019,
                    StartDate = new DateTime(2020, 09, 25),
                    EndDate = new DateTime(2020, 12, 27)
                }
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };
            var availableMarkets = new List<short> { 1, 2, 3, 4, 5 };
            var availableStations = StationsTestData.GetStations(marketCount, stationPerMarketCount);

            var inventoryOne = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 1, LegacyCallLetters = availableStations[0].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 2, LegacyCallLetters = availableStations[1].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 3, LegacyCallLetters = availableStations[2].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };
            var inventoryTwo = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 4, LegacyCallLetters = availableStations[3].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 5, LegacyCallLetters = availableStations[4].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo);
            var testClass = _GetTestClass();

            /*** Act ***/
            var result = testClass._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters, Guid.NewGuid());

            /*** Assert ***/
            Assert.AreEqual(5, result.Count);
            foreach (var station in availableStations)
            {
                var expectedCount = station.LegacyCallLetters == availableStations[5].LegacyCallLetters ? 0 : 1;
                Assert.AreEqual(expectedCount, result.Count(i => i.Station.LegacyCallLetters == station.LegacyCallLetters));
            }
        }

        private PlanDto _GetPlan()
        {
            return new PlanDto
            {
                FlightStartDate = new DateTime(2020, 1, 1),
                FlightEndDate = new DateTime(2020, 2, 1),
                FlightHiatusDays = new List<DateTime>(),
                FlightDays = new List<int> { 1, 2, 3, 4, 5 },
                AvailableMarkets = new List<PlanAvailableMarketDto>(),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 1,
                        StartTimeSeconds = 36000, // 10am
                        EndTimeSeconds = 39599 // 11am
                    }
                }
            };
        }

        private List<DateRange> _GetPlanFlightDateRanges()
        {
            return new List<DateRange>
            {
                new DateRange(new DateTime(2019, 12, 16), new DateTime(2019, 12, 17)), // Mon - Tue
                new DateRange(new DateTime(2019, 12, 21), new DateTime(2019, 12, 22)) // Sat - Sun
            };
        }

        private DisplayDaypart _GetPlanFlightDays()
        {
            return new DisplayDaypart
            {
                Monday = true,
                Tuesday = true,
                Saturday = true,
                Sunday = true
            };
        }

        private List<Day> _GetDays()
        {
            return new List<Day>
            {
                new Day
                {
                    Id = 1,
                    Name = "Monday"
                },
                new Day
                {
                    Id = 2,
                    Name = "Tuesday"
                },
                new Day
                {
                    Id = 3,
                    Name = "Wednesday"
                },
                new Day
                {
                    Id = 4,
                    Name = "Thursday"
                },
                new Day
                {
                    Id = 5,
                    Name = "Friday"
                },
                new Day
                {
                    Id = 6,
                    Name = "Saturday"
                },
                new Day
                {
                    Id = 7,
                    Name = "Sunday"
                }
            };
        }

        [Test]
        public void UsesSingleBook_ToProjectImpressions_WhenGatheringInventory_ForPricing()
        {
            // Arrange
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanPricingJobDiagnostic();

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = null;
            plan.PostingType = PostingTypeEnum.NTI;
            plan.ShareBookId = 201;
            plan.CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 5, Weight = 50 } };
            plan.AudienceId = 25;

            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(new List<DisplayBroadcastStation>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        SpotLengthId = 1,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "KOB"
                        },
                        ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                        {
                            new PlanPricingInventoryProgram.ManifestWeek
                            {
                                Id = 2
                            }
                        },
                        ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                        {
                            new PlanPricingInventoryProgram.ManifestDaypart
                            {
                                Id = 5,
                                Daypart = new DisplayDaypart { Id = 5 },
                                PrimaryProgramId = 0,
                                Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                                {
                                    new BasePlanInventoryProgram.ManifestDaypart.Program()
                                }
                            }
                        },
                        ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                        {
                            new BasePlanInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 10
                            }
                        }
                    }
                });

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(new QuarterDetailDto
                {
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2020, 3, 31)
                });

            _QuarterCalculationEngineMock
                .Setup(x => x.GetLastNQuarters(It.IsAny<QuarterDto>(), It.IsAny<int>()))
                .Returns(new List<QuarterDetailDto>
                {
                    new QuarterDetailDto
                    {
                        StartDate = new DateTime(2020, 4, 1),
                        EndDate = new DateTime(2020, 6, 30)
                    }
                });

            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Returns(new List<StationMonthDetailDto>());

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts());

            object passedParameters = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(
                    It.IsAny<IEnumerable<PlanPricingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanPricingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
                {
                    // deep copy
                    passedParameters = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        request,
                        audienceId
                    }));

                    foreach (var program in programs)
                    {
                        program.ProjectedImpressions = 1500;
                    }
                });
            
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        /// <summary>
        /// This allows diff reporter for the two variences.
        /// </summary>
        /// <param name="programSpotLengthId">The spotlength id for the program.</param>
        private void BaseUsesTwoBooksForPricingTest(int programSpotLengthId)
        {
            // Arrange
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanPricingJobDiagnostic();

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 202;
            plan.PostingType = PostingTypeEnum.NSI;
            plan.ShareBookId = 201;
            plan.CreativeLengths = new List<CreativeLength> 
            {
                new CreativeLength { SpotLengthId = 5, Weight = 50 },
                new CreativeLength { SpotLengthId = 1, Weight = 50 }
            };
            plan.AudienceId = 25;

            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(new List<DisplayBroadcastStation>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        SpotLengthId = programSpotLengthId,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "KOB"
                        },
                        ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                        {
                            new PlanPricingInventoryProgram.ManifestWeek
                            {
                                Id = 2
                            }
                        },
                        ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                        {
                            new PlanPricingInventoryProgram.ManifestDaypart
                            {
                                Id = 5,
                                Daypart = new DisplayDaypart { Id = 5 },
                                PrimaryProgramId = 0,
                                Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                                {
                                    new BasePlanInventoryProgram.ManifestDaypart.Program()
                                }
                            }
                        },
                        ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                        {
                            new BasePlanInventoryProgram.ManifestRate
                            {
                                SpotLengthId = programSpotLengthId,
                                Cost = (10 * programSpotLengthId)
                            }
                        }
                    }          
                });

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(new QuarterDetailDto
                {
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2020, 3, 31)
                });

            _QuarterCalculationEngineMock
                .Setup(x => x.GetLastNQuarters(It.IsAny<QuarterDto>(), It.IsAny<int>()))
                .Returns(new List<QuarterDetailDto>
                {
                    new QuarterDetailDto
                    {
                        StartDate = new DateTime(2020, 4, 1),
                        EndDate = new DateTime(2020, 6, 30)
                    }
                });

            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Returns(new List<StationMonthDetailDto>());

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts());

            object passedParameters = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(
                    It.IsAny<IEnumerable<PlanPricingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanPricingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
                {
                    // deep copy
                    passedParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                    {
                        programs,
                        request,
                        audienceId
                    })));

                    foreach (var program in programs)
                    {
                        program.ProjectedImpressions = 1500;
                    }
                });
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }
        
        [Test]
        public void UsesTwoBooks_ToProjectImpressions_WhenGatheringInventory_ForPricing()
        {
            // 30s SpotLength
            const int spotLengthId = 1;
            BaseUsesTwoBooksForPricingTest(spotLengthId);
        }

        [Test]
        public void GetInventoryForPlanForNonThirtySpotLength()
        {
            // Non-30s SpotLength
            const int spotLengthId = 5;
            BaseUsesTwoBooksForPricingTest(spotLengthId);
        }

        [Test]
        public void ProjectsImpressions_WhenGatheringInventory_ForPricing_v3()
        {
            // Arrange
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanPricingJobDiagnostic();

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 202;
            plan.PostingType = PostingTypeEnum.NSI;
            plan.ShareBookId = 201;
            plan.CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength { SpotLengthId = 5, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 50 }
                };
            plan.AudienceId = 25;

            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(new List<DisplayBroadcastStation>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                        new PlanPricingInventoryProgram
                        {
                            SpotLengthId = 1,
                            Station = new DisplayBroadcastStation
                            {
                                LegacyCallLetters = "KOB"
                            },
                            ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                            {
                                new PlanPricingInventoryProgram.ManifestWeek
                                {
                                    Id = 2
                                }
                            },
                            ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart
                                {
                                    Id = 5,
                                    Daypart = new DisplayDaypart {Id = 5},
                                    PrimaryProgramId = 0,
                                    Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                                    {
                                        new BasePlanInventoryProgram.ManifestDaypart.Program()
                                    }
                                }
                            },
                            ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                            {
                                new BasePlanInventoryProgram.ManifestRate
                                {
                                    SpotLengthId = 1,
                                    Cost = 10
                                }
                            }
                        }
                });

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(new QuarterDetailDto
                {
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2020, 3, 31)
                });

            _QuarterCalculationEngineMock
                .Setup(x => x.GetLastNQuarters(It.IsAny<QuarterDto>(), It.IsAny<int>()))
                .Returns(new List<QuarterDetailDto>
                {
                    new QuarterDetailDto
                    {
                        StartDate = new DateTime(2020, 4, 1),
                        EndDate = new DateTime(2020, 6, 30)
                    }
                });

            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Returns(new List<StationMonthDetailDto>());

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts());

            object passedParameters = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(
                    It.IsAny<IEnumerable<PlanPricingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanPricingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
                {
                        // deep copy
                        passedParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                    {
                        programs,
                        request,
                        audienceId
                    })));

                    foreach (var program in programs)
                    {
                        program.ProjectedImpressions = 1500;
                    }
                });
            
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void AppliesProvidedImpressions_WhenGatheringInventory_ForPricing_v3()
        {
            // Arrange
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanPricingJobDiagnostic();

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 202;
            plan.PostingType = PostingTypeEnum.NSI;
            plan.ShareBookId = 201;
            plan.CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength { SpotLengthId = 5, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 50 }
                };
            plan.AudienceId = 25;

            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(new List<DisplayBroadcastStation>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                        new PlanPricingInventoryProgram
                        {
                            SpotLengthId = 1,
                            Station = new DisplayBroadcastStation
                            {
                                LegacyCallLetters = "KOB"
                            },
                            ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                            {
                                new PlanPricingInventoryProgram.ManifestWeek
                                {
                                    Id = 2
                                }
                            },
                            ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart
                                {
                                    Id = 5,
                                    Daypart = new DisplayDaypart { Id = 5 },
                                    PrimaryProgramId = 2,
                                    Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                                    {
                                        new BasePlanInventoryProgram.ManifestDaypart.Program()
                                        {
                                            Id = 2
                                        }
                                    }
                                }
                            },
                            ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                            {
                                new BasePlanInventoryProgram.ManifestRate
                                {
                                    SpotLengthId = 1,
                                    Cost = 10
                                }
                            }
                        }
                });

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(new QuarterDetailDto
                {
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2020, 3, 31)
                });

            _QuarterCalculationEngineMock
                .Setup(x => x.GetLastNQuarters(It.IsAny<QuarterDto>(), It.IsAny<int>()))
                .Returns(new List<QuarterDetailDto>
                {
                    new QuarterDetailDto
                    {
                        StartDate = new DateTime(2020, 4, 1),
                        EndDate = new DateTime(2020, 6, 30)
                    }
                });

            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Returns(new List<StationMonthDetailDto>());

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts);

            object passedParameters = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProvidedImpressions(
                    It.IsAny<List<PlanPricingInventoryProgram>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Callback<List<PlanPricingInventoryProgram>, int?, int, bool>((programs, audienceId, spotLengthId, equivalized) =>
                {
                    // deep copy
                    passedParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                    {
                        programs,
                        audienceId,
                        spotLengthId,
                        equivalized
                    })));

                    foreach (var program in programs)
                    {
                        program.ProvidedImpressions = 1500;
                    }
                });
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void ApplyProvidedImpressions()
        {//AppliesProvidedImpressions_WhenGatheringInventory_ForPricing_v3_Proprietary
            // Arrange
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanPricingJobDiagnostic();

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 202;
            plan.PostingType = PostingTypeEnum.NSI;
            plan.ShareBookId = 201;
            plan.CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength { SpotLengthId = 5, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 50 }
                };
            plan.AudienceId = 25;

            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(new List<DisplayBroadcastStation>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                        new PlanPricingInventoryProgram
                        {
                            SpotLengthId = 1,
                            Station = new DisplayBroadcastStation
                            {
                                LegacyCallLetters = "KOB"
                            },
                            ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                            {
                                new PlanPricingInventoryProgram.ManifestWeek
                                {
                                    Id = 2
                                }
                            },
                            ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart
                                {
                                    Id = 5,
                                    Daypart = new DisplayDaypart { Id = 1 },
                                    PrimaryProgramId = 0,
                                    Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                                    {
                                        new BasePlanInventoryProgram.ManifestDaypart.Program()
                                    }
                                }
                            },
                            ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                            {
                                new BasePlanInventoryProgram.ManifestRate
                                {
                                    SpotLengthId = 1,
                                    Cost = 10
                                }
                            }
                        }
                });

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(new QuarterDetailDto
                {
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2020, 3, 31)
                });

            _QuarterCalculationEngineMock
                .Setup(x => x.GetLastNQuarters(It.IsAny<QuarterDto>(), It.IsAny<int>()))
                .Returns(new List<QuarterDetailDto>
                {
                    new QuarterDetailDto
                    {
                        StartDate = new DateTime(2020, 4, 1),
                        EndDate = new DateTime(2020, 6, 30)
                    }
                });

            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Returns(new List<StationMonthDetailDto>());

            var testDaypartDict = new Dictionary<int, DisplayDaypart>();
            testDaypartDict.Add(1, new DisplayDaypart
            {
                Monday = true,
                StartTime = 36000, // 10am
                EndTime = 39599 // 11am
            });

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(testDaypartDict);

            object passedParameters = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProvidedImpressions(
                    It.IsAny<List<PlanPricingInventoryProgram>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Callback<List<PlanPricingInventoryProgram>, int?, int, bool>((programs, audienceId, spotLengthId, equivalized) =>
                {
                    // deep copy
                    passedParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                    {
                        programs,
                        audienceId,
                        spotLengthId,
                        equivalized
                    })));

                    foreach (var program in programs)
                    {
                        program.ProvidedImpressions = 1500;
                    }
                });
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void GetsInventoryForQuoteFileGeneration()
        {
            // Arrange
            var request = new QuoteRequestDto
            {
                Margin = 20,
                Equivalized = true,
                HUTBookId = 101,
                ShareBookId = 102,
                PostingType = PostingTypeEnum.NTI,
                AudienceId = 12,
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto
                    {
                        AudienceId = 13
                    }
                },
                FlightStartDate = new DateTime(2020, 1, 1),
                FlightEndDate = new DateTime(2020, 1, 14),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2020, 1, 5),
                    new DateTime(2020, 1, 10)
                },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 2
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3
                    }
                },
                FlightDays = new List<int> { 1, 3, 5 },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 1,
                        StartTimeSeconds = 1800, // 12:30am
                        EndTimeSeconds = 5399, // 1:30am
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        Name = "Program B"
                                    }
                                }
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        StartTimeSeconds = 72000, // 8pm
                        EndTimeSeconds = 79199, // 10pm
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        Name = "Program B"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _InventoryRepositoryMock
                .Setup(x => x.GetInventorySources())
                .Returns(InventoryTestData.GetInventorySources());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            var parametersGetBroadcastStationsWithLatestDetailsByMarketCodes = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(StationsTestData.GetStations(2, 3))
                .Callback((List<short> marketCodes) => parametersGetBroadcastStationsWithLatestDetailsByMarketCodes.Add(marketCodes));

            var parametersGetProgramsForQuoteReport = new List<object>();
            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForQuoteReport(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<List<int>>()))
                .Callback((DateTime startDate,
                           DateTime endDate,
                           List<int> spotLengthIds,
                           IEnumerable<int> inventorySourceIds,
                           List<int> stationIds) =>
                {
                    // deep copy
                    parametersGetProgramsForQuoteReport.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        startDate,
                        endDate,
                        spotLengthIds,
                        inventorySourceIds,
                        stationIds
                    })));
                })
                .Returns(new List<QuoteProgram>
                {
                    new QuoteProgram
                    {
                        ManifestId = 1,
                        Station = StationsTestData.GetStation(1),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 2,
                                Daypart = new DisplayDaypart { Id = 1 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program A"
                                    },
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 2,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 2,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 2 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program C"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 3,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 3 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 4,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 4 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 5,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 1 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                });

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts());

            var parametersGetLatestStationMonthDetailsForStations = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Callback((List<int> stationsIds) => parametersGetLatestStationMonthDetailsForStations.Add(stationsIds))
                .Returns(new List<StationMonthDetailDto>
                {
                    new StationMonthDetailDto
                    {
                        StationId = 1,
                        Affiliation = "NBC latest",
                        MarketCode = 100
                    }
                });

            var parametersApplyProjectedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(It.IsAny<IEnumerable<QuoteProgram>>(), It.IsAny<ImpressionsRequestDto>(), It.IsAny<List<int>>()))
                .Callback((
                    IEnumerable<QuoteProgram> programs,
                    ImpressionsRequestDto impressionsRequest,
                    List<int> audienceIds) =>
                {
                    // deep copy
                    parametersApplyProjectedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        impressionsRequest,
                        audienceIds
                    })));

                    foreach (var program in programs)
                    {
                        foreach (var audienceId in audienceIds)
                        {
                            program.DeliveryPerAudience.Add(new QuoteProgram.ImpressionsPerAudience
                            {
                                AudienceId = audienceId,
                                ProjectedImpressions = (audienceId + program.ManifestId) * 1000
                            });
                        }
                    }
                });

            var parametersApplyProvidedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProvidedImpressions(It.IsAny<List<QuoteProgram>>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Callback((
                    List<QuoteProgram> programs,
                    int spotLengthId,
                    bool equivalized) =>
                {
                    parametersApplyProvidedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        spotLengthId,
                        equivalized
                    })));

                    programs.First().DeliveryPerAudience.ForEach(x => x.ProvidedImpressions = x.ProjectedImpressions * 1.1);
                });
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForQuote(request, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                inventory,
                parametersGetBroadcastStationsWithLatestDetailsByMarketCodes,
                parametersGetProgramsForQuoteReport,
                parametersGetLatestStationMonthDetailsForStations,
                parametersApplyProjectedImpressions,
                parametersApplyProvidedImpressions
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsInventoryForQuoteFileGenerationWkd()
        {
            // Arrange
            var wkdDaypart = DaypartsTestData.GetAllStandardDaypartsWithFullData().Single(s => s.Code.Equals("WKD"));

            var request = new QuoteRequestDto
            {
                Margin = 20,
                Equivalized = true,
                HUTBookId = 101,
                ShareBookId = 102,
                PostingType = PostingTypeEnum.NTI,
                AudienceId = 12,
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto
                    {
                        AudienceId = 13
                    }
                },
                FlightStartDate = new DateTime(2020, 1, 1),
                FlightEndDate = new DateTime(2020, 1, 14),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2020, 1, 5),
                    new DateTime(2020, 1, 10)
                },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 2
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3
                    }
                },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = wkdDaypart.Id,
                        StartTimeSeconds = wkdDaypart.DefaultStartTimeSeconds,
                        EndTimeSeconds = wkdDaypart.DefaultEndTimeSeconds,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        Name = "Program B"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _InventoryRepositoryMock
                .Setup(x => x.GetInventorySources())
                .Returns(InventoryTestData.GetInventorySources());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            var parametersGetBroadcastStationsWithLatestDetailsByMarketCodes = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(StationsTestData.GetStations(2, 3))
                .Callback((List<short> marketCodes) => parametersGetBroadcastStationsWithLatestDetailsByMarketCodes.Add(marketCodes));

            var parametersGetProgramsForQuoteReport = new List<object>();
            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForQuoteReport(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<List<int>>()))
                .Callback((DateTime startDate,
                           DateTime endDate,
                           List<int> spotLengthIds,
                           IEnumerable<int> inventorySourceIds,
                           List<int> stationIds) =>
                {
                    // deep copy
                    parametersGetProgramsForQuoteReport.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        startDate,
                        endDate,
                        spotLengthIds,
                        inventorySourceIds,
                        stationIds
                    })));
                })
                .Returns(new List<QuoteProgram>
                {
                    new QuoteProgram
                    {
                        ManifestId = 1,
                        Station = StationsTestData.GetStation(1),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 2,
                                Daypart = new DisplayDaypart { Id = wkdDaypart.Id },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program A"
                                    },
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 2,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 2,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 2 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program C"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 3,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 3 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 4,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 4 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 5,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 1 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                });

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts());

            var parametersGetLatestStationMonthDetailsForStations = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Callback((List<int> stationsIds) => parametersGetLatestStationMonthDetailsForStations.Add(stationsIds))
                .Returns(new List<StationMonthDetailDto>
                {
                    new StationMonthDetailDto
                    {
                        StationId = 1,
                        Affiliation = "NBC latest",
                        MarketCode = 100
                    }
                });

            var parametersApplyProjectedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(It.IsAny<IEnumerable<QuoteProgram>>(), It.IsAny<ImpressionsRequestDto>(), It.IsAny<List<int>>()))
                .Callback((
                    IEnumerable<QuoteProgram> programs,
                    ImpressionsRequestDto impressionsRequest,
                    List<int> audienceIds) =>
                {
                    // deep copy
                    parametersApplyProjectedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        impressionsRequest,
                        audienceIds
                    })));

                    foreach (var program in programs)
                    {
                        foreach (var audienceId in audienceIds)
                        {
                            program.DeliveryPerAudience.Add(new QuoteProgram.ImpressionsPerAudience
                            {
                                AudienceId = audienceId,
                                ProjectedImpressions = (audienceId + program.ManifestId) * 1000
                            });
                        }
                    }
                });

            var parametersApplyProvidedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProvidedImpressions(It.IsAny<List<QuoteProgram>>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Callback((
                    List<QuoteProgram> programs,
                    int spotLengthId,
                    bool equivalized) =>
                {
                    parametersApplyProvidedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        spotLengthId,
                        equivalized
                    })));

                    programs.FirstOrDefault()?.DeliveryPerAudience.ForEach(x => x.ProvidedImpressions = x.ProjectedImpressions * 1.1);
                });
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForQuote(request, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                inventory,
                parametersGetBroadcastStationsWithLatestDetailsByMarketCodes,
                parametersGetProgramsForQuoteReport,
                parametersGetLatestStationMonthDetailsForStations,
                parametersApplyProjectedImpressions,
                parametersApplyProvidedImpressions
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void GetsInventoryForQuoteFileGenerationWkdWithNoPrograms()
        {
            // Arrange
            var wkdDaypart = DaypartsTestData.GetAllStandardDaypartsWithFullData().Single(s => s.Code.Equals("WKD"));

            var request = new QuoteRequestDto
            {
                Margin = 20,
                Equivalized = true,
                HUTBookId = 101,
                ShareBookId = 102,
                PostingType = PostingTypeEnum.NTI,
                AudienceId = 12,
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto
                    {
                        AudienceId = 13
                    }
                },
                FlightStartDate = new DateTime(2020, 1, 1),
                FlightEndDate = new DateTime(2020, 1, 14),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2020, 1, 5),
                    new DateTime(2020, 1, 10)
                },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 2
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3
                    }
                },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = wkdDaypart.Id,
                        // override to mis-align with the actual daypart
                        StartTimeSeconds = 0,
                        EndTimeSeconds = wkdDaypart.DefaultStartTimeSeconds - 1,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        Name = "Program B"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _InventoryRepositoryMock
                .Setup(x => x.GetInventorySources())
                .Returns(InventoryTestData.GetInventorySources());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            var parametersGetBroadcastStationsWithLatestDetailsByMarketCodes = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(StationsTestData.GetStations(2, 3))
                .Callback((List<short> marketCodes) => parametersGetBroadcastStationsWithLatestDetailsByMarketCodes.Add(marketCodes));

            var parametersGetProgramsForQuoteReport = new List<object>();
            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForQuoteReport(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<List<int>>()))
                .Callback((DateTime startDate,
                           DateTime endDate,
                           List<int> spotLengthIds,
                           IEnumerable<int> inventorySourceIds,
                           List<int> stationIds) =>
                {
                    // deep copy
                    parametersGetProgramsForQuoteReport.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        startDate,
                        endDate,
                        spotLengthIds,
                        inventorySourceIds,
                        stationIds
                    })));
                })
                .Returns(new List<QuoteProgram>
                {
                    new QuoteProgram
                    {
                        ManifestId = 1,
                        Station = StationsTestData.GetStation(1),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 2,
                                Daypart = new DisplayDaypart { Id = wkdDaypart.Id },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program A"
                                    },
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 2,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 2,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 2 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program C"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 3,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 3 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 4,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 4 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 5,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 1 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                });

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts());

            var parametersGetLatestStationMonthDetailsForStations = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Callback((List<int> stationsIds) => parametersGetLatestStationMonthDetailsForStations.Add(stationsIds))
                .Returns(new List<StationMonthDetailDto>
                {
                    new StationMonthDetailDto
                    {
                        StationId = 1,
                        Affiliation = "NBC latest",
                        MarketCode = 100
                    }
                });

            var parametersApplyProjectedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(It.IsAny<IEnumerable<QuoteProgram>>(), It.IsAny<ImpressionsRequestDto>(), It.IsAny<List<int>>()))
                .Callback((
                    IEnumerable<QuoteProgram> programs,
                    ImpressionsRequestDto impressionsRequest,
                    List<int> audienceIds) =>
                {
                    // deep copy
                    parametersApplyProjectedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        impressionsRequest,
                        audienceIds
                    })));

                    foreach (var program in programs)
                    {
                        foreach (var audienceId in audienceIds)
                        {
                            program.DeliveryPerAudience.Add(new QuoteProgram.ImpressionsPerAudience
                            {
                                AudienceId = audienceId,
                                ProjectedImpressions = (audienceId + program.ManifestId) * 1000
                            });
                        }
                    }
                });

            var parametersApplyProvidedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProvidedImpressions(It.IsAny<List<QuoteProgram>>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Callback((
                    List<QuoteProgram> programs,
                    int spotLengthId,
                    bool equivalized) =>
                {
                    parametersApplyProvidedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        spotLengthId,
                        equivalized
                    })));

                    programs.FirstOrDefault()?.DeliveryPerAudience.ForEach(x => x.ProvidedImpressions = x.ProjectedImpressions * 1.1);
                });
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForQuote(request, Guid.NewGuid());

            // Assert
            Assert.IsEmpty(inventory);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsInventoryForQuoteFileGenerationWkdMix()
        {
            // Arrange
            var wkdDaypart = DaypartsTestData.GetAllStandardDaypartsWithFullData().Single(s => s.Code.Equals("WKD"));

            var request = new QuoteRequestDto
            {
                Margin = 20,
                Equivalized = true,
                HUTBookId = 101,
                ShareBookId = 102,
                PostingType = PostingTypeEnum.NTI,
                AudienceId = 12,
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto
                    {
                        AudienceId = 13
                    }
                },
                FlightStartDate = new DateTime(2020, 1, 1),
                FlightEndDate = new DateTime(2020, 1, 14),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2020, 1, 5),
                    new DateTime(2020, 1, 10)
                },
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 2
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3
                    }
                },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = wkdDaypart.Id,
                        StartTimeSeconds = wkdDaypart.DefaultStartTimeSeconds,
                        EndTimeSeconds = wkdDaypart.DefaultEndTimeSeconds,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        Name = "Program B"
                                    },
                                    new ProgramDto
                                    {
                                        Name = "Program D"
                                    }
                                }
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 3,
                        StartTimeSeconds = 72000, // 8pm
                        EndTimeSeconds = 79199, // 10pm
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        Name = "Program B"
                                    },
                                    new ProgramDto
                                    {
                                        Name = "Program D"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _InventoryRepositoryMock
                .Setup(x => x.GetInventorySources())
                .Returns(InventoryTestData.GetInventorySources());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            var parametersGetBroadcastStationsWithLatestDetailsByMarketCodes = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(StationsTestData.GetStations(2, 3))
                .Callback((List<short> marketCodes) => parametersGetBroadcastStationsWithLatestDetailsByMarketCodes.Add(marketCodes));

            var parametersGetProgramsForQuoteReport = new List<object>();
            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForQuoteReport(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<List<int>>()))
                .Callback((DateTime startDate,
                           DateTime endDate,
                           List<int> spotLengthIds,
                           IEnumerable<int> inventorySourceIds,
                           List<int> stationIds) =>
                {
                    // deep copy
                    parametersGetProgramsForQuoteReport.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        startDate,
                        endDate,
                        spotLengthIds,
                        inventorySourceIds,
                        stationIds
                    })));
                })
                .Returns(new List<QuoteProgram>
                {
                    new QuoteProgram
                    {
                        ManifestId = 1,
                        Station = StationsTestData.GetStation(1),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 2,
                                Daypart = new DisplayDaypart { Id = wkdDaypart.Id },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program A"
                                    },
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 2,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 2,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 2 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program C"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 3,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 3 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 4,
                        Station = StationsTestData.GetStation(3),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 4 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new QuoteProgram
                    {
                        ManifestId = 5,
                        Station = StationsTestData.GetStation(2),
                        ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                        {
                            new QuoteProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart { Id = 1 },
                                Programs = new List<QuoteProgram.ManifestDaypart.Program>
                                {
                                    new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<QuoteProgram.ManifestRate>
                        {
                            new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                });

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(DaypartsTestData.GetAllDisplayDayparts());

            var parametersGetLatestStationMonthDetailsForStations = new List<object>();
            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Callback((List<int> stationsIds) => parametersGetLatestStationMonthDetailsForStations.Add(stationsIds))
                .Returns(new List<StationMonthDetailDto>
                {
                    new StationMonthDetailDto
                    {
                        StationId = 1,
                        Affiliation = "NBC latest",
                        MarketCode = 100
                    }
                });

            var parametersApplyProjectedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(It.IsAny<IEnumerable<QuoteProgram>>(), It.IsAny<ImpressionsRequestDto>(), It.IsAny<List<int>>()))
                .Callback((
                    IEnumerable<QuoteProgram> programs,
                    ImpressionsRequestDto impressionsRequest,
                    List<int> audienceIds) =>
                {
                    // deep copy
                    parametersApplyProjectedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        impressionsRequest,
                        audienceIds
                    })));

                    foreach (var program in programs)
                    {
                        foreach (var audienceId in audienceIds)
                        {
                            program.DeliveryPerAudience.Add(new QuoteProgram.ImpressionsPerAudience
                            {
                                AudienceId = audienceId,
                                ProjectedImpressions = (audienceId + program.ManifestId) * 1000
                            });
                        }
                    }
                });

            var parametersApplyProvidedImpressions = new List<object>();
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProvidedImpressions(It.IsAny<List<QuoteProgram>>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Callback((
                    List<QuoteProgram> programs,
                    int spotLengthId,
                    bool equivalized) =>
                {
                    parametersApplyProvidedImpressions.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                    {
                        programs,
                        spotLengthId,
                        equivalized
                    })));

                    programs.FirstOrDefault()?.DeliveryPerAudience.ForEach(x => x.ProvidedImpressions = x.ProjectedImpressions * 1.1);
                });
            var testClass = _GetTestClass();

            // Act
            var inventory = testClass.GetInventoryForQuote(request, Guid.NewGuid());

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                inventory,
                parametersGetBroadcastStationsWithLatestDetailsByMarketCodes,
                parametersGetProgramsForQuoteReport,
                parametersGetLatestStationMonthDetailsForStations,
                parametersApplyProjectedImpressions,
                parametersApplyProvidedImpressions
            });

            Approvals.Verify(resultJson);
        }
    }
}
