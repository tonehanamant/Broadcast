using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.PlanPricing
{
    public interface IPlanPricingMarketResultsEngine
    {
        PlanPricingResultMarketsDto Calculate(List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult allocationResult,
            PlanPricingParametersDto parametersDto,
            PlanDto plan,
            List<MarketCoverage> marketCoverages);
    }

    public class PlanPricingMarketResultsEngine : IPlanPricingMarketResultsEngine
    {
        public PlanPricingResultMarketsDto Calculate(List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult allocationResult,
            PlanPricingParametersDto parametersDto,
            PlanDto plan,
            List<MarketCoverage> marketCoverages)
        {
            // flatten out to something we can easily aggregate.
            var flatSpots = allocationResult.Spots.Select(s =>
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
            var details = new List<PlanPricingResultMarketDetailsDto>();
            var relevantMarketCodes = flatSpots.Select(s => s.MarketCode).Distinct().ToList();
            foreach (var marketCode in relevantMarketCodes)
            {
                var marketCoverage = marketCode < 1
                    ? new MarketCoverage {Rank = marketCode, PercentageOfUS = 0.0 }
                    : marketCoverages.Single(c => c.MarketCode.Equals(marketCode));

                var planMarket = plan.AvailableMarkets.SingleOrDefault(c => c.MarketCode == marketCode);

                var marketGroup = flatSpots.Where(s => s.MarketCode.Equals(marketCode)).ToList();
                var aggregatedSpotCostWithMargin = marketGroup.Sum(s => s.TotalCostWithMargin);
                var aggregatedImpressions = marketGroup.Sum(s => s.TotalImpressions);

                var agg = new PlanPricingResultMarketDetailsDto
                {
                    Rank = marketCoverage.Rank ?? -2,
                    MarketCoveragePercent = marketCoverage.PercentageOfUS,
                    Stations = marketGroup.Select(s => s.StationId).Distinct().Count(),
                    Spots = marketGroup.Sum(k => k.TotalSpots),
                    Impressions = aggregatedImpressions,
                    Budget = aggregatedSpotCostWithMargin,
                    Cpm = ProposalMath.CalculateCpm(aggregatedSpotCostWithMargin, aggregatedImpressions),
                    ShareOfVoiceGoalPercentage = planMarket?.ShareOfVoicePercent
                };
                details.Add(agg);
            }

            // calculate our totals
            var totalImpressions = details.Sum(d => d.Impressions);
            var totalBudget = details.Sum(d => d.Budget);
            var totals = new PlanPricingResultMarketsTotalsDto
            {
                Markets = relevantMarketCodes.Count,
                CoveragePercent = details.Sum(d => d.MarketCoveragePercent),
                Stations = details.Sum(d => d.Stations),
                Spots = details.Sum(d => d.Spots),
                Impressions = totalImpressions,
                Cpm = ProposalMath.CalculateCpm(totalBudget, totalImpressions),
                Budget = totalBudget
            };

            // determine percentage of totals
            details.ForEach(d => d.ImpressionsPercentage = (d.Impressions / totals.Impressions) * 100);

            // enforce the ordering here for output readability
            details = details.OrderBy(s => s.Rank).ToList();

            var result = new PlanPricingResultMarketsDto
            {
                PlanVersionId = allocationResult.PlanVersionId,
                PricingJobId = allocationResult.JobId,
                Totals = totals,
                MarketDetails = details
            };

            return result;
        }
    }
}