using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PricingGuideDistributionEngineTests
    {
        private readonly IPricingGuideDistributionEngine pricingGuideDistributionEngine = 
            IntegrationTestApplicationServiceFactory.GetApplicationService<IPricingGuideDistributionEngine>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateMarketDistributionTest()
        {
            var pricingGuide = new PricingGuideOpenMarketInventory
            {
                MarketCoverage = 0.8d,
                Markets = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>
                {
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 1,
                        MarketCoverage = 0.6d,
                        MinCpm = 6m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                         MarketId = 2,
                        MarketCoverage = 0.2d,
                        MinCpm = 2m
                    },
                      new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                          MarketId = 3,
                        MarketCoverage = 0.2d,
                        MinCpm = 3m
                    }
                },
                AllMarkets = _LoadAllMarketsObject()
            };
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                OpenMarketPricingGuide = new OpenMarketPricingGuide
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }
            };

            pricingGuideDistributionEngine.CalculateMarketDistribution(pricingGuide, request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuide.Markets));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateMarketDistributionTestMarketsHaveLessCoverageThanGoal()
        {
            var pricingGuide = new PricingGuideOpenMarketInventory
            {
                MarketCoverage = 0.8d,
                Markets = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>
                {
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 0.1d,
                        MinCpm = 6m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 0.2d,
                        MinCpm = 2m
                    },
                      new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 0.2d,
                        MinCpm = 3m
                    }
                },
                AllMarkets = _LoadAllMarketsObject()
            };
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                OpenMarketPricingGuide = new OpenMarketPricingGuide
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }
            };

            pricingGuideDistributionEngine.CalculateMarketDistribution(pricingGuide, request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuide.Markets));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateMarketDistributionTestCoverageLessThanAHundredPercent()
        {
            var pricingGuide = new PricingGuideOpenMarketInventory
            {
                MarketCoverage = 3d,
                Markets = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>
                {
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 0.2d,
                        MinCpm = 6m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 0.5d,
                        MinCpm = 10m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 0.1d,
                        MinCpm = 3m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 0.1d,
                        MinCpm = 1m
                    }
                },
                AllMarkets = _LoadAllMarketsObject()
            };
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                OpenMarketPricingGuide = new OpenMarketPricingGuide
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }
            };

            pricingGuideDistributionEngine.CalculateMarketDistribution(pricingGuide, request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuide.Markets));
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateMarketDistributionTestSelectMarketsMaxCpm()
        {
            var pricingGuide = new PricingGuideOpenMarketInventory
            {
                MarketCoverage = 10d,
                Markets = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>
                {
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 1,
                        MarketCoverage = 5d,
                        MinCpm = 6m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 2,
                        MarketCoverage = 5d,
                        MinCpm = 10m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 3,
                        MarketCoverage = 1d,
                        MinCpm = 3m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                         MarketId = 4,
                        MarketCoverage = 1d,
                        MinCpm = 1m
                    }
                },
                AllMarkets = _LoadAllMarketsObject()
            };
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                OpenMarketPricingGuide = new OpenMarketPricingGuide
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Max
                }
            };

            pricingGuideDistributionEngine.CalculateMarketDistribution(pricingGuide, request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuide.Markets));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateMarketDistributionTestSelectMarketsAvgCpm()
        {
            var pricingGuide = new PricingGuideOpenMarketInventory
            {
                MarketCoverage = 10d,
                Markets = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>
                {
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 1,
                        MarketCoverage = 5d,
                        MinCpm = 6m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 2,
                        MarketCoverage = 5d,
                        MinCpm = 10m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 3,
                        MarketCoverage = 1d,
                        MinCpm = 3m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                         MarketId = 4,
                        MarketCoverage = 1d,
                        MinCpm = 1m
                    }
                },
                AllMarkets = _LoadAllMarketsObject()
            };
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                OpenMarketPricingGuide = new OpenMarketPricingGuide
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Avg
                }
            };

            pricingGuideDistributionEngine.CalculateMarketDistribution(pricingGuide, request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuide.Markets));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateMarketDistributionTest_WithNotSelectedMarkets()
        {
            var pricingGuide = new PricingGuideOpenMarketInventory
            {
                MarketCoverage = 0.00008d,
                Markets = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>
                {
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketId = 1,
                        MarketCoverage = 0.0001d,
                        MinCpm = 6m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                         MarketId = 2,
                        MarketCoverage = 0.00002d,
                        MinCpm = 2m
                    },
                      new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                          MarketId = 3,
                        MarketCoverage = 0.00002d,
                        MinCpm = 3m
                    }
                },
                AllMarkets = _LoadAllMarketsObject()
            };
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                OpenMarketPricingGuide = new OpenMarketPricingGuide
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }
            };

            pricingGuideDistributionEngine.CalculateMarketDistribution(pricingGuide, request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuide.Markets));
        }

        private static List<PricingGuideMarketTotalsDto> _LoadAllMarketsObject()
        {
            return new List<PricingGuideMarketTotalsDto>
                {
                    new PricingGuideMarketTotalsDto
                    {
                        Id = 1
                    },
                    new PricingGuideMarketTotalsDto
                    {
                        Id = 2
                    },
                    new PricingGuideMarketTotalsDto
                    {
                        Id = 3
                    },
                    new PricingGuideMarketTotalsDto
                    {
                        Id = 4
                    }
                };
        }
    }
}
