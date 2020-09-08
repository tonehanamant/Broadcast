using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.TestData;
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
		private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;

		[SetUp]
		public void SetUp()
		{
			_BroadcastAudienceRepositoryMock = new Mock<IBroadcastAudienceRepository>();
			_InventoryProprietarySummaryRepository = new Mock<IInventoryProprietarySummaryRepository>();
			_DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
			_MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();

			_DataRepositoryFactoryMock
				.Setup(x => x.GetDataRepository<IBroadcastAudienceRepository>())
				.Returns(_BroadcastAudienceRepositoryMock.Object);

			_DataRepositoryFactoryMock
				.Setup(x => x.GetDataRepository<IInventoryProprietarySummaryRepository>())
				.Returns(_InventoryProprietarySummaryRepository.Object);

			_DataRepositoryFactoryMock
				.Setup(x => x.GetDataRepository<IMarketCoverageRepository>())
				.Returns(_MarketCoverageRepositoryMock.Object);

			_PlanInventoryProprietarySummaryService =
				new PlanInventoryProprietarySummaryService(_DataRepositoryFactoryMock.Object);
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