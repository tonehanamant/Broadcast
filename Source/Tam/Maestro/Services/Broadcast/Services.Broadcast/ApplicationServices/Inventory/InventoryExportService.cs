using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories.Inventory;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices.Inventory
{
	public interface IInventoryExportService : IApplicationService
	{
		InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId);
		List<LookupDto> GetOpenMarketExportGenreTypes();
		int GenerateExportForOpenMarket(InventoryExportRequestDto inventoryExportDto, string userName);
		Tuple<string, Stream, string> DownloadOpenMarketExportFile(int fileId);
	}

	public class InventoryExportService : BroadcastBaseClass, IInventoryExportService
	{
		#region Private Members

		private readonly IInventoryExportRepository _InventoryExportRepository;
		private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

		#endregion

		#region Constructor

		public InventoryExportService(IDataRepositoryFactory broadcastDataRepositoryFactory,
			IQuarterCalculationEngine quarterCalculationEngine)
		{
			_InventoryExportRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryExportRepository>();
			_QuarterCalculationEngine = quarterCalculationEngine;
		}

		#endregion

		#region Public Methods

		public List<LookupDto> GetOpenMarketExportGenreTypes()
		{
			return EnumExtensions.ToLookupDtoList<InventoryExportGenreTypeEnum>()
				.OrderBy(i => i.Display).ToList();
		}

		/// <summary>
		///     Get OpenMarket Export Inventory Quarters
		/// </summary>
		/// <param name="inventorySourceId"></param>
		/// <returns></returns>
		public InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId)
		{
			var quarters = _InventoryExportRepository.GetInventoryQuartersForSource(inventorySourceId);
			var quarterDetails = quarters.Select(i => _QuarterCalculationEngine.GetQuarterDetail(i.Quarter, i.Year))
				.ToList();

			return new InventoryQuartersDto
				{Quarters = quarterDetails, DefaultQuarter = quarterDetails.FirstOrDefault()};
		}


		/// <summary>
		///     Generate Export For OpenMarket
		/// </summary>
		/// <param name="inventoryExportDto"></param>
		/// <param name="userName"></param>
		/// <returns></returns>
		public int GenerateExportForOpenMarket(InventoryExportRequestDto inventoryExportDto, string userName)
		{
			return -1;
		}

		/// <summary>
		///     Download Open Market Export File
		/// </summary>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public Tuple<string, Stream, string> DownloadOpenMarketExportFile(int fileId)
		{
			Stream fileStream = new MemoryStream();
			var fileMimeType = MimeMapping.GetMimeMapping("DummyFile.xls");
			var response = new Tuple<string, Stream, string>("DummyFile", fileStream, fileMimeType);
			return response;
		}

		#endregion
	}
}