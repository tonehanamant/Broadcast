using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
	[TestFixture]
	public class InventoryProprietarySummaryRepositoryTests
	{
		private InventoryFileTestHelper _InventoryFileTestHelper;

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
		[Category("long_running")]
		[UseReporter(typeof(DiffReporter))]
		public void GetInventoryProprietarySummaryTest()
		{
			using (new TransactionScopeWrapper())
			{
				/*** Arrange ***/
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx", 
                    processInventoryRatings: true);

                var src = new InventorySource
                {
                    Id = 5,
                    Name = "CNN",
                    InventoryType = InventorySourceTypeEnum.Barter,
                    IsActive = true
                };

				var inventoryProperietarySummaryRepo = IntegrationTestApplicationServiceFactory
					.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryProprietarySummaryRepository>();

				/*** Act ***/
				var quarterSummary =
					inventoryProperietarySummaryRepo.GetInventoryProprietaryQuarterSummaries(
                        src, 
                        new DateTime(2025, 1, 1),
						new DateTime(2025, 3, 31));

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarterSummary, _GetJsonSettings()));
			}
		}

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryProprietarySummaryTestOneWeek()
        {
            using (new TransactionScopeWrapper())
            {
                /*** Arrange ***/
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_OneWeek.xlsx",
                    processInventoryRatings: true);

                var src = new InventorySource
                {
                    Id = 5,
                    Name = "CNN",
                    InventoryType = InventorySourceTypeEnum.Barter,
                    IsActive = true
                };

                var inventoryProperietarySummaryRepo = IntegrationTestApplicationServiceFactory
                    .BroadcastDataRepositoryFactory.GetDataRepository<IInventoryProprietarySummaryRepository>();

                /*** Act ***/
                var quarterSummary =
                    inventoryProperietarySummaryRepo.GetInventoryProprietaryQuarterSummaries(src,
                        new DateTime(2018, 1, 1),
                        new DateTime(2018, 1, 7));

                /*** Assert ***/
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarterSummary, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryProprietarySummaryTestTwoWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                /*** Arrange ***/
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_TwoWeeks.xlsx",
                    processInventoryRatings: true);

                var src = new InventorySource
                {
                    Id = 5,
                    Name = "CNN",
                    InventoryType = InventorySourceTypeEnum.Barter,
                    IsActive = true
                };

                var inventoryProperietarySummaryRepo = IntegrationTestApplicationServiceFactory
                    .BroadcastDataRepositoryFactory.GetDataRepository<IInventoryProprietarySummaryRepository>();

                /*** Act ***/
                var quarterSummary =
                    inventoryProperietarySummaryRepo.GetInventoryProprietaryQuarterSummaries(src,
                        new DateTime(2018, 1, 1),
                        new DateTime(2018, 1, 14));

                /*** Assert ***/
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarterSummary, _GetJsonSettings()));
            }
        }
		
		private JsonSerializerSettings _GetJsonSettings()
		{
			var jsonResolver = new IgnorableSerializerContractResolver();
			jsonResolver.Ignore(typeof(List<InventoryProprietarySummaryAudiencesDto>), "Audiences");

			var jsonSettings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				ContractResolver = jsonResolver
			};
			return jsonSettings;
		}
	}
}