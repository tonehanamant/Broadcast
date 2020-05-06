using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Inventory;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices.Inventory
{
	[TestFixture]
	public class InventoryExportServiceTests
	{
		private readonly IInventoryExportService _InventoryExportService =
			IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryExportService>();


		private readonly int openMarket_InventorySourceId = 1;

		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void GetOpenMarketExportInventoryQuartersTest()
		{
			using (new TransactionScopeWrapper())
			{
				var quarters =
					_InventoryExportService.GetOpenMarketExportInventoryQuarters(openMarket_InventorySourceId);

				Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
			}
		}

		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void GetOpenMarketExportGenreTypesTest()
		{
			using (new TransactionScopeWrapper())
			{
				var genreTypes =
					_InventoryExportService.GetOpenMarketExportGenreTypes();

				Approvals.Verify(IntegrationTestHelper.ConvertToJson(genreTypes));
			}
		}
	}
}