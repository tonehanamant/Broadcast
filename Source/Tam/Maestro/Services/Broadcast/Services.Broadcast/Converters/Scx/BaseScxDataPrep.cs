using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Entities.ProprietaryInventory;
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
    public abstract class BaseScxDataPrep
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

        public BaseScxDataPrep(IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
            ISpotLengthEngine spotLengthEngine, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
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

        protected ScxData CreateScxData(DateTime startDate,
            DateTime endDate,
            InventorySource inventorySource,
            List<market> markets,
            List<StationInventoryManifest> manifests,
            IEnumerable<StationInventoryManifestWeek> weeks,
            int spotLength,
            string daypartCode,
            string unitName)
        {
            int? inventoryFileId = null;

            var scxData = new ScxData
            {
                DaypartCode = daypartCode,
                StartDate = weeks.Min(x => x.StartDate),
                EndDate = weeks.Max(x => x.EndDate),
                SpotLength = spotLength.ToString(),
                InventorySource = inventorySource,
                UnitName = unitName,
                InventoryMarkets = manifests.GroupBy(y => y.Station.MarketCode).Select(y =>
                {
                    var groupedManifests = y.ToList();
                    var firstManifestMarkets = groupedManifests.First();
                    inventoryFileId = firstManifestMarkets.InventoryFileId;
                    return new ScxMarketDto
                    {
                        MarketId = firstManifestMarkets.Station.MarketCode,
                        MarketName = firstManifestMarkets.Station.MarketCode.HasValue
                                ? markets.Single(z => z.market_code == firstManifestMarkets.Station.MarketCode.Value).geography_name
                                : null,
                        Stations = LoadStations(groupedManifests)
                    };
                }).ToList()
            };

            SetScxDataProperties(startDate, endDate, manifests, inventoryFileId, scxData);

            return scxData;
        }

        protected void SetScxDataProperties(DateTime startDate, DateTime endDate, List<Entities.StationInventory.StationInventoryManifest> manifests, int? inventoryFileId, ScxData scxData)
        {
            scxData.WeekData = SetProgramWeeks(manifests, startDate, endDate);

            var inventoryHeader = _InventoryRepository.GetInventoryFileHeader(inventoryFileId.Value);
            var marketSubscribers = _NsiUniverseRepository.GetUniverseDataByAudience(inventoryHeader.ShareBookId.Value, new List<int> { inventoryHeader.Audience.Id });
            var marketRankings = _NsiMarketRepository.GetMarketRankingsByMediaMonth(inventoryHeader.ShareBookId.Value);
            var marketCoverages = _MarketCoverageRepository.GetLatestMarketCoverages(scxData.MarketIds).MarketCoveragesByMarketCode;

            SetMarketProperties(scxData, marketRankings, marketSubscribers, marketCoverages);
            SetDmaMarketName(scxData.InventoryMarkets, scxData.MarketIds);
            SetMarketSurveyData(scxData, inventoryHeader);
        }

        protected static List<ScxStation> LoadStations(List<StationInventoryManifest> groupedManifests)
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

        protected void SetMarketSurveyData(ScxData data, ProprietaryInventoryHeader inventoryHeader)
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

        protected void SetDmaMarketName(List<ScxMarketDto> inventoryMarkets, List<int> marketIds)
        {
            var dmaMarketNames = _MarketDmaMapRepository.GetMarketMapFromMarketCodes(marketIds)
                                           .ToDictionary(k => (int)k.market_code, v => v.dma_mapped_value);
            foreach (var id in marketIds)
            {
                inventoryMarkets.Single(x => x.MarketId == id).DmaMarketName = dmaMarketNames[id];
            }
        }

        protected List<ScxMarketStationProgramSpotWeek> SetProgramWeeks(List<StationInventoryManifest> manifests, DateTime startDate, DateTime endDate)
        {
            var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksIntersecting(startDate, endDate);
            var weekIndex = 1;
            var weeks = new List<ScxMarketStationProgramSpotWeek>();

            foreach (var mediaWeek in mediaWeeks)
            {
                var weekData = new ScxMarketStationProgramSpotWeek();

                weeks.Add(weekData);
                weekData.MediaWeek = mediaWeek;
                weekData.StartDate = mediaWeek.StartDate;
                weekData.WeekNumber = weekIndex++;

                var weekManifests = manifests.Where(m => m.ManifestWeeks.Select(w => w.MediaWeek.Id).Contains(mediaWeek.Id)).ToList();

                weekData.InventoryWeek =
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

        protected void SetMarketProperties(ScxData InventoryScxData
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

