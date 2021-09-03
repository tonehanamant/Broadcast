using Services.Broadcast.Entities.Plan.Buying;
using System;
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

        /// <summary>
        /// Aggregate result of plan buying RepFirm result
        /// </summary>
        /// <param name="planBuyingStationResult">The Plan Buying Station Result dto.</param>
        PlanBuyingResultRepFirmDto CalculateAggregateOfRepFirm(PlanBuyingStationResultDto planBuyingStationResult);
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

        public PlanBuyingResultRepFirmDto CalculateAggregateOfRepFirm(PlanBuyingStationResultDto planBuyingStationResult)
        {
            var groupOfRepFirm = planBuyingStationResult.Details.GroupBy(u => u.RepFirm, StringComparer.InvariantCultureIgnoreCase).Select(grp => grp.ToList()).ToList();
            var details = new List<PlanBuyingStationDto>();
            details = groupOfRepFirm.Select(repFirm => new PlanBuyingStationDto
            {
                Spots = repFirm.Sum(x => x.Spots),
                Impressions = repFirm.Sum(x => x.Impressions),
                Budget = repFirm.Sum(x => x.Budget),
                Cpm = ProposalMath.CalculateCpm(repFirm.Sum(x => x.Budget), repFirm.Sum(x => x.Impressions)),
                ImpressionsPercentage = repFirm.Sum(x => x.ImpressionsPercentage),
                Affiliate = repFirm.Select(x => x.Affiliate).FirstOrDefault(),
                RepFirm = repFirm.Select(x => x.RepFirm).FirstOrDefault(),
                OwnerName = repFirm.Select(x => x.OwnerName).FirstOrDefault(),
                LegacyCallLetters = repFirm.Select(x => x.LegacyCallLetters).FirstOrDefault(),
                Station = Convert.ToString(repFirm.Select(x => x.Station).Distinct().Count()),
                Market = Convert.ToString(repFirm.Select(x => x.Market).Distinct().Count())
            }).ToList();

            var planBuyingResultrepFirmDetails = details.Select(d => new PlanBuyingResultRepFirmDetailsDto
            {
                Budget = d.Budget,
                Cpm = d.Cpm,
                Impressions = d.Impressions,
                ImpressionsPercentage = d.ImpressionsPercentage,
                MarketCount = Convert.ToInt32(d.Market),
                SpotCount = d.Spots,
                StationCount = Convert.ToInt32(d.Station),
                RepFirmName = d.RepFirm
            }).OrderByDescending(p => p.ImpressionsPercentage).ThenByDescending(p => p.Budget).ToList();

            var totals = new PlanBuyingProgramTotalsDto
            {
                MarketCount = planBuyingResultrepFirmDetails.Sum(x => x.MarketCount),
                Budget = planBuyingResultrepFirmDetails.Sum(x => x.Budget),
                AvgCpm = ProposalMath.CalculateCpm(planBuyingResultrepFirmDetails.Sum(x => x.Budget), planBuyingResultrepFirmDetails.Sum(x => x.Impressions)),
                Impressions = planBuyingResultrepFirmDetails.Sum(x => x.Impressions),
                StationCount = planBuyingResultrepFirmDetails.Sum(x => x.StationCount),
                SpotCount = planBuyingResultrepFirmDetails.Sum(x => x.SpotCount),
                ImpressionsPercentage = (decimal)planBuyingResultrepFirmDetails.Sum(x => x.ImpressionsPercentage)
            };

            var result = new PlanBuyingResultRepFirmDto
            {
                BuyingJobId = planBuyingStationResult.BuyingJobId,
                PlanVersionId = planBuyingStationResult.PlanVersionId ?? default(int),
                Totals = totals,
                Details = planBuyingResultrepFirmDetails,
                SpotAllocationModelMode = planBuyingStationResult.SpotAllocationModelMode,
                PostingType = planBuyingStationResult.PostingType

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
