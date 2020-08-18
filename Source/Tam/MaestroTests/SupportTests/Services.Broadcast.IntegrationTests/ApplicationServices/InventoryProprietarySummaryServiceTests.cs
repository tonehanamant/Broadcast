using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.InventoryProprietary;
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

		//private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository =
		//	IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
		//		.GetDataRepository<IInventoryProprietarySummaryRepository>();
	

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
		public void AggregateInventoryProprietarySummaryDataTest_InvalidRequest()
		{
			using (new TransactionScopeWrapper())
			{
				/*** Arrange ***/
				DateTime StartDate = new DateTime(2025, 1, 1);
				DateTime EndDate = new DateTime(2025, 3, 31);
				List<int> DaypartIds = new List<int> { 59803 };
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
					processInventoryRatings: true);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_2.xlsx",
					processInventoryRatings: false);


				_InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
					.GetApplicationService<IInventoryProprietarySummaryService>();
				_InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, StartDate,
					EndDate);
				List<PlanDaypartRequest> planReqList = new List<PlanDaypartRequest>
				{
					new PlanDaypartRequest{DefaultDayPartId= 15, StartTimeSeconds= 36000, EndTimeSeconds=84600},
					new PlanDaypartRequest{DefaultDayPartId= 16, StartTimeSeconds= 36000, EndTimeSeconds=84600}
				};
				InventoryProprietarySummaryRequest req = new InventoryProprietarySummaryRequest
				{ FlightStartDate = StartDate, FlightEndDate = EndDate, PlanDaypartRequests = planReqList, AudienceId = 5 };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(req);

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
			}
		}

		[Test]
		[Category("long_running")]
		[UseReporter(typeof(DiffReporter))]
		public void AggregateInventoryProprietarySummaryDataTest_ValidRequest()
		{
			using (new TransactionScopeWrapper())
			{
				/*** Arrange ***/
				DateTime StartDate = new DateTime(2025, 1, 1);
				DateTime EndDate = new DateTime(2025, 3, 20);
				List<int> DaypartIds = new List<int> { 59803 };
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
					processInventoryRatings: true);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_2.xlsx",
					processInventoryRatings: false);


				_InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
					.GetApplicationService<IInventoryProprietarySummaryService>();
				_InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, StartDate,
					EndDate);
				List<PlanDaypartRequest> planReqList = new List<PlanDaypartRequest>
				{
					new PlanDaypartRequest{DefaultDayPartId= 1, StartTimeSeconds= 14400, EndTimeSeconds=36000},
					new PlanDaypartRequest{DefaultDayPartId= 4, StartTimeSeconds= 14400, EndTimeSeconds=36000}
				};
				InventoryProprietarySummaryRequest req = new InventoryProprietarySummaryRequest
				{ FlightStartDate = StartDate, FlightEndDate = EndDate, PlanDaypartRequests = planReqList, AudienceId = 5 };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(req);

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
			}
		}
		[Test]
		[Category("long_running")]
		[UseReporter(typeof(DiffReporter))]
		public void AggregateInventoryProprietarySummaryDataTest_Valid_OverWriteExistingForSameQuarter()
		{
			using (new TransactionScopeWrapper())
			{
				/*** Arrange ***/
				DateTime StartDate = new DateTime(2025, 1, 1);
				DateTime EndDate = new DateTime(2025, 3, 20);
				List<int> DaypartIds = new List<int> { 59803 };
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
					processInventoryRatings: true);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_2.xlsx",
					processInventoryRatings: false);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_3.xlsx",
					processInventoryRatings: false);

				_InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
					.GetApplicationService<IInventoryProprietarySummaryService>();
				_InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, StartDate,
					EndDate);
				List<PlanDaypartRequest> planReqList = new List<PlanDaypartRequest>
				{
					new PlanDaypartRequest{DefaultDayPartId= 1, StartTimeSeconds= 14400, EndTimeSeconds=36000},
					new PlanDaypartRequest{DefaultDayPartId= 4, StartTimeSeconds= 14400, EndTimeSeconds=36000}
				};
				InventoryProprietarySummaryRequest req = new InventoryProprietarySummaryRequest
					{ FlightStartDate = StartDate, FlightEndDate = EndDate, PlanDaypartRequests = planReqList, AudienceId = 5 };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(req);

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
			}
		}
		private JsonSerializerSettings _GetJsonSettings()
		{
			var jsonResolver = new IgnorableSerializerContractResolver();
		
			jsonResolver.Ignore(typeof(InventoryProprietarySummary), "Id");
			
			var jsonSettings = new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				ContractResolver = jsonResolver
			};
			return jsonSettings;
		}

	}
}