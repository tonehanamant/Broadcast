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
                var summary = new InventoryProprietarySummary
                {
                    UnitCost = GetUnitCost(summaryId),
                    ImpressionsTotal = GetTotalImpressions(summaryId, request.PlanPrimaryAudienceId)
                };

                summary.Cpm = ProposalMath.CalculateCpm(summary.UnitCost, summary.ImpressionsTotal);

                summaryList.Add(summary);
			}

			var totalImpressions = summaryList.Select(i => i.ImpressionsTotal).Sum();

            response.Impressions = totalImpressions;
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

		private decimal GetUnitCost(int id)
		{
			var unitCost = _InventoryProprietarySummaryRepository.GetProprietarySummaryUnitCost(id);
			return unitCost ?? 0;
		}

		private decimal GetTotalCpm(List<InventoryProprietarySummary> inventoryProprietarySummaries)
		{
			var totalCost = 0m;
			var totalImpressions = 0d;

			foreach (var summary in inventoryProprietarySummaries)
			{
				totalCost += summary.UnitCost;
				totalImpressions += summary.ImpressionsTotal;
			}

			return ProposalMath.CalculateCpm(totalCost, totalImpressions);
		}

		private double GetTotalMarketCoverage(List<int> summaryIds)
		{
			var marketData = _InventoryProprietarySummaryRepository.GetMarketDataBySummaryIds(summaryIds);
			return marketData.GroupBy(i => i.MarketCode).Select(i => i.First().PercentageOfUS).Sum();
		}

		private double GetTotalImpressions(int id, int planPrimaryAudienceId)
		{
			var summaryAudienceIds = _AudienceRepository
				.GetRatingsAudiencesByMaestroAudience(new List<int> {planPrimaryAudienceId})
				.Select(am => am.rating_audience_id).Distinct().ToList();

			var impressionsTotal = Math.Round(
				_InventoryProprietarySummaryRepository.GetTotalImpressionsBySummaryIdAndAudienceIds(id,
					summaryAudienceIds));

			return impressionsTotal;
		}
	}
}