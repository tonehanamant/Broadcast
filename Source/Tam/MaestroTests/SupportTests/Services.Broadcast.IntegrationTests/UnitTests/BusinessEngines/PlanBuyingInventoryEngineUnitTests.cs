using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Plan.Pricing;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanBuyingInventoryEngineUnitTests
    {
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCache;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IImpressionsCalculationEngine> _ImpressionsCalculationEngineMock;
        private Mock<IDayRepository> _DayRepositoryMock;
        private Mock<INtiToNsiConversionRepository> _NtiToNsiConversionRepositoryMock;
        private Mock<IPlanBuyingInventoryQuarterCalculatorEngine> _PlanBuyingInventoryQuarterCalculatorEngineMock;
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
            _PlanBuyingInventoryQuarterCalculatorEngineMock = new Mock<IPlanBuyingInventoryQuarterCalculatorEngine>();
            _MediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();

            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _StationRepositoryMock = new Mock<IStationRepository>();
            _DaypartCacheMock = new Mock<IDaypartCache>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
            _StandardDaypartRepository = _GetMockDaypartDefaultRepository();

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

        private PlanBuyingInventoryEngineTestClass _GetTestClass()
        {
            // setup feature flags
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);

            var testClass = new PlanBuyingInventoryEngineTestClass(
                _DataRepositoryFactoryMock.Object,
                _ImpressionsCalculationEngineMock.Object,
                _PlanBuyingInventoryQuarterCalculatorEngineMock.Object,
                _MediaMonthAndWeekAggregateCache.Object,
                _DaypartCacheMock.Object,
                _QuarterCalculationEngineMock.Object,
                _SpotLengthEngineMock.Object,
                featureToggleHelper,
                _ConfigurationSettingsHelper.Object);

            return testClass;
        }

        private Mock<IStandardDaypartRepository> _GetMockDaypartDefaultRepository()
        {
            var daypartDefaultRepository = new Mock<IStandardDaypartRepository>();

            daypartDefaultRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);

            daypartDefaultRepository.Setup(s => s.GetAllStandardDaypartsWithAllData())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithFullData);

            var testDefaultDays = DaypartsTestData.GetDayIdsFromStandardDayparts();
            daypartDefaultRepository.Setup(s => s.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>()))
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
        [TestCase(null, null, 4)]
        [TestCase(12.00, null, 3)]
        [TestCase(null, 15.00, 3)]
        [TestCase(12.00, 15.00, 2)]
        public void ExcludeProgramsWithOutOfBoundsMinAndMaxCPM(double? minCPM, double? maxCPM, int expectedCount)
        {
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 350.00M,
                        }
                    },
                    ProjectedImpressions = 33615.275,
                    ProvidedImpressions = 29600.0
                },
                new PlanBuyingInventoryProgram
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 400.00M,
                        }
                    },
                    ProjectedImpressions = 44998.25,
                    ProvidedImpressions = 30200.0
                },
                new PlanBuyingInventoryProgram
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 375.00M,
                        }
                    },
                    ProjectedImpressions = 30683.0,
                    ProvidedImpressions = 21800.0
                },
                new PlanBuyingInventoryProgram
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M,
                        }
                    },
                    ProjectedImpressions = 40034.6875,
                    ProvidedImpressions = 23400.0
                }
            };
            var testEngine = _GetTestClass();

            var result = testEngine._CalculateProgramCpmAndFilterByMinAndMaxCpms(programs, (decimal?)minCPM, (decimal?)maxCPM);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatesProgramCPM()
        {
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 50.00M,
                        }
                    },
                    ProvidedImpressions = 10000
                }
            };
            var testEngine = _GetTestClass();

            var result = testEngine._CalculateProgramCpmAndFilterByMinAndMaxCpms(programs, null, null);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatesProgramCPM_v3()
        {
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 2,
                            Cost = 50.00M,
                        },
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 25.00M,
                        }
                    },
                    ProvidedImpressions = 10000
                }
            };

            var testEngine = _GetTestClass();
            var result = testEngine._CalculateProgramCpmAndFilterByMinAndMaxCpms(programs, null, null);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(null, 300.0)]
        [TestCase(5.0, 315.0)]
        [TestCase(10.0, 330.0)]
        [TestCase(4.5, 313.5)]
        public void ApplyInflationToSpotCost_ProgramsWithPlanInventoryBuyingQuarterType(double? inflationFactor, double expectedResult)
        {
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    InventoryBuyingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M
                        }
                    }
                }
            };

            var testEngine = _GetTestClass();
            testEngine._ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.Single().ManifestRates.Single().Cost);
        }

        [Test]
        [TestCase(null, 300.0)]
        [TestCase(5.0, 315.0)]
        [TestCase(10.0, 330.0)]
        [TestCase(4.5, 313.5)]
        public void ApplyInflationToSpotCost_ProgramsWithFallbackInventoryBuyingQuarterType(double? inflationFactor, double expectedResult)
        {
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    InventoryBuyingQuarterType = InventoryPricingQuarterType.Fallback,
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M
                        }
                    }
                }
            };
            var testEngine = _GetTestClass();

            testEngine._ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.Single().ManifestRates.Single().Cost);
        }

        [Test]
        [TestCase(null, 300.0)]
        [TestCase(5.0, 315.0)]
        [TestCase(10.0, 330.0)]
        [TestCase(4.5, 313.5)]
        public void ApplyInflationToSpotCost_ProgramsWithMixedInventoryBuyingQuarterType(double? inflationFactor, double expectedResult)
        {
            const decimal defaultSpotCost = 300;
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    InventoryBuyingQuarterType = InventoryPricingQuarterType.Fallback,
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = defaultSpotCost
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    InventoryBuyingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = defaultSpotCost
                        }
                    }
                }
            };
            var testEngine = _GetTestClass();

            testEngine._ApplyInflationFactorToSpotCost(programs, inflationFactor);

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
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram // CPM 11.8243
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 350.00M
                        }
                    },
                    ProvidedImpressions = 29600.0
                },
                new PlanBuyingInventoryProgram // CPM 13.2450
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 400.00M
                        }
                    },
                    ProvidedImpressions = 30200.0
                },
                new PlanBuyingInventoryProgram // CPM 17.2018
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 375.00M
                        }
                    },
                    ProvidedImpressions = 21800.0
                },
                new PlanBuyingInventoryProgram // CPM 12.8205
                {
                    ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                    {
                        new PlanBuyingInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 300.00M
                        }
                    },
                    ProvidedImpressions = 23400.0
                }
            };
            var testEngine = _GetTestClass();

            testEngine._ApplyInflationFactorToSpotCost(programs, inflationFactor);
            var result = testEngine._CalculateProgramCpmAndFilterByMinAndMaxCpms(programs, (decimal?)minCPM, (decimal?)maxCPM);

            Assert.AreEqual(expectedCount, result.Count);
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
            var testEngine = _GetTestClass();

            var result = testEngine._GetDaypartDaysFromFlight(flightDays, planFlightDateRanges, planDaypartDayIds);

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
            var testEngine = _GetTestClass();

            var result = testEngine._GetDaypartDaysFromFlight(planFlightDays, planFlightDateRanges, planDaypartDayIds);

            Assert.AreEqual(expectedDaypart, result);
        }

        [Test]
        public void DoesNotApplyNTIConversionToNSI_WhenPlanPostingType_IsNotNTI()
        {
            var plan = _GetPlan();
            plan.PostingType = PostingTypeEnum.NSI;
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    ProjectedImpressions = 1000,
                    ProvidedImpressions = 2000,
                    StandardDaypartId=1
                }
            };
            var testEngine = _GetTestClass();

            testEngine._ApplyNTIConversionToNSI(plan, programs);

            Assert.AreEqual(750, programs.Single().ProjectedImpressions);
            Assert.AreEqual(1500, programs.Single().ProvidedImpressions);
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
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    StandardDaypartId = 2,
                    ProjectedImpressions = 1000,
                    ProvidedImpressions = 2000,
                    ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                    {
                        new PlanBuyingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Monday = true,
                                StartTime = 28800, // 8am
                                EndTime = 37799 // 10:30am
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>()
                }
            };
            var testEngine = _GetTestClass();

            testEngine._ApplyNTIConversionToNSI(plan, programs);

            Assert.AreEqual(850, programs.Single().ProjectedImpressions);
            Assert.AreEqual(1700, programs.Single().ProvidedImpressions);
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
                   new PlanBuyingInventoryProgram
                   {
                       Station = new DisplayBroadcastStation { Id = s.Id, LegacyCallLetters = s.LegacyCallLetters },
                       ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>()
                   })
                .ToList();

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.Setup(s => s.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventory);
            var testEngine = _GetTestClass();

            /*** Act ***/
            var result = testEngine._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters);

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

            var inventoryOne = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 1, LegacyCallLetters = availableStations[0].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 2, LegacyCallLetters = availableStations[1].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 3, LegacyCallLetters = availableStations[2].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };
            var inventoryTwo = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 4, LegacyCallLetters = availableStations[3].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 5, LegacyCallLetters = availableStations[4].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 6, LegacyCallLetters = availableStations[5].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo);
            var testEngine = _GetTestClass();

            /*** Act ***/
            var result = testEngine._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters);

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

            var inventoryOne = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 1, LegacyCallLetters = availableStations[0].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 2, LegacyCallLetters = availableStations[1].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 3, LegacyCallLetters = availableStations[2].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };
            var inventoryTwo = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 4, LegacyCallLetters = availableStations[3].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 5, LegacyCallLetters = availableStations[4].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 6, LegacyCallLetters = availableStations[5].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 826
                        }
                    }
                },
            };

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(planQuarter);
            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo)
                .Returns(inventoryOne)
                .Returns(inventoryTwo);
            var testEngine = _GetTestClass();

            /*** Act ***/
            var result = testEngine._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters);

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

            var inventoryOne = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 1, LegacyCallLetters = availableStations[0].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>()
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 2, LegacyCallLetters = availableStations[1].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>()
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 3, LegacyCallLetters = availableStations[2].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>()
                }
            };
            var inventoryTwo = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 4, LegacyCallLetters = availableStations[3].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>()
                },
                new PlanBuyingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { Id = 5, LegacyCallLetters = availableStations[4].LegacyCallLetters },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>()
                }
            };

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo);

            var testEngine = _GetTestClass();

            /*** Act ***/
            var result = testEngine._GetFullPrograms(flightDateRanges, new List<int> { spotLengthId }, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarters);

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
        public void UsesSingleBook_ToProjectImpressions_WhenGatheringInventory_ForBuying()
        {
            // Arrange
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanBuyingJobDiagnostic();

            parameters.HUTBookId = 101;
            parameters.ShareBookId = 102;
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
                .Setup(x => x.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        SpotLengthId = 1,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "KOB"
                        },
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Id = 2
                            }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                Id = 5,
                                Daypart = new DisplayDaypart { Id = 5 },
                                PrimaryProgramId = 0,
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program()
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 10
                            }
                        }
                    }
                });

            _PlanBuyingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanBuyingInventoryQuarterCalculatorEngineMock
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
                    It.IsAny<IEnumerable<PlanBuyingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanBuyingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
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
            var testEngine = _GetTestClass();

            // Act
            var inventory = testEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

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
            var diagnostic = new PlanBuyingJobDiagnostic();

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 202;
            plan.PostingType = PostingTypeEnum.NSI;
            plan.ShareBookId = 201;
            parameters.HUTBookId = 202;
            parameters.ShareBookId = 201;

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
                .Setup(x => x.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        SpotLengthId = programSpotLengthId,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "KOB"
                        },
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Id = 2
                            }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                Id = 5,
                                Daypart = new DisplayDaypart { Id = 5 },
                                PrimaryProgramId = 0,
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program()
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = programSpotLengthId,
                                Cost = (10 * programSpotLengthId)
                            }
                        }
                    }
                });

            _PlanBuyingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanBuyingInventoryQuarterCalculatorEngineMock
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
                    It.IsAny<IEnumerable<PlanBuyingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanBuyingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
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
            var testEngine = _GetTestClass();

            // Act
            var inventory = testEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void UsesTwoBooks_ToProjectImpressions_WhenGatheringInventory_ForBuying()
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
        public void ProjectsImpressions_WhenGatheringInventory_ForBuying_v3()
        {
            // Arrange
            
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanBuyingJobDiagnostic();
            parameters.HUTBookId = 101;
            parameters.ShareBookId = 102;
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
                .Setup(x => x.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        SpotLengthId = 1,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "KOB"
                        },
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Id = 2
                            }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                Id = 5,
                                Daypart = new DisplayDaypart {Id = 5},
                                PrimaryProgramId = 0,
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program()
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 10
                            }
                        }
                    }
                });

            _PlanBuyingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanBuyingInventoryQuarterCalculatorEngineMock
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
                    It.IsAny<IEnumerable<PlanBuyingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanBuyingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
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
            var testEngine = _GetTestClass();

            // Act
            var inventory = testEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void AppliesProvidedImpressions_WhenGatheringInventory_ForBuying_v3()
        {
            // Arrange
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanBuyingJobDiagnostic();

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
                .Setup(x => x.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        SpotLengthId = 1,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "KOB"
                        },
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Id = 2
                            }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                Id = 5,
                                Daypart = new DisplayDaypart { Id = 5 },
                                PrimaryProgramId = 2,
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program()
                                    {
                                        Id = 2
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 10
                            }
                        }
                    }
                });

            _PlanBuyingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetFallbackDateRanges(
                    It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(),
                    It.IsAny<QuarterDetailDto>()))
                .Returns(new List<DateRange>());

            _PlanBuyingInventoryQuarterCalculatorEngineMock
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
                    It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Callback<List<PlanBuyingInventoryProgram>, int?, int, bool>((programs, audienceId, spotLengthId, equivalized) =>
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
            var testEngine = _GetTestClass();

            // Act
            var inventory = testEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void GetInventoryForPlanWkd()
        {
            // Arrange
            var wkdDaypart = DaypartsTestData.GetAllStandardDaypartsWithFullData().Single(s => s.Code.Equals("WKD"));
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanBuyingJobDiagnostic();
            parameters.HUTBookId = 101;
            parameters.ShareBookId = 102;
            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var flightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2020, 05, 01),
                    new DateTime(2020, 05, 30))
            };

            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 101;
            plan.PostingType = PostingTypeEnum.NTI;
            plan.ShareBookId = 102;
            plan.CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 5, Weight = 50 } };
            plan.AudienceId = 12;
            plan.FlightDays = new List<int> {1, 2, 3, 4, 5, 6, 7};
            plan.FlightStartDate = new DateTime(2020, 1, 1);
            plan.FlightEndDate = new DateTime(2020, 1, 14);
            plan.Dayparts = new List<PlanDaypartDto>
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
                            ContainType = ContainTypeEnum.Include,
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

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        Station = StationsTestData.GetStation(1),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 2,
                                Daypart = new DisplayDaypart {Id = wkdDaypart.Id},
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program A"
                                    },
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 2,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        Station = StationsTestData.GetStation(2),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 2 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program C"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 3,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 3 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 4,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 4 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 5,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 1 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    }
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

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(planQuarter);

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            object parametersApplyProvidedImpressions = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(
                    It.IsAny<IEnumerable<PlanBuyingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanBuyingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
                {
                    // deep copy
                    parametersApplyProvidedImpressions = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
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
            var testEngine = _GetTestClass();

            // Act
            var inventory = testEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                inventory,
                parametersGetBroadcastStationsWithLatestDetailsByMarketCodes,
                parametersGetLatestStationMonthDetailsForStations,
                parametersApplyProvidedImpressions
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void GetInventoryForPlanWkdWithNoPrograms()
        {
            // Arrange
            var wkdDaypart = DaypartsTestData.GetAllStandardDaypartsWithFullData().Single(s => s.Code.Equals("WKD"));
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanBuyingJobDiagnostic();

            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var flightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2020, 05, 01),
                    new DateTime(2020, 05, 30))
            };

            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };
            parameters.HUTBookId = 101;
            parameters.ShareBookId = 102;
            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 101;
            plan.PostingType = PostingTypeEnum.NTI;
            plan.ShareBookId = 102;
            plan.CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 5, Weight = 50 } };
            plan.AudienceId = 12;
            plan.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            plan.FlightStartDate = new DateTime(2020, 1, 1);
            plan.FlightEndDate = new DateTime(2020, 1, 14);
            plan.Dayparts = new List<PlanDaypartDto>
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
                            ContainType = ContainTypeEnum.Include,
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

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        Station = StationsTestData.GetStation(1),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 2,
                                Daypart = new DisplayDaypart {Id = wkdDaypart.Id},
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program A"
                                    },
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 2,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        Station = StationsTestData.GetStation(2),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 2 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program C"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 3,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 3 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 4,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 4 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 5,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 1 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    }
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

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(planQuarter);

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            object parametersApplyProvidedImpressions = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(
                    It.IsAny<IEnumerable<PlanBuyingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanBuyingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
                {
                    // deep copy
                    parametersApplyProvidedImpressions = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
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
            var testEngine = _GetTestClass();

            // Act
            var inventory = testEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                inventory,
                parametersGetBroadcastStationsWithLatestDetailsByMarketCodes,
                parametersGetLatestStationMonthDetailsForStations,
                parametersApplyProvidedImpressions
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        public void GetInventoryForPlanWkdMix()
        {
            // Arrange
            var wkdDaypart = DaypartsTestData.GetAllStandardDaypartsWithFullData().Single(s => s.Code.Equals("WKD"));
            var parameters = new ProgramInventoryOptionalParametersDto();
            var inventorySourceIds = new List<int>();
            var diagnostic = new PlanBuyingJobDiagnostic();

            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var flightDateRanges = new List<DateRange>
            {
                new DateRange(new DateTime(2020, 05, 01),
                    new DateTime(2020, 05, 30))
            };

            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };
            parameters.HUTBookId = 101;
            parameters.ShareBookId = 102;
            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.HUTBookId = 101;
            plan.PostingType = PostingTypeEnum.NTI;
            plan.ShareBookId = 102;
            plan.CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 5, Weight = 50 } };
            plan.AudienceId = 12;
            plan.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            plan.FlightStartDate = new DateTime(2020, 1, 1);
            plan.FlightEndDate = new DateTime(2020, 1, 14);
            plan.Dayparts = new List<PlanDaypartDto>
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
                            ContainType = ContainTypeEnum.Include,
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
                            ContainType = ContainTypeEnum.Include,
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

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForBuyingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        Station = StationsTestData.GetStation(1),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 2,
                                Daypart = new DisplayDaypart {Id = wkdDaypart.Id},
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program A"
                                    },
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 2,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        Station = StationsTestData.GetStation(2),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 2 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program C"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 3,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 3 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 4,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 4 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program D"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 5,
                        Station = StationsTestData.GetStation(3),
                        SpotLengthId = 1,
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek { Id = 12 }
                        },
                        ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                        {
                            new PlanBuyingInventoryProgram.ManifestDaypart
                            {
                                PrimaryProgramId = 1,
                                Daypart = new DisplayDaypart {Id = 1 },
                                Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                                {
                                    new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = 1,
                                        Name = "Program B"
                                    }
                                }
                            }
                        },
                        ManifestRates = new List<PlanBuyingInventoryProgram.ManifestRate>
                        {
                            new PlanBuyingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 15
                            }
                        }
                    }
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

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetPlanQuarter(It.IsAny<PlanDto>()))
                .Returns(planQuarter);

            _PlanBuyingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            object parametersApplyProvidedImpressions = null;
            _ImpressionsCalculationEngineMock
                .Setup(x => x.ApplyProjectedImpressions(
                    It.IsAny<IEnumerable<PlanBuyingInventoryProgram>>(),
                    It.IsAny<ImpressionsRequestDto>(),
                    It.IsAny<int>()))
                .Callback<IEnumerable<PlanBuyingInventoryProgram>, ImpressionsRequestDto, int>((programs, request, audienceId) =>
                {
                    // deep copy
                    parametersApplyProvidedImpressions = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
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
            var testEngine = _GetTestClass();

            // Act
            var inventory = testEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                inventory,
                parametersGetBroadcastStationsWithLatestDetailsByMarketCodes,
                parametersGetLatestStationMonthDetailsForStations,
                parametersApplyProvidedImpressions
            });

            Approvals.Verify(resultJson);
        }
    }
}
