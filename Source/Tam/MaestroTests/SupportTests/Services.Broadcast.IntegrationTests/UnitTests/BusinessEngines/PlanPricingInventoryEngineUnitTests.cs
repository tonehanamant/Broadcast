﻿using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Microsoft.Practices.Unity;
using ApprovalTests.Reporters;
using ApprovalTests;
using IntegrationTests.Common;
using static Services.Broadcast.BusinessEngines.PlanPricingInventoryEngine;
using Common.Services;
using Newtonsoft.Json;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class PlanPricingInventoryEngineUnitTests
    {
        private PlanPricingInventoryEngineTestClass _PlanPricingInventoryEngine;
        private IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IImpressionsCalculationEngine> _ImpressionsCalculationEngineMock;
        private Mock<IGenreCache> _GenreCacheMock;
        private Mock<IDayRepository> _DayRepositoryMock;
        private Mock<INtiToNsiConversionRepository> _NtiToNsiConversionRepositoryMock;
        private Mock<IPlanPricingInventoryQuarterCalculatorEngine> _PlanPricingInventoryQuarterCalculatorEngineMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IStationProgramRepository> _StationProgramRepositoryMock;
        private Mock<IStationRepository> _StationRepositoryMock;
        private Mock<IDaypartCache> _DaypartCacheMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _ImpressionsCalculationEngineMock = new Mock<IImpressionsCalculationEngine>();
            _GenreCacheMock = new Mock<IGenreCache>();
            _DayRepositoryMock = new Mock<IDayRepository>();
            _NtiToNsiConversionRepositoryMock = new Mock<INtiToNsiConversionRepository>();
            _PlanPricingInventoryQuarterCalculatorEngineMock = new Mock<IPlanPricingInventoryQuarterCalculatorEngine>();
            _MediaMonthAndWeekAggregateCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IMediaMonthAndWeekAggregateCache>();

            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _StationRepositoryMock = new Mock<IStationRepository>();
            _DaypartCacheMock = new Mock<IDaypartCache>();

            _DayRepositoryMock
                .Setup(x => x.GetDays())
                .Returns(_GetDays());

            _NtiToNsiConversionRepositoryMock
                .Setup(x => x.GetLatestNtiToNsiConversionRates())
                .Returns(_GetNtiToNsiConversionRates());

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

            _PlanPricingInventoryEngine = new PlanPricingInventoryEngineTestClass(
                _DataRepositoryFactoryMock.Object,
                _ImpressionsCalculationEngineMock.Object,
                _GenreCacheMock.Object,
                _PlanPricingInventoryQuarterCalculatorEngineMock.Object,
                _MediaMonthAndWeekAggregateCache,
                _DaypartCacheMock.Object);
        }

        [Test]
        public void DoesNotIncludeProgram_WhenDoesNotMatchFlightDays()
        {
            const int expectedCount = 0;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = false,
                                Tuesday = false,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = false,
                                Sunday = false,
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void DoesNotIncludeProgram_WhenDoesNotMatchDaypartTime()
        {
            const int expectedCount = 0;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 46800, // 13pm
                                EndTime = 50399, // 14pm
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void IncludesProgram_WhenNoRestrictionsSpecified()
        {
            const int expectedCount = 1;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [TestCase("Early news", ContainTypeEnum.Exclude, "Early news", 0)]
        [TestCase("Early news", ContainTypeEnum.Include, "Early news", 1)]
        [TestCase("Early news", ContainTypeEnum.Exclude, "Late news", 1)]
        [TestCase("Early news", ContainTypeEnum.Include, "Late news", 0)]
        public void ProgramNameRestrictionsTest(
            string restrictedProgramName,
            ContainTypeEnum containType,
            string inventoryProgramName,
            int expectedCount)
        {
            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            plan.Dayparts.First().Restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = containType,
                Programs = new List<ProgramDto>
                {
                    new ProgramDto
                    {
                        Name = restrictedProgramName
                    }
                }
            };

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Name = inventoryProgramName
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    Name = inventoryProgramName
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [TestCase("Comedy", ContainTypeEnum.Exclude, "Comedy", 0)]
        [TestCase("Comedy", ContainTypeEnum.Include, "Comedy", 1)]
        [TestCase("Comedy", ContainTypeEnum.Exclude, "Crime", 1)]
        [TestCase("Comedy", ContainTypeEnum.Include, "Crime", 0)]
        public void GenreRestrictionsTest(
            string restrictedGenre,
            ContainTypeEnum containType,
            string inventoryGenre,
            int expectedCount)
        {
            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            plan.Dayparts.First().Restrictions.GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
            {
                ContainType = containType,
                Genres = new List<LookupDto>
                {
                    new LookupDto
                    {
                        Display = restrictedGenre
                    }
                }
            };

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Genre = inventoryGenre
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    Genre = inventoryGenre
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [TestCase("Mini-Movie", ContainTypeEnum.Exclude, "Mini-Movie", 0)]
        [TestCase("Mini-Movie", ContainTypeEnum.Include, "Mini-Movie", 1)]
        [TestCase("Mini-Movie", ContainTypeEnum.Exclude, "Sports", 1)]
        [TestCase("Mini-Movie", ContainTypeEnum.Include, "Sports", 0)]
        public void ShowTypeRestrictionsTest(
            string restrictedShowType,
            ContainTypeEnum containType,
            string inventoryShowType,
            int expectedCount)
        {
            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            plan.Dayparts.First().Restrictions.ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
            {
                ContainType = containType,
                ShowTypes = new List<LookupDto>
                {
                    new LookupDto
                    {
                        Display = restrictedShowType
                    }
                }
            };

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = inventoryShowType
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = inventoryShowType
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void DoesNotIncludeProgram_WhenItIsBothIncludedAndExcluded()
        {
            const int expectedCount = 0;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var restrictions = plan.Dayparts.First().Restrictions;
            restrictions.ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
            {
                ContainType = ContainTypeEnum.Exclude,
                ShowTypes = new List<LookupDto>
                {
                    new LookupDto
                    {
                        Display = "Sports"
                    }
                }
            };
            restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = ContainTypeEnum.Include,
                Programs = new List<ProgramDto>
                {
                    new ProgramDto
                    {
                        Name = "Early news"
                    }
                }
            };

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Sports",
                                Name = "Early news"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Early news"
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void IncludesProgram_WhenItIsExcludedByAnotherDaypart()
        {
            const int expectedCount = 1;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var restrictions = plan.Dayparts.First().Restrictions;
            restrictions.ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
            {
                ContainType = ContainTypeEnum.Exclude,
                ShowTypes = new List<LookupDto>
                {
                    new LookupDto
                    {
                        Display = "Sports"
                    }
                }
            };

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Sports",
                                Name = "Early news"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Early news"
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            plan.Dayparts.Add(new PlanDaypartDto
            {
                StartTimeSeconds = 36000, // 10am
                EndTimeSeconds = 39599 // 11am
            });

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void IncludesProgramTest()
        {
            const int expectedCount = 1;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var restrictions = plan.Dayparts.First().Restrictions;
            restrictions.ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
            {
                ContainType = ContainTypeEnum.Exclude,
                ShowTypes = new List<LookupDto>
                {
                    new LookupDto
                    {
                        Display = "Sports"
                    }
                }
            };
            restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = ContainTypeEnum.Include,
                Programs = new List<ProgramDto>
                {
                    new ProgramDto
                    {
                        Name = "Early news"
                    }
                }
            };
            restrictions.GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
            {
                ContainType = ContainTypeEnum.Include,
                Genres = new List<LookupDto>
                {
                    new LookupDto
                    {
                        Display = "Comedy"
                    }
                }
            };

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Mini-Movie",
                                Name = "Early news",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Early news",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Mini-Movie",
                                Name = "Late news",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Early news",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 36000, // 10am
                                EndTime = 39599, // 11am
                                Monday = true,
                                Tuesday = true,
                                Wednesday = true,
                                Thursday = true,
                                Friday = true,
                                Saturday = true,
                                Sunday = true,
                            },
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Mini-Movie",
                                Name = "Late news",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Early news",
                                    Genre = "Comedy"
                                },
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Early news",
                                    Genre = "Sports"
                                }
                            }
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDaypartsAndAssociateWithAppropriateStandardDaypart(plan, programs, planFlightDays);

            Assert.AreEqual(expectedCount, result.Count);
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
                    SpotCost = 350.00M,
                    ProjectedImpressions = 33615.275,
                    ProvidedImpressions = 29600.0
                },
                new PlanPricingInventoryProgram
                {
                    SpotCost = 400.00M,
                    ProjectedImpressions = 44998.25,
                    ProvidedImpressions = 30200.0
                },
                new PlanPricingInventoryProgram
                {
                    SpotCost = 375.00M,
                    ProjectedImpressions = 30683.0,
                    ProvidedImpressions = 21800.0
                },
                new PlanPricingInventoryProgram
                {
                    SpotCost = 300.00M,
                    ProjectedImpressions = 40034.6875,
                    ProvidedImpressions = 23400.0
                }
            };
            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByMinAndMaxCPM(programs, (decimal?)minCPM, (decimal?)maxCPM);

            Assert.AreEqual(expectedCount, result.Count);
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
                    SpotCost = 300.00M,
                }
            };

            _PlanPricingInventoryEngine.UT_ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.Single().SpotCost);
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
                    SpotCost = 300.00M,
                }
            };

            _PlanPricingInventoryEngine.UT_ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.Single().SpotCost);
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
                    SpotCost = defaultSpotCost,
                },
                new PlanPricingInventoryProgram
                {
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = defaultSpotCost
                }
            };

            _PlanPricingInventoryEngine.UT_ApplyInflationFactorToSpotCost(programs, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, programs.FirstOrDefault().SpotCost);
            Assert.AreEqual(defaultSpotCost, programs.LastOrDefault().SpotCost);
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
                    SpotCost = 350.00M,
                    ProvidedImpressions = 29600.0
                },
                new PlanPricingInventoryProgram // CPM 13.2450
                {
                    SpotCost = 400.00M,
                    ProvidedImpressions = 30200.0
                },
                new PlanPricingInventoryProgram // CPM 17.2018
                {
                    SpotCost = 375.00M,
                    ProvidedImpressions = 21800.0
                },
                new PlanPricingInventoryProgram // CPM 12.8205
                {
                    SpotCost = 300.00M,
                    ProvidedImpressions = 23400.0
                }
            };

            _PlanPricingInventoryEngine.UT_ApplyInflationFactorToSpotCost(programs, inflationFactor);
            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByMinAndMaxCPM(programs, (decimal?)minCPM, (decimal?)maxCPM);

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
            var plan = new PlanDto
            {
                FlightDays = new List<int> { 1, 3, 5, 7 }
            };

            var result = _PlanPricingInventoryEngine.UT_GetPlanDaypartDaysFromPlanFlight(plan, planFlightDateRanges);

            Assert.AreEqual(expectedDaypart, result);
        }

        [Test]
        public void DoesNotApplyNTIConversionToNSI_WhenPlanPostingType_IsNotNTI()
        {
            var plan = _GetPlan();
            plan.PostingType = PostingTypeEnum.NSI;
            var planFlightDays = _GetPlanFlightDays();
            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ProjectedImpressions = 1000,
                    ProvidedImpressions = 2000
                }
            };

            _PlanPricingInventoryEngine.UT_ApplyNTIConversionToNSI(plan, programs, planFlightDays);

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

            var planFlightDays = _GetPlanFlightDays();
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

            _PlanPricingInventoryEngine.UT_ApplyNTIConversionToNSI(plan, programs, planFlightDays);

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
            var fallbackQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };
            var availableMarkets = new List<short> { 1,2,3,4,5 };
            var availableStations = _GetAvailableStations(marketCount, stationPerMarketCount);

            var inventory = availableStations.Select(s =>
                   new PlanPricingInventoryProgram
                   {
                       Station = new DisplayBroadcastStation { LegacyCallLetters = s.LegacyCallLetters },
                       ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                   })
                .ToList();

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.Setup(s => s.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventory);

            /*** Act ***/
            var result = _PlanPricingInventoryEngine.UT_GetFullPrograms(flightDateRanges, spotLengthId, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarter);

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
        [UseReporter(typeof(DiffReporter))]
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
            var fallbackQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2019, 10, 1),
                EndDate = new DateTime(2019, 12, 31)
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };

            var availableMarkets = new List<short> { 1, 2, 3, 4, 5 };
            var availableStations = _GetAvailableStations(marketCount, stationPerMarketCount);

            var inventoryOne = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[0].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[1].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[2].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[3].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[4].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[5].LegacyCallLetters },
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
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo);

            /*** Act ***/
            var result = _PlanPricingInventoryEngine.UT_GetFullPrograms(flightDateRanges, spotLengthId, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarter);

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
        [UseReporter(typeof(DiffReporter))]
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
            var fallbackQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2019, 10, 1),
                EndDate = new DateTime(2019, 12, 31)
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };

            var availableMarkets = new List<short> { 1, 2, 3, 4, 5 };
            var availableStations = _GetAvailableStations(marketCount, stationPerMarketCount);

            var inventoryOne = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[0].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[1].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[2].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[3].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[4].LegacyCallLetters },
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
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[5].LegacyCallLetters },
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
            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetInventoryFallbackQuarter())
                .Returns(fallbackQuarter);
            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo)
                .Returns(inventoryOne)
                .Returns(inventoryTwo);

            /*** Act ***/
            var result = _PlanPricingInventoryEngine.UT_GetFullPrograms(flightDateRanges, spotLengthId, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarter);

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
            var fallbackQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2020, 09, 25),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallbackDateRanges = new List<DateRange>() { flightDateRanges[0] };
            var availableMarkets = new List<short> { 1, 2, 3, 4, 5 };
            var availableStations = _GetAvailableStations(marketCount, stationPerMarketCount);

            var inventoryOne = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[0].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[1].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[2].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };
            var inventoryTwo = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[3].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = availableStations[4].LegacyCallLetters },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>()
                }
            };

            _StationRepositoryMock.Setup(s => s.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(availableStations);

            _PlanPricingInventoryQuarterCalculatorEngineMock.Setup(s => s.GetFallbackDateRanges(It.IsAny<DateRange>(),
                    It.IsAny<QuarterDetailDto>(), It.IsAny<QuarterDetailDto>()))
                .Returns(fallbackDateRanges);

            _StationProgramRepositoryMock.SetupSequence(s => s.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(),
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Returns(inventoryOne)
                .Returns(inventoryTwo);

            /*** Act ***/
            var result = _PlanPricingInventoryEngine.UT_GetFullPrograms(flightDateRanges, spotLengthId, supportedInventorySourceTypes,
                availableMarkets, planQuarter, fallbackQuarter);

            /*** Assert ***/
            Assert.AreEqual(5, result.Count);
            foreach (var station in availableStations)
            {
                var expectedCount = station.LegacyCallLetters == availableStations[5].LegacyCallLetters ? 0 : 1;
                Assert.AreEqual(expectedCount, result.Count(i => i.Station.LegacyCallLetters == station.LegacyCallLetters));
            }
        }

        private List<DisplayBroadcastStation> _GetAvailableStations(int marketCount, int stationsPerMarket)
        {
            var availableStations = new List<DisplayBroadcastStation>();
            var stationId = 0;

            for (var i = 1; i <= marketCount; i++)
            {
                for (var j = 1; j <= stationsPerMarket; j++)
                {
                    var stationName = $"ST{(++stationId).ToString().PadLeft(2, '0')}";
                    availableStations.Add(new DisplayBroadcastStation { LegacyCallLetters = stationName, MarketCode = j });
                }
            }

            return availableStations;
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
        
        private List<NtiToNsiConversionRate> _GetNtiToNsiConversionRates()
        {
            return new List<NtiToNsiConversionRate>
            {
                new NtiToNsiConversionRate
                {
                    DaypartDefaultId = 1,
                    ConversionRate = 0.75
                },
                new NtiToNsiConversionRate
                {
                    DaypartDefaultId = 2,
                    ConversionRate = 0.85
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
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
            plan.SpotLengthId = 5;
            plan.CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 5, Weight = 50 } };
            plan.AudienceId = 25;

            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(new List<DisplayBroadcastStation>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(), 
                    It.IsAny<DateTime>(), 
                    It.IsAny<int>(),
                    It.IsAny<List<int>>(), 
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
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
                                Daypart = new DisplayDaypart()
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

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetInventoryFallbackQuarter())
                .Returns(new QuarterDetailDto
                {
                    StartDate = new DateTime(2020, 4, 1),
                    EndDate = new DateTime(2020, 6, 30)
                });

            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Returns(new List<StationMonthDetailDto>());
            
            _DaypartCacheMock
                .Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(new DisplayDaypart
                {
                    Monday = true,
                    StartTime = 36000, // 10am
                    EndTime = 39599 // 11am
                });

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

            // Act
            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UsesTwoBooks_ToProjectImpressions_WhenGatheringInventory_ForPricing()
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
            plan.SpotLengthId = 5;
            plan.CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 5, Weight = 50 } };
            plan.AudienceId = 25;

            _StationRepositoryMock
                .Setup(x => x.GetBroadcastStationsWithLatestDetailsByMarketCodes(It.IsAny<List<short>>()))
                .Returns(new List<DisplayBroadcastStation>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetProgramsForPricingModel(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
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
                                Daypart = new DisplayDaypart()
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

            _PlanPricingInventoryQuarterCalculatorEngineMock
                .Setup(x => x.GetInventoryFallbackQuarter())
                .Returns(new QuarterDetailDto
                {
                    StartDate = new DateTime(2020, 4, 1),
                    EndDate = new DateTime(2020, 6, 30)
                });

            _StationRepositoryMock
                .Setup(x => x.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>()))
                .Returns(new List<StationMonthDetailDto>());

            _DaypartCacheMock
                .Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(new DisplayDaypart
                {
                    Monday = true,
                    StartTime = 36000, // 10am
                    EndTime = 39599 // 11am
                });

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

            // Act
            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(plan, parameters, inventorySourceIds, diagnostic);

            // Assert
            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                passedParameters,
                inventory
            });

            Approvals.Verify(resultJson);
        }
    }
}
