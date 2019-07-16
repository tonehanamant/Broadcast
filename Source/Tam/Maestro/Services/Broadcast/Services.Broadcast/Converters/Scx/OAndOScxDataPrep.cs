using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.Scx
{
    public class OAndOScxDataPrep : BaseScxDataPrep, IInventoryScxDataPrep
    {
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IMarketRepository _MarketRepository;
        private readonly ISpotLengthEngine _SpotLengthEngine;

        public OAndOScxDataPrep(IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
            ISpotLengthEngine spotLengthEngine, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache) : 
            base(broadcastDataDataRepositoryFactory, spotLengthEngine, mediaMonthAndWeekAggregateCache)
        {
            _InventoryRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _MarketRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _SpotLengthEngine = spotLengthEngine;
        }

        public List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
            var inventory = _InventoryRepository.GetInventoryScxDataForOAndO(inventorySourceId, daypartCodeId, startDate, endDate);
            var markets = _MarketRepository.GetMarketsByMarketCodes(inventory
                                                .Where(y => y.Station.MarketCode != null)
                                                .Select(y => y.Station.MarketCode.Value)
                                                .Distinct().ToList());
            var result = new List<ScxData>();
            var manifests = inventory.ToList();

            if (!manifests.Any())
                return result;

            var weeks = manifests.SelectMany(x => x.ManifestWeeks);
            var firstManifest = inventory.First();
            var daypartCode = firstManifest.DaypartCode;
            var unitName = firstManifest.DaypartCode;
            var spotLength = _SpotLengthEngine.GetSpotLengthValueById(firstManifest.SpotLengthId);
            var scxData = CreateScxData(startDate, endDate, inventorySource, markets, manifests, weeks, spotLength, daypartCodeId, daypartCode, unitName);

            result.Add(scxData);

            return result;
        }
    }
}
