using BroadcastLogging;
using Common.Services.Repositories;
using log4net;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
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
    public interface IPlanBuyingScxDataPrep
    {
        PlanScxData GetScxData(PlanBuyingScxExportRequest request, DateTime generated,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency,
            PostingTypeEnum postingType = PostingTypeEnum.NSI);
    }

    public class PlanBuyingScxDataPrep : BaseScxDataPrep, IPlanBuyingScxDataPrep
    {
        private const ProposalEnums.ProposalPlaybackType PLAYBACK_TYPE = ProposalEnums.ProposalPlaybackType.LivePlus3;

        private readonly ILog _Log;
        private readonly IPlanBuyingRepository _PlanBuyingRepository;
        private readonly IPlanRepository _PlanRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IStandardDaypartRepository _StandardDaypartRepository;

        public PlanBuyingScxDataPrep(
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

            _PlanBuyingRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
            _PlanRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _InventoryRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _StationRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _StandardDaypartRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
        }

        public PlanScxData GetScxData(PlanBuyingScxExportRequest request, DateTime generated, 
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, 
            PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            _GetValidatedPlanAndJob(request, out var plan, out var job);
            var optimalCPM = _GetOptimalCPM(job.Id, spotAllocationModelMode, postingType);
            var spots = _GetSpots(request.UnallocatedCpmThreshold, optimalCPM, job.Id, spotAllocationModelMode, plan.Equivalized, postingType);
            var inventory = _InventoryRepository.GetPlanBuyingScxInventory(job.Id);            
            var sortedMediaWeeks = GetSortedMediaWeeks(plan.FlightStartDate.Value, plan.FlightEndDate.Value);
            var audienceIds = _GetAudienceIds(plan);
            var demos = _GetDemos(audienceIds);
            var demoRanksDictionary = demos.ToDictionary(x => x.Demo.Id, x => x.DemoRank);
            var dmaMarketNames = GetDmaMarketNames(inventory);
            var standardDaypartDaypartIds = _StandardDaypartRepository.GetStandardDaypartIdDaypartIds();
            var standardDaypartCodes = _StandardDaypartRepository.GetAllStandardDayparts()
                    .ToDictionary(d => d.Id, d => d.Code);

            var stationIds = inventory.Select(s => s.Station.Id).Distinct().ToList();
            var stations = _StationRepository.GetBroadcastStationsByIds(stationIds);

            var surveyString = GetSurveyString(plan.ShareBookId, PLAYBACK_TYPE);            
            var orders = _GetOrders(plan, inventory, spots, demoRanksDictionary, dmaMarketNames, standardDaypartDaypartIds, standardDaypartCodes,
                stations, sortedMediaWeeks, surveyString);
            
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
            CalculateTotals(new List<ScxData>{ scxData });

            return scxData;
        }

        internal List<OrderData> _GetOrders(PlanDto plan, List<StationInventoryManifest> inventory, List<PlanBuyingAllocatedSpot> spots, 
            Dictionary<int,int> demoRanksDictionary, Dictionary<int, string> dmaMarketNames, 
            Dictionary<int,int> standardDaypartDaypartIds, Dictionary<int, string> standardDaypartCodes,
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
                        .GroupBy(g => new {g.InventoryId, g.StandardDaypartId })
                        .ToList();

                    var programs = new List<ScxMarketDto.ScxStation.ScxProgram>();
                    foreach (var daypartGroup in daypartGroups)
                    {
                        var daypartId = standardDaypartDaypartIds[daypartGroup.Key.StandardDaypartId];
                        var programAssignedDaypartCode = standardDaypartCodes[daypartGroup.Key.StandardDaypartId];

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

        internal List<int> _GetAudienceIds(PlanDto plan)
        {
            // only the primary audience for the plan. 
            // that's what the buying spot impressions are all in terms of.
            var audienceIds = new List<int> { plan.AudienceId };
            return audienceIds;
        }

        internal void _GetValidatedPlanAndJob(PlanBuyingScxExportRequest request, out PlanDto plan, out PlanBuyingJob job)
        {
            plan = null;
            job = _PlanBuyingRepository.GetLatestBuyingJob(request.PlanId);
            if (job == null)
            {
                throw new InvalidOperationException($"A buying job execution was not found for plan id '{request.PlanId}'.");
            }
            if (!job.PlanVersionId.HasValue)
            {
                throw new InvalidOperationException($"The buying job '{job.Id}' for plan '{request.PlanId}' does not have a plan version.");
            }

            var planVersionId = job.PlanVersionId.Value;
            plan = _PlanRepository.GetPlan(request.PlanId, planVersionId);

            if (!plan.TargetCPM.HasValue)
            {
                throw new InvalidOperationException($"The plan '{request.PlanId}' version id '{planVersionId}' does not have a required target cpm.");
            }
        }

        internal List<PlanBuyingAllocatedSpot> _GetSpots(int? unallocatedCpmThreshold, decimal planTargetCpm, int jobId, 
            SpotAllocationModelMode spotAllocationModelMode, 
            bool isEquivalized = false, 
            PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            var jobSpotsResults = _PlanBuyingRepository.GetBuyingApiResultsByJobId(jobId, spotAllocationModelMode, postingType);            
            var unfilteredUnallocatedCount = jobSpotsResults.UnallocatedSpots.Count;

            if (isEquivalized)
            {
                _UnEquivalizeSpots(jobSpotsResults);
            }

            var unallocated = unallocatedCpmThreshold.HasValue
                ? _ApplyCpmThreshold(unallocatedCpmThreshold.Value, planTargetCpm, jobSpotsResults.UnallocatedSpots)
                : jobSpotsResults.UnallocatedSpots;

            var filteredUnallocatedCount = unallocated.Count;

            _LogInfo($"Exporting Buying Scx for job id '{jobId}'. UnallocatedCpmThreshold filtered '{unfilteredUnallocatedCount}' records to '{filteredUnallocatedCount}'.");

            var allSpots = jobSpotsResults.AllocatedSpots.Concat(unallocated).ToList();
            return allSpots;
        }

        internal void _UnEquivalizeSpots(PlanBuyingAllocationResult jobSpotsResults)
        {
            if (jobSpotsResults == null) return;

            var jobSpotsFrequencies = jobSpotsResults.AllocatedSpots.Union(jobSpotsResults.UnallocatedSpots)
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

        internal List<PlanBuyingAllocatedSpot> _ApplyCpmThreshold(int cpmThresholdPercent, decimal goalCpm,  List<PlanBuyingAllocatedSpot> spots)
        {
            var results = new List<PlanBuyingAllocatedSpot>();
            var tolerance = goalCpm * (cpmThresholdPercent / 100.0m);
            var maxCpm = goalCpm + tolerance;
            var minCpm = goalCpm - tolerance;
            var deliveryMultipliers = _SpotLengthEngine.GetDeliveryMultipliers();
            var costMultipliers = _SpotLengthEngine.GetCostMultipliers();

            spots.ForEach(s =>
            {
                var keptFrequencies = new List<SpotFrequency>();
                // PlanBuyingAllocatedSpot.Total* methods are sum([metric] * [frequency]);
                // When called with unallocated spots the frequency will be 0.
                // Therefore we cannot use the PlanBuyingAllocatedSpot.Total* methods and must calculate manually.
                s.SpotFrequencies.ForEach(f =>
                {
                    var totalImpressionsPerSpot30Sec = f.Impressions * deliveryMultipliers[f.SpotLengthId];
                    var totalCost = _CalculateSpotCostPer30s(f.SpotCost, costMultipliers[f.SpotLengthId]);
                    var spotCpm = ProposalMath.CalculateCpm(totalCost, totalImpressionsPerSpot30Sec);
                    if (spotCpm >= minCpm && spotCpm <= maxCpm)
                    {
                        keptFrequencies.Add(f);
                    }
                });

                if (keptFrequencies.Any())
                {
                    var keptSpot = new PlanBuyingAllocatedSpot
                    {
                        Id = s.Id,
                        StationInventoryManifestId = s.StationInventoryManifestId,
                        InventoryMediaWeek = s.InventoryMediaWeek,
                        ContractMediaWeek = s.ContractMediaWeek,
                        Impressions30sec = s.Impressions30sec,
                        StandardDaypart = s.StandardDaypart,
                        SpotFrequencies = keptFrequencies
                    };
                    results.Add(keptSpot);
                }
            }
            );
            return results;
        }

        private decimal _GetOptimalCPM(int jobId, SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType)
        {
            decimal optimalCPM = _PlanBuyingRepository.GetOptimalCPM(jobId, spotAllocationModelMode, postingType);
            return optimalCPM;
        }

        private decimal _CalculateSpotCostPer30s(decimal spotCost, decimal spotCostMultiplier)
        {
            var costPer30s = spotCost * spotCostMultiplier;                                  
            return costPer30s;
        }

        public class ExpandedSpot
        {
            public PlanBuyingAllocatedSpot Spot { get; }
            public StationInventoryManifest SpotInventory { get; }
            public int InventoryId { get; }
            public int MarketCode { get; }
            public int StationId { get; }
            public int StandardDaypartId { get; }

            public ExpandedSpot(PlanBuyingAllocatedSpot spot, StationInventoryManifest spotInventory)
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

        #endregion // #region Logging Methods
    }
}