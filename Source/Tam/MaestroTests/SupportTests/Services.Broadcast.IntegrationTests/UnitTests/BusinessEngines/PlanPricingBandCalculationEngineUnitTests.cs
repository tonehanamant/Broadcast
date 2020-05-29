using ApprovalTests;
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
    public class PlanPricingBandCalculationEngineUnitTests
    {
        PlanPricingBandCalculationEngine _BandCalculator;

        [SetUp]
        public void SetUp()
        {
            _BandCalculator = new PlanPricingBandCalculationEngine(new PlanPricingUnitCapImpressionsCalculationEngine());
        }

        [Test]
        public void CalculatePricingBandTest()
        {
            var inventory = _GetInventory();

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

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var bands = _BandCalculator.CalculatePricingBands(inventory, allocationResult, parameters);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands));
        }

        [Test]
        public void CalculatePricingBandTest_HasSevenBands()
        {
            var inventory = _GetInventory();

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

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var pricingBand = _BandCalculator.CalculatePricingBands(inventory, allocationResult, parameters);

            Assert.AreEqual(7, pricingBand.Bands.Count);
        }

        [Test]
        public void CalculatePricingBandTest_MinBandIsTenPercentOfCpm()
        {
            var inventory = _GetInventory();

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

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var pricingBand = _BandCalculator.CalculatePricingBands(inventory, allocationResult, parameters);

            Assert.AreEqual(1, pricingBand.Bands[0].MaxBand);
        }

        [Test]
        public void CalculatePricingBandTest_MaxBandIsTwoTimesTheCpm()
        {
            var inventory = _GetInventory();

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

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var pricingBand = _BandCalculator.CalculatePricingBands(inventory, allocationResult, parameters);

            Assert.AreEqual(20, pricingBand.Bands[6].MinBand);
        }

        [Test]
        public void CalculatePricingBandTest_BandHasValidLimitCpms()
        {
            var inventory = _GetInventory();

            var allocationResult = new PlanPricingAllocationResult()
            {
                JobId = 1,
                PlanVersionId = 1,
                PricingCpm = 50,
                RequestId = "750f2c26-b011-4be9-b766-70a655c73a5e",
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 10,
                        Impressions = 1000,
                        SpotCost = 50
                    }
                }
            };

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 50,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var pricingBand = _BandCalculator.CalculatePricingBands(inventory, allocationResult, parameters);
           
            Assert.AreEqual(5, pricingBand.Bands[1].MinBand);
            Assert.AreEqual(24, pricingBand.Bands[1].MaxBand);
        }

        [Test]
        public void CalculatePricingBandTest_ProgramIsOnlyInOneBand()
        {
            var inventory = _GetInventory();

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

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 70,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var pricingBand = _BandCalculator.CalculatePricingBands(inventory, allocationResult, parameters);

            Assert.AreEqual(0, pricingBand.Bands[0].Impressions);
            Assert.AreEqual(1000, pricingBand.Bands[1].Impressions);
        }

        [Test]
        public void CalculatePricingBandTest_ImpressionsPercentage()
        {
            var inventory = _GetInventory();

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

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 70,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var pricingBand = _BandCalculator.CalculatePricingBands(inventory, allocationResult, parameters);

            Assert.AreEqual(100, pricingBand.Bands[1].ImpressionsPercentage);
            Assert.AreEqual(20, pricingBand.Bands[1].AvailableInventoryPercent);
        }

        private List<PlanPricingInventoryProgram> _GetInventory()
        {
            return new List<PlanPricingInventoryProgram>()
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    InventoryPricingQuarterType = InventoryPricingQuarterType.Plan,
                    SpotCost = 50,
                    ProjectedImpressions = 5000,
                    ProvidedImpressions = 5000,
                    Cpm = 10,
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
        }
    }
}
