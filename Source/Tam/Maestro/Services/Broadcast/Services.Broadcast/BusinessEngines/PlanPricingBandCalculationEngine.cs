using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingBandCalculationEngine
    {
        PlanPricingBand CalculatePricingBands(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult allocationResult,
            PlanPricingParametersDto parametersDto,
            ProprietaryInventoryData proprietaryInventoryData,
            PostingTypeEnum postingType);
    }

    public class PlanPricingBandCalculationEngine : IPlanPricingBandCalculationEngine
    {
        public IPlanPricingUnitCapImpressionsCalculationEngine _PlanPricingUnitCapImpressionsCalculationEngine;

        public PlanPricingBandCalculationEngine(
            IPlanPricingUnitCapImpressionsCalculationEngine planPricingUnitCapImpressionsCalculationEngine)
        {
            _PlanPricingUnitCapImpressionsCalculationEngine = planPricingUnitCapImpressionsCalculationEngine;
        }

        public PlanPricingBand CalculatePricingBands(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult allocationResult,
            PlanPricingParametersDto parametersDto,
            ProprietaryInventoryData proprietaryInventoryData,
            PostingTypeEnum postingType)
        {
            var allocatedInventory = _GetAllocatedInventory(allocationResult);
            var pricingBand = new PlanPricingBand
            {
                PlanVersionId = allocationResult.PlanVersionId,
                JobId = allocationResult.JobId,
                Totals = _GetPricingBandTotals(allocatedInventory, proprietaryInventoryData),
                PostingType = postingType
            };

            var allBands = _CreateBands(parametersDto.AdjustedCPM);

            foreach (var band in allBands)
            {
                var minBand = band.MinBand ?? 0m;
                var maxBand = band.MaxBand ?? decimal.MaxValue;
                var proprietarySummaries = proprietaryInventoryData.ProprietarySummaries.Where(x => x.Cpm >= minBand && x.Cpm < maxBand).ToList();

                var openMarketBand = _GetOpenMarketBand(minBand, maxBand, band.MinBand, band.MaxBand, allocatedInventory);
                var proprietaryBand = _GetProprietaryBand(band.MinBand, band.MaxBand, proprietarySummaries);
                var bands = new List<PlanPricingBandDetail> { openMarketBand, proprietaryBand };

                _CalculateAllocatedInventoryPercentage(bands, pricingBand.Totals.Impressions);
                _CalculateAvailableInventoryPercentage(
                    minBand,
                    maxBand,
                    bands,
                    inventory,
                    parametersDto,
                    proprietarySummaries);

                pricingBand.Bands.AddRange(bands);
            }

            return pricingBand;
        }

        private void _CalculateAllocatedInventoryPercentage(
            List<PlanPricingBandDetail> bands,
            double totalAllocatedImpressions)
        {
            foreach (var band in bands)
            {
                band.ImpressionsPercentage = totalAllocatedImpressions == 0 ? 0 : (band.Impressions / totalAllocatedImpressions) * 100;
            }
        }

        private void _CalculateAvailableInventoryPercentage(
            decimal minBand,
            decimal maxBand,
            List<PlanPricingBandDetail> bands,
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingParametersDto parametersDto,
            List<ProprietarySummary> proprietarySummaries)
        {
            var inventoryBandPrograms = inventory.Where(x => x.Cpm >= minBand && x.Cpm < maxBand).ToList();
            var totalOpenMarketInventoryImpressions = _PlanPricingUnitCapImpressionsCalculationEngine.CalculateTotalImpressionsForUnitCaps(inventoryBandPrograms, parametersDto);
            var totalProprietaryInventoryImpressions = proprietarySummaries.Sum(x => x.TotalImpressions);
            var totalInventoryImpressions = totalOpenMarketInventoryImpressions + totalProprietaryInventoryImpressions;

            foreach (var band in bands)
            {
                band.AvailableInventoryPercent = totalInventoryImpressions == 0 ? 0 : (band.Impressions / totalInventoryImpressions) * 100;
            }
        }

        private PlanPricingBandDetail _GetProprietaryBand(
            decimal? originalMinBand,
            decimal? originalMaxBand,
            List<ProprietarySummary> proprietarySummaries)
        {
            var impressions = proprietarySummaries.Sum(x => x.TotalImpressions);
            var budget = proprietarySummaries.Sum(x => x.TotalCostWithMargin);

            return new PlanPricingBandDetail
            {
                IsProprietary = true,
                MinBand = originalMinBand,
                MaxBand = originalMaxBand,
                Spots = proprietarySummaries.Sum(x => x.TotalSpots),
                Impressions = impressions,
                Budget = budget,
                Cpm = ProposalMath.CalculateCpm(budget, impressions)
            };
        }

        private PlanPricingBandDetail _GetOpenMarketBand(
            decimal minBand,
            decimal maxBand,
            decimal? originalMinBand,
            decimal? originalMaxBand,
            List<PlanPricingProgram> allocatedInventory)
        {
            var allocatedBandPrograms = allocatedInventory.Where(x => x.AvgCpm >= minBand && x.AvgCpm < maxBand);
            var impressions = allocatedBandPrograms.Sum(x => x.TotalImpressions);
            var budget = allocatedBandPrograms.Sum(x => x.TotalCost);

            return new PlanPricingBandDetail
            {
                IsProprietary = false,
                MinBand = originalMinBand,
                MaxBand = originalMaxBand,
                Spots = allocatedBandPrograms.Sum(x => x.TotalSpots),
                Impressions = impressions,
                Budget = budget,
                Cpm = ProposalMath.CalculateCpm(budget, impressions)
            };
        }

        private PlanPricingBandTotals _GetPricingBandTotals(
            List<PlanPricingProgram> allocatedInventory,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            var totalAllocatedCost = allocatedInventory.Sum(x => x.TotalCost) + proprietaryInventoryData.TotalCostWithMargin;
            var totalAllocatedImpressions = allocatedInventory.Sum(x => x.TotalImpressions) + proprietaryInventoryData.TotalImpressions;

            return new PlanPricingBandTotals
            {
                Spots = allocatedInventory.Sum(x => x.TotalSpots) + proprietaryInventoryData.TotalSpots,
                Impressions = totalAllocatedImpressions,
                Budget = totalAllocatedCost,
                Cpm = ProposalMath.CalculateCpm(totalAllocatedCost, totalAllocatedImpressions),
            };
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

        private List<PlanPricingProgram> _GetAllocatedInventory(PlanPricingAllocationResult allocationResult)
        {
            var result = new List<PlanPricingProgram>();

            foreach (var spot in allocationResult.Spots)
            {
                var pricingProgram = new PlanPricingProgram
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
