using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.TestData;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class ProgramRestrictionsHelperUnitTests
    {
        private const int ThresholdInSecondsForProgramIntersect = 1800;
        private const bool UseTrueIndependentStations = true;

        [Test]
        public void DoesNotIncludeProgram_WhenDoesNotMatchFlightDays()
        {
            const int expectedCount = 0;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays, 
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [TestCase(46800, 50399, 0)] // 13pm-14pm
        [TestCase(36000, 37739, 0)] // 10am-10:29am, 29 minutes intersection
        [TestCase(36000, 37799, 1)] // 10am-10:30am, 30 minutes intersection
        public void MatchingByDaypartTime(int startTime, int endTime, int expectedCount)
        {
            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                StartTime = startTime,
                                EndTime = endTime,
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

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void IncludesProgram_WhenNoRestrictionsSpecified()
        {
            const int expectedCount = 1;

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

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

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
                        {
                            // Make sure this program name is not considered.
                            ProgramName = "SomethingToBeIgnored",
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                Name = inventoryProgramName
                            },
                            // Make sure only the primary program is considered
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Name = inventoryProgramName
                                },
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Name = "Early news"
                                },
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Name = "Late news"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

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

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                Genre = inventoryGenre
                            },
                            // Make sure only the primary program is considered
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Genre = inventoryGenre
                                },
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Genre = "Comedy"
                                },
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Genre = "Crime"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [TestCase("ABC", ContainTypeEnum.Include, "ABC", true, 3)]
        [TestCase("ABC", ContainTypeEnum.Include, "ABC", true, 3)]
        [TestCase("ABC", ContainTypeEnum.Include, "ABC", false, 3)]
        [TestCase("ABC", ContainTypeEnum.Include, "ABC", false, 3)]
        [TestCase("ABC", ContainTypeEnum.Exclude, "ABC", true, 0)]
        [TestCase("ABC", ContainTypeEnum.Exclude, "ABC", true, 0)]
        [TestCase("ABC", ContainTypeEnum.Exclude, "ABC", false, 0)]
        [TestCase("ABC", ContainTypeEnum.Exclude, "ABC", false, 0)]
        [TestCase("IND", ContainTypeEnum.Include, "IND", true, 3)]
        [TestCase("IND", ContainTypeEnum.Include, "IND", true, 3)]
        [TestCase("IND", ContainTypeEnum.Exclude, "IND", true, 0)]
        [TestCase("IND", ContainTypeEnum.Exclude, "IND", true, 0)]
        [TestCase("IND", ContainTypeEnum.Exclude, "IND", false, 3)]
        [TestCase("IND", ContainTypeEnum.Include, "CBS", true, 0)]
        public void AffiliateRestrictionsTest(
            string restrictedAffiliate,
            ContainTypeEnum containType,
            string inventoryAffiliate,
            bool isStationTrueIndependent,
            int expectedCount)
        {
            // setup feature flags

            var plan = _GetPlan();
            var planFlightDays = _GetPlanFlightDays();

            plan.Dayparts.First().Restrictions.AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
            {
                ContainType = containType,
                Affiliates = new List<LookupDto>
                {
                    new LookupDto
                    {
                        Display = restrictedAffiliate
                    }
                }
            };

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                Genre = "comedy"
                            },
                            // Make sure only the primary program is considered
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Genre = "Comedy"
                                }
                            }
                        }
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Affiliation = inventoryAffiliate,
                        IsTrueInd = isStationTrueIndependent
                    }
                },
                 new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                Genre = "comedy"
                            },
                            // Make sure only the primary program is considered
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Genre = "Comedy"
                                }
                            }
                        }
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Affiliation = inventoryAffiliate,
                        IsTrueInd = isStationTrueIndependent
                    }
                },
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                Genre = "comedy"
                            },
                            // Make sure only the primary program is considered
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Genre = "Comedy"
                                }
                            }
                        }
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Affiliation = inventoryAffiliate,
                        IsTrueInd = isStationTrueIndependent
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

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

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = inventoryShowType
                            },
                            // Make sure only the primary program is considered
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = inventoryShowType
                                },
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie"
                                },
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

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

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Sports",
                                Name = "Early news"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Early news"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

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

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Sports",
                                Name = "Early news"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Early news"
                                }
                            }
                        }
                    }
                }
            };

            plan.Dayparts.Add(new PlanDaypartDto
            {
                DaypartCodeId = 14,
                StartTimeSeconds = 36000, // 10am
                EndTimeSeconds = 39599 // 11am
            });

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void AssignsCorrectDaypartWkdMix(bool programPlaysOnWeekend)
        {
            // Expecting to have Daypart 1 to be selected
            const int expectedCount = 1;

            var plan = _GetPlan();
            var daypartsData = DaypartsTestData.GetAllStandardDaypartsWithFullData();
            var daypartEm = daypartsData.Single(s => s.Code == "EM");
            var daypartWkd = daypartsData.Single(s => s.Code == "WKD");

            // while WKD covers more time in 1 day, EM covers more time overall.
            var expectedProgramStandardDaypartId = daypartEm.Id;

            var restrictions = new PlanDaypartDto.RestrictionsDto
            {
                ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                {
                    ContainType = ContainTypeEnum.Include,
                    Programs = new List<ProgramDto>
                    {
                        new ProgramDto { Name = "Program B" }
                    }
                }
            };

            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartTypeId = daypartEm.DaypartType,
                    DaypartCodeId = daypartEm.Id,
                    StartTimeSeconds = 18000, // 5am
                    EndTimeSeconds = 21599, // 6am
                    Restrictions = restrictions
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = daypartWkd.DaypartType,
                    DaypartCodeId = daypartWkd.Id,
                    StartTimeSeconds = 18000, // 5am
                    EndTimeSeconds = 359999, // 10am
                    Restrictions = restrictions
                }
            };

            // this should account for 
            // - flight days of the week
            // - all daypart days of the week
            // in this case that is all days of the week
            var planCoveredDays = new DisplayDaypart
            {
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = true,
                Sunday = true
            };

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 999,
                                StartTime = 18000, // 5am
                                EndTime = 25199, // 7am
                                Monday = true,
                                Tuesday = false,
                                Wednesday = true,
                                Thursday = false,
                                Friday = true,
                                Saturday = programPlaysOnWeekend,
                                Sunday = programPlaysOnWeekend,
                            },
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Sports",
                                Name = "Program B"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Program B"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planCoveredDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
            Assert.AreEqual(expectedProgramStandardDaypartId, result.First().StandardDaypartId);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void AssignsCorrectDaypartWkdMixAndFlightDays(bool programPlaysOnFriday)
        {
            // Expecting to have Daypart 1 to be selected
            const int expectedCount = 1;

            var plan = _GetPlan();

            var daypartsData = DaypartsTestData.GetAllStandardDaypartsWithFullData();
            var daypartEm = daypartsData.Single(s => s.Code == "EM");
            var daypartWkd = daypartsData.Single(s => s.Code == "WKD");

            // while WKD covers more time in 1 day, EM covers more time overall.
            var expectedProgramStandardDaypartId = daypartWkd.Id;

            var restrictions = new PlanDaypartDto.RestrictionsDto
            {
                ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                {
                    ContainType = ContainTypeEnum.Include,
                    Programs = new List<ProgramDto>
                    {
                        new ProgramDto { Name = "Program B" }
                    }
                }
            };

            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartTypeId = daypartEm.DaypartType,
                    DaypartCodeId = daypartEm.Id,
                    StartTimeSeconds = 18000, // 5am
                    EndTimeSeconds = 21599, // 6am
                    Restrictions = restrictions
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = daypartWkd.DaypartType,
                    DaypartCodeId = daypartWkd.Id,
                    StartTimeSeconds = 18000, // 5am
                    EndTimeSeconds = 359999, // 10am
                    Restrictions = restrictions
                }
            };

            // this should account for 
            // - flight days of the week
            // - all daypart days of the week
            var planCoveredDays = new DisplayDaypart
            {
                Monday = false,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = true,
                Saturday = true,
                Sunday = true
            };

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 999,
                                StartTime = 18000, // 5am
                                EndTime = 25199, // 7am
                                Monday = true,
                                Tuesday = false,
                                Wednesday = true,
                                Thursday = false,
                                Friday = programPlaysOnFriday,
                                Saturday = true,
                                Sunday = true
                            },
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Sports",
                                Name = "Program B"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Program B"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planCoveredDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
            Assert.AreEqual(expectedProgramStandardDaypartId, result.First().StandardDaypartId);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void AssignsCorrectDaypartWkdMixAndFlightDaysTie(bool programPlaysOnSatWed)
        {
            // Expecting to have Daypart 1 to be selected
            const int expectedCount = 1;

            var plan = _GetPlan();
            var daypartsData = DaypartsTestData.GetAllStandardDaypartsWithFullData();
            var daypartEm = daypartsData.Single(s => s.Code == "EM");
            var daypartWkd = daypartsData.Single(s => s.Code == "WKD");

            // while WKD covers more time in 1 day, EM covers more time overall.
            var expectedProgramStandardDaypartId = daypartEm.Id;

            var restrictions = new PlanDaypartDto.RestrictionsDto
            {
                ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                {
                    ContainType = ContainTypeEnum.Include,
                    Programs = new List<ProgramDto>
                    {
                        new ProgramDto { Name = "Program B" }
                    }
                }
            };

            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartTypeId = daypartEm.DaypartType,
                    DaypartCodeId = daypartEm.Id,
                    StartTimeSeconds = 18000, // 5am
                    EndTimeSeconds = 21599, // 6am
                    Restrictions = restrictions
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = daypartWkd.DaypartType,
                    DaypartCodeId = daypartWkd.Id,
                    StartTimeSeconds = 18000, // 5am
                    EndTimeSeconds = 359999, // 10am
                    Restrictions = restrictions
                }
            };

            // this should account for 
            // - flight days of the week
            // - all daypart days of the week
            var planCoveredDays = new DisplayDaypart
            {
                Monday = false,
                Tuesday = false,
                Wednesday = true,
                Thursday = false,
                Friday = true,
                Saturday = true,
                Sunday = true
            };

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 999,
                                StartTime = 18000, // 5am
                                EndTime = 25199, // 7am
                                Monday = true,
                                Tuesday = false,
                                Wednesday = programPlaysOnSatWed,
                                Thursday = false,
                                Friday = true,
                                Saturday = programPlaysOnSatWed,
                                Sunday = true
                            },
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Sports",
                                Name = "Program B"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Sports",
                                    Name = "Program B"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planCoveredDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
            Assert.AreEqual(expectedProgramStandardDaypartId, result.First().StandardDaypartId);
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

            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Mini-Movie",
                                Name = "Early news",
                                Genre = "Comedy"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Early news",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    }
                },
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Mini-Movie",
                                Name = "Late news",
                                Genre = "News"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Late news",
                                    Genre = "News"
                                }
                            }
                        }
                    }
                },
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Mini-Movie",
                                Name = "Sports news",
                                Genre = "Sports"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Late news",
                                    Genre = "News"
                                },
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Mini-Movie",
                                    Name = "Sports news",
                                    Genre = "Sports"
                                }
                            }
                        }
                    }
                }
            };

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedCount, result.Count);
        }

        #region GenreOrProgramsTests

        [Test]
        [TestCase(false, true, 1, true, false)]
        [TestCase(true, true, 2, true, false)]
        [TestCase(true, false, 1, false, false)]
        [TestCase(false, false, 1, false, true)]
        public void GenreOrProgramIncludeExcludeTest(bool includeGenreComedy, bool includeFriends, 
            int expectedProgramCount, bool expectFriends, bool expectNews)
        {
            var plan = _GetPlan();
            var restrictions = plan.Dayparts.First().Restrictions;
            restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = includeFriends ? ContainTypeEnum.Include : ContainTypeEnum.Exclude,
                Programs = new List<ProgramDto> {new ProgramDto {Name = "Friends"}}
            };
            restrictions.GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
            {
                ContainType = includeGenreComedy ? ContainTypeEnum.Include : ContainTypeEnum.Exclude,
                Genres = new List<LookupDto> {new LookupDto {Display = "Comedy"}}
            };

            var planFlightDays = new DisplayDaypart
            {
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = true,
                Sunday = true
            };

            var programs = _GetProgramsForGenreOrProgramsTests();

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(expectedProgramCount, result.Count);
            if (expectFriends)
            {
                var foundProgram = result.SingleOrDefault(p =>
                    p.ManifestDayparts[0].PrimaryProgram.Name.Equals("Friends")
                    && p.ManifestDayparts[0].PrimaryProgram.Genre.Equals("Comedy")
                    );
                Assert.IsNotNull(foundProgram);
            }
            if (expectNews)
            {
                var foundProgram = result.SingleOrDefault(p =>
                    p.ManifestDayparts[0].PrimaryProgram.Name.Equals("AM News")
                    && p.ManifestDayparts[0].PrimaryProgram.Genre.Equals("News")
                );
                Assert.IsNotNull(foundProgram);
            }
        }

        [Test]
        public void GenreOrProgramTest_IncludeOtherGenre()
        {
            var plan = _GetPlan();
            var restrictions = plan.Dayparts.First().Restrictions;
            restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = ContainTypeEnum.Include,
                Programs = new List<ProgramDto>
                {
                    new ProgramDto
                    {
                        Name = "Friends"
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
                        Display = "News"
                    }
                }
            };

            var planFlightDays = new DisplayDaypart
            {
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = true,
                Sunday = true
            };

            var programs = _GetProgramsForGenreOrProgramsTests();

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Friends", result[0].ManifestDayparts[0].PrimaryProgram.Name);
            Assert.AreEqual("Comedy", result[0].ManifestDayparts[0].PrimaryProgram.Genre);
        }

        [Test]
        public void GenreOrProgramTest_IncludeWhenNoRestrictions()
        {
            var plan = _GetPlan();

            var planFlightDays = new DisplayDaypart
            {
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = true,
                Sunday = true
            };

            var programs = _GetProgramsForGenreOrProgramsTests();

            var result = ProgramRestrictionsHelper.FilterProgramsByDaypartAndSetStandardDaypart(plan.Dayparts, programs, planFlightDays,
                _GetCadentDayDefinitions(), DaypartsTestData.GetDayIdsFromStandardDayparts(),
                ThresholdInSecondsForProgramIntersect);

            Assert.AreEqual(programs.Count, result.Count);
        }

        [Test]
        public void ApplyGeneralFilterForPricingPrograms()
        {
            const int expectedCount = 2;
            List<PlanPricingInventoryProgram> allPrograms = _GetProgramsForApplyGeneralFilterForPricingProgramsTests();
            var result = ProgramRestrictionsHelper.ApplyGeneralFilterForPricingPrograms(allPrograms);
            Assert.AreEqual(expectedCount, result.Count);
        }

        public void ApplyGeneralFilterForBuyingPrograms()
        {
            const int expectedCount = 2;
            List<PlanBuyingInventoryProgram> allPrograms = _GetProgramsForApplyGeneralFilterForBuyingProgramsTests();
            var result = ProgramRestrictionsHelper.ApplyGeneralFilterForBuyingPrograms(allPrograms);
            Assert.AreEqual(expectedCount, result.Count);
        }

        private List<BasePlanInventoryProgram> _GetProgramsForGenreOrProgramsTests()
        {
            var programs = new List<BasePlanInventoryProgram>
            {
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Series",
                                Name = "Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Friends",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    }
                },
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Series",
                                Name = "Not-Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Not-Friends",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    }
                },
                new BasePlanInventoryProgram
                {
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        new BasePlanInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Series",
                                Name = "AM News",
                                Genre = "News"
                            },
                            Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                            {
                                new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "AM News",
                                    Genre = "News"
                                }
                            }
                        }
                    }
                }
            };
            return programs;
        }

        #endregion // #region GenreOrProgramsTests

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

        private List<Day> _GetCadentDayDefinitions()
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


        private List<PlanPricingInventoryProgram> _GetProgramsForApplyGeneralFilterForPricingProgramsTests()
        {
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
                                ShowType = "Series",
                                Name = "Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Friends",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    }
                },new PlanPricingInventoryProgram
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
                                ShowType = "Series",
                                Name = "Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Friends",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    }
                },new PlanPricingInventoryProgram
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
                                ShowType = "Series",
                                Name = "Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanPricingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanPricingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Friends",
                                    Genre = "Unmatched"
                                }
                            }
                        }
                    }
                }
            };
            return programs;
        }

        private List<PlanBuyingInventoryProgram> _GetProgramsForApplyGeneralFilterForBuyingProgramsTests()
        {
            var programs = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                    {
                        new PlanBuyingInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new PlanBuyingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Series",
                                Name = "Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Friends",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    }
                },new PlanBuyingInventoryProgram
                {
                    ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                    {
                        new PlanBuyingInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new PlanBuyingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Series",
                                Name = "Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Friends",
                                    Genre = "Comedy"
                                }
                            }
                        }
                    }
                },new PlanBuyingInventoryProgram
                {
                    ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                    {
                        new PlanBuyingInventoryProgram.ManifestDaypart
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
                            PrimaryProgram = new PlanBuyingInventoryProgram.ManifestDaypart.Program
                            {
                                ShowType = "Series",
                                Name = "Friends",
                                Genre = "Comedy"
                            },
                            Programs = new List<PlanBuyingInventoryProgram.ManifestDaypart.Program>
                            {
                                new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                {
                                    ShowType = "Series",
                                    Name = "Friends",
                                    Genre = "Unmatched"
                                }
                            }
                        }
                    }
                }
            };
            return programs;
        }
    }
}