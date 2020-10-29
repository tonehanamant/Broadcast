using Services.Broadcast.Entities.Plan.Buying;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.PlanBuying
{
    public interface IPlanBuyingRepFirmEngine
    {
        /// <summary>
        /// Calculates the specified inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="allocationResult">The allocation result.</param>
        /// <param name="parametersDto">The parameters dto.</param>
        /// <returns>PlanBuyingResultRepFirmDto object</returns>
        PlanBuyingResultRepFirmDto Calculate(List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto);

        /// <summary>
        /// Converts the buying ownership group impressions to user format.
        /// </summary>
        /// <param name="results">The buying ownership group results.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingResultRepFirmDto results);
    }

    public class PlanBuyingRepFirmEngine : IPlanBuyingRepFirmEngine
    {
        /// <inheritdoc/>
        public PlanBuyingResultRepFirmDto Calculate(List<PlanBuyingInventoryProgram> inventory, PlanBuyingAllocationResult allocationResult
            , PlanBuyingParametersDto parametersDto)
        {
            var inventoryDictionary = inventory.ToDictionary(x => x.ManifestId, x => x);
            // flatten out to something we can easily aggregate.
            var flatSpots = allocationResult.AllocatedSpots.Select(s =>
            {
                PlanBuyingInventoryProgram inventoryItem = inventoryDictionary[s.Id];
                var item = new
                {
                    InventoryId = s.Id,
                    StationId = inventoryItem.Station.Id,
                    inventoryItem.Station.MarketCode,
                    s.TotalSpots,
                    s.TotalCostWithMargin,
                    s.TotalImpressions,
                    inventoryItem.Station.RepFirmName
                };
                return item;
            }).ToList();

            // perform the main aggregation
            var details = new List<PlanBuyingResultRepFirmDetailsDto>();
            foreach (var repFirmGroup in flatSpots.GroupBy(x => x.RepFirmName))
            {
                var repFirmItems = repFirmGroup.ToList();
                var aggregatedSpotCostWithMargin = repFirmItems.Sum(s => s.TotalCostWithMargin);
                var aggregatedImpressions = repFirmItems.Sum(s => s.TotalImpressions);

                var agg = new PlanBuyingResultRepFirmDetailsDto
                {
                    RepFirmName = repFirmGroup.Key ?? string.Empty,
                    MarketCount = repFirmItems.Select(x => x.MarketCode).Distinct().Count(),
                    StationCount = repFirmItems.Select(s => s.StationId).Distinct().Count(),
                    SpotCount = repFirmItems.Sum(k => k.TotalSpots),
                    Impressions = aggregatedImpressions,
                    Budget = aggregatedSpotCostWithMargin,
                    Cpm = ProposalMath.CalculateCpm(aggregatedSpotCostWithMargin, aggregatedImpressions),
                };
                details.Add(agg);
            }

            // calculate our totals
            var totalImpressions = details.Sum(d => d.Impressions);

            // determine percentage of totals
            details.ForEach(d => d.ImpressionsPercentage = (d.Impressions / totalImpressions) * 100);

            // enforce the ordering here for output readability
            details = details.OrderByDescending(s => s.ImpressionsPercentage).ThenByDescending(x => x.Budget).ToList();

            var result = new PlanBuyingResultRepFirmDto
            {
                PlanVersionId = allocationResult.PlanVersionId,
                BuyingJobId = allocationResult.JobId,
                Details = details
            };

            return result;
        }

        /// <inheritdoc/>
        public void ConvertImpressionsToUserFormat(PlanBuyingResultRepFirmDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.Details)
            {
                detail.Impressions /= 1000;
            }
        }
    }
}
