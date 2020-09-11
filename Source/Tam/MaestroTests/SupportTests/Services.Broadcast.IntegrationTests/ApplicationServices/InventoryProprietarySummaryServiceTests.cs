using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.IntegrationTests.Helpers;
using System;
using System.Collections.Generic;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Moq;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")]
    [UseReporter(typeof(DiffReporter))]
    public class InventoryProprietarySummaryServiceTests
	{
		private InventoryFileTestHelper _InventoryFileTestHelper;
		private IInventoryProprietarySummaryService _InventoryProprietarySummaryService;
		private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
		private Mock<IBroadcastAudienceRepository> _BroadcastAudienceRepositoryMock;
		private Mock<IInventoryProprietarySummaryRepository> _InventoryProprietarySummaryRepository;
		private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;
		private IInventoryProprietarySummaryService _InventoryProprietarySummaryServiceMock;
		private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;


		[SetUp]
		public void Init()
		{
			try
			{
				_InventoryFileTestHelper = new InventoryFileTestHelper();
				_BroadcastAudienceRepositoryMock = new Mock<IBroadcastAudienceRepository>();
				_InventoryProprietarySummaryRepository = new Mock<IInventoryProprietarySummaryRepository>();
				_DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
				_MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
				_SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();

				_DataRepositoryFactoryMock
					.Setup(x => x.GetDataRepository<IBroadcastAudienceRepository>())
					.Returns(_BroadcastAudienceRepositoryMock.Object);

				_DataRepositoryFactoryMock
					.Setup(x => x.GetDataRepository<IInventoryProprietarySummaryRepository>())
					.Returns(_InventoryProprietarySummaryRepository.Object);

				_DataRepositoryFactoryMock
					.Setup(x => x.GetDataRepository<IMarketCoverageRepository>())
					.Returns(_MarketCoverageRepositoryMock.Object);
				_DataRepositoryFactoryMock
					.Setup(x => x.GetDataRepository<ISpotLengthRepository>())
					.Returns(_SpotLengthRepositoryMock.Object);
				_InventoryProprietarySummaryServiceMock =
					new InventoryProprietarySummaryService(_DataRepositoryFactoryMock.Object,null );

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		[Test]
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
                    AudienceId = 5,
                    SpotLengthIds = new List<int> { 2, 6 }
                };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
			}
		}

		[Test]
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
                    AudienceId = 34,
                    SpotLengthIds = new List<int> { 3 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
	                    new WeeklyBreakdownWeek
	                    {
		                    MediaWeekId = 875,
		                    DaypartCodeId = null,
		                    SpotLengthId = null,
		                    WeeklyAdu = 0,
		                    WeeklyImpressions = 4000000,
		                    WeeklyBudget = 100000.00M,
		                    WeeklyRatings = 29.8892007328832,
		                    UnitImpressions = 2000000,
		                    NumberOfActiveDays=7,
		                    ActiveDays="M-Su"
	                    }
                    }
                };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(req);

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
			}
		}

		[Test]
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
                    AudienceId = 5,
                    SpotLengthIds = new List<int> { 1, 3 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
	                    new WeeklyBreakdownWeek
	                    {
		                    MediaWeekId = 875,
		                    DaypartCodeId = null,
		                    SpotLengthId = null,
		                    WeeklyAdu = 0,
		                    WeeklyImpressions = 4000000,
		                    WeeklyBudget = 100000.00M,
		                    WeeklyRatings = 29.8892007328832,
		                    UnitImpressions = 2000000,
		                    NumberOfActiveDays=7,
		                    ActiveDays="M-Su"
	                    }
                    }
                };

				/*** Act ***/
				var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

				/*** Assert ***/
				Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
			}
		}

        [Test]
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
                    AudienceId = 5,
                    SpotLengthIds = new List<int> { 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
	                    new WeeklyBreakdownWeek
	                    {
		                    MediaWeekId = 875,
		                    DaypartCodeId = null,
		                    SpotLengthId = null,
		                    WeeklyAdu = 0,
		                    WeeklyImpressions = 4000000,
		                    WeeklyBudget = 100000.00M,
		                    WeeklyRatings = 29.8892007328832,
		                    UnitImpressions = 2000000,
		                    NumberOfActiveDays=7,
		                    ActiveDays="M-Su"
	                    }
                    }
                };

                /*** Act ***/
                var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

                /*** Assert ***/
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
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
                    AudienceId = 5,
                    SpotLengthIds = new List<int> { 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
	                    new WeeklyBreakdownWeek
	                    {
		                    MediaWeekId = 875,
		                    DaypartCodeId = null,
		                    SpotLengthId = null,
		                    WeeklyAdu = 0,
		                    WeeklyImpressions = 4000000,
		                    WeeklyBudget = 100000.00M,
		                    WeeklyRatings = 29.8892007328832,
		                    UnitImpressions = 2000000,
		                    NumberOfActiveDays=7,
		                    ActiveDays="M-Su"
	                    }
                    }
                };

                /*** Act ***/
                var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

                /*** Assert ***/
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        public void AggregateInventoryProprietarySummaryDataTest_SameProgramName()
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
                    AudienceId = 5,
                    SpotLengthIds = new List<int> { 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
	                    new WeeklyBreakdownWeek
	                    {
		                    MediaWeekId = 875,
		                    DaypartCodeId = null,
		                    SpotLengthId = null,
		                    WeeklyAdu = 0,
		                    WeeklyImpressions = 4000000,
		                    WeeklyBudget = 100000.00M,
		                    WeeklyRatings = 29.8892007328832,
		                    UnitImpressions = 2000000,
		                    NumberOfActiveDays=7,
		                    ActiveDays="M-Su"
	                    }
                    }
                };

                /*** Act ***/
                var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

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
		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void GetPlanProprietarySummaryAggregationTest_fourSummaries()
		{
			// Arrange
			_BroadcastAudienceRepositoryMock
				.Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
				.Returns(new List<audience_audiences>
				{
					new audience_audiences {rating_audience_id = 7},
					new audience_audiences {rating_audience_id = 9}
				});

			_InventoryProprietarySummaryRepository.Setup(x => x.GetProprietarySummaryUnitCost(1))
				.Returns(900);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetProprietarySummaryUnitCost(2))
				.Returns(1000);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetProprietarySummaryUnitCost(3))
				.Returns(600);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetProprietarySummaryUnitCost(4))
				.Returns(1600);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(1, It.IsAny<List<int>>()))
				.Returns(100000);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(2, It.IsAny<List<int>>()))
				.Returns(200000);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(3, It.IsAny<List<int>>()))
				.Returns(100000);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(4, It.IsAny<List<int>>()))
				.Returns(200000);

			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetMarketCodesBySummaryIds(It.IsAny<IEnumerable<int>>()))
				.Returns(new List<short> { 325, 334, 346, 421, 101 });

			_MarketCoverageRepositoryMock
				.Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
				.Returns(MarketsTestData.GetLatestMarketCoverages());

			var request = new TotalInventoryProprietarySummaryRequest
			{
				InventoryProprietarySummaryIds = new List<int> { 1, 2, 3, 4 },
				PlanPrimaryAudienceId = 33,
				PlanGoalImpressions = 900000,
				WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
				{
					new WeeklyBreakdownWeek
					{
						MediaWeekId = 875,
						DaypartCodeId = null,
						SpotLengthId = null,
						WeeklyAdu = 0,
						WeeklyImpressions = 4000000,
						WeeklyBudget = 100000.00M,
						WeeklyRatings = 29.8892007328832,
						UnitImpressions = 2000000,
						NumberOfActiveDays=7,
						ActiveDays="M-Su"
					}
				}
			};

			// Act
			var result = _InventoryProprietarySummaryServiceMock.GetPlanProprietarySummaryAggregation(request);

			// Assert
			Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
		}
	}
}