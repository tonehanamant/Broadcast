using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;

namespace Services.Broadcast.BusinessEngines
{
    /// <summary>
    /// A calculator for performing Plan Market SOV calculations.
    /// </summary>
    public interface IPlanMarketSovCalculator
    {
        /// <summary>
        /// Calculates the market weight change.
        /// </summary>
        /// <param name="availableMarkets">The available markets.</param>
        /// <param name="modifiedMarketCode">The modified market code.</param>
        /// <param name="userEnteredValue">The user entered value.</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketWeightChange(List<PlanAvailableMarketDto> availableMarkets,
            short modifiedMarketCode, double? userEnteredValue);

        /// <summary>
        /// Add the markets and recalculate the unlocked weights.
        /// </summary>
        /// <param name="beforeMarkets">The before markets.</param>
        /// <param name="addedMarkets">A list of markets to add.</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketsAdded(List<PlanAvailableMarketDto> beforeMarkets,
            List<PlanAvailableMarketDto> addedMarkets);

        /// <summary>
        /// Remove the markets and recalculate the unlocked weights.
        /// </summary>
        /// <param name="beforeMarkets">The before markets.</param>
        /// <param name="removedMarketCodes">A list of the market codes to remove</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketsRemoved(List<PlanAvailableMarketDto> beforeMarkets,
            List<short> removedMarketCodes);

        /// <summary>
        /// Calculates the market weights.
        /// </summary>
        /// <param name="markets">The markets.</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketWeights(List<PlanAvailableMarketDto> markets);
    }

    /// <summary>
    /// Performs Plan Market Share Of Voice calculations per business rules.
    /// </summary>
    public class PlanMarketSovCalculator : IPlanMarketSovCalculator
    {
        const int ShareOfVoiceDecimalPlaces = 5;

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketWeightChange(List<PlanAvailableMarketDto> availableMarkets,
            short modifiedMarketCode, double? userEnteredValue)
        {
            var markets = _GetCopy(availableMarkets);
            // find the modified
            var modifiedMarket = markets.Single(m => m.MarketCode == modifiedMarketCode);
            // modify it
            modifiedMarket.ShareOfVoicePercent = userEnteredValue;
            modifiedMarket.IsUserShareOfVoicePercent = userEnteredValue.HasValue;
            // balance
            var results = CalculateMarketWeights(markets);
            return results;
        }

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketsAdded(List<PlanAvailableMarketDto> beforeMarkets,
            List<PlanAvailableMarketDto> addedMarkets)
        {
            var markets = _GetCopy(beforeMarkets);
            markets.AddRange(addedMarkets);
            var results = CalculateMarketWeights(markets);
            return results;
        }

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketsRemoved(List<PlanAvailableMarketDto> beforeMarkets,
            List<short> removedMarketCodes)
        {
            var markets = _GetCopyAndRemoveMarkets(beforeMarkets, removedMarketCodes);
            var results = CalculateMarketWeights(markets);
            return results;
        }

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketWeights(List<PlanAvailableMarketDto> markets)
        {
            var copiedMarkets = _GetCopy(markets);
            _BalanceMarketWeights(copiedMarkets);
            var results = _GetResults(copiedMarkets);
            return results;
        }

        internal void _BalanceMarketWeights(List<PlanAvailableMarketDto> markets)
        {
            var totalUserEnteredCoverage = markets.Where(m => m.IsUserShareOfVoicePercent)
                .Sum(m => m.ShareOfVoicePercent ?? 0);

            if (totalUserEnteredCoverage >= 100)
            {
                markets.Where(m => !m.IsUserShareOfVoicePercent).ForEach(m => m.ShareOfVoicePercent = 0);
                return;
            }

            var totalRepresentedCoverage = markets.Sum(m => m.PercentageOfUS);
            var totalRepresentedCoverageNonUser = markets.Where(m => !m.IsUserShareOfVoicePercent).Sum(m => m.PercentageOfUS);
            var representedMarkets = markets.Select(m => new RepresentedMarket
            {
                Market = m,
                PercentOfRepresentedCoverage = (m.PercentageOfUS / totalRepresentedCoverage) * 100.0
            }).ToList();

            var remainderToDistribute = 100.0 - totalUserEnteredCoverage;
            representedMarkets.Where(m => !m.Market.IsUserShareOfVoicePercent).ForEach(m => 
                m.Market.ShareOfVoicePercent = Math.Round(remainderToDistribute * (m.Market.PercentageOfUS / totalRepresentedCoverageNonUser), ShareOfVoiceDecimalPlaces));
        }

        internal PlanAvailableMarketCalculationResult _GetResults(List<PlanAvailableMarketDto> markets)
        {
            var orderedMarkets = markets.OrderBy(m => m.Rank).ToList();
            var totalWeight = _GetTotalWeight(markets);
            var result = new PlanAvailableMarketCalculationResult
            {
                AvailableMarkets = orderedMarkets,
                TotalWeight = totalWeight
            };
            return result;
        }

        internal double _GetTotalWeight(List<PlanAvailableMarketDto> markets)
        {
            var totalWeightDecimalPlaces = 3;
            var totalWeight = Math.Round(markets.Sum(m => m.ShareOfVoicePercent ?? 0), totalWeightDecimalPlaces);
            return totalWeight;
        }

        internal List<PlanAvailableMarketDto> _GetCopy(List<PlanAvailableMarketDto> toCopy)
        {
            var result = new List<PlanAvailableMarketDto>();
            toCopy.ForEach(c => result.Add(_GetCopy(c)));
            return result;
        }

        internal List<PlanAvailableMarketDto> _GetCopyAndRemoveMarkets(List<PlanAvailableMarketDto> toCopy, List<short> removedMarketCodes)
        {
            var result = new List<PlanAvailableMarketDto>();
            toCopy.ForEach(c =>
            {
                if (!removedMarketCodes.Contains(c.MarketCode))
                {
                    result.Add(_GetCopy(c));
                }
            });
            return result;
        }

        private PlanAvailableMarketDto _GetCopy(PlanAvailableMarketDto toCopy)
        {
            var copy = new PlanAvailableMarketDto
            {
                Id = toCopy.Id,
                MarketCode = toCopy.MarketCode,
                MarketCoverageFileId = toCopy.MarketCoverageFileId,
                Rank = toCopy.Rank,
                PercentageOfUS = toCopy.PercentageOfUS,
                Market = toCopy.Market,
                ShareOfVoicePercent = toCopy.ShareOfVoicePercent,
                IsUserShareOfVoicePercent = toCopy.IsUserShareOfVoicePercent
            };
            return copy;
        }

        private class RepresentedMarket
        {
            public PlanAvailableMarketDto Market { get; set; }
            public double? PercentOfRepresentedCoverage { get; set; }
        }
    }
}