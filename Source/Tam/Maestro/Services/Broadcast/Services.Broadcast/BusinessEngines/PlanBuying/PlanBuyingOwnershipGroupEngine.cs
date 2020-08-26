using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.BusinessEngines.PlanBuying
{
    public interface IPlanBuyingOwnershipGroupEngine
    {
        /// <summary>
        /// Calculates the specified inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="allocationResult">The allocation result.</param>
        /// <param name="parametersDto">The parameters dto.</param>
        /// <returns>PlanBuyingResultOwnershipGroupDto object</returns>
        PlanBuyingResultOwnershipGroupDto Calculate(List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto);

        /// <summary>
        /// Converts the buying ownership group impressions to user format.
        /// </summary>
        /// <param name="results">The buying ownership group results.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingResultOwnershipGroupDto results);
    }

    public class PlanBuyingOwnershipGroupEngine : IPlanBuyingOwnershipGroupEngine
    {
        /// <inheritdoc/>
        public PlanBuyingResultOwnershipGroupDto Calculate(List<PlanBuyingInventoryProgram> inventory, PlanBuyingAllocationResult allocationResult
            , PlanBuyingParametersDto parametersDto)
        {
            var inventoryDictionary = inventory.ToDictionary(x => x.ManifestId, x => x);
            // flatten out to something we can easily aggregate.
            var flatSpots = allocationResult.Spots.Select(s =>
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
                    inventoryItem.Station.OwnershipGroupName
                };
                return item;
            }).ToList();

            // perform the main aggregation
            var details = new List<PlanBuyingResultOwnershipGroupDetailsDto>();
            foreach (var ownershipNameGroup in flatSpots.GroupBy(x=>x.OwnershipGroupName))
            {
                var ownershipItems = ownershipNameGroup.ToList();
                var aggregatedSpotCostWithMargin = ownershipItems.Sum(s => s.TotalCostWithMargin);
                var aggregatedImpressions = ownershipItems.Sum(s => s.TotalImpressions);

                var agg = new PlanBuyingResultOwnershipGroupDetailsDto
                {
                    OwnershipGroupName = ownershipNameGroup.Key,
                    Markets = ownershipItems.Select(x=>x.MarketCode).Distinct().Count(),
                    Stations = ownershipItems.Select(s => s.StationId).Distinct().Count(),
                    Spots = ownershipItems.Sum(k => k.TotalSpots),
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
            details = details.OrderByDescending(s => s.ImpressionsPercentage).ThenByDescending(x=>x.Budget).ToList();

            var result = new PlanBuyingResultOwnershipGroupDto
            {
                PlanVersionId = allocationResult.PlanVersionId,
                BuyingJobId = allocationResult.JobId,
                Details = details
            };

            return result;
        }

        /// <inheritdoc/>
        public void ConvertImpressionsToUserFormat(PlanBuyingResultOwnershipGroupDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.Details)
            {
                detail.Impressions /= 1000;
            }
        }
    }
}
