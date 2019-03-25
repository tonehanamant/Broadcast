using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IBarterFileImporterFactory : IApplicationService
    {
        BarterFileImporterBase GetFileImporterInstance(InventorySource inventorySource);
    }

    public class BarterFileImporterFactory : IBarterFileImporterFactory
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IImpressionsService _ImpressionsService;

        public BarterFileImporterFactory(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            IImpressionsService impressionsService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _InventoryDaypartParsingEngine = inventoryDaypartParsingEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _StationProcessingEngine = stationProcessingEngine;
            _SpotLengthEngine = spotLengthEngine;
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _ImpressionsService = impressionsService;
        }

        public BarterFileImporterBase GetFileImporterInstance(InventorySource inventorySource)
        {
            BarterFileImporterBase fileImporter;

            switch (inventorySource.InventoryType)
            {
                case InventorySourceTypeEnum.ProprietaryOAndO:
                    fileImporter = new OAndOBarterFileImporter(
                        _BroadcastDataRepositoryFactory,
                        _BroadcastAudiencesCache,
                        _InventoryDaypartParsingEngine,
                        _MediaMonthAndWeekAggregateCache,
                        _StationProcessingEngine,
                        _SpotLengthEngine);
                    break;

                case InventorySourceTypeEnum.Barter:
                    fileImporter = new BarterFileImporter(
                        _BroadcastDataRepositoryFactory,
                        _BroadcastAudiencesCache,
                        _InventoryDaypartParsingEngine,
                        _MediaMonthAndWeekAggregateCache,
                        _StationProcessingEngine,
                        _SpotLengthEngine,
                        _ProprietarySpotCostCalculationEngine,
                        _ImpressionsService);
                    break;

                default:
                    throw new NotImplementedException("Unsupported inventory source");
            }

            return fileImporter;
        }
    }
}
