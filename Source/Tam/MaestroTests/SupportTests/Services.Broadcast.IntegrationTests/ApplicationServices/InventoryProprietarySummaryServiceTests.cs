using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.IntegrationTests.Helpers;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
	public class InventoryProprietarySummaryServiceTests
	{
		private InventoryFileTestHelper _InventoryFileTestHelper;
		private IInventoryProprietarySummaryService _InventoryProprietarySummaryService;

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
				var startDate = new DateTime(2025, 1, 1);
				var endDate = new DateTime(2025, 3, 31);
				var daypartIds = new List<int> { 59803 };

                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
					processInventoryRatings: true);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_2.xlsx",
					processInventoryRatings: false);

                _InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
					.GetApplicationService<IInventoryProprietarySummaryService>();

                _InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, startDate,
					endDate);

                var planDaypartRequests = new List<PlanDaypartRequest>
				{
					new PlanDaypartRequest
                    {
                        DefaultDayPartId = 15,
                        StartTimeSeconds = 36000,
                        EndTimeSeconds = 84600
                    },
					new PlanDaypartRequest
                    {
                        DefaultDayPartId = 16,
                        StartTimeSeconds = 36000,
                        EndTimeSeconds = 84600
                    }
				};

				var request = new InventoryProprietarySummaryRequest
				{
                    FlightStartDate = startDate,
                    FlightEndDate = endDate,
                    PlanDaypartRequests = planDaypartRequests,
                    AudienceId = 5
                };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

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
				var startDate = new DateTime(2025, 1, 1);
				var EndDate = new DateTime(2025, 3, 20);
				var daypartIds = new List<int> { 59803 };

                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
					processInventoryRatings: true);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_2.xlsx",
					processInventoryRatings: false);

				_InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
					.GetApplicationService<IInventoryProprietarySummaryService>();

				_InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, startDate,
					EndDate);

				var planDaypartRequest = new List<PlanDaypartRequest>
				{
					new PlanDaypartRequest
                    {
                        DefaultDayPartId = 1,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    },
					new PlanDaypartRequest
                    {
                        DefaultDayPartId = 4,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    }
				};

				var req = new InventoryProprietarySummaryRequest
                {
                    FlightStartDate = startDate,
                    FlightEndDate = EndDate,
                    PlanDaypartRequests = planDaypartRequest,
                    AudienceId = 34
                };

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
				var startDate = new DateTime(2025, 1, 1);
				var endDate = new DateTime(2025, 3, 20);
				var daypartIds = new List<int> { 59803 };

				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
					processInventoryRatings: true);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_2.xlsx",
					processInventoryRatings: false);
				_InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_3.xlsx",
					processInventoryRatings: false);

				_InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
					.GetApplicationService<IInventoryProprietarySummaryService>();

				_InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, startDate, endDate);

				var planDaypartRequests = new List<PlanDaypartRequest>
				{
					new PlanDaypartRequest
                    {
                        DefaultDayPartId = 1,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    },
					new PlanDaypartRequest
                    {
                        DefaultDayPartId = 4,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    }
				};

				var request = new InventoryProprietarySummaryRequest
                {
                    FlightStartDate = startDate,
                    FlightEndDate = endDate,
                    PlanDaypartRequests = planDaypartRequests,
                    AudienceId = 5
                };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
			}
		}

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateInventoryProprietarySummaryDataTest_ValidWithUnitCost()
        {
            using (new TransactionScopeWrapper())
            {
                /*** Arrange ***/
                var startDate = new DateTime(2018, 1, 1);
                var endDate = new DateTime(2018, 3, 20);
                var daypartIds = new List<int> { 6724 };

                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN.xlsx",
                    processInventoryRatings: true);                

                _InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IInventoryProprietarySummaryService>();

                _InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, startDate, endDate);

                var planDaypartRequests = new List<PlanDaypartRequest>
                {
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 1,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    },
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 4,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    }
                };

                var request = new InventoryProprietarySummaryRequest
                {
                    FlightStartDate = startDate,
                    FlightEndDate = endDate,
                    PlanDaypartRequests = planDaypartRequests,
                    AudienceId = 5
                };

                /*** Act ***/
                var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

                /*** Assert ***/
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateInventoryProprietarySummaryDataTest_ValidWithUnitCostMultipleSpots()
        {
            using (new TransactionScopeWrapper())
            {
                /*** Arrange ***/
                var startDate = new DateTime(2018, 1, 1);
                var endDate = new DateTime(2018, 3, 20);
                var daypartIds = new List<int> { 6724 };

                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_2.xlsx",
                    processInventoryRatings: true);

                _InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IInventoryProprietarySummaryService>();

                _InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, startDate, endDate);

                var planDaypartRequests = new List<PlanDaypartRequest>
                {
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 1,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    },
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 4,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    }
                };

                var request = new InventoryProprietarySummaryRequest
                {
                    FlightStartDate = startDate,
                    FlightEndDate = endDate,
                    PlanDaypartRequests = planDaypartRequests,
                    AudienceId = 5
                };

                /*** Act ***/
                var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

                /*** Assert ***/
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateInventoryProprietarySummaryDataTest_SameProgramName()
        {
            using (new TransactionScopeWrapper())
            {
                using (new TransactionScopeWrapper())
                {
                    /*** Arrange ***/
                    var startDate = new DateTime(2025, 1, 1);
                    var endDate = new DateTime(2025, 3, 20);
                    var daypartIds = new List<int> { 59803 };

                    _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025.xlsx",
                        processInventoryRatings: true);
                    _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN_Q1_2025_4.xlsx",
                        processInventoryRatings: true);

                    _InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory
                        .GetApplicationService<IInventoryProprietarySummaryService>();

                    _InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(5, startDate, endDate);

                    var planDaypartRequests = new List<PlanDaypartRequest>
                {
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 1,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    },
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 2,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    }
                };

                    var request = new InventoryProprietarySummaryRequest
                    {
                        FlightStartDate = startDate,
                        FlightEndDate = endDate,
                        PlanDaypartRequests = planDaypartRequests,
                        AudienceId = 5
                    };

                    /*** Act ***/
                    var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

                    /*** Assert ***/
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
                }
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