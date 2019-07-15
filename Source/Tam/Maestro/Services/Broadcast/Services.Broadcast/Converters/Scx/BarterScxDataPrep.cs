using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.Scx
{
    public class BarterScxDataPrep : BaseScxDataPrep, IInventoryScxDataPrep
    {
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IMarketRepository _MarketRepository;
        private readonly ISpotLengthEngine _SpotLengthEngine;

        public BarterScxDataPrep(IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
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
            var inventory = _InventoryRepository.GetInventoryScxDataForBarter(inventorySourceId, daypartCodeId, startDate, endDate, unitNames);
            var markets = _MarketRepository.GetMarketsByMarketCodes(inventory
                                                .SelectMany(x => x.Manifests
                                                .Where(y => y.Station.MarketCode != null)
                                                .Select(y => y.Station.MarketCode.Value)
                                                .Distinct().ToList())
                                    .ToList());
            var result = new List<ScxData>();

            foreach (var group in inventory.GroupBy(x => new { GroupName = x.Name, InventorySourceName = x.InventorySource.Name }))   //group by unit name and inventory source name
            {
                var values = group;
                var items = group.ToList(); //all the manifests for this group and inventory source
                var manifests = items.SelectMany(w => w.Manifests);
                var weeks = manifests.SelectMany(x => x.ManifestWeeks);
                var firstGroup = items.First();
                var daypartCode = firstGroup.Manifests.First().DaypartCode;
                var unitName = group.Key.GroupName;
                var inventorySource = firstGroup.InventorySource;
                var spotLength = _SpotLengthEngine.GetSpotLengthValueById(firstGroup.Manifests.First().SpotLengthId);
                var scxData = CreateScxData(startDate, endDate, inventorySource, markets, manifests.ToList(), weeks, spotLength, daypartCode, unitName);

                result.Add(scxData);
            }

            return result;
        }
    }
}
