using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines.PlanPricing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    public class PlanPricingMarketResultsEngineUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate()
        {
            // Arrange
            var parametersDto = new PlanPricingParametersDto();
            
            var marketCoverages = Enumerable.Range(1, 10)
                .Select(s => new MarketCoverage {MarketCode = s, Rank = s, PercentageOfUS = 10})
                .ToList();
            
            var stations = Enumerable.Range(1, 20)
                .Select(s => new DisplayBroadcastStation {Id = s, MarketCode = (s % marketCoverages.Count) + 1})
                .ToList();
            
            var inventory = Enumerable.Range(1, 20)
                .Select(s => new PlanPricingInventoryProgram
                    {ManifestId = s, Station = stations.Single(a => a.Id.Equals(s))})
                .ToList();
            
            var allocationResult = new PlanPricingAllocationResult
            {
                PlanVersionId = 12,
                JobId = 13,
                Spots = Enumerable.Range(1, 20)
                    .Select(s => new PlanPricingAllocatedSpot 
                    { 
                        Id = s,
                        SpotFrequencies = new List<PlanPricingAllocatedSpot.SpotFrequency>
                        {
                            new PlanPricingAllocatedSpot.SpotFrequency
                            {
                                SpotCost = 10,
                                SpotCostWithMargin = 10,
                                Spots = 1
                            }
                        },
                        Impressions30sec = 100,
                        TotalImpressions = 100
                    })
                    .ToList()
            };

            var plan = new PlanDto
            {
                AvailableMarkets = marketCoverages.Select(s => new PlanAvailableMarketDto
                {
                    MarketCode = Convert.ToInt16(s.MarketCode),
                    ShareOfVoicePercent = 10
                }).ToList()
            };

            var engine = new PlanPricingMarketResultsEngine();

            // Act
            var result = engine.Calculate(inventory, allocationResult, parametersDto, plan, marketCoverages);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_WithMargin()
        {
            // Arrange
            var parametersDto = new PlanPricingParametersDto { Margin = 10};

            var marketCoverages = Enumerable.Range(1, 10)
                .Select(s => new MarketCoverage { MarketCode = s, Rank = s, PercentageOfUS = 10 })
                .ToList();

            var stations = Enumerable.Range(1, 20)
                .Select(s => new DisplayBroadcastStation { Id = s, MarketCode = (s % marketCoverages.Count) + 1 })
                .ToList();

            var inventory = Enumerable.Range(1, 20)
                .Select(s => new PlanPricingInventoryProgram
                    { ManifestId = s, Station = stations.Single(a => a.Id.Equals(s)) })
                .ToList();

            var allocationResult = new PlanPricingAllocationResult
            {
                PlanVersionId = 12,
                JobId = 13,
                Spots = Enumerable.Range(1, 20)
                    .Select(s => new PlanPricingAllocatedSpot 
                    {
                        Id = s,
                        SpotFrequencies = new List<PlanPricingAllocatedSpot.SpotFrequency>
                        {
                            new PlanPricingAllocatedSpot.SpotFrequency
                            {
                                SpotCost = 10,
                                SpotCostWithMargin = 11,
                                Spots = 1
                            }
                        },
                        Impressions30sec = 100,
                        TotalImpressions = 100
                    })
                    .ToList()
            };

            var plan = new PlanDto
            {
                AvailableMarkets = marketCoverages.Select(s => new PlanAvailableMarketDto
                {
                    MarketCode = Convert.ToInt16(s.MarketCode),
                    ShareOfVoicePercent = 10
                }).ToList()
            };

            var engine = new PlanPricingMarketResultsEngine();

            // Act
            var result = engine.Calculate(inventory, allocationResult, parametersDto, plan, marketCoverages);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_InventoryWithoutMarketOrStationOrPricingJob()
        {
            // Arrange
            var parametersDto = new PlanPricingParametersDto();

            var marketCoverages = Enumerable.Range(1, 10)
                .Select(s => new MarketCoverage { MarketCode = s, Rank = s, PercentageOfUS = 10 })
                .ToList();
            marketCoverages[0].Rank = null;

            var stations = Enumerable.Range(1, 20)
                .Select(s => new DisplayBroadcastStation { Id = s, MarketCode = (s % marketCoverages.Count) + 1 })
                .ToList();

            var inventory = Enumerable.Range(1, 20)
                .Select(s => new PlanPricingInventoryProgram
                    { ManifestId = s, Station = stations.Single(a => a.Id.Equals(s)) })
                .ToList();

            // adjust the inventory stations and market for our use cases
            inventory[0].Station = null;
            inventory[1].Station.MarketCode = null;

            var allocationResult = new PlanPricingAllocationResult
            {
                PlanVersionId = 12,
                Spots = Enumerable.Range(1, 20)
                    .Select(s => new PlanPricingAllocatedSpot 
                    { 
                        Id = s,
                        SpotFrequencies = new List<PlanPricingAllocatedSpot.SpotFrequency>
                        {
                            new PlanPricingAllocatedSpot.SpotFrequency
                            {
                                SpotCost = 10,
                                SpotCostWithMargin = 10,
                                Spots = 1
                            }
                        },
                        Impressions30sec = 100,
                        TotalImpressions = 100
                    })
                    .ToList()
            };

            var plan = new PlanDto
            {
                AvailableMarkets = marketCoverages.Select(s => new PlanAvailableMarketDto
                {
                    MarketCode = Convert.ToInt16(s.MarketCode),
                    ShareOfVoicePercent = 10
                }).ToList()
            };

            var engine = new PlanPricingMarketResultsEngine();

            // Act
            var result = engine.Calculate(inventory, allocationResult, parametersDto, plan, marketCoverages);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_WithEmptyPlanMarketShareOfVoice()
        {
            // Arrange
            var parametersDto = new PlanPricingParametersDto();

            var marketCoverages = Enumerable.Range(1, 10)
                .Select(s => new MarketCoverage { MarketCode = s, Rank = s, PercentageOfUS = 10 })
                .ToList();

            var stations = Enumerable.Range(1, 20)
                .Select(s => new DisplayBroadcastStation { Id = s, MarketCode = (s % marketCoverages.Count) + 1 })
                .ToList();

            var inventory = Enumerable.Range(1, 20)
                .Select(s => new PlanPricingInventoryProgram
                    { ManifestId = s, Station = stations.Single(a => a.Id.Equals(s)) })
                .ToList();

            var allocationResult = new PlanPricingAllocationResult
            {
                PlanVersionId = 12,
                JobId = 13,
                Spots = Enumerable.Range(1, 20)
                    .Select(s => new PlanPricingAllocatedSpot 
                    { 
                        Id = s,
                        SpotFrequencies = new List<PlanPricingAllocatedSpot.SpotFrequency>
                        {
                            new PlanPricingAllocatedSpot.SpotFrequency
                            {
                                SpotCost = 10,
                                SpotCostWithMargin = 10,
                                Spots = 1
                            }
                        },
                        Impressions30sec = 100,
                        TotalImpressions = 100
                    })
                    .ToList()
            };

            var plan = new PlanDto
            {
                AvailableMarkets = marketCoverages.Select(s => new PlanAvailableMarketDto
                {
                    MarketCode = Convert.ToInt16(s.MarketCode),
                    ShareOfVoicePercent =(s.MarketCode % 2) == 0 ? (double?)null : 5.0
                }).ToList()
            };

            var engine = new PlanPricingMarketResultsEngine();

            // Act
            var result = engine.Calculate(inventory, allocationResult, parametersDto, plan, marketCoverages);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}