using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            return PerformAggregations(campaignId, plans);
        }

        protected CampaignSummaryDto PerformAggregations(int campaignId, List<PlanDto> plans, bool runParallelAggregations = true)
        {
            var summary = new CampaignSummaryDto { CampaignId = campaignId };
            if (plans.Count < 1)
            {
                // When campaign does not contain any plan, campaign will have working status and PlanStatusCountCanceled will reset.
                summary.CampaignStatus = PlanStatusEnum.Working;
                summary.PlanStatusCountCanceled = null;
                return summary;
            }

            // We are excluding plans with statuses Scenario, Canceled and Rejected from campaign goals
            // We still need the Scenario plans to show the sum of them on campaign cards
            var filteredPlans = plans
                .Where(p => 
                    p.Status != PlanStatusEnum.Scenario && 
                    p.Status != PlanStatusEnum.Canceled &&
                    p.Status != PlanStatusEnum.Rejected &&
                    p.IsDraft != true) 
                .ToList();

            var aggFunctions = GetAggregationFunctionList();

            if (runParallelAggregations)
            {
                Parallel.ForEach(aggFunctions, (aggF) => aggF(plans, filteredPlans, summary));
                return summary;
            }

            foreach (var aggF in aggFunctions)
            {
                aggF(plans, filteredPlans, summary);
            }
            return summary;
        }
        
        private delegate void AggregationFunction(List<PlanDto> plans, List<PlanDto> filteredPlans, CampaignSummaryDto summary);

        private List<AggregationFunction> GetAggregationFunctionList()
        {
            return new List<AggregationFunction>
            {
                AggregateFlightInfo,
                AggregateBudgetAndGoalsInfo,
                AggregateCampaignStatus,
                AggregatePlansStatuses,
                AggregateComponentsModifiedTime,
            };
        }

        protected void AggregateFlightInfo(List<PlanDto> plans, List<PlanDto> filteredPlans, CampaignSummaryDto summary)
        {
            summary.FlightStartDate = filteredPlans.Min(p => p.FlightStartDate);
            summary.FlightEndDate = filteredPlans.Max(p => p.FlightEndDate);

            if (!summary.FlightStartDate.HasValue || !summary.FlightEndDate.HasValue)
            {
                return;
            }

            summary.FlightHiatusDays = filteredPlans.Sum(p => p.FlightHiatusDays.Count);
            summary.FlightActiveDays = (Convert.ToInt32(summary.FlightEndDate.Value.Subtract(summary.FlightStartDate.Value).TotalDays) + 1) - summary.FlightHiatusDays;
        }

        protected void AggregateBudgetAndGoalsInfo(List<PlanDto> plans, List<PlanDto> filteredPlans, CampaignSummaryDto summary)
        {
            // validate now to make sure cannot divide by zero
            if (!filteredPlans.Any())
            {
                return;
            }

            summary.Budget = filteredPlans.Sum(p => p.Budget);
            summary.HHImpressions = filteredPlans.Sum(p => p.HHImpressions);
            summary.HHCPM = (filteredPlans.Sum(p => p.Budget) / Convert.ToDecimal(summary.HHImpressions)) * 1000;
            summary.HHRatingPoints = filteredPlans.Sum(p => p.HHRatingPoints);
            summary.HHAduImpressions = filteredPlans.Sum(p => p.HhAduImpressions);
        }

        protected void AggregateCampaignStatus(List<PlanDto> plans, List<PlanDto> filteredPlans, CampaignSummaryDto summary)
        {
            summary.CampaignStatus = plans
                .OrderByDescending(p => p.Status == PlanStatusEnum.Scenario)
                .ThenByDescending(p => p.Status == PlanStatusEnum.Canceled)
                .ThenByDescending(p => p.Status == PlanStatusEnum.Rejected)
                .ThenBy(p => p.Status)
                .Last().Status;
        }

        protected void AggregatePlansStatuses(List<PlanDto> plans, List<PlanDto> filteredPlans, CampaignSummaryDto summary)
        {
            summary.PlanStatusCountWorking = plans.Count(p => p.Status == PlanStatusEnum.Working);
            summary.PlanStatusCountReserved = plans.Count(p => p.Status == PlanStatusEnum.Reserved);
            summary.PlanStatusCountClientApproval = plans.Count(p => p.Status == PlanStatusEnum.ClientApproval);
            summary.PlanStatusCountContracted = plans.Count(p => p.Status == PlanStatusEnum.Contracted);
            summary.PlanStatusCountLive = plans.Count(p => p.Status == PlanStatusEnum.Live);
            summary.PlanStatusCountComplete = plans.Count(p => p.Status == PlanStatusEnum.Complete);
            summary.PlanStatusCountScenario = plans.Count(p => p.Status == PlanStatusEnum.Scenario);
            summary.PlanStatusCountCanceled = plans.Count(p => p.Status == PlanStatusEnum.Canceled);
            summary.PlanStatusCountRejected = plans.Count(p => p.Status == PlanStatusEnum.Rejected);
        }

        protected void AggregateComponentsModifiedTime(List<PlanDto> plans, List<PlanDto> filteredPlans, CampaignSummaryDto summary)
        {
            summary.ComponentsModified = plans.Max(p => p.ModifiedDate);
        }
    }
}