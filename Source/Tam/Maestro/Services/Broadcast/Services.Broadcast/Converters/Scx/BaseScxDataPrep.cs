using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.Converters.Scx
{
    public abstract class BaseScxDataPrep
    {
        private readonly IMarketDmaMapRepository _MarketDmaMapRepository;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;

        protected readonly ISpotLengthEngine _SpotLengthEngine;
        protected readonly IInventoryRepository InventoryRepository;

        public BaseScxDataPrep(
            IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
            ISpotLengthEngine spotLengthEngine, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache broadcastAudiencesCache)
        {
            InventoryRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _MarketDmaMapRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IMarketDmaMapRepository>();
            _SpotLengthEngine = spotLengthEngine;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
        }

        protected IOrderedEnumerable<MediaWeek> GetSortedMediaWeeks(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekCache.GetMediaWeeksIntersecting(startDate, endDate).OrderBy(x => x.StartDate);
        }

        protected string GetSpotLengthString(int spotLengthId)
        {
            return _SpotLengthEngine.GetSpotLengthValueById(spotLengthId).ToString();
        }

        protected Dictionary<int, string> GetDmaMarketNames(IEnumerable<StationInventoryManifest> manifests)
        {
            var marketCodes = manifests
                .Where(x => x.Station.MarketCode.HasValue)
                .Select(x => x.Station.MarketCode.Value)
                .Distinct();

            return _MarketDmaMapRepository.GetMarketMapFromMarketCodes(marketCodes)
                                         .ToDictionary(k => (int)k.market_code, v => v.dma_mapped_value);
        }

        protected List<DemoData> _GetDemos(IEnumerable<int?> audienceIds)
        {
            var audiences = _BroadcastAudiencesCache.GetAllEntities().Where(x => audienceIds.Contains(x.Id));

            return audiences.Select((audience, index) => new DemoData
            {
                DemoRank = index + 1,
                Demo = audience
            }).ToList();
        }

        protected string GetSurveyString(int? bookingMediaMonthId, ProposalPlaybackType playbackType)
        {
            var bookingMediaMonth = _MediaMonthAndWeekCache.GetMediaMonthById(bookingMediaMonthId);
            var bookingMediaMonthInfo = bookingMediaMonth.Abbreviation + bookingMediaMonth.Year.ToString().Substring(2);
            return bookingMediaMonthInfo + " DMA Nielsen " + playbackType.ToString().Replace("Plus", "+");
        }
        
        protected void CalculateTotals(List<ScxData> data)
        {
            var orders = data.SelectMany(x => x.Orders);

            foreach (var order in orders)
            {
                foreach (var market in order.InventoryMarkets)
                {
                    foreach (var station in market.Stations)
                    {
                        foreach (var program in station.Programs)
                        {
                            program.TotalSpots = program.Weeks.Sum(x => x.Spots);
                            program.TotalCost = program.SpotCost * program.TotalSpots;
                        }

                        station.TotalSpots = station.Programs.Sum(x => x.TotalSpots);
                        station.TotalCost = station.Programs.Sum(x => x.TotalCost);
                    }

                    market.TotalSpots = market.Stations.Sum(x => x.TotalSpots);
                    market.TotalCost = market.Stations.Sum(x => x.TotalCost);
                }

                order.TotalSpots = order.InventoryMarkets.Sum(x => x.TotalSpots);
                order.TotalCost = order.InventoryMarkets.Sum(x => x.TotalCost);
            }
        }
    }
}
