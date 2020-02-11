using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventoryEngineUnitTests
    {
        private readonly Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private readonly Mock<IImpressionsCalculationEngine> _ImpressionsCalculationEngineMock;
        private readonly Mock<IGenreCache> _GenreCache;
        private readonly PlanPricingInventoryEngineTestClass _PlanPricingInventoryEngine;

        public PlanPricingInventoryEngineUnitTests()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _ImpressionsCalculationEngineMock = new Mock<IImpressionsCalculationEngine>();
            _GenreCache = new Mock<IGenreCache>();

            _PlanPricingInventoryEngine = new PlanPricingInventoryEngineTestClass(
                _DataRepositoryFactoryMock.Object,
                _ImpressionsCalculationEngineMock.Object,
                _GenreCache.Object);
        }

        [Test]
        public void NoPlanDayparts()
        {
            var plan = _GetPlan();
            var programs = new List<PlanPricingInventoryProgram> { new PlanPricingInventoryProgram() };
            var expectedCount = programs.Count;
            var planFlightDateRanges = _GetPlanFlightDateRanges();

            plan.Dayparts = new List<PlanDaypartDto>();

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void DoesNotIncludeProgram_WhenDoesNotMatchFlightDays()
        {
            const int expectedCount = 0;

            var plan = _GetPlan();
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                    }
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void DoesNotIncludeProgram_WhenDoesNotMatchDaypartTime()
        {
            const int expectedCount = 0;

            var plan = _GetPlan();
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                    }
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void IncludesProgram_WhenNoRestrictionsSpecified()
        {
            const int expectedCount = 1;

            var plan = _GetPlan();
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                    }
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

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
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    Name = inventoryProgramName
                                }
                            }
                        }
                    }
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

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
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    MaestroGenre = inventoryGenre
                                }
                            }
                        }
                    }
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

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
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = inventoryShowType
                                }
                            }
                        }
                    }
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void DoesNotIncludeProgram_WhenItIsBothIncludedAndExcluded()
        {
            const int expectedCount = 0;

            var plan = _GetPlan();
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Early news"
                                }
                            }
                        }
                    }
                }
            };

            plan.Dayparts.Insert(0, new PlanDaypartDto
            {
                StartTimeSeconds = 36000, // 10am
                EndTimeSeconds = 39599 // 11am
            });

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void IncludesProgramTest()
        {
            const int expectedCount = 1;

            var plan = _GetPlan();
            var planFlightDateRanges = _GetPlanFlightDateRanges();

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
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Early news",
                                    MaestroGenre = "Comedy"
                                }
                            }
                        }
                    }
                }
            };

            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByDayparts(plan, programs, planFlightDateRanges);

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
        public void ApplyInflationToSpotCost(double? inflationFactor, double expectedResult)
        {
            var program = new PlanPricingInventoryProgram
            {
                SpotCost = 300.00M,
            };

            _PlanPricingInventoryEngine.UT_ApplyInflationFactorToSpotCost(program, inflationFactor);

            Assert.AreEqual((decimal)expectedResult, program.SpotCost);
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
            
            programs.ForEach(program => _PlanPricingInventoryEngine.UT_ApplyInflationFactorToSpotCost(program, inflationFactor));
            var result = _PlanPricingInventoryEngine.UT_FilterProgramsByMinAndMaxCPM(programs, (decimal?)minCPM, (decimal?)maxCPM);

            Assert.AreEqual(expectedCount, result.Count);
        }

        private PlanDto _GetPlan()
        {
            return new PlanDto
            {
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
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
    }
}
