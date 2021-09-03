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

        /// <summary>
        /// Aggregate result of plan buying ownership group result
        /// </summary>
        /// <param name="planBuyingStationResult">The Plan BuyingStation Result dto.</param>
        PlanBuyingResultOwnershipGroupDto CalculateAggregateOfOwnershipGroup(PlanBuyingStationResultDto planBuyingStationResult);
    }

    public class PlanBuyingOwnershipGroupEngine : IPlanBuyingOwnershipGroupEngine
    {
        /// <inheritdoc/>
        public PlanBuyingResultOwnershipGroupDto Calculate(List<PlanBuyingInventoryProgram> inventory, PlanBuyingAllocationResult allocationResult
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
                    OwnershipGroupName = ownershipNameGroup.Key ?? string.Empty,
                    MarketCount = ownershipItems.Select(x=>x.MarketCode).Distinct().Count(),
                    StationCount = ownershipItems.Select(s => s.StationId).Distinct().Count(),
                    SpotCount = ownershipItems.Sum(k => k.TotalSpots),
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

        public PlanBuyingResultOwnershipGroupDto CalculateAggregateOfOwnershipGroup(PlanBuyingStationResultDto planBuyingStationResult)
        {
            var groupOfOwnershipGroups = planBuyingStationResult.Details.GroupBy(u => u.OwnerName, StringComparer.InvariantCultureIgnoreCase).Select(grp => grp.ToList()).ToList();
            var details = new List<PlanBuyingStationDto>();
            details = groupOfOwnershipGroups.Select(ownershipGroup => new PlanBuyingStationDto
            {
                Spots = ownershipGroup.Sum(x => x.Spots),
                Impressions = ownershipGroup.Sum(x => x.Impressions),
                Budget = ownershipGroup.Sum(x => x.Budget),
                Cpm = ProposalMath.CalculateCpm(ownershipGroup.Sum(x => x.Budget), ownershipGroup.Sum(x => x.Impressions)),
                ImpressionsPercentage = ownershipGroup.Sum(x => x.ImpressionsPercentage),
                Affiliate = ownershipGroup.Select(x => x.Affiliate).FirstOrDefault(),
                RepFirm = ownershipGroup.Select(x => x.RepFirm).FirstOrDefault(),
                OwnerName = ownershipGroup.Select(x => x.OwnerName).FirstOrDefault(),
                LegacyCallLetters = ownershipGroup.Select(x => x.LegacyCallLetters).FirstOrDefault(),
                Station = Convert.ToString(ownershipGroup.Select(x => x.Station).Distinct().Count()),
                Market = Convert.ToString(ownershipGroup.Select(x => x.Market).Distinct().Count())
            }).ToList();

            var planBuyingResultOwnershipGroupDetails = details.Select(d => new PlanBuyingResultOwnershipGroupDetailsDto
            {
                Budget = d.Budget,
                Cpm = d.Cpm,
                Impressions = d.Impressions,
                ImpressionsPercentage = d.ImpressionsPercentage,
                MarketCount = Convert.ToInt32(d.Market),
                SpotCount = d.Spots,
                StationCount = Convert.ToInt32(d.Station),
                OwnershipGroupName = d.OwnerName
            }).OrderByDescending(p => p.ImpressionsPercentage).ThenByDescending(p => p.Budget).ToList();

            var totals = new PlanBuyingProgramTotalsDto
            {
                MarketCount = planBuyingResultOwnershipGroupDetails.Sum(x => x.MarketCount),
                Budget = planBuyingResultOwnershipGroupDetails.Sum(x => x.Budget),
                AvgCpm = ProposalMath.CalculateCpm(planBuyingResultOwnershipGroupDetails.Sum(x => x.Budget), planBuyingResultOwnershipGroupDetails.Sum(x => x.Impressions)),
                Impressions = planBuyingResultOwnershipGroupDetails.Sum(x => x.Impressions),
                StationCount = planBuyingResultOwnershipGroupDetails.Sum(x => x.StationCount),
                SpotCount = planBuyingResultOwnershipGroupDetails.Sum(x => x.SpotCount),
                ImpressionsPercentage = (decimal)planBuyingResultOwnershipGroupDetails.Sum(x=>x.ImpressionsPercentage)
            };

            var result = new PlanBuyingResultOwnershipGroupDto
            {
                BuyingJobId = planBuyingStationResult.BuyingJobId,
                PlanVersionId = planBuyingStationResult.PlanVersionId ?? default(int),
                Totals = totals,
                Details = planBuyingResultOwnershipGroupDetails,
                SpotAllocationModelMode = planBuyingStationResult.SpotAllocationModelMode,
                PostingType = planBuyingStationResult.PostingType
               
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
