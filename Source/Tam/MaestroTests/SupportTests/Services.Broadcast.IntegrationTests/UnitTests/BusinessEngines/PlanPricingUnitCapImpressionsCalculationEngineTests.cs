using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanPricingUnitCapImpressionsCalculationEngineTests
    {
        [Test]
        public void CalculateTotalImpressionsForUnitCapsTest_PerHourTest()
        {
            var unitCapImpressionsCalculator = new PlanPricingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanPricingInventoryProgram.ManifestRate>
                    {
                        new PlanPricingInventoryProgram.ManifestRate
                        {
                            Cost = 50
                        }
                    },
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    InventorySource = new InventorySource
                    {
                        Id = 1,
                        InventoryType = InventorySourceTypeEnum.OpenMarket,
                        IsActive = true,
                        Name = "Open Market"
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 1000, 2000, true, false, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        }
                    }
                }
            };
    
            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var totalImpressions = unitCapImpressionsCalculator.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            // Program runs once per week, one time a week, has 5000 impressions.
            // Unit cap of 10 equals to 50000 total impressions.
            Assert.AreEqual(50000, totalImpressions);
        }

        [Test]
        public void CalculateTotalImpressionsForUnitCapsTest_PerMultipleHourTest()
        {
            var unitCapImpressionsCalculator = new PlanPricingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanPricingInventoryProgram.ManifestRate>
                    {
                        new PlanPricingInventoryProgram.ManifestRate
                        {
                            Cost = 50
                        }
                    },
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    InventorySource = new InventorySource
                    {
                        Id = 1,
                        InventoryType = InventorySourceTypeEnum.OpenMarket,
                        IsActive = true,
                        Name = "Open Market"
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 0, 8000, true, false, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        }
                    }
                }
            };

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var totalImpressions = unitCapImpressionsCalculator.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            // Program runs once per week, for two hours, one time a week, has 5000 impressions.
            // Unit cap of 1 equals to 10000 total impressions.
            Assert.AreEqual(10000, totalImpressions);
        }

        [Test]
        public void CalculateTotalImpressionsForUnitCaps_MultipleProgramsTest()
        {
            var unitCapImpressionsCalculator = new PlanPricingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanPricingInventoryProgram.ManifestRate>
                    {
                        new PlanPricingInventoryProgram.ManifestRate
                        {
                            Cost = 50
                        }
                    },
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    InventorySource = new InventorySource
                    {
                        Id = 1,
                        InventoryType = InventorySourceTypeEnum.OpenMarket,
                        IsActive = true,
                        Name = "Open Market"
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 1000, 2000, true, false, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanPricingInventoryProgram.ManifestRate>
                    {
                        new PlanPricingInventoryProgram.ManifestRate
                        {
                            Cost = 50
                        }
                    },
                    ProjectedImpressions = 2000,
                    ProvidedImpressions = 2000,
                    InventorySource = new InventorySource
                    {
                        Id = 1,
                        InventoryType = InventorySourceTypeEnum.OpenMarket,
                        IsActive = true,
                        Name = "Open Market"
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 1000, 2000, true, false, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        }
                    }
                }
            };

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.PerWeek
            };

            var totalImpressions = unitCapImpressionsCalculator.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            // Only the program with the highest impressions is used.
            Assert.AreEqual(50000, totalImpressions);
        }

        [Test]
        public void CalculateTotalImpressionsForUnitCaps_PerWeekMultipleWeeksTest()
        {
            var unitCapImpressionsCalculator = new PlanPricingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanPricingInventoryProgram.ManifestRate>
                    {
                        new PlanPricingInventoryProgram.ManifestRate
                        {
                            Cost = 50
                        }
                    },
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    InventorySource = new InventorySource
                    {
                        Id = 1,
                        InventoryType = InventorySourceTypeEnum.OpenMarket,
                        IsActive = true,
                        Name = "Open Market"
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 1000, 2000, true, false, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 2
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 3
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 4
                        }
                    }
                }
            };

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerWeek
            };

            var totalImpressions = unitCapImpressionsCalculator.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            // Four weeks times the impressions (5000) = 20000
            Assert.AreEqual(20000, totalImpressions);
        }

        [Test]
        public void CalculateTotalImpressionsForUnitCaps_PerMonthTest()
        {
            var unitCapImpressionsCalculator = new PlanPricingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanPricingInventoryProgram.ManifestRate>
                    {
                        new PlanPricingInventoryProgram.ManifestRate
                        {
                            Cost = 50
                        }
                    },
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    InventorySource = new InventorySource
                    {
                        Id = 1,
                        InventoryType = InventorySourceTypeEnum.OpenMarket,
                        IsActive = true,
                        Name = "Open Market"
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 1000, 2000, true, false, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 2
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 3
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 4
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 5
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 6
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 7
                        },
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 8
                        }
                    }
                }
            };

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerMonth
            };

            var totalImpressions = unitCapImpressionsCalculator.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            // Two months times the impressions (5000) = 10000
            Assert.AreEqual(10000, totalImpressions);
        }

        [Test]
        public void CalculateTotalImpressionsForUnitCapsTest_Per30MinTest()
        {
            var unitCapImpressionsCalculator = new PlanPricingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    ManifestRates = new List<PlanPricingInventoryProgram.ManifestRate>
                    {
                        new PlanPricingInventoryProgram.ManifestRate
                        {
                            Cost = 50
                        }
                    },
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    InventorySource = new InventorySource
                    {
                        Id = 1,
                        InventoryType = InventorySourceTypeEnum.OpenMarket,
                        IsActive = true,
                        Name = "Open Market"
                    },
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanPricingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 0, 8000, true, false, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanPricingInventoryProgram.ManifestWeek>
                    {
                        new PlanPricingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        }
                    }
                }
            };

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min
            };

            var totalImpressions = unitCapImpressionsCalculator.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            // Program runs once per week, for two hours, one time a week, has 5000 impressions.
            // Unit cap of 1 for 30 minutes equals to 20000 total impressions.
            // Same as the hours test, but times two;
            Assert.AreEqual(20000, totalImpressions);
        }
    }
}
