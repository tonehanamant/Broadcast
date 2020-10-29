﻿using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.PlanBuying
{
    public interface IPlanBuyingMarketResultsEngine
    {
        /// <summary>
        /// Calculates the specified inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="allocationResult">The allocation result.</param>
        /// <param name="parametersDto">The parameters dto.</param>
        /// <param name="plan">The plan.</param>
        /// <param name="marketCoverages">The market coverages.</param>
        /// <returns>PlanBuyingResultMarketsDto object</returns>
        PlanBuyingResultMarketsDto Calculate(List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto,
            PlanDto plan,
            List<MarketCoverage> marketCoverages);

        /// <summary>
        /// Converts the buying market impressions to user format.
        /// </summary>
        /// <param name="results">The buying market results.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingResultMarketsDto results);
    }

    public class PlanBuyingMarketEngine : IPlanBuyingMarketResultsEngine
    {
        /// <inheritdoc/>
        public PlanBuyingResultMarketsDto Calculate(List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto,
            PlanDto plan,
            List<MarketCoverage> marketCoverages)
        {
            // flatten out to something we can easily aggregate.
            var flatSpots = allocationResult.AllocatedSpots.Select(s =>
            {
                var inventoryItem = inventory.Single(i => i.ManifestId.Equals(s.Id));
                var item = new
                {
                    InventoryId = s.Id,
                    StationId = inventoryItem.Station?.Id ?? -1,
                    MarketCode = inventoryItem.Station?.MarketCode ?? -1,
                    s.TotalSpots,
                    s.TotalCostWithMargin,
                    s.TotalImpressions
                };
                return item;
            }).ToList();

            // perform the main aggregation
            var details = new List<PlanBuyingResultMarketDetailsDto>();
            var relevantMarketCodes = flatSpots.Select(s => s.MarketCode).Distinct().ToList();
            foreach (var marketCode in relevantMarketCodes)
            {
                var marketCoverage = marketCode < 1
                    ? new MarketCoverage { Rank = marketCode, PercentageOfUS = 0.0 }
                    : marketCoverages.Single(c => c.MarketCode.Equals(marketCode));

                var planMarket = plan.AvailableMarkets.SingleOrDefault(c => c.MarketCode == marketCode);

                var marketGroup = flatSpots.Where(s => s.MarketCode.Equals(marketCode)).ToList();
                var aggregatedSpotCostWithMargin = marketGroup.Sum(s => s.TotalCostWithMargin);
                var aggregatedImpressions = marketGroup.Sum(s => s.TotalImpressions);

                var agg = new PlanBuyingResultMarketDetailsDto
                {
                    MarketName = marketCoverage.Market,
                    Rank = marketCoverage.Rank ?? -2,
                    MarketCoveragePercent = marketCoverage.PercentageOfUS,
                    StationCount = marketGroup.Select(s => s.StationId).Distinct().Count(),
                    SpotCount = marketGroup.Sum(k => k.TotalSpots),
                    Impressions = aggregatedImpressions,
                    Budget = aggregatedSpotCostWithMargin,
                    Cpm = ProposalMath.CalculateCpm(aggregatedSpotCostWithMargin, aggregatedImpressions),
                    ShareOfVoiceGoalPercentage = planMarket?.ShareOfVoicePercent
                };
                details.Add(agg);
            }

            // calculate our totals
            var totalImpressions = details.Sum(d => d.Impressions);
            var totals = new PlanBuyingProgramTotalsDto
            {
                MarketCoveragePercent = details.Sum(d => d.MarketCoveragePercent),
            };

            // determine percentage of totals
            details.ForEach(d => d.ImpressionsPercentage = (d.Impressions / totalImpressions) * 100);

            // enforce the ordering here for output readability
            details = details.OrderBy(s => s.Rank).ToList();

            var result = new PlanBuyingResultMarketsDto
            {
                PlanVersionId = allocationResult.PlanVersionId,
                BuyingJobId = allocationResult.JobId,
                Totals = totals,
                Details = details
            };

            return result;
        }

        /// <inheritdoc/>
        public void ConvertImpressionsToUserFormat(PlanBuyingResultMarketsDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.Details)
            {
                detail.Impressions /= 1000;
            }
        }
    }
}