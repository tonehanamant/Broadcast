using System;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
	[TestFixture]
	public class InventoryProprietarySummaryServiceTests
	{
		private InventoryFileTestHelper _InventoryFileTestHelper;
		private IInventoryProprietarySummaryService _InventoryProprietarySummaryService;

		private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository =
			IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
				.GetDataRepository<IInventoryProprietarySummaryRepository>();

		[SetUp]
		public void Init()
		{
			try
			{
				_InventoryFileTestHelper = new InventoryFileTestHelper();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		[Test]
		public void AggregateInventoryProprietarySummaryDataTest()
		{
			using (new TransactionScopeWrapper())
			{
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
					processInventoryRatings: true);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_2.xlsx",
					processInventoryRatings: false);


				_InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
					.GetApplicationService<IInventoryProprietarySummaryService>();
				_InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, new DateTime(2025, 1, 1),
					new DateTime(2025, 3, 31));
			}
		}
	}
}