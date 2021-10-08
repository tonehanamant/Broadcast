using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanBuyingUnitCapImpressionsCalculationEngineUnitTests
    {
        [Test]
        [TestCase(UnitCapEnum.PerMonth, 10000)]
        [TestCase(UnitCapEnum.PerWeek, 40000)]
        [TestCase(UnitCapEnum.PerDay, 80000)]
        [TestCase(UnitCapEnum.PerHour, 160000)]
        [TestCase(UnitCapEnum.Per30Min, 320000)]
        public void CalculateTotalImpressionsForUnitCaps_From_PlanBuyingInventoryProgram(UnitCapEnum unitCapEnum, double expectedTotalImpressions)
        {
            var planBuyingUnitCapImpressionsCalculationEngine = new PlanBuyingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanBuyingInventoryProgram>()
            {
                new PlanBuyingInventoryProgram
                {
                    ManifestId = 1,
                    ProjectedImpressions = 3000,
                    ProvidedImpressions = 3000,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                    {
                        new PlanBuyingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanBuyingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 0, 8000, true, true, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 2
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 3
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 4
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 5
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 6
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 7
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 8
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    ManifestId = 1,
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "AAAA",
                        MarketCode = 404
                    },
                    ManifestDayparts = new List<PlanBuyingInventoryProgram.ManifestDaypart>
                    {
                        new PlanBuyingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            PrimaryProgramId = 1,
                            PrimaryProgram = new PlanBuyingInventoryProgram.ManifestDaypart.Program
                            {
                                Id = 1,
                                Name = "True Detective"
                            },
                            Daypart = new DisplayDaypart(1, 0, 8000, true, true, false, false, false, false, false)
                        }
                    },
                    ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                    {
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 1
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 2
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 3
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 4
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 5
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 6
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 7
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            Id = 8
                        }
                    }
                }
            };

            var parameters = new PlanBuyingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = unitCapEnum
            };

            var totalImpressions = planBuyingUnitCapImpressionsCalculationEngine.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            Assert.AreEqual(expectedTotalImpressions, totalImpressions);
        }

        [Test]
        [TestCase(UnitCapEnum.PerMonth, 10000)]
        [TestCase(UnitCapEnum.PerWeek, 40000)]
        [TestCase(UnitCapEnum.PerDay, 80000)]
        [TestCase(UnitCapEnum.PerHour, 160000)]
        [TestCase(UnitCapEnum.Per30Min, 320000)]
        public void CalculateTotalImpressionsForUnitCaps_From_PlanBuyingBandStationDetail(UnitCapEnum unitCapEnum, double expectedTotalImpressions)
        {
            var planBuyingUnitCapImpressionsCalculationEngine = new PlanBuyingUnitCapImpressionsCalculationEngine();

            var inventory = new List<PlanBuyingBandStationDetailDto>()
            {
                new PlanBuyingBandStationDetailDto
                {
                    StationId = 1,
                    ManifestWeeksCount = 8,
                    Impressions = 3000,
                    PlanBuyingBandStationDayparts = new List<PlanBuyingBandStationDaypartDto>
                    { 
                        new PlanBuyingBandStationDaypartDto()
                        { 
                            ActiveDays = 1,
                            Hours = 1
                        }
                    }                    
                },
                new PlanBuyingBandStationDetailDto
                {
                    StationId = 1,
                    ManifestWeeksCount = 8,
                    Impressions = 5000,
                    PlanBuyingBandStationDayparts = new List<PlanBuyingBandStationDaypartDto>
                    {
                        new PlanBuyingBandStationDaypartDto()
                        {
                            ActiveDays = 2,
                            Hours = 2
                        }
                    }
                }
            };

            var parameters = new PlanBuyingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = unitCapEnum
            };

            var totalImpressions = planBuyingUnitCapImpressionsCalculationEngine.CalculateTotalImpressionsForUnitCaps(inventory, parameters);

            Assert.AreEqual(expectedTotalImpressions, totalImpressions);
        }
    }
}
