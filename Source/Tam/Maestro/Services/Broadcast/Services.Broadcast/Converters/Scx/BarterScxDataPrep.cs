using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation.ScxProgram;

namespace Services.Broadcast.Converters.Scx
{
    public class BarterScxDataPrep : BaseScxDataPrep, IInventoryScxDataPrep
    {
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;

        public BarterScxDataPrep(
            IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
            ISpotLengthEngine spotLengthEngine, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache broadcastAudiencesCache) : 

            base(broadcastDataDataRepositoryFactory, 
                 spotLengthEngine, 
                 mediaMonthAndWeekAggregateCache,
                 broadcastAudiencesCache)
        {
            _BroadcastAudienceRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
        }

        public List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            var result = new List<ScxData>();
            var inventory = InventoryRepository.GetInventoryScxDataForBarter(inventorySourceId, daypartCodeId, startDate, endDate, unitNames);
            var inventorySource = InventoryRepository.GetInventorySource(inventorySourceId);
            var fileIds = inventory.SelectMany(x => x.Manifests).Select(x => x.InventoryFileId.Value).Distinct();
            var fileHeaders = InventoryRepository.GetInventoryFileHeader(fileIds);
            var allManifests = inventory.SelectMany(x => x.Manifests);
            var dmaMarketNames = GetDmaMarketNames(allManifests);
            var audienceIds = fileHeaders.Select(x => x.Value.Audience.Id);
            var audienceComponents = _BroadcastAudienceRepository.GetRatingAudiencesGroupedByMaestroAudience(audienceIds);

            foreach (var groups in inventory.GroupBy(x => new { GroupName = x.Name, InventorySourceName = x.InventorySource.Name }))
            {
                var manifests = _FilterOutInvalidManifests(groups);

                if (!manifests.Any())
                    continue;
                
                var demos = _GetDemos(audienceIds);
                var demoRanksDictionary = demos.ToDictionary(x => x.Demo.Id, x => x.DemoRank);

                var scxData = new ScxData
                {
                    DaypartCode = manifests.First().DaypartCode,
                    DaypartCodeId = daypartCodeId,
                    UnitName = groups.Key.GroupName,
                    InventorySource = inventorySource,
                    AllSortedMediaWeeks = GetSortedMediaWeeks(startDate, endDate),
                    Orders = _GetOrders(manifests, inventorySource, fileHeaders, dmaMarketNames, demoRanksDictionary, audienceComponents),
                    Demos = demos
                };

                scxData.StartDate = scxData.AllSortedMediaWeeks.First().StartDate;
                scxData.EndDate = scxData.AllSortedMediaWeeks.Last().EndDate;

                result.Add(scxData);
            }

            CalculateTotals(result);

            return result;
        }

        private List<OrderData> _GetOrders(
            IEnumerable<StationInventoryManifest> manifests,
            InventorySource inventorySource,
            Dictionary<int, ProprietaryInventoryHeader> fileHeaders,
            Dictionary<int, string> dmaMarketNames,
            Dictionary<int, int> demoRanksDictionary,
            Dictionary<int, List<int>> audienceComponents)
        {
            var result = new List<OrderData>();
            var manifestsByShareBookAndPlaybackType = manifests
                    .Select(x => new
                    {
                        Manifest = x,
                        ShareBookId = fileHeaders[x.InventoryFileId.Value].ShareBookId.Value,

                        // PlaybackType is the same for all audiences of a manifest
                        PlaybackType = x.ManifestAudiences.First().SharePlaybackType.Value
                    })
                    .GroupBy(x => new { x.ShareBookId, x.PlaybackType });

            foreach (var grouping in manifestsByShareBookAndPlaybackType)
            {
                result.Add(new OrderData
                {
                    SurveyString = GetSurveyString(grouping.Key.ShareBookId, grouping.Key.PlaybackType),
                    InventoryMarkets = _GetMarkets(
                        grouping.Select(x => x.Manifest), 
                        inventorySource, 
                        fileHeaders, 
                        dmaMarketNames,
                        demoRanksDictionary,
                        audienceComponents)
                });
            }

            return result;
        }

        private IEnumerable<StationInventoryManifest> _FilterOutInvalidManifests(IEnumerable<StationInventoryGroup> groups)
        {
            // if manifest does not have impressions with playback type set, 
            // the projection had been done before the story that deals with share book was implemented
            // skip such manifests
            var manifests = groups.SelectMany(w => w.Manifests).Where(x => x.ManifestAudiences.Any(a => a.SharePlaybackType.HasValue));

            // skip manifests with unknown stations
            manifests = manifests.Where(x => x.Station.MarketCode.HasValue);

            // skip manifests without spot cost, maybe there is bad data
            manifests = manifests.Where(x => x.ManifestRates.Any(r => r.SpotLengthId == x.SpotLengthId));

            return manifests;
        }

        private List<ScxMarketDto> _GetMarkets(
            IEnumerable<StationInventoryManifest> manifests, 
            InventorySource inventorySource,
            Dictionary<int, ProprietaryInventoryHeader> fileHeaders,
            Dictionary<int, string> dmaMarketNames,
            Dictionary<int, int> demoRanksDictionary,
            Dictionary<int, List<int>> audienceComponents)
        {
            return manifests
                .GroupBy(x => x.Station.MarketCode.Value)
                .Select(marketManifests => new ScxMarketDto
                {
                    MarketId = marketManifests.Key,
                    DmaMarketName = dmaMarketNames[marketManifests.Key],
                    Stations = marketManifests.GroupBy(m => m.Station.Code.Value).Select(stationManifests => new ScxStation
                    {
                        StationCode = stationManifests.Key,
                        LegacyCallLetters = stationManifests.First().Station.LegacyCallLetters,
                        Programs = _GetPrograms(stationManifests, inventorySource, fileHeaders, demoRanksDictionary, audienceComponents)
                    }).ToList()
                }).ToList();
        }

        private List<ScxProgram> _GetPrograms(
            IEnumerable<StationInventoryManifest> manifests,
            InventorySource inventorySource,
            Dictionary<int, ProprietaryInventoryHeader> fileHeaders,
            Dictionary<int, int> demoRanksDictionary,
            Dictionary<int, List<int>> audienceComponents)
        {
            var result = new List<ScxProgram>();

            foreach (var manifest in manifests)
            {
                var scxProgram = new ScxProgram
                {
                    // default program name since Barter template does not contain program names
                    ProgramName = $"{inventorySource.Name} Block",

                    // if a template row contains only 1 daypart we use daypart from the row
                    // otherwise we use contracted daypart from the file header
                    DaypartId = manifest.ManifestDayparts.Count == 1 ?
                        manifest.ManifestDayparts.First().Daypart.Id :
                        fileHeaders[manifest.InventoryFileId.Value].ContractedDaypartId.Value,

                    SpotLength = GetSpotLengthString(manifest.SpotLengthId),
                    SpotCost = manifest.ManifestRates.Single(x => x.SpotLengthId == manifest.SpotLengthId).SpotCost,

                    Weeks = manifest.ManifestWeeks.Select(w => new ScxWeek
                    {
                        Spots = w.Spots,
                        MediaWeek = w.MediaWeek
                    }).ToList()
                };

                var header = fileHeaders[manifest.InventoryFileId.Value];
                var audienceIdFromHeader = header.Audience.Id;
                var audienceFromHeader = manifest.ManifestAudiences.SingleOrDefault(x => x.Audience.Id == audienceIdFromHeader);
                double impressions;

                // if the specified audience is not a component audience 
                // then we have to combine it from its component audiences
                if (audienceFromHeader == null)
                {
                    var componentIds = audienceComponents[audienceIdFromHeader];
                    impressions = manifest.ManifestAudiences
                        .Where(x => componentIds.Contains(x.Audience.Id) && x.Impressions.HasValue)
                        .Sum(x => x.Impressions.Value);
                }
                else
                {
                    impressions = audienceFromHeader.Impressions ?? 0;
                }

                scxProgram.DemoValues = new List<DemoValue>
                {
                    new DemoValue
                    {
                        DemoRank = demoRanksDictionary[audienceIdFromHeader],
                        Impressions = impressions
                    }
                };

                result.Add(scxProgram);
            }

            return result;
        }
    }
}
