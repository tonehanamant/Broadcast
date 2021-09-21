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
        /// Calculates the specified inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="apiResponse">The API response.</param>
        /// <param name="goalsFulfilledByProprietaryInventory">True or False</param>
        /// <returns>PlanBuyingResultBaseDto object</returns>
        PlanBuyingResultBaseDto CalculateProgramStations(List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult apiResponse, bool goalsFulfilledByProprietaryInventory);

        /// <summary>
        /// Converts the impressions to user format.
        /// </summary>
        /// <param name="planBuyingResult">The plan buying result.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingResultProgramsDto planBuyingResult);

        /// <summary>
        /// Aggregate program data based on station level.
        /// </summary>
        /// <param name="planBuyingResultPrograms">The plan buying result.</param>
        /// <returns>Return aggregated result of program</returns>
        PlanBuyingResultProgramsDto GetAggregatedProgramStations(PlanBuyingResultProgramsDto planBuyingResultPrograms);
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
                SpotCount = x.TotalSpots
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
                SpotCount = totalSpotsForAllPrograms
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
            var manifestIds = apiResponse.AllocatedSpots.Select(s => s.Id).Distinct();
            var result = new List<PlanBuyingManifestWithManifestDaypart>();
            return programInventory.Where(p => manifestIds.Contains(p.Manifest.ManifestId)).Select(p => p.Manifest.Station).ToList();
        }

        private List<PlanBuyingAllocatedSpot> _GetAllocatedProgramSpots(PlanBuyingAllocationResult apiResponse
            , List<PlanBuyingManifestWithManifestDaypart> programInventory)
        {
            var result = new List<PlanBuyingAllocatedSpot>();

            foreach (var spot in apiResponse.AllocatedSpots)
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

        /// <inheritdoc/>
        public PlanBuyingResultBaseDto CalculateProgramStations(
           List<PlanBuyingInventoryProgram> inventory,
           PlanBuyingAllocationResult apiResponse,
           bool goalsFulfilledByProprietaryInventory)
        {
            var programs = _AggregateProgramStations(inventory, apiResponse);
            var planBuyingResult = new PlanBuyingResultBaseDto
            {
                GoalFulfilledByProprietary = goalsFulfilledByProprietaryInventory,
                OptimalCpm = apiResponse.BuyingCpm,
                JobId = apiResponse.JobId,
                PlanVersionId = apiResponse.PlanVersionId,
                Programs = programs.Select(x => new PlanBuyingProgramDto
                {
                    ProgramName = x.ProgramName,
                    Genre = x.Genre,
                    Station = x.Station,
                    Impressions = x.TotalImpressions,
                    SpotCount = x.TotalSpots,
                    Budget = x.TotalCost
                }).ToList()
            };
            return planBuyingResult;
        }

        private List<PlanBuyingProgram> _AggregateProgramStations(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult apiResponse)
        {
            var planBuyingPrograms = new List<PlanBuyingProgram>();

            var manifestIds = apiResponse.AllocatedSpots.Select(s => s.Id).Distinct();
            var groupedInventories = inventory
                .Where(y => manifestIds.Contains(y.ManifestId))
                .SelectMany(x => x.ManifestDayparts.Select(d => new PlanBuyingManifestWithManifestDaypart
                {
                    Manifest = x,
                    ManifestDaypart = d
                }))
                .GroupBy(x => new
                {
                    ProgramName = x.ManifestDaypart.PrimaryProgram.Name,
                    Genre = x.ManifestDaypart.PrimaryProgram.Genre,
                    StationName = x.Manifest.Station.LegacyCallLetters
                });

            foreach (var groupedInventory in groupedInventories)
            {
                var programInventory = groupedInventory.ToList();
                var allocatedProgramSpots = _GetAllocatedProgramSpots(apiResponse, programInventory);
                _CalculateProgramTotals(allocatedProgramSpots, out var programCost, out var programImpressions, out var programSpots);
                if (programSpots == 0)
                    continue;

                var planBuyingProgram = new PlanBuyingProgram
                {
                    ProgramName = groupedInventory.Key.ProgramName,
                    Genre = groupedInventory.Key.Genre,
                    Station = groupedInventory.Key.StationName,
                    TotalImpressions = programImpressions,
                    TotalCost = programCost,
                    TotalSpots = programSpots
                };
                planBuyingPrograms.Add(planBuyingProgram);
            };
            return planBuyingPrograms;
        }

        public PlanBuyingResultProgramsDto GetAggregatedProgramStations(PlanBuyingResultProgramsDto planBuyingResultPrograms)
        {
            List<PlanBuyingProgramProgramDto> planBuyingProgramStationDetails = new List<PlanBuyingProgramProgramDto>();
            var groupOfProgram = planBuyingResultPrograms.Details.GroupBy(u => u.ProgramName, StringComparer.InvariantCultureIgnoreCase).Select(grp => grp.ToList()).ToList();
            double totalImpressionsForAllPrograms = groupOfProgram.SelectMany(y => y).Sum(x => x.Impressions);
            planBuyingProgramStationDetails = groupOfProgram.Select(program => new PlanBuyingProgramProgramDto
            {
                ProgramName = program.Select(x => x.ProgramName).First(),
                Genre = program.Select(x => x.Genre).First(),
                RepFirm = program.Select(x => x.RepFirm).First(),
                OwnerName = program.Select(x => x.OwnerName).First(),
                LegacyCallLetters = program.Select(x => x.LegacyCallLetters).First(),
                MarketCode = program.Select(x => x.MarketCode).First(),
                Station = program.Select(x => x.Station).First(),
                Impressions = program.Sum(x => x.Impressions),
                ImpressionsPercentage = ProposalMath.CalculateImpressionsPercentage(program.Sum(x => x.Impressions), totalImpressionsForAllPrograms),
                Budget = program.Sum(x => x.Budget),
                Spots = program.Sum(x => x.Spots),
                StationCount = program.Select(s => s.Station).Distinct().Count(),
                MarketCount = program.Select(s => s.MarketCode).Distinct().Count()
            }).ToList();
            var totals = new PlanBuyingProgramTotalsDto
            {
                MarketCount = planBuyingProgramStationDetails.Select(x => x.MarketCode).Count(),
                StationCount = planBuyingProgramStationDetails.Sum(x => x.StationCount),
                AvgCpm = ProposalMath.CalculateCpm(planBuyingProgramStationDetails.Sum(x => x.Budget), planBuyingProgramStationDetails.Sum(x => x.Impressions)),
                AvgImpressions = ProposalMath.CalculateAvgImpressions(totalImpressionsForAllPrograms, planBuyingProgramStationDetails.Sum(x => x.Spots)),               
                Budget = planBuyingProgramStationDetails.Sum(x => x.Budget),
                SpotCount = planBuyingProgramStationDetails.Sum(x => x.Spots),
                Impressions = planBuyingProgramStationDetails.Sum(x => x.Impressions)                
            };
            var result = new PlanBuyingResultProgramsDto
            {
                PostingType = planBuyingResultPrograms.PostingType,
                SpotAllocationModelMode = planBuyingResultPrograms.SpotAllocationModelMode,
                Totals = planBuyingResultPrograms.Totals,
                Details = planBuyingProgramStationDetails
            };
            return result;
        }
    }
}
