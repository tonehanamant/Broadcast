using BroadcastLogging;
using Common.Services.Repositories;
using log4net;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Converters.Scx
{
    public interface IPlanPricingScxDataPrep
    {

        /// <summary>
        /// Gets the SCX data.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="generated">The generated.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="postingType">Type of the posting.</param>
        /// <returns></returns>
        PlanScxData GetScxData(int planId, DateTime generated, SpotAllocationModelMode spotAllocationModelMode,
            PostingTypeEnum postingType);
    }

    public class PlanPricingScxDataPrep : BaseScxDataPrep, IPlanPricingScxDataPrep
    {
        private const ProposalEnums.ProposalPlaybackType PLAYBACK_TYPE = ProposalEnums.ProposalPlaybackType.LivePlus3;

        private readonly ILog _Log;
        private readonly IPlanRepository _PlanRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IStationRepository _StationRepository;

        public PlanPricingScxDataPrep(
            IDataRepositoryFactory broadcastDataDataRepositoryFactory,
            ISpotLengthEngine spotLengthEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache broadcastAudiencesCache) :

            base(broadcastDataDataRepositoryFactory,
                spotLengthEngine,
                mediaMonthAndWeekAggregateCache,
                broadcastAudiencesCache)
        {
            _Log = LogManager.GetLogger(GetType());

            _PlanRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _InventoryRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _StationRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IStationRepository>();
        }

        /// <inheritdoc/>
        public PlanScxData GetScxData(int planId, DateTime generated,
            SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType)
        {
            _GetValidatedPlanAndPricingJob(planId, out var plan, out var job);
            var spots = _GetPricingSpots(job.Id, spotAllocationModelMode, postingType);
            var stationInventoryManifestIds = spots.Select(s => s.StationInventoryManifestId).ToList();
            var inventory = _InventoryRepository.GetStationInventoryManifestsByIds(stationInventoryManifestIds);
            var sortedMediaWeeks = GetSortedMediaWeeks(plan.FlightStartDate.Value, plan.FlightEndDate.Value);
            var audienceIds = _GetAudienceIds(plan);
            var demos = _GetDemos(audienceIds);
            var demoRanksDictionary = demos.ToDictionary(x => x.Demo?.Id, x => x.DemoRank);
            var dmaMarketNames = GetDmaMarketNames(inventory);

            var stationIds = inventory.Select(s => s.Station.Id).Distinct().ToList();
            var stations = _StationRepository.GetBroadcastStationsByIds(stationIds);

            var surveyString = GetSurveyString(plan.ShareBookId, PLAYBACK_TYPE);
            var orders = _GetOrders(plan, inventory, spots, demoRanksDictionary, dmaMarketNames, stations, sortedMediaWeeks, surveyString);

            var scxData = new PlanScxData
            {
                PlanName = plan.Name,
                Generated = generated,
                Demos = demos,
                AllSortedMediaWeeks = sortedMediaWeeks,
                Orders = orders,
                StartDate = plan.FlightStartDate.Value,
                EndDate = plan.FlightEndDate.Value
            };
            CalculateTotals(new List<ScxData> { scxData });

            return scxData;
        }

        internal List<OrderData> _GetOrders(PlanDto plan, List<StationInventoryManifest> inventory, List<PlanPricingAllocatedSpot> spots,
            Dictionary<int?, int> demoRanksDictionary, Dictionary<int, string> dmaMarketNames,
            List<DisplayBroadcastStation> stations, IOrderedEnumerable<MediaWeek> sortedMediaWeeks,
            string surveyString)
        {
            var expandedSpots = spots.Select(s =>
            {
                var spotInventory = inventory.Single(i => i.Id == s.StationInventoryManifestId);
                var expandedSpot = new ExpandedSpot(s, spotInventory);
                return expandedSpot;
            }).ToList();

            var orders = new List<OrderData>();
            var orderMarkets = new List<ScxMarketDto>();
            var demoRank = demoRanksDictionary[plan.AudienceId];

            var marketGroups = expandedSpots.GroupBy(g => g.MarketCode).ToList();
            foreach (var marketGroup in marketGroups)
            {
                var marketStations = new List<ScxMarketDto.ScxStation>();
                var marketStationGroups = marketGroup.GroupBy(m => m.StationId).ToList();

                foreach (var marketStationGroup in marketStationGroups)
                {
                    var daypartGroups = marketStationGroup
                        .GroupBy(x => new { x.InventoryId, DaypartId = x.SpotInventory.ManifestDayparts.Select(d => d.Daypart.Id).FirstOrDefault() })
                        .ToList();

                    var programs = new List<ScxMarketDto.ScxStation.ScxProgram>();
                    foreach (var daypartGroup in daypartGroups)
                    {
                        var daypart = daypartGroup.SelectMany(g => g.SpotInventory.ManifestDayparts.Select(d => d.Daypart)).First();
                        var daypartId = daypart.Id;
                        var programAssignedDaypartCode = daypart.Code;

                        // While inventory manifest dayparts is a list, there are no examples in the db of an inventory having more than one daypart.
                        // So we will grab the first record and go with it.
                        var programName = daypartGroup.SelectMany(g =>
                                g.SpotInventory.ManifestDayparts.Select(d =>
                                    d.Programs.SingleOrDefault(p => p.Id == d.PrimaryProgramId)))
                            .FirstOrDefault()?.ProgramName;

                        var spotLengthsIds = daypartGroup
                            .SelectMany(g => g.Spot.SpotFrequencies.Select(f => f.SpotLengthId))
                            .Distinct()
                            .ToList();

                        foreach (var spotLengthId in spotLengthsIds)
                        {
                            var firstFreq = daypartGroup
                                .SelectMany(g => g.Spot.SpotFrequencies)
                                .FirstOrDefault(f => f.SpotLengthId == spotLengthId);

                            if (firstFreq == null)
                            {
                                continue;
                            }

                            var spotLength = GetSpotLengthString(spotLengthId);
                            var spotCost = firstFreq.SpotCost;
                            var spotImpressions = firstFreq.Impressions;

                            var programWeeks = new List<ScxMarketDto.ScxStation.ScxProgram.ScxWeek>();
                            foreach (var mediaWeek in sortedMediaWeeks)
                            {
                                var weekFreq = daypartGroup.Where(g => g.Spot.ContractMediaWeek.Id == mediaWeek.Id)
                                    .SelectMany(g => g.Spot.SpotFrequencies)
                                    .SingleOrDefault(f => f.SpotLengthId == spotLengthId);

                                var spotFrequency = weekFreq?.Spots ?? 0;

                                var programWeek = new ScxMarketDto.ScxStation.ScxProgram.ScxWeek
                                {
                                    MediaWeek = mediaWeek,
                                    Spots = spotFrequency
                                };
                                programWeeks.Add(programWeek);
                            }

                            var demoValues = new List<ScxMarketDto.ScxStation.ScxProgram.DemoValue>
                            {
                                new ScxMarketDto.ScxStation.ScxProgram.DemoValue
                                {
                                    DemoRank = demoRank,
                                    Impressions = spotImpressions
                                }
                            };

                            var scxProgram = new ScxMarketDto.ScxStation.ScxProgram
                            {
                                ProgramName = programName,
                                DaypartId = daypartId,
                                ProgramAssignedDaypartCode = programAssignedDaypartCode,
                                SpotLength = spotLength,
                                SpotCost = spotCost,
                                Weeks = programWeeks,
                                DemoValues = demoValues
                            };
                            programs.Add(scxProgram);
                        }
                    }

                    var broadcastStation = stations.Single(s => s.Id == marketStationGroup.Key);

                    var station = new ScxMarketDto.ScxStation
                    {
                        StationCode = broadcastStation.Code.Value,
                        LegacyCallLetters = broadcastStation.LegacyCallLetters,
                        Programs = programs
                    };
                    marketStations.Add(station);
                }

                var orderMarket = new ScxMarketDto
                {
                    MarketId = marketGroup.Key,
                    DmaMarketName = dmaMarketNames[marketGroup.Key],
                    Stations = marketStations
                };
                orderMarkets.Add(orderMarket);
            }

            var order = new OrderData
            {
                SurveyString = surveyString,
                InventoryMarkets = orderMarkets
            };
            orders.Add(order);
            return orders;
        }

        internal List<int?> _GetAudienceIds(PlanDto plan)
        {
            // only the primary audience for the plan. 
            // that's what the buying spot impressions are all in terms of.
            var audienceIds = new List<int?> { plan.AudienceId };
            return audienceIds;
        }

        internal void _GetValidatedPlanAndPricingJob(int planId, out PlanDto plan, out PlanPricingJob job)
        {
            plan = null;
            job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);
            if (job == null)
            {
                throw new InvalidOperationException($"A pricing job execution was not found for plan id '{planId}'.");
            }
            if (!job.PlanVersionId.HasValue)
            {
                throw new InvalidOperationException($"The pricing job '{job.Id}' for plan '{planId}' does not have a plan version.");
            }

            var planVersionId = job.PlanVersionId.Value;
            plan = _PlanRepository.GetPlan(planId, planVersionId);

            if (!plan.TargetCPM.HasValue)
            {
                throw new InvalidOperationException($"The plan '{planId}' version id '{planVersionId}' does not have a required target cpm.");
            }
        }

        internal List<PlanPricingAllocatedSpot> _GetPricingSpots(int jobId,
            SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType)
        {
            var jobSpotsResults = _PlanRepository.GetPricingApiResultsByJobId(jobId, spotAllocationModelMode, postingType);

            var spots = jobSpotsResults.Spots;
            return spots;
        }

        internal void _UnEquivalizeSpots(PlanPricingAllocationResult jobSpotsResults)
        {
            if (jobSpotsResults == null) return;

            var jobSpotsFrequencies = jobSpotsResults.Spots
                .SelectMany(x => x.SpotFrequencies)
                .ToArray();

            var deliveryMultipliers = _SpotLengthEngine.GetDeliveryMultipliers();

            for (var i = 0; i < jobSpotsFrequencies.Count(); i++)
            {
                var spot = jobSpotsFrequencies[i];
                var deliveryMultiplier = deliveryMultipliers[spot.SpotLengthId];
                spot.Impressions /= deliveryMultiplier;
            }
        }

        public class ExpandedSpot
        {
            public PlanPricingAllocatedSpot Spot { get; }
            public StationInventoryManifest SpotInventory { get; }
            public int InventoryId { get; }
            public int MarketCode { get; }
            public int StationId { get; }
            public int StandardDaypartId { get; }

            public ExpandedSpot(PlanPricingAllocatedSpot spot, StationInventoryManifest spotInventory)
            {
                Spot = spot;
                SpotInventory = spotInventory;

                InventoryId = SpotInventory.Id.Value;
                MarketCode = SpotInventory.Station.MarketCode.Value;
                StationId = SpotInventory.Station.Id;

                StandardDaypartId = Spot.StandardDaypart.Id;
            }
        }

        #region Logging Methods

        protected virtual void _LogInfo(string message, string username = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
        }

        protected virtual void _LogError(string message, Exception ex = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }

        #endregion // #region Logging Methods
    }
}