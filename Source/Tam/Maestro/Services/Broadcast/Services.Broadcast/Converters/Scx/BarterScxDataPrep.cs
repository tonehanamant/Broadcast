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
        private readonly ISpotLengthEngine _SpotLengthEngine;

        public BarterScxDataPrep(IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
            ISpotLengthEngine spotLengthEngine, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache) : 
            base(broadcastDataDataRepositoryFactory, spotLengthEngine, mediaMonthAndWeekAggregateCache)
        {
            _InventoryRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _SpotLengthEngine = spotLengthEngine;
        }

        public List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            var inventory = _InventoryRepository.GetInventoryScxDataForBarter(inventorySourceId, daypartCodeId, startDate, endDate, unitNames);
            var result = new List<ScxData>();

            foreach (var groups in inventory.GroupBy(x => new { GroupName = x.Name, InventorySourceName = x.InventorySource.Name }))   //group by unit name and inventory source name
            {
                var manifests = groups.SelectMany(w => w.Manifests);

                if (!manifests.Any())
                    continue;

                var firstGroup = groups.First();
                var daypartCode = firstGroup.Manifests.First().DaypartCode;
                var unitName = groups.Key.GroupName;
                var inventorySource = firstGroup.InventorySource;
                var spotLength = _SpotLengthEngine.GetSpotLengthValueById(firstGroup.Manifests.First().SpotLengthId);
                var scxData = CreateScxData(startDate, endDate, inventorySource, manifests.ToList(), spotLength, daypartCodeId, daypartCode, unitName);

                result.Add(scxData);
            }

            return result;
        }
    }
}
