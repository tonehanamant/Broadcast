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

        /// <summary>
        /// Clears the user entered values and recalculates the weights.
        /// </summary>
        /// <param name="availableMarkets">The available markets.</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketWeightsClearAll(List<PlanAvailableMarketDto> availableMarkets);

        /// <summary>
        /// Determines if the sum of the given market sov weights exceeds the given threshold.
        /// </summary>
        /// <param name="markets">The markets.</param>
        /// <param name="threshold">The threshold.</param>
        /// <returns></returns>
        bool DoesMarketSovTotalExceedThreshold(List<PlanAvailableMarketDto> markets, double threshold);
    }

    /// <summary>
    /// Performs Plan Market Share Of Voice calculations per business rules.
    /// </summary>
    public class PlanMarketSovCalculator : IPlanMarketSovCalculator
    {
        private const int MarketTotalSovDecimals = 2;

        private const int MarketSovDecimals = 3;

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketWeightChange(List<PlanAvailableMarketDto> availableMarkets,
            short modifiedMarketCode, double? userEnteredValue)
        {
            var markets = _GetCopy(availableMarkets);
            // find the modified
            var modifiedMarket = markets.Single(m => m.MarketCode == modifiedMarketCode);
            // modify it
            modifiedMarket.ShareOfVoicePercent = userEnteredValue.HasValue ?
                    Math.Round(userEnteredValue.Value, MarketSovDecimals)
                : userEnteredValue;
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
            addedMarkets.Where(s => s.IsUserShareOfVoicePercent && !s.ShareOfVoicePercent.HasValue)
                .ForEach(s => s.IsUserShareOfVoicePercent = false);
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

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketWeightsClearAll(List<PlanAvailableMarketDto> availableMarkets)
        {
            var markets = _GetCopy(availableMarkets, clearUserValue:true);
            _BalanceMarketWeights(markets);
            var results = _GetResults(markets);
            return results;
        }

        /// <inheritdoc />
        public bool DoesMarketSovTotalExceedThreshold(List<PlanAvailableMarketDto> markets, double threshold)
        {
            var totalMarketSov = Math.Round(markets.Sum(m => m.ShareOfVoicePercent ?? 0), MarketTotalSovDecimals);
            var result = totalMarketSov > threshold;
            return result;
        }

        private void _BalanceMarketWeights(List<PlanAvailableMarketDto> markets)
        {
            var totalUserEnteredSov = markets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent ?? 0);

            if (totalUserEnteredSov >= 100)
            {
                markets.Where(m => !m.IsUserShareOfVoicePercent).ForEach(m => m.ShareOfVoicePercent = 0);
                return;
            }

            var totalRepresentedCoverage = markets.Sum(m => m.PercentageOfUS);
            var totalRepresentedCoverageNonUser = 
                markets.Where(m => !m.IsUserShareOfVoicePercent).Sum(m => m.PercentageOfUS);
            
            var remainderToDistribute = 100.0 - totalUserEnteredSov;
            markets.Where(m => !m.IsUserShareOfVoicePercent).ForEach(m =>
                m.ShareOfVoicePercent = remainderToDistribute * (m.PercentageOfUS / totalRepresentedCoverageNonUser));
        }

        private PlanAvailableMarketCalculationResult _GetResults(List<PlanAvailableMarketDto> markets)
        {
            var orderedMarkets = markets.OrderBy(m => m.Rank).ToList();
            var totalWeight = orderedMarkets.Sum(m => m.ShareOfVoicePercent ?? 0);
            var result = new PlanAvailableMarketCalculationResult
            {
                AvailableMarkets = orderedMarkets,
                TotalWeight = totalWeight
            };
            _RoundResults(result);
            return result;
        }

        private void _RoundResults(PlanAvailableMarketCalculationResult result)
        {
            result.AvailableMarkets.ForEach(s => s.ShareOfVoicePercent = Math.Round(s.ShareOfVoicePercent.Value, MarketSovDecimals));
            result.TotalWeight = Math.Round(result.TotalWeight, MarketTotalSovDecimals);
        }

        private List<PlanAvailableMarketDto> _GetCopy(List<PlanAvailableMarketDto> toCopy, bool clearUserValue = false)
        {
            var result = new List<PlanAvailableMarketDto>();
            toCopy.ForEach(c => result.Add(_GetCopy(c, clearUserValue)));
            return result;
        }

        private List<PlanAvailableMarketDto> _GetCopyAndRemoveMarkets(List<PlanAvailableMarketDto> toCopy, List<short> removedMarketCodes)
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

        private PlanAvailableMarketDto _GetCopy(PlanAvailableMarketDto toCopy, bool clearUserValue = false)
        {
            var copy = new PlanAvailableMarketDto
            {
                Id = toCopy.Id,
                MarketCode = toCopy.MarketCode,
                MarketCoverageFileId = toCopy.MarketCoverageFileId,
                Rank = toCopy.Rank,
                PercentageOfUS = toCopy.PercentageOfUS,
                Market = toCopy.Market,
                ShareOfVoicePercent = clearUserValue ? null : toCopy.ShareOfVoicePercent,
                IsUserShareOfVoicePercent = !clearUserValue && toCopy.IsUserShareOfVoicePercent
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