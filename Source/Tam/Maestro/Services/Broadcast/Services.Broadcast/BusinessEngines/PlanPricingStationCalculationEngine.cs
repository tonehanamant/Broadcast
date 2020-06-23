using Common.Services.Repositories;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingStationCalculationEngine
    {
        PlanPricingStationResultDto Calculate(
            List<PlanPricingInventoryProgram> inventories,
            PlanPricingAllocationResult apiResponse,
            PlanPricingParametersDto parametersDto);
    }

    public class PlanPricingStationCalculationEngine : IPlanPricingStationCalculationEngine
    {
        private readonly IMarketRepository _MarketRepository;
        private readonly IStationRepository _StationRepository;

        public PlanPricingStationCalculationEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _MarketRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
        }

        public PlanPricingStationResultDto Calculate(List<PlanPricingInventoryProgram> inventories, PlanPricingAllocationResult apiResponse, PlanPricingParametersDto parametersDto)
        {
            var result = new PlanPricingStationResultDto()
            {
                JobId = parametersDto.JobId,
                PlanVersionId = parametersDto.PlanVersionId,
            };

            var allocatedStationIds = _GetAllocatedStationIds(apiResponse, inventories);

            var stationMonthDetails = _StationRepository.GetLatestStationMonthDetailsForStations(allocatedStationIds);

            var inventoriesByStation = inventories.Where(i => allocatedStationIds.Contains(i.Station.Id)).GroupBy(i => i.Station.Id);

            int totalSpots = 0;
            double totalImpressions = 0;
            decimal totalBudget = 0;
            foreach (var inventoryByStation in inventoriesByStation)
            {
                var inventoryItems = inventoryByStation.ToList();

                var allocatedSpots = _GetAllocatedProgramSpots(apiResponse, inventoryItems);

                int totalSpotsPerStation = 0;
                double totalImpressionsPerStation = 0;
                decimal totalBudgetPerStation = 0;
                foreach (var allocatedSpot in allocatedSpots)
                {
                    var spots = allocatedSpot.Spots;
                    var spotCost = allocatedSpot.SpotCost;
                    var totalCost = spots * spotCost;
                    var impressionsPerSpot = allocatedSpot.Impressions;
                    var impressions = spots * impressionsPerSpot;

                    totalSpotsPerStation += spots;
                    totalImpressionsPerStation += impressions;
                    totalBudgetPerStation += totalCost;
                }

                var stationMonthDetail = stationMonthDetails.Single(s => s.StationId == inventoryByStation.Key);
                var market = _MarketRepository.GetMarket(stationMonthDetail.MarketCode.GetValueOrDefault());

                var station = new PlanPricingStationDto
                {
                    Station = inventoryItems.First().Station.LegacyCallLetters,
                    Cpm = ProposalMath.CalculateCpm(totalBudgetPerStation, totalImpressionsPerStation),
                    Budget = totalBudgetPerStation,
                    Impressions = totalImpressionsPerStation,
                    Market = market.Display,
                    Spots = totalSpotsPerStation
                };

                totalSpots += totalSpotsPerStation;
                totalImpressions += totalImpressionsPerStation;
                totalBudget += totalBudgetPerStation;

                result.Stations.Add(station);
            }

            result.Totals.Station = result.Stations.Count;
            result.Totals.Spots = totalSpots;
            result.Totals.Impressions = totalImpressions;
            result.Totals.ImpressionsPercentage = 100;
            result.Totals.Cpm = ProposalMath.CalculateCpm(totalBudget, totalImpressions);
            result.Totals.Budget = totalBudget;

            result.Stations.ForEach(s => s.ImpressionsPercentage = ProposalMath.CalculateImpressionsPercentage(s.Impressions, totalImpressions));

            return result;
        }

        private List<int> _GetAllocatedStationIds(PlanPricingAllocationResult apiResponse, List<PlanPricingInventoryProgram> inventories)
        {
            var manifestIds = apiResponse.Spots.Select(s => s.Id).Distinct();
            return inventories
                .Where(i => manifestIds.Contains(i.ManifestId))
                .Select(i => i.Station.Id)
                .Distinct()
                .ToList();
        }

        private List<PlanPricingAllocatedSpot> _GetAllocatedProgramSpots(PlanPricingAllocationResult apiResponse, List<PlanPricingInventoryProgram> inventories)
        {
            var result = new List<PlanPricingAllocatedSpot>();

            foreach (var spot in apiResponse.Spots)
            {
                // until we use only OpenMarket inventory it`s fine
                // this needs to be updated when we start using inventory that can have more than one daypart
                // we should match spots by some unique value which represents a combination of a manifest week and a manifest daypart
                // and not by manifest id as it is done now
                if (inventories.Any(x => x.ManifestId == spot.Id))
                {
                    result.Add(spot);
                }
            }

            return result;
        }
    }
}
