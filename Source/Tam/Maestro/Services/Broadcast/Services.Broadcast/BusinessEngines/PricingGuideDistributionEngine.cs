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
    }

    public class DistributionMarket
    {
        public decimal Cpm { get; set; }
        public int MarketCoverage { get; set; }
    }

    public class PricingGuideDistributionEngine : IPricingGuideDistributionEngine
    {
        private const double MaxMarketCoverage = 100d;

        public void CalculateMarketDistribution(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, PricingGuideOpenMarketInventoryRequestDto request)
        {
            if (!pricingGuideOpenMarketInventory.MarketCoverage.HasValue)
                return;

            var totalMarketCoverage = pricingGuideOpenMarketInventory.Markets.Sum(x => x.MarketCoverage) / 100;

            // The sum of the coverage of the available markets is less than the desired market coverage.
            // Therefore, all available markets will be selected.
            if (totalMarketCoverage < pricingGuideOpenMarketInventory.MarketCoverage)
                return;

            var distributionMarkets = _MapToDistributionMarkets(pricingGuideOpenMarketInventory.Markets);

            // We convert the coverage to int. That's necessary for the Knapsack algorithm.
            var marketCoverage = (int)(pricingGuideOpenMarketInventory.MarketCoverage * 100000);

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
            _SetPricingGuideMarkets(itemsToKeep, marketCoverage, distributionMarkets, pricingGuideOpenMarketInventory);
        }

        private bool[,] _FindMarkets(int coverage, List<DistributionMarket> markets, PricingGuideOpenMarketInventoryRequestDto request)
        {
            if (request.OpenMarketPricing == null)
                throw new Exception("No Open Market Pricing parameters available");

            if (!request.OpenMarketPricing.OpenMarketCpmTarget.HasValue)
                throw new Exception("No Open Market Pricing CPM target available");

            var cpmTarget = request.OpenMarketPricing.OpenMarketCpmTarget.Value;

            if (cpmTarget == OpenMarketCpmTarget.Max)
                return _FindExpensiveMarketsKnapsack(coverage, markets);

            return _FindCheapestMarketsKnapsack(coverage, markets);
        }

        private List<DistributionMarket> _MapToDistributionMarkets(List<PricingGuideMarket> markets)
        {
            return markets.Select(x => new DistributionMarket
            {
                Cpm = x.MinCpm,
                MarketCoverage = (int)Math.Truncate(x.MarketCoverage * 1000)
            }).ToList();
        }

        public static bool[,] _FindCheapestMarketsKnapsack(int coverage, List<DistributionMarket> markets)
        {
            var price = new decimal[markets.Count + 1, coverage + 1];
            var itemsToKeep = new bool[markets.Count + 1, coverage + 1];

            for (var j = 1; j < coverage + 1; j++)
                price[0, j] = 99999;

            for (int priceItemIndex = 1; priceItemIndex <= markets.Count; priceItemIndex++)
            {
                var itemIndex = priceItemIndex - 1;
                var currentMarket = markets[itemIndex];

                for (int coverageIndex = 1; coverageIndex <= coverage; coverageIndex++)
                {
                    if (currentMarket.MarketCoverage <= coverageIndex)
                    {
                        var currentValuePlusValueForRemainingWeight = currentMarket.Cpm + price[itemIndex, coverageIndex - currentMarket.MarketCoverage];

                        if (currentValuePlusValueForRemainingWeight < price[itemIndex, coverageIndex])
                        {
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

        public void _SetPricingGuideMarkets(bool[,] itemsToSelect, int coverage, List<DistributionMarket> markets, PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            var selectedMarkets = new List<PricingGuideMarket>();
            var itemIndex = markets.Count;
            var remainingCoverage = coverage;
            var coverageCount = remainingCoverage;

            while (itemIndex > 0)
            {
                while (coverageCount > 0)
                {
                    if (itemsToSelect[itemIndex, coverageCount])
                    {
                        selectedMarkets.Add(pricingGuideOpenMarketInventory.Markets[itemIndex - 1]);
                        remainingCoverage = remainingCoverage - markets[itemIndex - 1].MarketCoverage;
                        coverageCount = remainingCoverage;
                        break;
                    }

                    coverageCount--;
                }

                coverageCount = remainingCoverage;
                itemIndex--;
            }

            pricingGuideOpenMarketInventory.Markets = selectedMarkets;
        }
    }
}
