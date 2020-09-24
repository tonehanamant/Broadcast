using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.PlanPricing
{
    public interface IPlanPricingMarketResultsEngine
    {
        PlanPricingResultMarkets Calculate(List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            List<MarketCoverage> marketCoverages,
            ProprietaryInventoryData proprietaryInventoryData);
    }

    public class PlanPricingMarketResultsEngine : IPlanPricingMarketResultsEngine
    {
        public PlanPricingResultMarkets Calculate(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            List<MarketCoverage> marketCoverages,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            var result = new PlanPricingResultMarkets
            {
                PlanVersionId = allocationResult.PlanVersionId,
                PricingJobId = allocationResult.JobId
            };

            var marketCoverageByMarketCode = marketCoverages.ToDictionary(x => x.MarketCode, x => x);
            var planMarketByMarketCode = plan.AvailableMarkets.ToDictionary(x => x.MarketCode, x => x);
            var stationsByMarketCode = new Dictionary<short, List<int>>();

            result.MarketDetails.AddRange(_GetOpenMarketMarketDetails(
                inventory, 
                allocationResult, 
                marketCoverageByMarketCode, 
                planMarketByMarketCode,
                stationsByMarketCode,
                out var openMarketStationIds));

            result.MarketDetails.AddRange(_GetProprietaryMarketDetails(
                proprietaryInventoryData,
                marketCoverageByMarketCode,
                planMarketByMarketCode,
                stationsByMarketCode,
                out var proprietaryStationIds));

            var stationIds = openMarketStationIds.Union(proprietaryStationIds).ToList();

            result.Totals = _GetTotals(result.MarketDetails, marketCoverageByMarketCode, stationIds);

            result.MarketDetails.ForEach(x => x.ImpressionsPercentage = (x.Impressions / result.Totals.Impressions) * 100);
            result.MarketDetails.ForEach(x => x.StationsPerMarket = stationsByMarketCode[x.MarketCode].Count);

            return result;
        }

        private List<PlanPricingResultMarketDetails> _GetProprietaryMarketDetails(
            ProprietaryInventoryData proprietaryInventoryData,
            Dictionary<int, MarketCoverage> marketCoverageByMarketCode,
            Dictionary<short, PlanAvailableMarketDto> planMarketByMarketCode,
            Dictionary<short, List<int>> stationsByMarketCode,
            out List<int> allStationIds)
        {
            allStationIds = new List<int>();

            var result = new List<PlanPricingResultMarketDetails>();

            foreach (var groupingByMarket in proprietaryInventoryData.ProprietarySummaries.SelectMany(x => x.ProprietarySummaryByStations).GroupBy(x => x.MarketCode))
            {
                var marketCode = groupingByMarket.Key;
                var marketCoverage = marketCoverageByMarketCode[marketCode];
                var marketGroup = groupingByMarket.ToList();

                var stationIds = marketGroup.Select(s => s.StationId).Distinct().ToList();

                stationsByMarketCode[marketCode] = stationsByMarketCode.TryGetValue(marketCode, out var existingStationIds) ?
                    existingStationIds.Union(stationIds).ToList() :
                    stationIds;

                var detail = new PlanPricingResultMarketDetails
                {
                    MarketCode = marketCode,
                    MarketName = marketCoverage.Market,
                    Rank = marketCoverage.Rank.Value,
                    MarketCoveragePercent = marketCoverage.PercentageOfUS,
                    Stations = stationIds.Count,
                    Impressions = marketGroup.Sum(x => x.TotalImpressions),
                    Budget = marketGroup.Sum(s => s.TotalCostWithMargin),
                    Spots = marketGroup.Sum(x => x.TotalSpots),
                    IsProprietary = true
                };

                if (planMarketByMarketCode.TryGetValue(marketCode, out var planMarket))
                    detail.ShareOfVoiceGoalPercentage = planMarket.ShareOfVoicePercent;

                result.Add(detail);
            }

            return result;
        }

        private PlanPricingResultMarketsTotals _GetTotals(
            List<PlanPricingResultMarketDetails> details,
            Dictionary<int, MarketCoverage> marketCoverageByMarketCode,
            List<int> stationIds)
        {
            var totalImpressions = details.Sum(d => d.Impressions);
            var totalBudget = details.Sum(d => d.Budget);
            var marketCodes = details.Select(x => x.MarketCode).Distinct().ToList();

            return new PlanPricingResultMarketsTotals
            {
                Markets = marketCodes.Count,
                CoveragePercent = marketCodes.Sum(x => marketCoverageByMarketCode[x].PercentageOfUS),
                Stations = stationIds.Count,
                Spots = details.Sum(d => d.Spots),
                Impressions = totalImpressions,
                Budget = totalBudget,
                Cpm = ProposalMath.CalculateCpm(totalBudget, totalImpressions)
            };
        }

        private List<PlanPricingResultMarketDetails> _GetOpenMarketMarketDetails(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult allocationResult,
            Dictionary<int, MarketCoverage> marketCoverageByMarketCode,
            Dictionary<short, PlanAvailableMarketDto> planMarketByMarketCode,
            Dictionary<short, List<int>> stationsByMarketCode,
            out List<int> allStationIds)
        {
            allStationIds = new List<int>();

            var result = new List<PlanPricingResultMarketDetails>();
            var inventoryByManifestId = inventory.ToDictionary(x => x.ManifestId, x => x);

            // flatten out to something we can easily aggregate.
            var flatSpots = allocationResult.Spots
                .Select(s =>
                {
                    var inventoryItem = inventoryByManifestId[s.Id];
                    var item = new
                    {
                        InventoryId = s.Id,
                        StationId = inventoryItem.Station.Id,
                        MarketCode = inventoryItem.Station.MarketCode.Value,
                        s.TotalSpots,
                        s.TotalCostWithMargin,
                        s.TotalImpressions
                    };
                    return item;
                })
                .ToList();

            allStationIds.AddRange(flatSpots.Select(x => x.StationId).Distinct());

            foreach (var group in flatSpots.GroupBy(x => x.MarketCode))
            {
                var marketCode = (short)group.Key;
                var marketGroup = group.ToList();
                var marketCoverage = marketCoverageByMarketCode[marketCode];
                var stationIds = marketGroup.Select(s => s.StationId).Distinct().ToList();
                stationsByMarketCode[marketCode] = stationIds;

                var detail = new PlanPricingResultMarketDetails
                {
                    MarketCode = marketCode,
                    MarketName = marketCoverage.Market,
                    Rank = marketCoverage.Rank.Value,
                    MarketCoveragePercent = marketCoverage.PercentageOfUS,
                    Stations = stationIds.Count,
                    Impressions = marketGroup.Sum(s => s.TotalImpressions),
                    Budget = marketGroup.Sum(s => s.TotalCostWithMargin),
                    Spots = marketGroup.Sum(k => k.TotalSpots),
                    IsProprietary = false
                };

                if (planMarketByMarketCode.TryGetValue(marketCode, out var planMarket))
                    detail.ShareOfVoiceGoalPercentage = planMarket.ShareOfVoicePercent;

                result.Add(detail);
            }

            return result;
        }
    }
}