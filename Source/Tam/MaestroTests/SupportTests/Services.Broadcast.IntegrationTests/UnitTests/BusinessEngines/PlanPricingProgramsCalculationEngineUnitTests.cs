using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanPricingProgramsCalculationEngineUnitTests
    {
        PlanPricingProgramCalculationEngine _ProgramEngine = new PlanPricingProgramCalculationEngine();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePricingBandTest()
        {
            var inventory = _GetInventory();
            var proprietaryInventory = new ProprietaryInventoryData
            {
                ProprietarySummaries = new List<ProprietarySummary>
                {
                    new ProprietarySummary
                    {
                        Cpm = 1,
                        ProgramName = "Early Morning News",
                        Genre = "News",
                        ProprietarySummaryByStations = new List<ProprietarySummaryByStation>
                        {
                            new ProprietarySummaryByStation
                            {
                                TotalCostWithMargin = 100,
                                TotalSpots = 5,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        TotalImpressions = 1000
                                    }
                                }
                            }
                        }
                    },
                    new ProprietarySummary
                    {
                        Cpm = 4,
                        ProgramName = "Late Morning News",
                        Genre = "News",
                        ProprietarySummaryByStations = new List<ProprietarySummaryByStation>
                        {
                            new ProprietarySummaryByStation
                            {
                                TotalCostWithMargin = 200,
                                TotalSpots = 7,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        TotalImpressions = 2000
                                    }
                                }
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
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                Spots = 10,
                                SpotCost = 50,
                                SpotCostWithMargin = 50,
                                Impressions = 5000
                            }
                        },
                        Impressions30sec = 5000
                    }
                }
            };

            var parameters = new PlanPricingParametersDto
            {
                AdjustedCPM = 10,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.PerHour
            };

            var bands = _ProgramEngine.CalculateProgramResults(inventory, allocationResult, false, proprietaryInventory, PostingTypeEnum.NSI);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands));
        }

        private List<PlanPricingInventoryProgram> _GetInventory()
        {
            return new List<PlanPricingInventoryProgram>()
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
