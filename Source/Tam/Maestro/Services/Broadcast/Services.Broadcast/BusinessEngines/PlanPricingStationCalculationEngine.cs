using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingStationCalculationEngine
    {
        PlanPricingStationResult Calculate(
            List<PlanPricingInventoryProgram> inventories,
            PlanPricingAllocationResult apiResponse,
            PlanPricingParametersDto parametersDto,
            ProprietaryInventoryData proprietaryInventoryData,
            PostingTypeEnum postingType);
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

        public PlanPricingStationResult Calculate(
            List<PlanPricingInventoryProgram> inventories, 
            PlanPricingAllocationResult apiResponse, 
            PlanPricingParametersDto parametersDto,
            ProprietaryInventoryData proprietaryInventoryData,
            PostingTypeEnum postingType)
        {
            var result = new PlanPricingStationResult()
            {
                JobId = parametersDto.JobId,
                PlanVersionId = parametersDto.PlanVersionId,
            };

            result.Stations.AddRange(_GetOpenMarketAggregationByStation(inventories, apiResponse));
            result.Stations.AddRange(_GetProprietaryAggregationByStation(proprietaryInventoryData));

            result.Totals = _GetTotals(result.Stations);

            result.Stations.ForEach(s => s.ImpressionsPercentage = ProposalMath.CalculateImpressionsPercentage(s.Impressions, result.Totals.Impressions));
            result.PostingType = postingType;
            return result;
        }

        private PlanPricingStationTotals _GetTotals(List<PlanPricingStation> stations)
        {
            var result = new PlanPricingStationTotals();

            result.Station = stations.GroupBy(x => x.Station).Count();
            result.Spots = stations.Sum(x => x.Spots);
            result.Impressions = stations.Sum(x => x.Impressions);
            result.ImpressionsPercentage = 100;
            result.Budget = stations.Sum(x => x.Budget);
            result.Cpm = ProposalMath.CalculateCpm(result.Budget, result.Impressions);

            return result;
        }

        private List<PlanPricingStation> _GetProprietaryAggregationByStation(ProprietaryInventoryData proprietaryInventoryData)
        {
            var result = new List<PlanPricingStation>();

            var summaryByStation = proprietaryInventoryData.ProprietarySummaries
                .SelectMany(x => x.ProprietarySummaryByStations)
                .GroupBy(x => x.StationId)
                .ToList();

            var stationIds = summaryByStation.Select(x => x.Key).ToList();
            var stationById = _StationRepository.GetBroadcastStationsByIds(stationIds).ToDictionary(x => x.Id, x => x);
            var marketDisplayByStationId = _GetMarketDisplayByStationId(stationIds, stationById);

            foreach (var grouping in summaryByStation)
            {
                var items = grouping.ToList();

                var pricingStation = new PlanPricingStation
                {
                    Station = stationById[grouping.Key].LegacyCallLetters,
                    Market = marketDisplayByStationId[grouping.Key],
                    Spots = items.Sum(x => x.TotalSpots),
                    Impressions = items.Sum(x => x.TotalImpressions),
                    Budget = items.Sum(x => x.TotalCostWithMargin),
                    IsProprietary = true
                };

                pricingStation.Cpm = ProposalMath.CalculateCpm(pricingStation.Budget, pricingStation.Impressions);

                result.Add(pricingStation);
            }

            return result;
        }

        private List<PlanPricingStation> _GetOpenMarketAggregationByStation(
            List<PlanPricingInventoryProgram> inventories,
            PlanPricingAllocationResult apiResponse)
        {
            var result = new List<PlanPricingStation>();

            var inventoryByManifestId = inventories.ToDictionary(x => x.ManifestId, x => x);
            var allocatedSpotsWithInventory = apiResponse.Spots
                .Select(x => new
                {
                    allocatedSpot = x,
                    inventory = inventoryByManifestId[x.Id]
                })
                .ToList();

            var stationIds = allocatedSpotsWithInventory.Select(x => x.inventory.Station.Id).Distinct().ToList();
            var stationById = _StationRepository.GetBroadcastStationsByIds(stationIds).ToDictionary(x => x.Id, x => x);
            var marketDisplayByStationId = _GetMarketDisplayByStationId(stationIds, stationById);

            foreach (var grouping in allocatedSpotsWithInventory.GroupBy(x => x.inventory.Station.Id))
            {
                var items = grouping.ToList();

                var pricingStation = new PlanPricingStation
                {
                    Station = items.First().inventory.Station.LegacyCallLetters,
                    Market = marketDisplayByStationId[grouping.Key],
                    Spots = items.Sum(x => x.allocatedSpot.TotalSpots),
                    Impressions = items.Sum(x => x.allocatedSpot.TotalImpressions),
                    Budget = items.Sum(x => x.allocatedSpot.TotalCostWithMargin),
                    IsProprietary = false
                };

                pricingStation.Cpm = ProposalMath.CalculateCpm(pricingStation.Budget, pricingStation.Impressions);

                result.Add(pricingStation);
            }

            return result;
        }

        private Dictionary<int, string> _GetMarketDisplayByStationId(
            List<int> stationIds,
            Dictionary<int, DisplayBroadcastStation> stationById)
        {
            var result = new Dictionary<int, string>();
            var marketCodeByStationId = new Dictionary<int, int>();

            var stationMonthDetails = _StationRepository.GetLatestStationMonthDetailsForStations(stationIds);

            foreach (var stationId in stationIds)
            {
                var stationMonthDetail = stationMonthDetails.FirstOrDefault(s => s.StationId == stationId);
                marketCodeByStationId[stationId] = stationMonthDetail?.MarketCode ?? stationById[stationId].MarketCode.Value;
            }

            var marketNameByMarketCode = _MarketRepository
                .GetMarketsByMarketCodes(marketCodeByStationId.Values.ToList())
                .ToDictionary(x => x.market_code, x => x.geography_name);

            foreach (var stationId in stationIds)
            {
                var marketCode = marketCodeByStationId[stationId];
                result[stationId] = marketNameByMarketCode[(short)marketCode];
            }

            return result;
        }
    }
}
