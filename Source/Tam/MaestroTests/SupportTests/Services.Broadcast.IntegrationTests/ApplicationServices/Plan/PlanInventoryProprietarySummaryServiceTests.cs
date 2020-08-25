using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.ApplicationServices.Plan
{
	[TestFixture]
	public class PlanInventoryProprietarySummaryServiceTests
	{
		private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
		private Mock<IBroadcastAudienceRepository> _BroadcastAudienceRepositoryMock;
		private Mock<IInventoryProprietarySummaryRepository> _InventoryProprietarySummaryRepository;
		private IPlanInventoryProprietarySummaryService _PlanInventoryProprietarySummaryService;

		[SetUp]
		public void SetUp()
		{
			_BroadcastAudienceRepositoryMock = new Mock<IBroadcastAudienceRepository>();
			_InventoryProprietarySummaryRepository = new Mock<IInventoryProprietarySummaryRepository>();
			_DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
			_DataRepositoryFactoryMock
				.Setup(x => x.GetDataRepository<IBroadcastAudienceRepository>())
				.Returns(_BroadcastAudienceRepositoryMock.Object);
			_DataRepositoryFactoryMock
				.Setup(x => x.GetDataRepository<IInventoryProprietarySummaryRepository>())
				.Returns(_InventoryProprietarySummaryRepository.Object);
			_PlanInventoryProprietarySummaryService =
				new PlanInventoryProprietarySummaryService(_DataRepositoryFactoryMock.Object);
		}

		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void GetPlanProprietarySummaryAggregationTest_OneSummary()
		{
			// Arrange
			_BroadcastAudienceRepositoryMock
				.Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
				.Returns(new List<audience_audiences>
				{
					new audience_audiences {rating_audience_id = 7},
					new audience_audiences {rating_audience_id = 9}
				});

			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(1))
				.Returns(9.00M);

			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(1, It.IsAny<List<int>>()))
				.Returns(100000);

			var markeData = new List<PlanMarketDto>
			{
				new PlanMarketDto {MarketCode = 325, PercentageOfUS = 0.225},
				new PlanMarketDto {MarketCode = 334, PercentageOfUS = 0.069},
				new PlanMarketDto {MarketCode = 346, PercentageOfUS = 0.111},
				new PlanMarketDto {MarketCode = 421, PercentageOfUS = 0.06}
			};

			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetMarketDataBySummaryIds(It.IsAny<List<int>>()))
				.Returns(markeData);

			var request = new PlanInventoryProprietarySummaryRequest
			{
				InventoryProprietarySummaryIds = new List<int> {1}, PlanPrimaryAudienceId = 33,
				PlanGoalImpressions = 900000
			};

			// Act
			var result = _PlanInventoryProprietarySummaryService.GetPlanProprietarySummaryAggregation(request);

			// Assert
			Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
		}

		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void GetPlanProprietarySummaryAggregationTest_TwoSummaries()
		{
			// Arrange
			_BroadcastAudienceRepositoryMock
				.Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
				.Returns(new List<audience_audiences>
				{
					new audience_audiences {rating_audience_id = 7},
					new audience_audiences {rating_audience_id = 9}
				});

			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(1))
				.Returns(9.00M);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(2))
				.Returns(5.00M);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(1, It.IsAny<List<int>>()))
				.Returns(100000);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(2, It.IsAny<List<int>>()))
				.Returns(200000);
			var markeData = new List<PlanMarketDto>
			{
				new PlanMarketDto {MarketCode = 325, PercentageOfUS = 0.225},
				new PlanMarketDto {MarketCode = 334, PercentageOfUS = 0.069},
				new PlanMarketDto {MarketCode = 346, PercentageOfUS = 0.111},
				new PlanMarketDto {MarketCode = 421, PercentageOfUS = 0.06},
				new PlanMarketDto {MarketCode = 325, PercentageOfUS = 0.225},
				new PlanMarketDto {MarketCode = 334, PercentageOfUS = 0.069},
				new PlanMarketDto {MarketCode = 346, PercentageOfUS = 0.111}
			};

			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetMarketDataBySummaryIds(It.IsAny<List<int>>()))
				.Returns(markeData);

			var request = new PlanInventoryProprietarySummaryRequest
			{
				InventoryProprietarySummaryIds = new List<int> {1, 2},
				PlanPrimaryAudienceId = 33,
				PlanGoalImpressions = 900000
			};

			// Act
			var result = _PlanInventoryProprietarySummaryService.GetPlanProprietarySummaryAggregation(request);

			// Assert
			Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
		}

		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void GetPlanProprietarySummaryAggregationTest_ThreeSummaries()
		{
			// Arrange
			_BroadcastAudienceRepositoryMock
				.Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
				.Returns(new List<audience_audiences>
				{
					new audience_audiences {rating_audience_id = 7},
					new audience_audiences {rating_audience_id = 9}
				});

			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(1))
				.Returns(9.00M);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(2))
				.Returns(5.00M);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(3))
				.Returns(6.00M);

			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(1, It.IsAny<List<int>>()))
				.Returns(100000);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(2, It.IsAny<List<int>>()))
				.Returns(200000);
			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetTotalImpressionsBySummaryIdAndAudienceIds(3, It.IsAny<List<int>>()))
				.Returns(100000);

			var markeData = new List<PlanMarketDto>
			{
				new PlanMarketDto {MarketCode = 325, PercentageOfUS = 0.225},
				new PlanMarketDto {MarketCode = 334, PercentageOfUS = 0.069},
				new PlanMarketDto {MarketCode = 346, PercentageOfUS = 0.111},
				new PlanMarketDto {MarketCode = 421, PercentageOfUS = 0.06},
				new PlanMarketDto {MarketCode = 325, PercentageOfUS = 0.225},
				new PlanMarketDto {MarketCode = 334, PercentageOfUS = 0.069},
				new PlanMarketDto {MarketCode = 346, PercentageOfUS = 0.111}
			};

			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetMarketDataBySummaryIds(It.IsAny<List<int>>()))
				.Returns(markeData);

			var request = new PlanInventoryProprietarySummaryRequest
			{
				InventoryProprietarySummaryIds = new List<int> {1, 2, 3},
				PlanPrimaryAudienceId = 33,
				PlanGoalImpressions = 900000
			};

			// Act
			var result = _PlanInventoryProprietarySummaryService.GetPlanProprietarySummaryAggregation(request);

			// Assert
			Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
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

			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(1))
				.Returns(9.00M);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(2))
				.Returns(5.00M);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(3))
				.Returns(6.00M);
			_InventoryProprietarySummaryRepository.Setup(x => x.GetCPM(4))
				.Returns(8.00M);
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
			var markeData = new List<PlanMarketDto>
			{
				new PlanMarketDto {MarketCode = 325, PercentageOfUS = 0.225},
				new PlanMarketDto {MarketCode = 334, PercentageOfUS = 0.069},
				new PlanMarketDto {MarketCode = 346, PercentageOfUS = 0.111},
				new PlanMarketDto {MarketCode = 421, PercentageOfUS = 0.06},
				new PlanMarketDto {MarketCode = 325, PercentageOfUS = 0.225},
				new PlanMarketDto {MarketCode = 334, PercentageOfUS = 0.069},
				new PlanMarketDto {MarketCode = 346, PercentageOfUS = 0.111}
			};

			_InventoryProprietarySummaryRepository
				.Setup(x => x.GetMarketDataBySummaryIds(It.IsAny<List<int>>()))
				.Returns(markeData);

			var request = new PlanInventoryProprietarySummaryRequest
			{
				InventoryProprietarySummaryIds = new List<int> {1, 2, 3, 4},
				PlanPrimaryAudienceId = 33,
				PlanGoalImpressions = 900000
			};

			// Act
			var result = _PlanInventoryProprietarySummaryService.GetPlanProprietarySummaryAggregation(request);

			// Assert
			Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
		}
	}
}