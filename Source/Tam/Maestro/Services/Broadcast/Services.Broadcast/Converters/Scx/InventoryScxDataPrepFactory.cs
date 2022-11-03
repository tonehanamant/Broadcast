using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Converters.Scx
{
    public interface IInventoryScxDataPrepFactory : IApplicationService
    {
        IInventoryScxDataPrep GetInventoryDataPrep(InventorySourceTypeEnum inventorySourceType);
        IOpenMarketInventoryScxDataPrep GetOpenMarketInventoryDataPrep(InventorySourceTypeEnum inventorySourceType);
    }

    public class InventoryScxDataPrepFactory : IInventoryScxDataPrepFactory
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;

        public InventoryScxDataPrepFactory(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISpotLengthEngine spotLengthEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekCache,
            IBroadcastAudiencesCache broadcastAudiencesCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _SpotLengthEngine = spotLengthEngine;
            _MediaMonthAndWeekCache = mediaMonthAndWeekCache;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
        }

        public IInventoryScxDataPrep GetInventoryDataPrep(InventorySourceTypeEnum inventorySourceType)
        {
            if (inventorySourceType == InventorySourceTypeEnum.ProprietaryOAndO)
            {
                return new OAndOScxDataPrep(_BroadcastDataRepositoryFactory, _SpotLengthEngine, _MediaMonthAndWeekCache, _BroadcastAudiencesCache);
            }
            else
            {
                return new BarterScxDataPrep(_BroadcastDataRepositoryFactory, _SpotLengthEngine, _MediaMonthAndWeekCache, _BroadcastAudiencesCache);
            }
        }

        public IOpenMarketInventoryScxDataPrep GetOpenMarketInventoryDataPrep(InventorySourceTypeEnum inventorySourceType)
        {
                return new BarterScxDataPrep(_BroadcastDataRepositoryFactory, _SpotLengthEngine, _MediaMonthAndWeekCache, _BroadcastAudiencesCache);
        }
    }
}
