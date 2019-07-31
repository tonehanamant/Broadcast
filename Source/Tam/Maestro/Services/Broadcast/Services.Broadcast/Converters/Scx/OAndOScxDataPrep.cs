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
        private readonly ISpotLengthEngine _SpotLengthEngine;

        public OAndOScxDataPrep(IDataRepositoryFactory broadcastDataDataRepositoryFactory, 
            ISpotLengthEngine spotLengthEngine, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache) : 
            base(broadcastDataDataRepositoryFactory, spotLengthEngine, mediaMonthAndWeekAggregateCache)
        {
            _InventoryRepository = broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _SpotLengthEngine = spotLengthEngine;
        }

        public List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
            var manifests = _InventoryRepository.GetInventoryScxDataForOAndO(inventorySourceId, daypartCodeId, startDate, endDate);
            var result = new List<ScxData>();

            if (!manifests.Any())
                return result;
            
            var firstManifest = manifests.First();
            var daypartCode = firstManifest.DaypartCode;
            var unitName = firstManifest.DaypartCode;
            var spotLength = _SpotLengthEngine.GetSpotLengthValueById(firstManifest.SpotLengthId);
            var scxData = CreateScxData(startDate, endDate, inventorySource, manifests, spotLength, daypartCodeId, daypartCode, unitName);

            result.Add(scxData);

            return result;
        }
    }
}
