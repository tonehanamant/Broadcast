using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation.ScxProgram;

namespace Services.Broadcast.Converters.Scx
{
    public class OAndOScxDataPrep : BaseScxDataPrep, IInventoryScxDataPrep
    {
        public OAndOScxDataPrep(
            IDataRepositoryFactory broadcastDataDataRepositoryFactory,
            ISpotLengthEngine spotLengthEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache broadcastAudiencesCache) :

            base(broadcastDataDataRepositoryFactory,
                 spotLengthEngine,
                 mediaMonthAndWeekAggregateCache,
                 broadcastAudiencesCache)
        {
        }

        public List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            var result = new List<ScxData>();
            var inventory = InventoryRepository.GetInventoryScxDataForOAndO(inventorySourceId, daypartCodeId, startDate, endDate);
            var inventorySource = InventoryRepository.GetInventorySource(inventorySourceId);
            var fileIds = inventory.Select(x => x.InventoryFileId.Value).Distinct();
            var fileHeaders = InventoryRepository.GetInventoryFileHeader(fileIds);
            var audienceIds = fileHeaders.Select(x => x.Value.Audience.Id);
            var dmaMarketNames = GetDmaMarketNames(inventory);

            var manifests = _FilterOutInvalidManifests(inventory);

            if (!manifests.Any())
                return result;
            
            var demos = _GetDemos(audienceIds);
            var demoRanksDictionary = demos.ToDictionary(x => x.Demo.Id, x => x.DemoRank);

            var scxData = new ScxData
            {
                DaypartCode = manifests.First().DaypartCode,
                DaypartCodeId = daypartCodeId,
                UnitName = manifests.First().DaypartCode,
                InventorySource = inventorySource,
                AllSortedMediaWeeks = GetSortedMediaWeeks(startDate, endDate),
                Orders = _GetOrders(manifests, fileHeaders, dmaMarketNames, demoRanksDictionary),
                Demos = demos
            };

            scxData.StartDate = scxData.AllSortedMediaWeeks.First().StartDate;
            scxData.EndDate = scxData.AllSortedMediaWeeks.Last().EndDate;

            result.Add(scxData);

            CalculateTotals(result);

            return result;
        }

        private IEnumerable<StationInventoryManifest> _FilterOutInvalidManifests(IEnumerable<StationInventoryManifest> manifests)
        {
            // skip manifests with unknown stations
            manifests = manifests.Where(x => x.Station.MarketCode.HasValue);

            // skip manifests without spot cost, maybe there is bad data
            manifests = manifests.Where(x => x.ManifestRates.Any(r => r.SpotLengthId == x.SpotLengthId));

            return manifests;
        }

        private List<OrderData> _GetOrders(
            IEnumerable<StationInventoryManifest> manifests,
            Dictionary<int, ProprietaryInventoryHeader> fileHeaders,
            Dictionary<int, string> dmaMarketNames,
            Dictionary<int, int> demoRanksDictionary)
        {
            var result = new List<OrderData>();
            var manifestsByShareBookAndPlaybackType = manifests
                    .Select(x => 
                    {
                        var header = fileHeaders[x.InventoryFileId.Value];

                        return new
                        {
                            Manifest = x,
                            ShareBookId = header.ShareBookId.Value,
                            PlaybackType = header.PlaybackType.Value
                        };
                    })
                    .GroupBy(x => new { x.ShareBookId, x.PlaybackType });

            foreach (var grouping in manifestsByShareBookAndPlaybackType)
            {
                result.Add(new OrderData
                {
                    SurveyString = GetSurveyString(grouping.Key.ShareBookId, grouping.Key.PlaybackType),
                    InventoryMarkets = _GetMarkets(grouping.Select(x => x.Manifest), dmaMarketNames, demoRanksDictionary)
                });
            }

            return result;
        }

        private List<ScxMarketDto> _GetMarkets(
            IEnumerable<StationInventoryManifest> manifests,
            Dictionary<int, string> dmaMarketNames,
            Dictionary<int, int> demoRanksDictionary)
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
                        Programs = _GetPrograms(stationManifests, demoRanksDictionary)
                    }).ToList()
                }).ToList();
        }

        private List<ScxProgram> _GetPrograms(
            IEnumerable<StationInventoryManifest> manifests,
            Dictionary<int, int> demoRanksDictionary)
        {
            var result = new List<ScxProgram>();

            foreach (var manifest in manifests)
            {
                // O&O template contains only 1 daypart and program name for a row
                var manifestDaypart = manifest.ManifestDayparts.Single();

                // O&O template contains only 1 contracted audience with provided impressions
                var manifestAudience = manifest.ManifestAudiencesReferences.Single();

                var scxProgram = new ScxProgram
                {
                    ProgramName = manifestDaypart.ProgramName,
                    DaypartId = manifestDaypart.Daypart.Id,
                    SpotLength = GetSpotLengthString(manifest.SpotLengthId),
                    SpotCost = manifest.ManifestRates.Single(x => x.SpotLengthId == manifest.SpotLengthId).SpotCost,
                    Weeks = manifest.ManifestWeeks.Select(w => new ScxWeek
                    {
                        Spots = w.Spots,
                        MediaWeek = w.MediaWeek
                    }).ToList(),
                    DemoValues = new List<DemoValue>
                    {
                        new DemoValue
                        {
                            DemoRank = demoRanksDictionary[manifestAudience.Audience.Id],
                            Impressions = manifestAudience.Impressions.Value
                        }
                    }
                };

                result.Add(scxProgram);
            }

            return result;
        }
    }
}
