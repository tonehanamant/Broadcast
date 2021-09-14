using Services.Broadcast.Entities;
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

        /// <summary>
        /// Creates Aggregated results of Plan Buying Market.
        /// </summary>
        /// <param name="planBuyingStationResult">The PlanBuyingResultMarketsDto object.</param>
        /// <param name="marketCoverages">The market coverages.</param>
        /// <param name="plan">The plan object for available markets.</param>
        PlanBuyingResultMarketsDto CalculateAggregatedResultOfMarket(PlanBuyingStationResultDto planBuyingStationResult, List<MarketCoverage> marketCoverages, PlanDto plan);
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

        /// <inheritdoc/>
        public PlanBuyingResultMarketsDto CalculateAggregatedResultOfMarket(PlanBuyingStationResultDto planBuyingStationResult, List<MarketCoverage> marketCoverages, PlanDto plan)
        {
            var groupOfMarkets = planBuyingStationResult.Details.GroupBy(u => u.Market, StringComparer.InvariantCultureIgnoreCase).Select(grp => grp.ToList()).ToList();
            var planBuyingStationDetails = new List<PlanBuyingStationDto>();
            var planBuyingResultMarketDetails = new List<PlanBuyingResultMarketDetailsDto>();
            planBuyingStationDetails = groupOfMarkets.Select(market => new PlanBuyingStationDto
            {
                Spots = market.Sum(x => x.Spots),
                Impressions = market.Sum(x => x.Impressions),
                Budget = market.Sum(x => x.Budget),
                Cpm = ProposalMath.CalculateCpm(market.Sum(x => x.Budget), market.Sum(x => x.Impressions)),
                ImpressionsPercentage = market.Sum(x => x.ImpressionsPercentage),
                Affiliate = market.Select(x => x.Affiliate).FirstOrDefault(),
                RepFirm = market.Select(x => x.RepFirm).FirstOrDefault(),
                OwnerName = market.Select(x => x.OwnerName).FirstOrDefault(),
                LegacyCallLetters = market.Select(x => x.LegacyCallLetters).FirstOrDefault(),
                Station = market.Select(x => x.Station).FirstOrDefault(),
                Market = market.Select(x => x.Market).FirstOrDefault()
            }).ToList();
            
            var planBuyingMarketList = from d in planBuyingStationDetails
                                      join m in marketCoverages
                                        on d.Market equals m.Market
                                      select new
                                      { 
                                        d.Market,
                                        m.PercentageOfUS,
                                        m.MarketCode,
                                        m.Rank,
                                        d.Station,
                                        d.Spots,
                                        d.Impressions,
                                        d.Cpm,
                                        d.Budget
                                      };
            var marketCodes = planBuyingMarketList.Select(s => s.MarketCode).Distinct().ToList();

            
            foreach (var code in marketCodes) 
            {
                var planMarket = plan.AvailableMarkets.SingleOrDefault(c => c.MarketCode == code);
                var marketCoverage = marketCoverages.Single(c => c.MarketCode == code);
                var marketGroup = planBuyingMarketList.Where(s => s.MarketCode == code).ToList();
                var aggregatedSpotCostWithMargin = marketGroup.Sum(s => s.Budget);
                var aggregatedImpressions = marketGroup.Sum(s => s.Impressions);
                var aggPlanBuyingResultMarketDetails = new PlanBuyingResultMarketDetailsDto
                {
                    MarketName = marketCoverage.Market,
                    Rank = marketCoverage.Rank ?? default(int),
                    MarketCoveragePercent = marketCoverage.PercentageOfUS,
                    StationCount = marketGroup.Select(s => s.Station).Distinct().Count(),
                    SpotCount = marketGroup.Sum(k => k.Spots),
                    Impressions = aggregatedImpressions,
                    Budget = aggregatedSpotCostWithMargin,
                    Cpm = ProposalMath.CalculateCpm(aggregatedSpotCostWithMargin, aggregatedImpressions),
                    ShareOfVoiceGoalPercentage = planMarket?.ShareOfVoicePercent
                };
                planBuyingResultMarketDetails.Add(aggPlanBuyingResultMarketDetails);
            }

            var totals = new PlanBuyingProgramTotalsDto
            {
                MarketCount = planBuyingResultMarketDetails.Select(x => x.MarketName).Count(),
                Budget = planBuyingResultMarketDetails.Sum(x => x.Budget),
                AvgCpm = ProposalMath.CalculateCpm(planBuyingResultMarketDetails.Sum(x => x.Budget), planBuyingResultMarketDetails.Sum(x => x.Impressions)),
                Impressions = planBuyingResultMarketDetails.Sum(x => x.Impressions),
                StationCount = planBuyingResultMarketDetails.Sum(x => x.StationCount),
                SpotCount = planBuyingResultMarketDetails.Sum(x => x.SpotCount),
                MarketCoveragePercent = planBuyingResultMarketDetails.Sum(x=>x.MarketCoveragePercent)
            };

            var result = new PlanBuyingResultMarketsDto
            {
                BuyingJobId = planBuyingStationResult.BuyingJobId,
                PlanVersionId = planBuyingStationResult.PlanVersionId ?? default(int),
                Totals = totals,
                Details = planBuyingResultMarketDetails,
                SpotAllocationModelMode = planBuyingStationResult.SpotAllocationModelMode,
                PostingType = planBuyingStationResult.PostingType
            };
            return result;
        }
    }
}