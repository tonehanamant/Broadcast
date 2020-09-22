﻿using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanBuyingStationEngine
    {
        /// <summary>
        /// Aggregate data by station
        /// </summary>
        /// <param name="inventories">List of PlanBuyingInventoryProgram objects</param>
        /// <param name="apiResponse">PlanBuyingAllocationResult object</param>
        /// <param name="parametersDto">PlanBuyingParametersDto object</param>
        /// <returns>PlanBuyingStationResultDto object</returns>
        PlanBuyingStationResultDto Calculate(
            List<PlanBuyingInventoryProgram> inventories,
            PlanBuyingAllocationResult apiResponse,
            PlanBuyingParametersDto parametersDto);

        /// <summary>
        /// Converts the buying station impressions to user format.
        /// </summary>
        /// <param name="results">The buying ownership group results.</param>
        void ConvertImpressionsToUserFormat(PlanBuyingStationResultDto results);
    }

    public class PlanBuyingStationEngine : IPlanBuyingStationEngine
    {
        private readonly IMarketRepository _MarketRepository;
        private readonly IStationRepository _StationRepository;

        public PlanBuyingStationEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _MarketRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
        }

        public PlanBuyingStationResultDto Calculate(List<PlanBuyingInventoryProgram> inventories, PlanBuyingAllocationResult apiResponse
            , PlanBuyingParametersDto parametersDto)
        {
            var result = new PlanBuyingStationResultDto()
            {
                BuyingJobId = parametersDto.JobId,
                PlanVersionId = parametersDto.PlanVersionId,
            };

            var allocatedStationIds = _GetAllocatedStationIds(apiResponse, inventories);
            var stationMonthDetails = _StationRepository.GetLatestStationMonthDetailsForStations(allocatedStationIds);
            var inventoriesByStation = inventories.Where(i => allocatedStationIds.Contains(i.Station.Id)).GroupBy(i => i.Station.Id);

            double totalImpressions = 0;

            foreach (var inventoryByStation in inventoriesByStation)
            {
                var inventoryItems = inventoryByStation.ToList();
                var allocatedSpots = _GetAllocatedProgramSpots(apiResponse, inventoryItems);

                int totalSpotsPerStation = 0;
                double totalImpressionsPerStation = 0;
                decimal totalBudgetPerStation = 0;

                foreach (var allocatedSpot in allocatedSpots)
                {
                    totalSpotsPerStation += allocatedSpot.TotalSpots;
                    totalImpressionsPerStation += allocatedSpot.TotalImpressions;
                    totalBudgetPerStation += allocatedSpot.TotalCostWithMargin;
                }

                var marketDisplay = _GetMarketDisplay(stationMonthDetails, inventoryByStation);

                var buyingStationDto = new PlanBuyingStationDto
                {
                    Station = inventoryItems.First().Station.LegacyCallLetters,
                    Cpm = ProposalMath.CalculateCpm(totalBudgetPerStation, totalImpressionsPerStation),
                    Budget = totalBudgetPerStation,
                    Impressions = totalImpressionsPerStation,
                    Market = marketDisplay,
                    Spots = totalSpotsPerStation
                };

                totalImpressions += totalImpressionsPerStation;

                result.Details.Add(buyingStationDto);
            }

            result.Details.ForEach(s => s.ImpressionsPercentage = ProposalMath.CalculateImpressionsPercentage(s.Impressions, totalImpressions));

            return result;
        }

        private string _GetMarketDisplay(List<StationMonthDetailDto> stationMonthDetails, IGrouping<int, PlanBuyingInventoryProgram> inventoryByStation)
        {
            var stationMonthDetail = stationMonthDetails.FirstOrDefault(s => s.StationId == inventoryByStation.Key);

            int? marketCode;

            if (stationMonthDetail == null)
            {
                var station = _StationRepository.GetBroadcastStationById(inventoryByStation.Key);
                marketCode = station.MarketCode;
            }
            else
            {
                marketCode = stationMonthDetail.MarketCode;
            }

            var marketDisplay = string.Empty;

            if (marketCode.HasValue)
            {
                var market = _MarketRepository.GetMarket(marketCode.Value);

                marketDisplay = market?.Display;
            }

            return marketDisplay;
        }

        private List<int> _GetAllocatedStationIds(PlanBuyingAllocationResult apiResponse, List<PlanBuyingInventoryProgram> inventories)
        {
            var manifestIds = apiResponse.Spots.Select(s => s.Id).Distinct();
            return inventories
                .Where(i => manifestIds.Contains(i.ManifestId))
                .Select(i => i.Station.Id)
                .Distinct()
                .ToList();
        }

        private List<PlanBuyingAllocatedSpot> _GetAllocatedProgramSpots(PlanBuyingAllocationResult apiResponse, List<PlanBuyingInventoryProgram> inventories)
        {
            var result = new List<PlanBuyingAllocatedSpot>();

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

        public void ConvertImpressionsToUserFormat(PlanBuyingStationResultDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.Details)
            {
                detail.Impressions /= 1000;
            }
        }
    }
}