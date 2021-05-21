using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingProgramCalculationEngine
    {
        PlanPricingResultBaseDto CalculateProgramResults(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult apiResponse,
            bool goalsFulfilledByProprietaryInventory,
            ProprietaryInventoryData proprietaryInventoryData,
           PostingTypeEnum postingType);
    }

    public class PlanPricingProgramCalculationEngine : IPlanPricingProgramCalculationEngine
    {
        public PlanPricingResultBaseDto CalculateProgramResults(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult apiResponse,
            bool goalsFulfilledByProprietaryInventory,
            ProprietaryInventoryData proprietaryInventoryData,
            PostingTypeEnum postingType)
        {
            var result = new PlanPricingResultBaseDto();
            var openMarketPrograms = _GetAllocatedPrograms(inventory, apiResponse);
            var openMarketTotalCost = openMarketPrograms.Sum(x => x.TotalCost);
            var openMarketTotalImpressions = openMarketPrograms.Sum(x => x.TotalImpressions);
            var openMarketTotalSpots = openMarketPrograms.Sum(x => x.TotalSpots);

            var openMarketMarketCount = openMarketPrograms.SelectMany(x => x.MarketCodes).Distinct().Count();
            var openMarketStationCount = openMarketPrograms.SelectMany(x => x.Stations).Distinct().Count();

            var projectedImpressions = openMarketPrograms.Sum(x => x.ProjectedImpressions);
            var householdProjectedImpressions = openMarketPrograms.Sum(x => x.HouseholdProjectedImpressions);

            var proprietaryTotalCost = proprietaryInventoryData.ProprietarySummaries.Sum(x => x.TotalCostWithMargin);
            var proprietaryTotalImpressions = proprietaryInventoryData.ProprietarySummaries.Sum(x => x.TotalImpressions);
            var proprietaryTotalSpots = proprietaryInventoryData.ProprietarySummaries.Sum(x => x.TotalSpots);
            var proprietaryMarketCount = proprietaryInventoryData.ProprietarySummaries
                                            .SelectMany(x => x.ProprietarySummaryByStations.Select(y => y.MarketCode)).Distinct().Count();
            var proprietaryStationCount = proprietaryInventoryData.ProprietarySummaries
                                            .SelectMany(x => x.ProprietarySummaryByStations.Select(y => y.StationId)).Distinct().Count();

            var totalCost = openMarketTotalCost + proprietaryTotalCost;
            var totalImpressions = openMarketTotalImpressions + proprietaryTotalImpressions;
            var totalSpots = openMarketTotalSpots + proprietaryTotalSpots;

            result.Programs.AddRange(openMarketPrograms.Select(x => new PlanPricingProgramDto
            {
                ProgramName = x.ProgramName,
                Genre = x.Genre,
                StationCount = x.Stations.Count,
                MarketCount = x.MarketCodes.Count,
                AvgImpressions = x.AvgImpressions,
                Impressions = x.TotalImpressions,
                AvgCpm = x.AvgCpm,
                PercentageOfBuy = ProposalMath.CalculateImpressionsPercentage(x.TotalImpressions, totalImpressions),
                Budget = x.TotalCost,
                Spots = x.TotalSpots,
                IsProprietary = false,

            }));

            result.Programs.AddRange(proprietaryInventoryData.ProprietarySummaries.Select(x => new PlanPricingProgramDto
            {
                ProgramName = x.ProgramName,
                Genre = x.Genre,
                StationCount = x.ProprietarySummaryByStations.Count(),
                MarketCount = x.ProprietarySummaryByStations.Select(y => y.MarketCode).Distinct().Count(),
                AvgImpressions = ProposalMath.CalculateAvgImpressions(x.TotalImpressions, x.TotalSpots),
                Impressions = x.TotalImpressions,
                AvgCpm = x.Cpm,
                PercentageOfBuy = ProposalMath.CalculateImpressionsPercentage(x.TotalImpressions, totalImpressions),
                Budget = x.TotalCostWithMargin,
                Spots = x.TotalSpots,
                IsProprietary = true
            }));

            result.Totals = new PlanPricingProgramTotalsDto
            {
                MarketCount = openMarketMarketCount + proprietaryMarketCount,
                StationCount = openMarketStationCount + proprietaryStationCount,
                AvgImpressions = ProposalMath.CalculateAvgImpressions(totalImpressions, totalSpots),
                AvgCpm = ProposalMath.CalculateCpm(totalCost, totalImpressions),
                Budget = totalCost,
                Impressions = totalImpressions,
                Spots = totalSpots,
                CalculatedVPVH = ProposalMath.CalculateVPVH(projectedImpressions, householdProjectedImpressions)
            };

            result.GoalFulfilledByProprietary = goalsFulfilledByProprietaryInventory;
            result.OptimalCpm = apiResponse.PricingCpm;
            result.JobId = apiResponse.JobId;
            result.PlanVersionId = apiResponse.PlanVersionId;
            result.PostingType = postingType;

            return result;
        }

        private List<PlanPricingProgram> _GetAllocatedPrograms(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult apiResponse)
        {
            var result = new List<PlanPricingProgram>();

            var inventoryGroupedByProgramName = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new PlanPricingManifestWithManifestDaypart
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

                _CalculateProgramTotals(allocatedProgramSpots, out var programCost, out var programImpressions, out var programSpots, out var projectedImpressions, out var householdProjectedImpressions);

                if (programSpots == 0)
                    continue;

                var program = new PlanPricingProgram
                {
                    ProgramName = inventoryByProgramName.Key,
                    Genre = inventoryByProgramName.First().ManifestDaypart.PrimaryProgram.Genre, // we assume all programs with the same name have the same genre
                    AvgImpressions = ProposalMath.CalculateAvgImpressions(programImpressions, programSpots),
                    AvgCpm = ProposalMath.CalculateCpm(programCost, programImpressions),
                    TotalImpressions = programImpressions,
                    TotalCost = programCost,
                    TotalSpots = programSpots,
                    Stations = allocatedStations.Select(s => s.LegacyCallLetters).Distinct().ToList(),
                    MarketCodes = allocatedStations.Select(s => s.MarketCode.Value).Distinct().ToList(),
                    HouseholdProjectedImpressions = householdProjectedImpressions,
                    ProjectedImpressions = projectedImpressions


                };

                result.Add(program);
            };

            return result;
        }

        private List<DisplayBroadcastStation> _GetAllocatedStations(PlanPricingAllocationResult apiResponse, List<PlanPricingManifestWithManifestDaypart> programInventory)
        {
            var manifestIds = apiResponse.Spots.Select(s => s.Id).Distinct();
            var result = new List<PlanPricingManifestWithManifestDaypart>();
            return programInventory.Where(p => manifestIds.Contains(p.Manifest.ManifestId)).Select(p => p.Manifest.Station).ToList();
        }

        private List<PlanPricingAllocatedSpot> _GetAllocatedProgramSpots(PlanPricingAllocationResult apiResponse, List<PlanPricingManifestWithManifestDaypart> programInventory)
        {
            var result = new List<PlanPricingAllocatedSpot>();

            foreach (var spot in apiResponse.Spots)
            {
                // until we use only OpenMarket inventory it's fine
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
            IEnumerable<PlanPricingAllocatedSpot> allocatedProgramSpots,
            out decimal totalProgramCost,
            out double totalProgramImpressions,
            out int totalProgramSpots,
             out double projectedImpressions, out double householdProjectedImpressions)
        {
            totalProgramCost = 0;
            totalProgramImpressions = 0;
            totalProgramSpots = 0;
            projectedImpressions = 0;
            householdProjectedImpressions = 0;
            foreach (var apiProgram in allocatedProgramSpots)
            {
                totalProgramCost += apiProgram.TotalCostWithMargin;
                totalProgramImpressions += apiProgram.TotalImpressions;
                totalProgramSpots += apiProgram.TotalSpots;
                projectedImpressions += apiProgram.ProjectedImpressions;
                householdProjectedImpressions += apiProgram.HouseholdProjectedImpressions;
            }
        }
    }
}
