using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using static Services.Broadcast.Entities.OpenMarketInventory.ProposalOpenMarketInventoryWeekDto;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;

namespace Services.Broadcast.Converters.Scx
{
    public interface IInventoryScxDataPrep : IApplicationService
    {
        /// <summary>
        /// Gets the SCX Data for the quarter passed as parameter
        /// </summary>
        /// <param name="quarter">QuarterDetailDto object to filter the data by</param>
        /// <returns>List of ScxData objects containing the data required</returns>
        List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames);
    }

    public class InventoryScxDataPrep : IInventoryScxDataPrep
    {
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IMarketRepository _MarketRepository;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly INsiMarketRepository _NsiMarketRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IBroadcastAudienceRepository _AudienceRepository;
        private readonly IMarketDmaMapRepository _MarketDmaMapRepository;
        private readonly INsiUniverseRepository _NsiUniverseRepository;
        private readonly IRatingForecastRepository _RatingForecastRepository;
        private readonly IMarketCoverageRepository _MarketCoverageRepository;

        public InventoryScxDataPrep(BroadcastDataDataRepositoryFactory broadcastDataDataRepositoryFactory
            , ISpotLengthEngine spotLengthEngine
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _InventoryRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _MarketRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _NsiMarketRepository = broadcastDataDataRepositoryFactory.GetDataRepository<INsiMarketRepository>();
            _AudienceRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _MarketDmaMapRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IMarketDmaMapRepository>();
            _NsiUniverseRepository = broadcastDataDataRepositoryFactory.GetDataRepository<INsiUniverseRepository>();
            _RatingForecastRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            _MarketCoverageRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _SpotLengthEngine = spotLengthEngine;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
        }

        /// <summary>
        /// Gets the SCX Data for the quarter passed as parameter
        /// </summary>
        /// <param name="quarter">QuarterDetailDto object to filter the data by</param>
        /// <returns>List of ScxData objects containing the data required</returns>
        public List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            var inventory = _InventoryRepository.GetInventoryScxData(inventorySourceId, daypartCodeId, startDate, endDate, unitNames);
            var markets = _MarketRepository.GetMarketsByMarketCodes(inventory
                                                .SelectMany(x => x.Manifests
                                                .Where(y => y.Station.MarketCode != null)
                                                .Select(y => y.Station.MarketCode.Value)
                                                .Distinct().ToList())
                                    .ToList());

            List<ScxData> result = new List<ScxData>();
            int? inventoryFileId = null;
            foreach (var group in inventory.GroupBy(x => new { GroupName = x.Name, InventorySourceName = x.InventorySource.Name }))   //group by unit name and inventory source name
            {
                var values = group;
                List<StationInventoryGroup> items = group.ToList(); //all the manifests for this group and inventory source
                var manifests = items.SelectMany(w => w.Manifests);
                var weeks = manifests.SelectMany(x => x.ManifestWeeks);
                var firstGroup = items.First();
                ScxData scxData = new ScxData
                {
                    DaypartCode = firstGroup.Manifests.First().DaypartCode,
                    StartDate = weeks.Min(x => x.StartDate),
                    EndDate = weeks.Max(x => x.EndDate),
                    SpotLength = _SpotLengthEngine.GetSpotLengthValueById(firstGroup.Manifests.First().SpotLengthId).ToString(),
                    InventorySource = firstGroup.InventorySource,
                    UnitName = group.Key.GroupName,
                    InventoryMarkets = manifests.GroupBy(y => y.Station.MarketCode).Select(y =>
                    {
                        var groupedManifests = y.ToList();
                        var firstManifest = groupedManifests.First();
                        inventoryFileId = firstManifest.InventoryFileId;
                        return new ScxMarketDto
                        {
                            MarketId = firstManifest.Station.MarketCode,
                            MarketName = firstManifest.Station.MarketCode.HasValue
                                  ? markets.Single(z => z.market_code == firstManifest.Station.MarketCode.Value).geography_name
                                  : null,
                            Stations = _LoadStations(groupedManifests)
                        };
                    }).ToList()
                };

                scxData.WeekData = _SetProgramWeeks(items, startDate, endDate);

                ProprietaryInventoryHeader inventoryHeader = _InventoryRepository.GetInventoryFileHeader(inventoryFileId.Value);
                var marketSubscribers = _NsiUniverseRepository.GetUniverseDataByAudience(inventoryHeader.ShareBookId.Value, new List<int> { inventoryHeader.Audience.Id });
                var marketRankings = _NsiMarketRepository.GetMarketRankingsByMediaMonth(inventoryHeader.ShareBookId.Value);
                var marketCoverages = _MarketCoverageRepository.GetLatestMarketCoverages(scxData.MarketIds).MarketCoveragesByMarketCode;

                _SetMarketProperties(scxData, marketRankings, marketSubscribers, marketCoverages);
                _SetDmaMarketName(scxData.InventoryMarkets, scxData.MarketIds);
                _SetMarketSurveyData(scxData, inventoryHeader);
                
                result.Add(scxData);
            }
            return result;
        }

        private static List<ScxStation> _LoadStations(List<StationInventoryManifest> groupedManifests)
        {
            return groupedManifests.GroupBy(z => z.Station.LegacyCallLetters).Select(groupedStations =>
            {
                var firstStation = groupedStations.First().Station;
                return new ScxStation
                {
                    Affiliation = firstStation.Affiliation,
                    CallLetters = firstStation.CallLetters,
                    LegacyCallLetters = firstStation.LegacyCallLetters,
                    StationCode = firstStation.Code,
                    Programs = groupedStations.Select(program =>
                    {
                        return new ScxProgram
                        {
                            StationCode = firstStation.Code,
                            ProgramNames = program.ManifestDayparts.Select(w => w.ProgramName).ToList(),
                            Dayparts = program.ManifestDayparts.Select(w => new LookupDto
                            {
                                Display = w.Daypart.ToString(),
                                Id = w.Daypart.Id
                            }).ToList(),
                            ProgramId = program.Id.Value
                        };
                    }).ToList()
                };
            }).ToList();
        }

        private void _SetMarketSurveyData(ScxData data, ProprietaryInventoryHeader inventoryHeader)
        {
            var bookingMediaMonthId = inventoryHeader.HutBookId ?? inventoryHeader.ShareBookId;

            var mediaMonth = _MediaMonthAndWeekCache.GetMediaMonthById(bookingMediaMonthId.Value);
            string mediaMonthInfo = mediaMonth.Abbreviation + mediaMonth.Year.ToString().Substring(2);

            var rawData = _RatingForecastRepository.GetPlaybackForMarketBy(bookingMediaMonthId.Value, inventoryHeader.PlaybackType);
            data.MarketPlaybackTypes = rawData.Where(d => data.MarketIds.Contains(d.market_code)).ToList();
            data.SurveyData = rawData.ToDictionary(
                k => k.MarketId,
                v => mediaMonthInfo + " DMA Nielsen " + v.PlaybackType.ToString().Replace("Plus", "+"));
        }
        
        private void _SetDmaMarketName(List<ScxMarketDto> inventoryMarkets, List<int> marketIds)
        {
            var dmaMarketNames = _MarketDmaMapRepository.GetMarketMapFromMarketCodes(marketIds)
                                           .ToDictionary(k => (int)k.market_code, v => v.dma_mapped_value);
            foreach (var id in marketIds)
            {
                inventoryMarkets.Single(x => x.MarketId == id).DmaMarketName = dmaMarketNames[id];
            }
        }
        
        private List<ScxMarketStationProgramSpotWeek> _SetProgramWeeks(List<StationInventoryGroup> groups, DateTime startDate, DateTime endDate)
        {
            var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksIntersecting(startDate, endDate);

            int weekIndex = 1;
            var weeks = new List<ScxMarketStationProgramSpotWeek>();
            foreach (var mediaWeek in mediaWeeks)
            {
                var weeekData = new ScxMarketStationProgramSpotWeek();
                weeks.Add(weeekData);

                weeekData.MediaWeek = mediaWeek;
                weeekData.StartDate = mediaWeek.StartDate;
                weeekData.WeekNumber = weekIndex++;

                var weekManifests = groups.SelectMany(g => g.Manifests.Where(m => m.ManifestWeeks.Select(w => w.MediaWeek.Id).Contains(mediaWeek.Id))).ToList();
                weeekData.InventoryWeek =
                    new ProposalOpenMarketInventoryWeekDto
                    {
                        Markets = weekManifests.GroupBy(m => m.Station.MarketCode).Select(g => new InventoryWeekMarket
                        {
                            Stations = g.GroupBy(s => s.Station.CallLetters).Select(s =>
                            {
                                return new InventoryWeekStation
                                {
                                    Programs = s.Select(p => new InventoryWeekProgram
                                    {
                                        Spots = p.ManifestWeeks.Where(w => w.MediaWeek.Id == mediaWeek.Id).Select(w => w.Spots).SingleOrDefault(),
                                        ProgramId = p.Id.Value
                                    }).ToList(),
                                    StationCode = s.First().Station.Code
                                };
                            }).ToList(),
                            MarketId = g.Key ?? 0
                        }).ToList()
                    };
            }
            return weeks;
        }

        private void _SetMarketProperties(ScxData InventoryScxData
            , Dictionary<int, int> marketRankings
            , Dictionary<short, double> marketsSubscribers
            , Dictionary<int, double> marketsCoverage)
        {
            InventoryScxData.InventoryMarkets.Where(x => x.MarketId != null).ToList().ForEach(y =>
              {
                  marketRankings.TryGetValue(y.MarketId.Value, out var rank);
                  y.MarketRank = rank;

                  marketsSubscribers.TryGetValue((short)y.MarketId.Value, out double subscribers);
                  y.MarketSubscribers = subscribers;

                  marketsCoverage.TryGetValue(y.MarketId.Value, out var coverage);
                  y.MarketCoverage = coverage;
              });
        }
    }
}
