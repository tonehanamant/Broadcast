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
        PlanBuyingBandInventoryStationsDto CalculateBandInventoryStation(List<PlanBuyingInventoryProgram> inventories,PlanBuyingAllocationResult planBuyingAllocationResult);

        /// <summary>
        /// Calculates the specified band stations inventory.
        /// </summary>
        /// <param name="planBuyingBandStationsInventory">The band stations inventory.</param>
        /// <param name="planBuyingAllocationResult">The allocation result.</param>
        /// <param name="planBuyingParameter">The buying parameters.</param>
        /// <returns>The list of buying bands</returns>
        PlanBuyingBandsDto Calculate(PlanBuyingBandInventoryStationsDto planBuyingBandStationsInventory, PlanBuyingAllocationResult planBuyingAllocationResult, PlanBuyingParametersDto planBuyingParameter);

        /// <summary>
        /// Converts the impressions to user format.
        /// </summary>
        /// <param name="results">The results.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingBandsDto results);
    }

    public class PlanBuyingBandCalculationEngine : IPlanBuyingBandCalculationEngine
    {
        public IPlanBuyingUnitCapImpressionsCalculationEngine _PlanBuyingUnitCapImpressionsCalculationEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;

        public PlanBuyingBandCalculationEngine(
            IPlanBuyingUnitCapImpressionsCalculationEngine planBuyingUnitCapImpressionsCalculationEngine,
            ISpotLengthEngine spotLengthEngine)
        {
            _PlanBuyingUnitCapImpressionsCalculationEngine = planBuyingUnitCapImpressionsCalculationEngine;
            _SpotLengthEngine = spotLengthEngine;
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

        /// <inheritdoc/>
        public PlanBuyingBandsDto Calculate(PlanBuyingBandInventoryStationsDto planBuyingBandStationsInventory, PlanBuyingAllocationResult planBuyingAllocationResult, PlanBuyingParametersDto planBuyingParameter)
        {
            planBuyingBandStationsInventory.Details.ForEach(planBuyingBandStation =>
            {
                planBuyingBandStation.Cpm = ProposalMath.CalculateCpm(planBuyingBandStation.Cost, planBuyingBandStation.Impressions);
            });

            var spotFrequencies = planBuyingAllocationResult.AllocatedSpots.SelectMany(x => x.SpotFrequencies).ToList();
            spotFrequencies.ForEach(spotFrequency =>
            {
                spotFrequency.SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(spotFrequency.SpotCost, planBuyingParameter.Margin);
            });

            var allocatedInventory = _GetAllocatedInventory(planBuyingAllocationResult);
            var totalAllocatedImpressions = allocatedInventory.Sum(x => x.TotalImpressions);

            var planBuyingBandDetails = _CreateBandsForBuying(planBuyingParameter.AdjustedCPM);
            foreach (var band in planBuyingBandDetails)
            {
                var minBand = 0m;

                if (band.MinBand.HasValue)
                    minBand = band.MinBand.Value;

                var maxBand = decimal.MaxValue;

                if (band.MaxBand.HasValue)
                    maxBand = band.MaxBand.Value;

                var allocatedInventoryOfBand = allocatedInventory.Where(x => x.AvgCpm >= minBand && x.AvgCpm < maxBand);
                var planBuyingBandStationsInventoryOfBand = planBuyingBandStationsInventory.Details.Where(x => x.Cpm >= minBand && x.Cpm < maxBand).ToList();

                var totalInventoryImpressions = _PlanBuyingUnitCapImpressionsCalculationEngine.CalculateTotalImpressionsForUnitCaps(planBuyingBandStationsInventoryOfBand, planBuyingParameter);

                band.Spots = allocatedInventoryOfBand.Sum(x => x.TotalSpots);
                band.Impressions = allocatedInventoryOfBand.Sum(x => x.TotalImpressions);
                band.Budget = allocatedInventoryOfBand.Sum(x => x.TotalCost);
                band.Cpm = ProposalMath.CalculateCpm(band.Budget, band.Impressions);
                band.ImpressionsPercentage = totalAllocatedImpressions == 0 ? 0 : (band.Impressions / totalAllocatedImpressions) * 100;
                band.AvailableInventoryPercent = totalInventoryImpressions == 0 ? 0 : (band.Impressions / totalInventoryImpressions) * 100;
            }

            var planBuyingBands = new PlanBuyingBandsDto
            {
                PlanVersionId = planBuyingAllocationResult.PlanVersionId,
                BuyingJobId = planBuyingAllocationResult.JobId,
                PostingType = planBuyingBandStationsInventory.PostingType,
                SpotAllocationModelMode = planBuyingBandStationsInventory.SpotAllocationModelMode,
                Details = planBuyingBandDetails,
                Totals = new PlanBuyingProgramTotalsDto
                {
                    Budget = allocatedInventory.Sum(x => x.TotalCost),
                    Impressions = allocatedInventory.Sum(x => x.TotalImpressions),
                    AvgCpm = ProposalMath.CalculateCpm(allocatedInventory.Sum(x => x.TotalCost), allocatedInventory.Sum(x => x.TotalImpressions)),
                    SpotCount = allocatedInventory.Sum(x => x.TotalSpots)
                }
            };
            return planBuyingBands;
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
        public PlanBuyingBandInventoryStationsDto CalculateBandInventoryStation(List<PlanBuyingInventoryProgram> inventories, PlanBuyingAllocationResult planBuyingAllocationResult)
        {
            /*
             * As we need each band station inventory details while retrieving buying band details, here we calculate each inventory which we retrieves from data science separately as band station. 
             * It may possible that total impressions and total cost of band stations do not match with total impressions and total cost of plan version buying results.
             */
            var planBuyingBandStationDetails = inventories.Select(inventory => new PlanBuyingBandStationDetailDto()
            {
                StationId = inventory.Station.Id,
                Impressions = inventory.Impressions,
                Cost = _GetInventoryCost(inventory),
                ManifestWeeksCount = inventory.ManifestWeeks.Count(),
                PlanBuyingBandInventoryStationDayparts = _GetPlanBuyingBandInventoryStationDayparts(inventory)
            }).ToList();

            var planBuyingBandInventoryStations = new PlanBuyingBandInventoryStationsDto
            {
                PlanVersionId = planBuyingAllocationResult.PlanVersionId,
                BuyingJobId = planBuyingAllocationResult.JobId,
                Details = planBuyingBandStationDetails
            };
            return planBuyingBandInventoryStations;
        }

        private decimal _GetInventoryCost(PlanBuyingInventoryProgram planBuyingInventory)
        {
            decimal cost = 0;

            // for multi-length, we use impressions for :30 and that`s why the cost must be for :30 as well
            var rate = planBuyingInventory.ManifestRates.SingleOrDefault(x => x.SpotLengthId == BroadcastConstants.SpotLengthId30);
            if (rate != null)
            {
                cost = rate.Cost;
            }
            else
            {
                // if there is no rate for :30 in the plan, let`s calculate it
                // the formula that we use during inventory import is 
                // cost for spot length = cost for :30 * multiplier for the spot length
                // so, cost for :30 = cost for spot length / multiplier for the spot length

                rate = planBuyingInventory.ManifestRates.First();
                cost = rate.Cost / _SpotLengthEngine.GetSpotCostMultiplierBySpotLengthId(rate.SpotLengthId);
            }

            return cost;
        }

        private List<PlanBuyingBandInventoryStationDaypartDto> _GetPlanBuyingBandInventoryStationDayparts(PlanBuyingInventoryProgram planBuyingBandStationInventory)
        {
            if (planBuyingBandStationInventory == null)
            {
                return null;
            }

            List<PlanBuyingBandInventoryStationDaypartDto> planBuyingBandInventoryStationDayparts = null;
            if (planBuyingBandStationInventory.ManifestDayparts?.Any() ?? false)
            {
                planBuyingBandInventoryStationDayparts = planBuyingBandStationInventory.ManifestDayparts.Select(manifestDaypart => new PlanBuyingBandInventoryStationDaypartDto()
                {
                    ActiveDays = manifestDaypart.Daypart.ActiveDays,
                    Hours = manifestDaypart.Daypart.Hours
                }).ToList();
            }
            return planBuyingBandInventoryStationDayparts;
        }        
    }
}
