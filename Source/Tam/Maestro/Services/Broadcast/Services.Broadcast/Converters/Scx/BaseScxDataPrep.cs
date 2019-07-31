using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation.ScxProgram;

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

        public BaseScxDataPrep(
            IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
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

        protected ScxData CreateScxData(
            DateTime startDate,
            DateTime endDate,
            InventorySource inventorySource,
            List<StationInventoryManifest> manifests,
            int spotLength,
            int daypartCodeId,
            string daypartCode,
            string unitName)
        {
            var inventoryWeeks = manifests.SelectMany(x => x.ManifestWeeks);

            var scxData = new ScxData
            {
                DaypartCodeId = daypartCodeId,
                DaypartCode = daypartCode,
                StartDate = inventoryWeeks.Min(x => x.StartDate),
                EndDate = inventoryWeeks.Max(x => x.EndDate),
                SpotLength = spotLength.ToString(),
                InventorySource = inventorySource,
                UnitName = unitName,
                AllSortedMediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksIntersecting(startDate, endDate).OrderBy(x => x.StartDate),
                InventoryMarkets = manifests.GroupBy(m => m.Station.MarketCode).Select(marketManifests => new ScxMarketDto
                {
                    MarketId = marketManifests.First().Station.MarketCode,
                    Stations = marketManifests.GroupBy(m => m.Station.LegacyCallLetters).Select(stationManifests => new ScxStation
                    {
                        StationCode = stationManifests.First().Station.Code,
                        LegacyCallLetters = stationManifests.First().Station.LegacyCallLetters,
                        Programs = stationManifests.Select(m => new ScxProgram
                        {
                            ProgramId = m.Id.Value,
                            ProgramNames = m.ManifestDayparts.Select(d => d.ProgramName).ToList(),
                            Dayparts = m.ManifestDayparts.Select(d => new LookupDto
                            {
                                Id = d.Daypart.Id,
                                Display = d.Daypart.ToString()
                            }).ToList(),
                            Weeks = m.ManifestWeeks.Select(w => new ScxWeek
                            {
                                Spots = w.Spots,
                                MediaWeek = w.MediaWeek
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            var inventoryFileId = manifests.Last().InventoryFileId.Value; // Some weird logic that must be reviewed
            _SetScxDataProperties(inventoryFileId, scxData);

            return scxData;
        }

        private void _SetScxDataProperties(int inventoryFileId, ScxData scxData)
        {
            var inventoryHeader = _InventoryRepository.GetInventoryFileHeader(inventoryFileId);
            var marketSubscribers = _NsiUniverseRepository.GetUniverseDataByAudience(inventoryHeader.ShareBookId.Value, new List<int> { inventoryHeader.Audience.Id });
            var marketRankings = _NsiMarketRepository.GetMarketRankingsByMediaMonth(inventoryHeader.ShareBookId.Value);
            var marketCoverages = _MarketCoverageRepository.GetLatestMarketCoverages(scxData.MarketIds).MarketCoveragesByMarketCode;
            
            _SetDmaMarketName(scxData.InventoryMarkets, scxData.MarketIds);
            _SetMarketSurveyData(scxData, inventoryHeader);
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
    }
}
