using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanBuyingBandCalculationEngine
    {
        /// <summary>
        /// Calculates the specified inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="allocationResult">The allocation result.</param>
        /// <param name="parametersDto">The parameters dto.</param>
        /// <returns>PlanBuyingBandsDto object</returns>
        PlanBuyingBandsDto Calculate(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto);

        /// <summary>
        /// Calculates the specified inventory for band stations.
        /// </summary>
        /// <param name="inventories">The inventories.</param>
        /// <param name="planBuyingAllocationResult">The plan buying allocation result.</param>        
        /// <returns>The PlanBuyingBandStationsDto object</returns>
        PlanBuyingBandStationsDto CalculateBandStation(List<PlanBuyingInventoryProgram> inventories,PlanBuyingAllocationResult planBuyingAllocationResult);

        /// <summary>
        /// Converts the impressions to user format.
        /// </summary>
        /// <param name="results">The results.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingBandsDto results);
    }

    public class PlanBuyingBandCalculationEngine : IPlanBuyingBandCalculationEngine
    {
        public IPlanBuyingUnitCapImpressionsCalculationEngine _PlanBuyingUnitCapImpressionsCalculationEngine;

        public PlanBuyingBandCalculationEngine(
            IPlanBuyingUnitCapImpressionsCalculationEngine planBuyingUnitCapImpressionsCalculationEngine)
        {
            _PlanBuyingUnitCapImpressionsCalculationEngine = planBuyingUnitCapImpressionsCalculationEngine;
        }

        /// <inheritdoc/>
        public PlanBuyingBandsDto Calculate(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto)
        {
            var buyingBandDto = new PlanBuyingBandsDto
            {
                Details = _CreateBandsForBuying(parametersDto.AdjustedCPM),
                PlanVersionId = allocationResult.PlanVersionId,
                BuyingJobId = allocationResult.JobId
            };

            var allocatedInventory = _GetAllocatedInventory(allocationResult);
            var totalAllocatedImpressions = allocatedInventory.Sum(x => x.TotalImpressions);

            foreach (var band in buyingBandDto.Details)
            {
                var minBand = 0m;

                if (band.MinBand.HasValue)
                    minBand = band.MinBand.Value;

                var maxBand = decimal.MaxValue;

                if (band.MaxBand.HasValue)
                    maxBand = band.MaxBand.Value;

                var allocatedBandPrograms = allocatedInventory.Where(x => x.AvgCpm >= minBand && x.AvgCpm < maxBand);
                var inventoryBandPrograms = inventory.Where(x => x.Cpm >= minBand && x.Cpm < maxBand).ToList();
                var totalInventoryImpressions = 
                    _PlanBuyingUnitCapImpressionsCalculationEngine.CalculateTotalImpressionsForUnitCaps(inventoryBandPrograms, parametersDto);
                band.Spots = allocatedBandPrograms.Sum(x => x.TotalSpots);
                band.Impressions = allocatedBandPrograms.Sum(x => x.TotalImpressions);
                band.Budget = allocatedBandPrograms.Sum(x => x.TotalCost);
                band.Cpm = ProposalMath.CalculateCpm(band.Budget, band.Impressions);
                band.ImpressionsPercentage = totalAllocatedImpressions == 0 ?
                    0 : (band.Impressions / totalAllocatedImpressions) * 100;
                band.AvailableInventoryPercent = totalInventoryImpressions == 0 ?
                    0 : (band.Impressions / totalInventoryImpressions)  * 100;
            }

            return buyingBandDto;
        }

        private List<PlanBuyingBandDetailDto> _CreateBandsForBuying(decimal buyingCpm)
        {
            const decimal minBandMultiplier = 0.1m;
            const decimal maxBandMultiplier = 2;
            // The number of bands between the first and last band that are fixed.
            // Amounts to 9 bands.
            const decimal numberOfIntermediateBands = 7;

            var firstBand = buyingCpm * minBandMultiplier;
            var lastBand = buyingCpm * maxBandMultiplier;
            var bandDifference = lastBand - firstBand;
            var bandIncrement = bandDifference / numberOfIntermediateBands;

            var bands = new List<PlanBuyingBandDetailDto>()
            {
                new PlanBuyingBandDetailDto
                {
                    MaxBand = firstBand
                }
            };

            decimal? previousBand = firstBand;

            for (var index = 0; index < numberOfIntermediateBands; index++)
            {
                var band = new PlanBuyingBandDetailDto
                {
                    MinBand = previousBand,
                    MaxBand = previousBand + bandIncrement
                };

                bands.Add(band);

                previousBand = band.MaxBand;
            }

            bands.Add(new PlanBuyingBandDetailDto
            {
                MinBand = lastBand
            });

            return bands;
        }

        private List<PlanBuyingProgram> _GetAllocatedInventory(PlanBuyingAllocationResult allocationResult)
        {
            var result = new List<PlanBuyingProgram>();

            foreach(var spot in allocationResult.AllocatedSpots)
            {
                var pricingProgram = new PlanBuyingProgram
                {
                    AvgImpressions = ProposalMath.CalculateAvgImpressions(spot.TotalImpressions, spot.TotalSpots),
                    AvgCpm = ProposalMath.CalculateCpm(spot.TotalCostWithMargin, spot.TotalImpressions),
                    TotalImpressions = spot.TotalImpressions,
                    TotalCost = spot.TotalCostWithMargin,
                    TotalSpots = spot.TotalSpots
                };

                result.Add(pricingProgram);
            }

            return result;
        }

        /// <inheritdoc/>
        public void ConvertImpressionsToUserFormat(PlanBuyingBandsDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.Details)
            {
                detail.Impressions /= 1000;
            }
        }

        /// <inheritdoc/>
        public PlanBuyingBandStationsDto CalculateBandStation(List<PlanBuyingInventoryProgram> inventories, PlanBuyingAllocationResult planBuyingAllocationResult)
        {
            var manifestIds = planBuyingAllocationResult.AllocatedSpots.Select(s => s.Id).Distinct();
            var groupedInventories = inventories
                .Where(y => manifestIds.Contains(y.ManifestId))
                .GroupBy(x => x.Station.Id);

            var planBuyingBandStationDetails = new List<PlanBuyingBandStationDetailDto>();
            foreach (var groupedInventory in groupedInventories)
            {
                var planBuyingBandStationId = groupedInventory.Key;
                var planBuyingBandStationInventories = groupedInventory.ToList();
                var planBuyingAllocatedSpots = _GetPlanBuyingAllocatedSpots(planBuyingAllocationResult, planBuyingBandStationInventories);

                var planBuyingBandStationDetail = new PlanBuyingBandStationDetailDto()
                {
                    StationId = planBuyingBandStationId,
                    Impressions = planBuyingAllocatedSpots.Sum(x => x.TotalImpressions),
                    Cost = planBuyingAllocatedSpots.Sum(x => x.TotalCostWithMargin),
                    ManifestWeeksCount = planBuyingBandStationInventories.SelectMany(x => x.ManifestWeeks).Count(),
                    PlanBuyingBandStationDayparts = _GetPlanBuyingBandStationDayparts(planBuyingBandStationInventories)
                };
                planBuyingBandStationDetails.Add(planBuyingBandStationDetail);
            }

            var planBuyingBandStations = new PlanBuyingBandStationsDto
            {
                PlanVersionId = planBuyingAllocationResult.PlanVersionId,
                BuyingJobId = planBuyingAllocationResult.JobId,
                Details = planBuyingBandStationDetails
            };
            return planBuyingBandStations;
        }

        private List<PlanBuyingAllocatedSpot> _GetPlanBuyingAllocatedSpots(PlanBuyingAllocationResult planBuyingAllocationResult, List<PlanBuyingInventoryProgram> planBuyingBandStationInventories)
        {
            var planBuyingAllocatedSpots = new List<PlanBuyingAllocatedSpot>();
            foreach (var allocatedSpot in planBuyingAllocationResult.AllocatedSpots)
            {
                if (planBuyingBandStationInventories.Any(x => x.ManifestId == allocatedSpot.Id))
                {
                    planBuyingAllocatedSpots.Add(allocatedSpot);
                }
            }
            return planBuyingAllocatedSpots;
        }

        private List<PlanBuyingBandStationDaypartDto> _GetPlanBuyingBandStationDayparts(List<PlanBuyingInventoryProgram> planBuyingBandStationInventories)
        {
            var planBuyingBandStationDayparts = new List<PlanBuyingBandStationDaypartDto>();
            planBuyingBandStationInventories.ForEach(x =>
            {
                var manifestDayparts = x.ManifestDayparts;
                manifestDayparts.ForEach(manifestDaypart =>
                {
                    var planBuyingBandStationDaypart = new PlanBuyingBandStationDaypartDto()
                    {
                        ActiveDays = manifestDaypart.Daypart.ActiveDays,
                        Hours = manifestDaypart.Daypart.Hours
                    };
                    planBuyingBandStationDayparts.Add(planBuyingBandStationDaypart);
                });
            });
            return planBuyingBandStationDayparts;
        }        
    }
}
