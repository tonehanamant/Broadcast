using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IProprietaryFileImporterFactory : IApplicationService
    {
        ProprietaryFileImporterBase GetFileImporterInstance(InventorySource inventorySource);
    }

    public class ProprietaryFileImporterFactory : IProprietaryFileImporterFactory
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IImpressionsService _ImpressionsService;
        private readonly IDaypartCache _DaypartCache;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IFileService _FileService;

        public ProprietaryFileImporterFactory(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            IImpressionsService impressionsService,
            IDaypartCache daypartCache,
            IImpressionAdjustmentEngine impressionAdjustmentEngine,
            IFileService fileService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _InventoryDaypartParsingEngine = inventoryDaypartParsingEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _StationProcessingEngine = stationProcessingEngine;
            _SpotLengthEngine = spotLengthEngine;
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _ImpressionsService = impressionsService;
            _DaypartCache = daypartCache;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _FileService = fileService;
        }

        public ProprietaryFileImporterBase GetFileImporterInstance(InventorySource inventorySource)
        {
            ProprietaryFileImporterBase fileImporter;

            switch (inventorySource.InventoryType)
            {
                case InventorySourceTypeEnum.ProprietaryOAndO:
                    fileImporter = new OAndOProprietaryFileImporter(
                        _BroadcastDataRepositoryFactory,
                        _BroadcastAudiencesCache,
                        _InventoryDaypartParsingEngine,
                        _MediaMonthAndWeekAggregateCache,
                        _StationProcessingEngine,
                        _SpotLengthEngine,
                        _FileService);
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
                        _ImpressionsService,
                        _FileService);
                    break;

                case InventorySourceTypeEnum.Syndication:
                    fileImporter = new SyndicationFileImporter(
                        _BroadcastDataRepositoryFactory,
                        _BroadcastAudiencesCache,
                        _InventoryDaypartParsingEngine,
                        _MediaMonthAndWeekAggregateCache,
                        _StationProcessingEngine,
                        _SpotLengthEngine,
                        _DaypartCache,
                        _ImpressionAdjustmentEngine,
                        _FileService);
                    break;

                case InventorySourceTypeEnum.Diginet:
                    fileImporter = new DiginetFileImporter(
                        _BroadcastDataRepositoryFactory,
                        _BroadcastAudiencesCache,
                        _InventoryDaypartParsingEngine,
                        _MediaMonthAndWeekAggregateCache,
                        _StationProcessingEngine,
                        _SpotLengthEngine,
                        _DaypartCache,
                        _ImpressionAdjustmentEngine,
                        _FileService);
                    break;

                default:
                    throw new NotImplementedException("Unsupported inventory source");
            }

            return fileImporter;
        }
    }
}
