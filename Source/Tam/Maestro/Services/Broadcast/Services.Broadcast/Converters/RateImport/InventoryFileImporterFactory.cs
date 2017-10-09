﻿using Common.Services;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IInventoryFileImporterFactory
    {
        InventoryFileImporterBase GetFileImporterInstance(InventoryFile.InventorySourceType inventorySource);
    }

    public class InventoryFileImporterFactory : IInventoryFileImporterFactory
    {
        private BroadcastDataDataRepositoryFactory _broadcastDataDataRepositoryFactory;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        private ICNNStationInventoryGroupService _CNNStationInventoryGroupService;

        public InventoryFileImporterFactory(BroadcastDataDataRepositoryFactory broadcastDataFactory,
                                            IDaypartCache daypartCache, 
                                            MediaMonthAndWeekAggregateCache mediaWeekCache, 
                                            IBroadcastAudiencesCache audiencesCache,
                                            ICNNStationInventoryGroupService CNNStationInventoryGroupService)
        {
            _broadcastDataDataRepositoryFactory = broadcastDataFactory;
            _daypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaWeekCache;
            _AudiencesCache = audiencesCache;
            _CNNStationInventoryGroupService = CNNStationInventoryGroupService;
        }

        public InventoryFileImporterBase GetFileImporterInstance(InventoryFile.InventorySourceType inventorySource)
        {
            InventoryFileImporterBase fileImporter;
            switch (inventorySource)
            {
                case InventoryFile.InventorySourceType.CNN:
                    fileImporter = new CNNFileImporter(_CNNStationInventoryGroupService);
                    break;
                case InventoryFile.InventorySourceType.TTNW:
                    fileImporter = new TTNWExcelFileImporter();
                    break;
                default:
                    fileImporter = new OpenMarketFileImporter();
                    break;
            }

            fileImporter.BroadcastDataDataRepository = _broadcastDataDataRepositoryFactory;
            fileImporter.DaypartCache = _daypartCache;
            fileImporter.MediaMonthAndWeekAggregateCache = _MediaMonthAndWeekAggregateCache;
            fileImporter.AudiencesCache = _AudiencesCache;

            return fileImporter;
        }
    }
}
