using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Inventory;
using Services.Broadcast.BusinessEngines;
using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums.Inventory;
using Tam.Maestro.Data.Entities.DataTransferObjects;

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
            IMarketService marketService,
            INsiPostingBookService nsiPostingBookService)
        : base(broadcastDataRepositoryFactory, 
            quarterCalculationEngine, 
            mediaMonthAndWeekAggregateCache,
            inventoryExportEngine,
            fileService,
            spotLengthEngine,
            daypartCache,
            marketService,
            nsiPostingBookService)
        {
        }

		protected override string _GetBroadcastAppFolder()
		{
			return "BroadcastServiceSystemParameter.BroadcastAppFolder";
		}
		
        public DateTime? UT_DateTimeNow { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_DateTimeNow ?? DateTime.Now;
        }

        public List<int> UT_GetExportGenreIds(InventoryExportGenreTypeEnum genreType, List<LookupDto> genres)
        {
            return _GetExportGenreIds(genreType, genres);
        }
    }
}