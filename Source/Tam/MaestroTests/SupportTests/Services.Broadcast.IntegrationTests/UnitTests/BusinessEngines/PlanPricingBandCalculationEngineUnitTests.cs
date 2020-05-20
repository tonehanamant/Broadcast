using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanPricingBandCalculationEngineUnitTests
    {
        [Test]
        public void CalculatePricingBandTest()
        {
            var bandCalculator = new PlanPricingBandCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = 50,
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
                            }
                        }
                    }
                }
            };

            var allocationResult = new PlanPricingAllocationResult()
            {
                JobId = 1,
                PlanVersionId = 1,
                PricingCpm = 10,
                RequestId = "750f2c26-b011-4be9-b766-70a655c73a5e",
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 10,
                        Impressions = 5000,
                        SpotCost = 50
                    }
                }
            };

            var bands = bandCalculator.CalculatePricingBands(inventory, allocationResult);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands));
        }

        [Test]
        public void CalculatePricingBandTest_HasNineBands()
        {
            var bandCalculator = new PlanPricingBandCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = 50,
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
                            }
                        }
                    }
                }
            };

            var allocationResult = new PlanPricingAllocationResult()
            {
                JobId = 1,
                PlanVersionId = 1,
                PricingCpm = 10,
                RequestId = "750f2c26-b011-4be9-b766-70a655c73a5e",
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 10,
                        Impressions = 5000,
                        SpotCost = 50
                    }
                }
            };

            var pricingBand = bandCalculator.CalculatePricingBands(inventory, allocationResult);

            Assert.AreEqual(9, pricingBand.Bands.Count);
        }

        [Test]
        public void CalculatePricingBandTest_MinBandIsTenPercentOfCpm()
        {
            var bandCalculator = new PlanPricingBandCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = 50,
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
                            }
                        }
                    }
                }
            };

            var allocationResult = new PlanPricingAllocationResult()
            {
                JobId = 1,
                PlanVersionId = 1,
                PricingCpm = 10,
                RequestId = "750f2c26-b011-4be9-b766-70a655c73a5e",
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 10,
                        Impressions = 5000,
                        SpotCost = 50
                    }
                }
            };

            var pricingBand = bandCalculator.CalculatePricingBands(inventory, allocationResult);

            Assert.AreEqual(1, pricingBand.Bands[0].MaxBand);
        }

        [Test]
        public void CalculatePricingBandTest_MaxBandIsTwoTimesTheCpm()
        {
            var bandCalculator = new PlanPricingBandCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = 50,
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
                            }
                        }
                    }
                }
            };

            var allocationResult = new PlanPricingAllocationResult()
            {
                JobId = 1,
                PlanVersionId = 1,
                PricingCpm = 10,
                RequestId = "750f2c26-b011-4be9-b766-70a655c73a5e",
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 10,
                        Impressions = 5000,
                        SpotCost = 50
                    }
                }
            };

            var pricingBand = bandCalculator.CalculatePricingBands(inventory, allocationResult);

            Assert.AreEqual(20, pricingBand.Bands[8].MinBand);
        }

        [Test]
        public void CalculatePricingBandTest_BandHasValidLimitCpms()
        {
            var bandCalculator = new PlanPricingBandCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = 70,
                    ProjectedImpressions = 1000,
                    ProvidedImpressions = 1000,
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
                            }
                        }
                    }
                }
            };

            var allocationResult = new PlanPricingAllocationResult()
            {
                JobId = 1,
                PlanVersionId = 1,
                PricingCpm = 70,
                RequestId = "750f2c26-b011-4be9-b766-70a655c73a5e",
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 10,
                        Impressions = 1000,
                        SpotCost = 70
                    }
                }
            };

            var pricingBand = bandCalculator.CalculatePricingBands(inventory, allocationResult);
           
            Assert.AreEqual(7, pricingBand.Bands[1].MinBand);
            Assert.AreEqual(26, pricingBand.Bands[1].MaxBand);
        }

        [Test]
        public void CalculatePricingBandTest_ProgramIsOnlyInOneBand()
        {
            var bandCalculator = new PlanPricingBandCalculationEngine();

            var inventory = new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = 7,
                    ProjectedImpressions = 1000,
                    ProvidedImpressions = 1000,
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
                            }
                        }
                    }
                }
            };

            var allocationResult = new PlanPricingAllocationResult()
            {
                JobId = 1,
                PlanVersionId = 1,
                PricingCpm = 70,
                RequestId = "750f2c26-b011-4be9-b766-70a655c73a5e",
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 1,
                        Impressions = 1000,
                        SpotCost = 7
                    }
                }
            };

            var pricingBand = bandCalculator.CalculatePricingBands(inventory, allocationResult);

            Assert.AreEqual(0, pricingBand.Bands[0].Impressions);
            Assert.AreEqual(1000, pricingBand.Bands[1].Impressions);
        }
    }
}
