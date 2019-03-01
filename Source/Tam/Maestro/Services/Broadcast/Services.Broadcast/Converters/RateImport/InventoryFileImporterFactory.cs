using System;
using Common.Services;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IInventoryFileImporterFactory
    {
        InventoryFileImporterBase GetFileImporterInstance(InventorySource inventorySource);
    }

    public class InventoryFileImporterFactory : IInventoryFileImporterFactory
    {
        private readonly BroadcastDataDataRepositoryFactory _broadcastDataDataRepositoryFactory;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IInventoryFileValidator _InventoryFileValidator;

        private readonly ICNNStationInventoryGroupService _CNNStationInventoryGroupService;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryDaypartParsingEngine _inventoryDaypartParsingEngine;

        public InventoryFileImporterFactory(BroadcastDataDataRepositoryFactory broadcastDataFactory,
                                            IDaypartCache daypartCache, 
                                            MediaMonthAndWeekAggregateCache mediaWeekCache, 
                                            IBroadcastAudiencesCache audiencesCache,
                                            ICNNStationInventoryGroupService CNNStationInventoryGroupService,
                                            IInventoryFileValidator inventoryFileValidator,
                                            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine)
        {
            _broadcastDataDataRepositoryFactory = broadcastDataFactory;
            _daypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaWeekCache;
            _AudiencesCache = audiencesCache;
            _CNNStationInventoryGroupService = CNNStationInventoryGroupService;
            _InventoryFileValidator = inventoryFileValidator;
            _inventoryRepository = _broadcastDataDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _inventoryDaypartParsingEngine = inventoryDaypartParsingEngine;
        }

        public InventoryFileImporterBase GetFileImporterInstance(InventorySource inventorySource)
        {
            InventoryFileImporterBase fileImporter;
            
            switch (inventorySource.Name)
            {
                case "CNN":
                    fileImporter = new CNNFileImporter(_CNNStationInventoryGroupService,_InventoryFileValidator);
                    break;
                case "TTNW":
                    fileImporter = new TTNWFileImporter();
                    break;
                case "OpenMarket":
                    fileImporter = new OpenMarketFileImporter();
                    break;
                default:
                    throw new NotImplementedException("Unsupported inventory type.");
            }

            fileImporter.BroadcastDataDataRepository = _broadcastDataDataRepositoryFactory;
            fileImporter.DaypartCache = _daypartCache;
            fileImporter.MediaMonthAndWeekAggregateCache = _MediaMonthAndWeekAggregateCache;
            fileImporter.AudiencesCache = _AudiencesCache;
            fileImporter.InventorySource = inventorySource;
            fileImporter.InventoryDaypartParsingEngine = _inventoryDaypartParsingEngine;

            return fileImporter;
        }
    }
}
