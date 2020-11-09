using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class PlanPricingStationCalculationEngineUnitTests
    {
        private PlanPricingStationCalculationEngine _PlanPricingStationCalculationEngine;
        private Mock<IMarketRepository> _MarketRepository;
        private Mock<IStationRepository> _StationRepository;

        [SetUp]
        public void Setup()
        {
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _MarketRepository = new Mock<IMarketRepository>();
            _StationRepository = new Mock<IStationRepository>();

            dataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketRepository>())
                .Returns(_MarketRepository.Object);

            dataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationRepository>())
                .Returns(_StationRepository.Object);

            _StationRepository
                .Setup(x => x.GetBroadcastStationsByIds(It.IsAny<List<int>>()))
                .Returns(new List<DisplayBroadcastStation>
                { 
                    new DisplayBroadcastStation
                    {
                        Id = 30,
                        LegacyCallLetters = "KOB",
                        MarketCode = 100
                    },
                    new DisplayBroadcastStation
                    {
                        Id = 27,
                        LegacyCallLetters = "KOTA",
                        MarketCode = 200
                    },
                    new DisplayBroadcastStation
                    {
                        Id = 99,
                        LegacyCallLetters = "TVX",
                        MarketCode = 300
                    }
                });

            _MarketRepository
                .Setup(x => x.GetMarketsByMarketCodes(It.IsAny<List<int>>()))
                .Returns(new List<market>
                {
                    new market
                    {
                        market_code = 100,
                        geography_name = "New York"
                    },
                    new market
                    {
                        market_code = 200,
                        geography_name = "Las Vegas"
                    },
                    new market
                    {
                        market_code = 300,
                        geography_name = "Seattle"
                    }
                });

            _PlanPricingStationCalculationEngine = new PlanPricingStationCalculationEngine(dataRepositoryFactoryMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_Success()
        {
            var proprietaryInventory = new ProprietaryInventoryData
            {
                ProprietarySummaries = new List<ProprietarySummary>
                {
                    new ProprietarySummary
                    {
                        ProprietarySummaryByStations = new List<ProprietarySummaryByStation>
                        {
                            new ProprietarySummaryByStation
                            {
                                StationId = 30,
                                TotalSpots = 10,
                                TotalCostWithMargin = 50,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 1,
                                        TotalImpressions = 500
                                    },
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 2,
                                        TotalImpressions = 1000
                                    }
                                }
                            },
                            new ProprietarySummaryByStation
                            {
                                StationId = 27,
                                TotalSpots = 10,
                                TotalCostWithMargin = 50,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 1,
                                        TotalImpressions = 500
                                    },
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 2,
                                        TotalImpressions = 1000
                                    }
                                }
                            }
                        }
                    },
                    new ProprietarySummary
                    {
                        ProprietarySummaryByStations = new List<ProprietarySummaryByStation>
                        {
                            new ProprietarySummaryByStation
                            {
                                StationId = 30,
                                TotalSpots = 10,
                                TotalCostWithMargin = 50,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 1,
                                        TotalImpressions = 500
                                    },
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 2,
                                        TotalImpressions = 1000
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _StationRepository.Setup(s => s.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>())).Returns(_GetStationMonthDetails());

            var planPricingParameters = _GetPlanPricingParametersDto();
            var allocationResult = _GetPlanPricingAllocationResult();
            var inventory = _GetPlanPricingInventoryPrograms();

            var result = _PlanPricingStationCalculationEngine.Calculate(inventory, allocationResult, planPricingParameters, proprietaryInventory, PostingTypeEnum.NSI);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_SuccessNoStationMonthDetails()
        {
            var proprietaryInventory = new ProprietaryInventoryData();

            _StationRepository.Setup(s => s.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>())).Returns(new List<StationMonthDetailDto>());

            var planPricingParameters = _GetPlanPricingParametersDto();
            var allocationResult = _GetPlanPricingAllocationResult();
            var inventory = _GetPlanPricingInventoryPrograms();

            var result = _PlanPricingStationCalculationEngine.Calculate(inventory, allocationResult, planPricingParameters, proprietaryInventory, PostingTypeEnum.NSI);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_SuccessWithMargin()
        {
            var proprietaryInventory = new ProprietaryInventoryData();

            _StationRepository.Setup(s => s.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>())).Returns(_GetStationMonthDetails());

            var planPricingParameters = _GetPlanPricingParametersDto();
            var allocationResult = _GetPlanPricingAllocationResultWithMargin();
            var inventory = _GetPlanPricingInventoryPrograms();

            var result = _PlanPricingStationCalculationEngine.Calculate(inventory, allocationResult, planPricingParameters, proprietaryInventory, PostingTypeEnum.NSI);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        private List<StationMonthDetailDto> _GetStationMonthDetails() =>
                new List<StationMonthDetailDto>
                {
                new StationMonthDetailDto
                {
                    StationId = 30,
                    MarketCode = 100
                },
                new StationMonthDetailDto
                {
                    StationId = 27,
                    MarketCode = 200
                },
                new StationMonthDetailDto
                {
                    StationId = 99,
                    MarketCode = 300
                },
                };

        private PlanPricingParametersDto _GetPlanPricingParametersDto() =>
            new PlanPricingParametersDto
            {
                PlanVersionId = 15,
                JobId = 51
            };

        private PlanPricingAllocationResult _GetPlanPricingAllocationResult() =>
            new PlanPricingAllocationResult
            {
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 5,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 15.5m,
                                SpotCostWithMargin = 15.5m,
                                Spots = 5,
                                Impressions = 500
                            }
                        },
                        Impressions30sec = 500
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 20,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 9.99m,
                                SpotCostWithMargin = 9.99m,
                                Spots = 3,
                                Impressions = 750
                            }
                        },
                        Impressions30sec = 750
                    }
                }
            };

        private PlanPricingAllocationResult _GetPlanPricingAllocationResultWithMargin() =>
            new PlanPricingAllocationResult
            {
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 5,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 15.5m,
                                // 20% Margin
                                SpotCostWithMargin = 19.375m,
                                Spots = 5,
                                Impressions = 500
                            }
                        },
                        Impressions30sec = 500
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 20,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost =  9.99m,
                                // 20% Margin
                                SpotCostWithMargin = 12.4875m,
                                Spots = 3,
                                Impressions = 750
                            }
                        },
                        Impressions30sec = 750
                    }
                }
            };

        private List<PlanPricingInventoryProgram> _GetPlanPricingInventoryPrograms() =>
            new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                     ManifestId = 5,
                     Station = new DisplayBroadcastStation
                     {
                          Id = 30,
                          LegacyCallLetters = "KOB",
                     }
                },
                new PlanPricingInventoryProgram
                {
                     ManifestId = 20,
                     Station = new DisplayBroadcastStation
                     {
                          Id = 27,
                          LegacyCallLetters = "KOTA",
                     }
                },
                new PlanPricingInventoryProgram
                {
                     ManifestId = 6,
                     Station = new DisplayBroadcastStation
                     {
                          Id = 99,
                          LegacyCallLetters = "TVX",
                     }
                },
            };
    }
}
