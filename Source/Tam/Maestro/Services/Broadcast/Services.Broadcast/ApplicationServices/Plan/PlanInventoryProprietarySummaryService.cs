using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.ApplicationServices.Plan
{
	public interface IPlanInventoryProprietarySummaryService : IApplicationService
	{
		/// <summary>
		/// </summary>
		/// <param name="request"></param>
		/// <returns> Aggregated CPM, Impressions, Coverage and Percentage of Plan Impressions</returns>
		PlanInventoryProprietarySummaryResponse GetPlanProprietarySummaryAggregation(
			PlanInventoryProprietarySummaryRequest request);
	}

	public class PlanInventoryProprietarySummaryService : IPlanInventoryProprietarySummaryService
	{
		private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository;

		protected readonly IMarketCoverageRepository _MarketCoverageRepository;
		protected readonly IDaypartDefaultRepository _DaypartDefaultRepository;
		protected readonly IBroadcastAudienceRepository _AudienceRepository;

		public PlanInventoryProprietarySummaryService(IDataRepositoryFactory broadcastDataRepositoryFactory)
		{
			_DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();

			_InventoryProprietarySummaryRepository = broadcastDataRepositoryFactory
				.GetDataRepository<IInventoryProprietarySummaryRepository>();

			_MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
			_AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
		}

		public PlanInventoryProprietarySummaryResponse GetPlanProprietarySummaryAggregation(
			PlanInventoryProprietarySummaryRequest request)
		{
			var response = new PlanInventoryProprietarySummaryResponse();
			var summaryList = new List<InventoryProprietarySummary>();

			var summaryIds = request.InventoryProprietarySummaryIds;
			foreach (var summaryId in summaryIds)
			{
				var summary = new InventoryProprietarySummary();
				summary.Cpm = GetCpm(summaryId);
				summary.ImpressionsTotal = GetTotalImpressions(summaryId, request.PlanPrimaryAudienceId);

				summaryList.Add(summary);
			}

			var totalImpressions = summaryList.Select(i => i.ImpressionsTotal).Sum();
			response.Impressions = Math.Round(totalImpressions);

			response.MarketCoverage = GetTotalMarketCoverage(summaryIds);

			response.Cpm = GetTotalCpm(summaryList);

			response.PercentageOfPlanImpressions =
				GetPercentageOfPlanImpressions(request.PlanGoalImpressions, response.Impressions);

			return response;
		}

		private double GetPercentageOfPlanImpressions(double planGoalImpressions, double totalImpressions)
		{
			return Math.Round(GeneralMath.ConvertFractionToPercentage(totalImpressions / planGoalImpressions),
				0);
		}

		private decimal GetCpm(int id)
		{
			var cpm = _InventoryProprietarySummaryRepository.GetCPM(id);
			return cpm ?? 0;
		}

		private decimal GetTotalCpm(List<InventoryProprietarySummary> inventoryProprietarySummaries)
		{
			var totalCost = 0.0M;
			double totalImpressions = 0;
			foreach (var summary in inventoryProprietarySummaries)
			{
				summary.Cost = ProposalMath.CalculateCost(summary.Cpm, summary.ImpressionsTotal);
				totalCost += summary.Cost;
				totalImpressions += summary.ImpressionsTotal;
			}

			return ProposalMath.CalculateCpm(totalCost, totalImpressions);
		}

		private double GetTotalMarketCoverage(List<int> summaryIds)
		{
			var marketData = _InventoryProprietarySummaryRepository.GetMarketDataBySummaryIds(summaryIds);
			var marketCoverage = marketData.GroupBy(i => i.MarketCode).Select(i => i.First().PercentageOfUS).Sum();
			return Math.Round(marketCoverage, 0);
		}

		private double GetTotalImpressions(int id, int planPrimaryAudienceId)
		{
			var summaryAudienceIds = _AudienceRepository
				.GetRatingsAudiencesByMaestroAudience(new List<int> {planPrimaryAudienceId})
				.Select(am => am.rating_audience_id).Distinct().ToList();


			var impressionsTotal = Math.Round(
				_InventoryProprietarySummaryRepository.GetTotalImpressionsBySummaryIdAndAudienceIds(id,
					summaryAudienceIds)
			);


			return impressionsTotal;
		}
	}
}