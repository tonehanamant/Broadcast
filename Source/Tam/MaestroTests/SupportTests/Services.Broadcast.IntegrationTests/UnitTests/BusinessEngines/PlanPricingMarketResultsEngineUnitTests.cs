using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines.PlanPricing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
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
            var proprietaryData = new ProprietaryInventoryData
            {
                ProprietarySummaries = new List<ProprietarySummary>
                {
                    new ProprietarySummary
                    {
                        ProprietarySummaryId = 1,
                        ProprietarySummaryByStations = new List<ProprietarySummaryByStation>
                        {
                            new ProprietarySummaryByStation
                            {
                                StationId = 1,
                                MarketCode = 1,
                                TotalSpots = 100,
                                TotalCostWithMargin = 100,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 1,
                                        TotalImpressions = 1000
                                    },
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 2,
                                        TotalImpressions = 1000,
                                    }
                                }
                            },
                            new ProprietarySummaryByStation
                            {
                                StationId = 2,
                                MarketCode = 1,
                                TotalSpots = 100,
                                TotalCostWithMargin = 100,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 1,
                                        TotalImpressions = 1000
                                    },
                                }
                            },
                            new ProprietarySummaryByStation
                            {
                                StationId = 3,
                                MarketCode = 2,
                                TotalSpots = 100,
                                TotalCostWithMargin = 100,
                                ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                {
                                    new ProprietarySummaryByAudience
                                    {
                                        AudienceId = 1,
                                        TotalImpressions = 1000
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
            var marketCoverages = Enumerable.Range(1, 10)
                .Select(s => new MarketCoverage { Market = $"Market {s}", MarketCode = s, Rank = s, PercentageOfUS = 10})
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
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 10,
                                SpotCostWithMargin = 10,
                                Spots = 1,
                                Impressions = 100
                            }
                        },
                        Impressions30sec = 100
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
            var result = engine.Calculate(inventory, allocationResult, plan, marketCoverages, proprietaryData);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_WithMargin()
        {
            // Arrange
            var proprietaryData = new ProprietaryInventoryData();

            var marketCoverages = Enumerable.Range(1, 10)
                .Select(s => new MarketCoverage { Market = $"Market {s}", MarketCode = s, Rank = s, PercentageOfUS = 10 })
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
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 10,
                                SpotCostWithMargin = 11,
                                Spots = 1,
                                Impressions = 100
                            }
                        },
                        Impressions30sec = 100
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
            var result = engine.Calculate(inventory, allocationResult, plan, marketCoverages, proprietaryData);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_WithEmptyPlanMarketShareOfVoice()
        {
            // Arrange
            var proprietaryData = new ProprietaryInventoryData();

            var marketCoverages = Enumerable.Range(1, 10)
                .Select(s => new MarketCoverage { Market = $"Market {s}", MarketCode = s, Rank = s, PercentageOfUS = 10 })
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
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 10,
                                SpotCostWithMargin = 10,
                                Spots = 1,
                                Impressions = 100
                            }
                        },
                        Impressions30sec = 100
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
            var result = engine.Calculate(inventory, allocationResult, plan, marketCoverages, proprietaryData);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}