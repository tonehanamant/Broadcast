using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Operations for performing campaign aggregations.
    /// </summary>
    public interface ICampaignAggregator
    {
        /// <summary>
        /// Aggregates the specified campaign identifier.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        CampaignSummaryDto Aggregate(int campaignId);
    }

    /// <summary>
    /// Operations for performing campaign aggregations.
    /// </summary>
    public class CampaignAggregator : ICampaignAggregator
    {
        private readonly IPlanRepository _PlanRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignAggregator"/> class.
        /// </summary>
        /// <param name="broadcastDataRepositoryFactory">The broadcast data repository factory.</param>
        public CampaignAggregator(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
        }

        /// <inheritdoc />
        public CampaignSummaryDto Aggregate(int campaignId)
        {
            var plans = _PlanRepository.GetPlansForCampaign(campaignId);
            var summary = PerformAggregations(campaignId, plans);
            return summary;
        }

        protected CampaignSummaryDto PerformAggregations(int campaignId, List<PlanDto> plans, bool runParallelAggregations = true)
        {
            var summary = new CampaignSummaryDto { CampaignId = campaignId };
            if (plans.Count < 1)
            {
                return summary;
            }

            var aggFunctions = GetAggregationFunctionList();

            if (runParallelAggregations)
            {
                Parallel.ForEach(aggFunctions, (aggF) => aggF(plans, summary));
                return summary;
            }

            foreach (var aggF in aggFunctions)
            {
                aggF(plans, summary);
            }
            return summary;
        }
        
        private delegate void AggregationFunction(List<PlanDto> plans, CampaignSummaryDto summary);

        private List<AggregationFunction> GetAggregationFunctionList()
        {
            return new List<AggregationFunction>
            {
                AggregateFlightInfo,
                AggregateBudgetAndGoalsInfo,
                AggregateCampaignStatus,
                AggregatePlansStatuses,
                AggregateComponentsModifiedTime
            };
        }

        protected void AggregateFlightInfo(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.FlightStartDate = plans.Min(p => p.FlightStartDate);
            summary.FlightEndDate = plans.Max(p => p.FlightEndDate);

            if (summary.FlightStartDate.HasValue == false || summary.FlightEndDate.HasValue == false)
            {
                return;
            }

            summary.FlightHiatusDays = plans.Sum(p => p.FlightHiatusDays.Count);
            summary.FlightActiveDays = (Convert.ToInt32(summary.FlightEndDate.Value.Subtract(summary.FlightStartDate.Value).TotalDays) + 1) - summary.FlightHiatusDays;
        }

        protected void AggregateBudgetAndGoalsInfo(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.Budget = plans.Sum(p => p.Budget);
            summary.Impressions = plans.Sum(p => p.DeliveryImpressions);
            summary.CPM = plans.Sum(p => p.Budget) / Convert.ToDecimal(plans.Sum(p => p.DeliveryImpressions));
            summary.Rating = plans.Sum(p => p.DeliveryRatingPoints);
        }

        protected void AggregateCampaignStatus(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.CampaignStatus = plans.Max(p => p.Status);
        }

        protected void AggregatePlansStatuses(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.PlanStatusCountWorking = plans.Count(p => p.Status == PlanStatusEnum.Working);
            summary.PlanStatusCountReserved = plans.Count(p => p.Status == PlanStatusEnum.Reserved);
            summary.PlanStatusCountClientApproval = plans.Count(p => p.Status == PlanStatusEnum.ClientApproval);
            summary.PlanStatusCountContracted = plans.Count(p => p.Status == PlanStatusEnum.Contracted);
            summary.PlanStatusCountLive = plans.Count(p => p.Status == PlanStatusEnum.Live);
            summary.PlanStatusCountComplete = plans.Count(p => p.Status == PlanStatusEnum.Complete);
        }

        protected void AggregateComponentsModifiedTime(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.ComponentsModified = plans.Max(p => p.ModifiedDate);
        }
    }
}