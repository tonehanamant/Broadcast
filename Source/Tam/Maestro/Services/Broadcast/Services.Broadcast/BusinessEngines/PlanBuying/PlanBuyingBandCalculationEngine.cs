using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanBuyingBandCalculationEngine
    {
        PlanBuyingBandsDto CalculateBuyingBands(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto);
    }

    public class PlanBuyingBandCalculationEngine : IPlanBuyingBandCalculationEngine
    {
        public IPlanBuyingUnitCapImpressionsCalculationEngine _PlanBuyingUnitCapImpressionsCalculationEngine;

        public PlanBuyingBandCalculationEngine(
            IPlanBuyingUnitCapImpressionsCalculationEngine planBuyingUnitCapImpressionsCalculationEngine)
        {
            _PlanBuyingUnitCapImpressionsCalculationEngine = planBuyingUnitCapImpressionsCalculationEngine;
        }

        public PlanBuyingBandsDto CalculateBuyingBands(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult allocationResult,
            PlanBuyingParametersDto parametersDto)
        {
            var buyingBandDto = new PlanBuyingBandsDto
            {
                Bands = _CreateBandsForBuying(parametersDto.AdjustedCPM),
                PlanVersionId = allocationResult.PlanVersionId,
                JobId = allocationResult.JobId
            };

            var allocatedInventory = _GetAllocatedInventory(allocationResult);
            var totalAllocatedCost = allocatedInventory.Sum(x => x.TotalCost);
            var totalAllocatedImpressions = allocatedInventory.Sum(x => x.TotalImpressions);
            var totalAllocatedSpots = allocatedInventory.Sum(x => x.TotalSpots);

            foreach (var band in buyingBandDto.Bands)
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

            buyingBandDto.Totals = new PlanBuyingBandTotalsDto
            {
                Spots = totalAllocatedSpots,
                Impressions = totalAllocatedImpressions,
                Budget = totalAllocatedCost,
                Cpm = ProposalMath.CalculateCpm(totalAllocatedCost, totalAllocatedImpressions),
            };

            return buyingBandDto;
        }

        private List<PlanPricingBandDetailDto> _CreateBands(decimal pricingCpm)
        {
            const decimal minBandMultiplier = 0.1m;
            const decimal maxBandMultiplier = 2;
            // The number of bands between the first and last band that are fixed.
            // Amounts to 9 bands.
            const decimal numberOfIntermediateBands = 7;

            var firstBand = pricingCpm * minBandMultiplier;
            var lastBand = pricingCpm * maxBandMultiplier;
            var bandDifference = lastBand - firstBand;
            var bandIncrement = bandDifference / numberOfIntermediateBands;

            var bands = new List<PlanPricingBandDetailDto>()
            {
                new PlanPricingBandDetailDto
                {
                    MaxBand = firstBand
                }
            };

            decimal? previousBand = firstBand;

            for (var index = 0; index < numberOfIntermediateBands; index++)
            {
                var band = new PlanPricingBandDetailDto
                {
                    MinBand = previousBand,
                    MaxBand = previousBand + bandIncrement
                };

                bands.Add(band);

                previousBand = band.MaxBand;
            }

            bands.Add(new PlanPricingBandDetailDto
            {
                MinBand = lastBand
            });

            return bands;
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

            foreach(var spot in allocationResult.Spots)
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
    }
}
