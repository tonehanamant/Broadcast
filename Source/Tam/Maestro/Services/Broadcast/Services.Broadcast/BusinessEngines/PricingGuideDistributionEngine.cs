using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.DTO.PricingGuideOpenMarketInventory;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPricingGuideDistributionEngine : IApplicationService
    {
        void CalculateMarketDistribution(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventor, PricingGuideOpenMarketInventoryRequestDto request);

        void CalculateMarketDistribution(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventor, PricingGuideOpenMarketInventoryRequestDto request, double desiredCoverage);
    }

    public class DistributionMarket
    {
        public decimal Cpm { get; set; }
        public int MarketCoverage { get; set; }
    }

    public class PricingGuideDistributionEngine : IPricingGuideDistributionEngine
    {
        private const double MaxMarketCoverage = 100d;

        public void CalculateMarketDistribution(PricingGuideOpenMarketInventory inventory, PricingGuideOpenMarketInventoryRequestDto request)
        {
            CalculateMarketDistribution(inventory, request, inventory.MarketCoverage);
        }

        public void CalculateMarketDistribution(PricingGuideOpenMarketInventory inventory, PricingGuideOpenMarketInventoryRequestDto request, double desiredCoverage)
        {
            var totalMarketCoverage = inventory.Markets.Sum(x => x.MarketCoverage) / 100;

            // The sum of the coverage of the available markets is less than the desired market coverage.
            // Therefore, all available markets will be selected.
            if (totalMarketCoverage < desiredCoverage)
                return;

            var distributionMarkets = _MapToDistributionMarkets(inventory.Markets, request.OpenMarketPricing.OpenMarketCpmTarget.Value);

            // We convert the coverage to int. That's necessary for the Knapsack algorithm.
            var marketCoverage = (int)((totalMarketCoverage - desiredCoverage) * 100000);

            // The itemsToKeep is a matrix. The number of lines is the number of markets + 1.
            // The columns is the desired market coverage + 1.
            // It will store the solutions for possible market coverage up to the desired coverage.
            // We use the Knapsack algorithm to find the solution, 0/1 variant.
            var itemsToKeep = _FindMarkets(marketCoverage, distributionMarkets, request);

            // Based on the markets that should be kept, set the markets in the pricing guide.
            // This method will go over the matrix and select the markets based on the desired coverage.
            // Each selected market will reduced the remaining available coverage.
            // The matrix contains the solution for every possible combination, so it's a matter of iterating over it and
            // finding the best markets.
            _SetPricingGuideMarkets(itemsToKeep, marketCoverage, distributionMarkets, inventory, request);
        }

        private bool[,] _FindMarkets(int coverage, List<DistributionMarket> markets, PricingGuideOpenMarketInventoryRequestDto request)
        {
            if (request.OpenMarketPricing == null)
                throw new Exception("No Open Market Pricing parameters available");

            if (!request.OpenMarketPricing.OpenMarketCpmTarget.HasValue)
                throw new Exception("No Open Market Pricing CPM target available");

            // The logic here is reversed because the knapsack is the actual remainder between the total avaiable coverage and the desired coverage.
            // The actual selected markets are the markets that were not selected.
            // This ensures some left over coverage.
            if (request.OpenMarketPricing.OpenMarketCpmTarget == OpenMarketCpmTarget.Max)
                return _FindCheapestMarketsKnapsack(coverage, markets);

            return _FindExpensiveMarketsKnapsack(coverage, markets);
        }

        private List<DistributionMarket> _MapToDistributionMarkets(List<PricingGuideMarket> markets, OpenMarketCpmTarget target)
        {
            if(target == OpenMarketCpmTarget.Max)
            {
                return markets.Select(x => new DistributionMarket
                {
                    Cpm = x.MaxCpm,
                    MarketCoverage = (int)Math.Truncate(x.MarketCoverage * 1000)
                }).ToList();
            }

            if(target == OpenMarketCpmTarget.Avg)
            {
                return markets.Select(x => new DistributionMarket
                {
                    Cpm = x.AvgCpm,
                    MarketCoverage = (int)Math.Truncate(x.MarketCoverage * 1000)
                }).ToList();
            }
            
            return markets.Select(x => new DistributionMarket
            {
                Cpm = x.MinCpm,
                MarketCoverage = (int)Math.Truncate(x.MarketCoverage * 1000)
            }).ToList();
        }

        public static bool[,] _FindCheapestMarketsKnapsack(int coverage, List<DistributionMarket> markets)
        {
            var price = new decimal[markets.Count + 1, coverage + 1];
            var marketsToKeep = new bool[markets.Count + 1, coverage + 1];

            for (var j = 1; j < coverage + 1; j++)
                price[0, j] = 99999;

            for (int priceMarketIndex = 1; priceMarketIndex <= markets.Count; priceMarketIndex++)
            {
                var marketIndex = priceMarketIndex - 1;
                var currentMarket = markets[marketIndex];

                for (int coverageIndex = 1; coverageIndex <= coverage; coverageIndex++)
                {
                    if (currentMarket.MarketCoverage <= coverageIndex)
                    {
                        var currentValuePlusValueForRemainingWeight = currentMarket.Cpm + price[marketIndex, coverageIndex - currentMarket.MarketCoverage];

                        if (currentValuePlusValueForRemainingWeight < price[marketIndex, coverageIndex])
                        {
                            price[priceMarketIndex, coverageIndex] = currentValuePlusValueForRemainingWeight;

                            marketsToKeep[priceMarketIndex, coverageIndex] = true;
                        }
                        else
                        {
                            price[priceMarketIndex, coverageIndex] = price[marketIndex, coverageIndex];
                        }
                    }
                    else
                    {
                        price[priceMarketIndex, coverageIndex] = price[marketIndex, coverageIndex];
                    }
                }
            }

            return marketsToKeep;
        }

        public static bool[,] _FindExpensiveMarketsKnapsack(int coverage, List<DistributionMarket> markets)
        {
            var price = new decimal[markets.Count + 1, coverage + 1];
            var itemsToKeep = new bool[markets.Count + 1, coverage + 1];

            for (int priceItemIndex = 1; priceItemIndex <= markets.Count; priceItemIndex++)
            {
                var itemIndex = priceItemIndex - 1;
                var currentMarket = markets[itemIndex];

                for (int coverageIndex = 1; coverageIndex <= coverage; coverageIndex++)
                {
                    if (currentMarket.MarketCoverage <= coverageIndex)
                    {
                        // Evaluate if the current CPM plus what we have stored is greater than what we had.
                        var currentValuePlusValueForRemainingWeight = currentMarket.Cpm + price[itemIndex, coverageIndex - currentMarket.MarketCoverage];

                        if (currentValuePlusValueForRemainingWeight > price[itemIndex, coverageIndex])
                        {
                            // It's better than what we had.
                            price[priceItemIndex, coverageIndex] = currentValuePlusValueForRemainingWeight;

                            itemsToKeep[priceItemIndex, coverageIndex] = true;
                        }
                        else
                        {                            
                            price[priceItemIndex, coverageIndex] = price[itemIndex, coverageIndex];
                        }
                    }
                    else
                    {
                        price[priceItemIndex, coverageIndex] = price[itemIndex, coverageIndex];
                    }
                }
            }

            return itemsToKeep;
        }

        public void _SetPricingGuideMarkets(bool[,] itemsToSelect, int coverage, List<DistributionMarket> markets, PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, PricingGuideOpenMarketInventoryRequestDto request)
        {
            var selectedMarkets = new List<PricingGuideMarket>();
            var itemIndex = markets.Count;
            var remainingCoverage = coverage;
            var coverageCount = remainingCoverage;
            
            while (itemIndex > 0)
            {

                if (itemsToSelect[itemIndex, coverageCount])
                {
                    selectedMarkets.Add(pricingGuideOpenMarketInventory.Markets[itemIndex - 1]);
                    remainingCoverage = remainingCoverage - markets[itemIndex - 1].MarketCoverage;
                    coverageCount = remainingCoverage;
                }

                itemIndex--; 
            }

            pricingGuideOpenMarketInventory.Markets = pricingGuideOpenMarketInventory.Markets.Where(m => !selectedMarkets.Select(sm => sm.MarketId).Contains(m.MarketId)).ToList();                     
        }
    }
}
