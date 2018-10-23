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
                        MarketCoverage = 0.6d,
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
                }
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
                }
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
                }
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
                        MarketCoverage = 5d,
                        MinCpm = 6m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 5d,
                        MinCpm = 10m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 1d,
                        MinCpm = 3m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 1d,
                        MinCpm = 1m
                    }
                }
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
                        MarketCoverage = 5d,
                        MinCpm = 6m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 5d,
                        MinCpm = 10m
                    },
                    new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 1d,
                        MinCpm = 3m
                    },
                     new PricingGuideOpenMarketInventory.PricingGuideMarket
                    {
                        MarketCoverage = 1d,
                        MinCpm = 1m
                    }
                }
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
    }
}
