using System.Collections.Generic;
using System.Linq;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.Inventory
{
	public interface IInventoryExportRepository : IDataRepository
	{
		List<QuarterDto> GetInventoryQuartersForSource(int inventorySourceId);
	}

	public class InventoryExportRepository : BroadcastRepositoryBase, IInventoryExportRepository
	{
		public InventoryExportRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
			ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
			: base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
		{
		}


		public List<QuarterDto> GetInventoryQuartersForSource(int inventorySourceId)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var queryData = (from summaryQuarters in context.inventory_summary_quarters
						where summaryQuarters.inventory_source_id == inventorySourceId
						select new QuarterDto
						{
							Quarter = summaryQuarters.quarter_number,
							Year = summaryQuarters.quarter_year
						}).OrderByDescending(q => q.Year).ThenByDescending(q => q.Quarter).ToList();

					return queryData;
				});
		}
	}
}