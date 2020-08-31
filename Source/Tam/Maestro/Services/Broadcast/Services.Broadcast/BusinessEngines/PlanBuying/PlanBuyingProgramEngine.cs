using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines.PlanBuying
{
    public interface IPlanBuyingProgramEngine
    {
        /// <summary>
        /// Calculates the specified inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="apiResponse">The API response.</param>
        /// <param name="goalsFulfilledByProprietaryInventory">True or False</param>
        /// <returns>PlanBuyingResultBaseDto object</returns>
        PlanBuyingResultBaseDto Calculate(List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult apiResponse, bool goalsFulfilledByProprietaryInventory);

        /// <summary>
        /// Converts the impressions to user format.
        /// </summary>
        /// <param name="planBuyingResult">The plan buying result.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingResultProgramsDto planBuyingResult);
    }

    public class PlanBuyingProgramEngine : IPlanBuyingProgramEngine
    {
        /// <inheritdoc/>
        public PlanBuyingResultBaseDto Calculate(
           List<PlanBuyingInventoryProgram> inventory,
           PlanBuyingAllocationResult apiResponse,
           bool goalsFulfilledByProprietaryInventory)
        {
            var result = new PlanBuyingResultBaseDto();
            var programs = _AggregateResults(inventory, apiResponse);
            var totalCostForAllPrograms = programs.Sum(x => x.TotalCost);
            var totalImpressionsForAllPrograms = programs.Sum(x => x.TotalImpressions);
            var totalSpotsForAllPrograms = programs.Sum(x => x.TotalSpots);

            result.Programs.AddRange(programs.Select(x => new PlanBuyingProgramDto
            {
                ProgramName = x.ProgramName,
                Genre = x.Genre,
                StationCount = x.Stations.Count,
                MarketCount = x.MarketCodes.Count,
                AvgImpressions = x.AvgImpressions,
                Impressions = x.TotalImpressions,
                AvgCpm = x.AvgCpm,
                PercentageOfBuy = ProposalMath.CalculateImpressionsPercentage(x.TotalImpressions, totalImpressionsForAllPrograms),
                Budget = x.TotalCost,
                Spots = x.TotalSpots
            }).ToList());

            result.Totals = new PlanBuyingProgramTotalsDto
            {
                MarketCount = programs.SelectMany(x => x.MarketCodes).Distinct().Count(),
                MarketCoveragePercent = 0, //this value will be calculated when aggregating by markets
                StationCount = programs.SelectMany(x => x.Stations).Distinct().Count(),
                AvgImpressions = ProposalMath.CalculateAvgImpressions(totalImpressionsForAllPrograms, totalSpotsForAllPrograms),
                AvgCpm = ProposalMath.CalculateCpm(totalCostForAllPrograms, totalImpressionsForAllPrograms),
                Budget = totalCostForAllPrograms,
                Impressions = totalImpressionsForAllPrograms,
                Spots = totalSpotsForAllPrograms
            };

            result.GoalFulfilledByProprietary = goalsFulfilledByProprietaryInventory;
            result.OptimalCpm = apiResponse.BuyingCpm;
            result.JobId = apiResponse.JobId;
            result.PlanVersionId = apiResponse.PlanVersionId;

            return result;
        }

        private List<PlanBuyingProgram> _AggregateResults(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult apiResponse)
        {
            var result = new List<PlanBuyingProgram>();
            var inventoryGroupedByProgramName = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new PlanBuyingManifestWithManifestDaypart
                {
                    Manifest = x,
                    ManifestDaypart = d
                }))
                .GroupBy(x => x.ManifestDaypart.PrimaryProgram.Name);

            foreach (var inventoryByProgramName in inventoryGroupedByProgramName)
            {
                var programInventory = inventoryByProgramName.ToList();
                var allocatedStations = _GetAllocatedStations(apiResponse, programInventory);
                var allocatedProgramSpots = _GetAllocatedProgramSpots(apiResponse, programInventory);

                _CalculateProgramTotals(allocatedProgramSpots, out var programCost, out var programImpressions, out var programSpots);

                if (programSpots == 0)
                    continue;

                var program = new PlanBuyingProgram
                {
                    ProgramName = inventoryByProgramName.Key,
                    Genre = inventoryByProgramName.First().ManifestDaypart.PrimaryProgram.Genre, // we assume all programs with the same name have the same genre
                    AvgImpressions = ProposalMath.CalculateAvgImpressions(programImpressions, programSpots),
                    AvgCpm = ProposalMath.CalculateCpm(programCost, programImpressions),
                    TotalImpressions = programImpressions,
                    TotalCost = programCost,
                    TotalSpots = programSpots,
                    Stations = allocatedStations.Select(s => s.LegacyCallLetters).Distinct().ToList(),
                    MarketCodes = allocatedStations.Select(s => s.MarketCode.Value).Distinct().ToList()
                };

                result.Add(program);
            };

            return result;
        }

        private List<DisplayBroadcastStation> _GetAllocatedStations(PlanBuyingAllocationResult apiResponse
            , List<PlanBuyingManifestWithManifestDaypart> programInventory)
        {
            var manifestIds = apiResponse.Spots.Select(s => s.Id).Distinct();
            var result = new List<PlanBuyingManifestWithManifestDaypart>();
            return programInventory.Where(p => manifestIds.Contains(p.Manifest.ManifestId)).Select(p => p.Manifest.Station).ToList();
        }

        private List<PlanBuyingAllocatedSpot> _GetAllocatedProgramSpots(PlanBuyingAllocationResult apiResponse
            , List<PlanBuyingManifestWithManifestDaypart> programInventory)
        {
            var result = new List<PlanBuyingAllocatedSpot>();

            foreach (var spot in apiResponse.Spots)
            {
                // until we use only OpenMarket inventory it`s fine
                // this needs to be updated when we start using inventory that can have more than one daypart
                // we should match spots by some unique value which represents a combination of a manifest week and a manifest daypart
                // and not by manifest id as it is done now
                if (programInventory.Any(x => x.Manifest.ManifestId == spot.Id))
                {
                    result.Add(spot);
                }
            }

            return result;
        }

        private void _CalculateProgramTotals(
            IEnumerable<PlanBuyingAllocatedSpot> allocatedProgramSpots,
            out decimal totalProgramCost,
            out double totalProgramImpressions,
            out int totalProgramSpots)
        {
            totalProgramCost = 0;
            totalProgramImpressions = 0;
            totalProgramSpots = 0;

            foreach (var apiProgram in allocatedProgramSpots)
            {
                totalProgramCost += apiProgram.TotalCostWithMargin;
                totalProgramImpressions += apiProgram.TotalImpressions;
                totalProgramSpots += apiProgram.TotalSpots;
            }
        }

        /// <inheritdoc/>
        public void ConvertImpressionsToUserFormat(PlanBuyingResultProgramsDto planBuyingResult)
        {
            if (planBuyingResult == null)
                return;

            planBuyingResult.Totals.AvgImpressions /= 1000;
            planBuyingResult.Totals.Impressions /= 1000;

            foreach (var program in planBuyingResult.Details)
            {
                program.AvgImpressions /= 1000;
                program.Impressions /= 1000;
            }
        }
    }
}
