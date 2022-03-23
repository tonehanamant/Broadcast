using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanBuyingBandCalculationEngineUnitTests
    {
        private Mock<IPlanBuyingUnitCapImpressionsCalculationEngine> _PlanBuyingUnitCapImpressionsCalculationEngine;
        private Mock<ISpotLengthEngine> _SpotLengthEngine;

        [SetUp]
        public void SetUp()
        {
            _PlanBuyingUnitCapImpressionsCalculationEngine = new Mock<IPlanBuyingUnitCapImpressionsCalculationEngine>();
            _SpotLengthEngine = new Mock<ISpotLengthEngine>();
        }

        private PlanBuyingBandCalculationEngine _GetPlanBuyingBandCalculationEngine()
        {
            return new PlanBuyingBandCalculationEngine(
                _PlanBuyingUnitCapImpressionsCalculationEngine.Object
                ,_SpotLengthEngine.Object);
        }

        [Test]
        public void Calculate_From_PlanBuyingInventoryProgram()
        {
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

            var allocationResult = new PlanBuyingAllocationResult
            {
                BuyingCpm = 4.6841m,
                JobId = 1,
                BuyingVersion = "4",
                PlanVersionId = 1172,
                AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                {
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4340,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 6.25m, SpotCostWithMargin = 6.25m, Spots = 156, Impressions = 3183.33},
                            new SpotFrequency {SpotLengthId = 2, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 1, Impressions = 659.63}
                        },
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4341,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 22.58m, SpotCostWithMargin = 22.58m, Spots = 369, Impressions = 7227.10},
                        },
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4342,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 126.74m, SpotCostWithMargin = 126.74m, Spots = 18, Impressions = 33338.89},
                        },
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4343,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 465.28m, SpotCostWithMargin = 465.28m, Spots = 9, Impressions = 100000},
                        },
                        RepFirm = "KHBP",
                        OwnerName = "KHBP",
                        LegacyCallLetters = "KHBP"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4344,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 98.44m, SpotCostWithMargin = 98.44m, Spots = 52, Impressions = 8802.5},
                        },
                        RepFirm = "KHBS",
                        OwnerName = "KHBS",
                        LegacyCallLetters = "KHBS"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4345,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 50.44m, SpotCostWithMargin = 50.44m, Spots = 15, Impressions = 1000},
                        },
                        RepFirm = "WCCO",
                        OwnerName = "WCCO",
                        LegacyCallLetters = "WCCO"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4346,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 500.28m, SpotCostWithMargin = 500.28m, Spots = 9, Impressions = 75000},
                        },
                        RepFirm = "KHBP",
                        OwnerName = "KHBP",
                        LegacyCallLetters = "KHBP"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4347,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 100.28m, SpotCostWithMargin = 100.28m, Spots = 6, Impressions = 7596.25},
                        },
                        RepFirm = "KHBP",
                        OwnerName = "KHBP",
                        LegacyCallLetters = "KHBP"
                    }
                },
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                PostingType = PostingTypeEnum.NSI
            };

            var parameters = new PlanBuyingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min
            };

            var planBuyingBandCalculationEngine = _GetPlanBuyingBandCalculationEngine();

            _PlanBuyingUnitCapImpressionsCalculationEngine
                .Setup(x => x.CalculateTotalImpressionsForUnitCaps(It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingParametersDto>()))
                .Returns(57120);

            // Act
            var result = planBuyingBandCalculationEngine.Calculate(inventory, allocationResult, parameters);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void Calculate_From_PlanBuyingBandStation()
        {
            var inventory = new PlanBuyingBandInventoryStationsDto
            {
                PostingType = PostingTypeEnum.NSI,
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                Details = new List<PlanBuyingBandStationDetailDto>()
                {
                    new PlanBuyingBandStationDetailDto
                    {
                        StationId = 1,
                        ManifestWeeksCount = 8,
                        Impressions = 3000,
                        PlanBuyingBandInventoryStationDayparts = new List<PlanBuyingBandInventoryStationDaypartDto>
                        {
                            new PlanBuyingBandInventoryStationDaypartDto()
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
                        PlanBuyingBandInventoryStationDayparts = new List<PlanBuyingBandInventoryStationDaypartDto>
                        {
                            new PlanBuyingBandInventoryStationDaypartDto()
                            {
                                ActiveDays = 2,
                                Hours = 2
                            }
                        }
                    }
                }
            };

            var allocationResult = new PlanBuyingAllocationResult
            {
                BuyingCpm = 4.6841m,
                JobId = 1,
                BuyingVersion = "4",
                PlanVersionId = 1172,
                AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                {
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4340,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 6.25m, SpotCostWithMargin = 6.25m, Spots = 156, Impressions = 3183.33},
                            new SpotFrequency {SpotLengthId = 2, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 1, Impressions = 659.63}
                        },
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4341,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 22.58m, SpotCostWithMargin = 22.58m, Spots = 369, Impressions = 7227.10},
                        },
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4342,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 126.74m, SpotCostWithMargin = 126.74m, Spots = 18, Impressions = 33338.89},
                        },
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4343,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 465.28m, SpotCostWithMargin = 465.28m, Spots = 9, Impressions = 100000},
                        },
                        RepFirm = "KHBP",
                        OwnerName = "KHBP",
                        LegacyCallLetters = "KHBP"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4344,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 98.44m, SpotCostWithMargin = 98.44m, Spots = 52, Impressions = 8802.5},
                        },
                        RepFirm = "KHBS",
                        OwnerName = "KHBS",
                        LegacyCallLetters = "KHBS"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4345,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 50.44m, SpotCostWithMargin = 50.44m, Spots = 15, Impressions = 1000},
                        },
                        RepFirm = "WCCO",
                        OwnerName = "WCCO",
                        LegacyCallLetters = "WCCO"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4346,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 500.28m, SpotCostWithMargin = 500.28m, Spots = 9, Impressions = 75000},
                        },
                        RepFirm = "KHBP",
                        OwnerName = "KHBP",
                        LegacyCallLetters = "KHBP"
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 4347,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 1, SpotCost = 100.28m, SpotCostWithMargin = 100.28m, Spots = 6, Impressions = 7596.25},
                        },
                        RepFirm = "KHBP",
                        OwnerName = "KHBP",
                        LegacyCallLetters = "KHBP"
                    }
                },
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                PostingType = PostingTypeEnum.NSI
            };

            var parameters = new PlanBuyingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min
            };

            var planBuyingBandCalculationEngine = _GetPlanBuyingBandCalculationEngine();

            _PlanBuyingUnitCapImpressionsCalculationEngine
                .Setup(x => x.CalculateTotalImpressionsForUnitCaps(It.IsAny<List<PlanBuyingBandStationDetailDto>>(), It.IsAny<PlanBuyingParametersDto>()))
                .Returns(57120);

            // Act
            var result = planBuyingBandCalculationEngine.Calculate(inventory, allocationResult, parameters);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}
