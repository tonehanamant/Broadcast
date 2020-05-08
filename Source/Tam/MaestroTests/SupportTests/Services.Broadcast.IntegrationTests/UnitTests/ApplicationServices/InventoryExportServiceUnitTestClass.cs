﻿using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Inventory;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class InventoryExportServiceUnitTestClass : InventoryExportService
    {
        public InventoryExportServiceUnitTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IInventoryExportEngine inventoryExportEngine,
            IFileService fileService,
            ISpotLengthEngine spotLengthEngine,
            IDaypartCache daypartCache,
            IMarketService marketService)
        : base(broadcastDataRepositoryFactory, 
            quarterCalculationEngine, 
            mediaMonthAndWeekAggregateCache,
            inventoryExportEngine,
            fileService,
            spotLengthEngine,
            daypartCache,
            marketService)
        {
        }

        protected override string _GetBroadcastSharedFolders()
        {
            return "BroadcastServiceSystemParameter.BroadcastSharedFolder";
        }

        public string UT_GetInventoryFileName(InventorySource source)
        {
            return base._GetInventoryFileName(source);
        }

        public DateTime? UT_DateTimeNow { get; set; }

        protected override DateTime _GetDateTimeNow()
        {
            return UT_DateTimeNow ?? DateTime.Now;
        }
    }
}