﻿using Common.Services.Repositories;
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
            return PerformAggregations(campaignId, plans);
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
                AggregateComponentsModifiedTime,
            };
        }

        protected void AggregateFlightInfo(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.FlightStartDate = plans.Min(p => p.FlightStartDate);
            summary.FlightEndDate = plans.Max(p => p.FlightEndDate);

            if (!summary.FlightStartDate.HasValue || !summary.FlightEndDate.HasValue)
            {
                return;
            }

            summary.FlightHiatusDays = plans.Sum(p => p.FlightHiatusDays.Count);
            summary.FlightActiveDays = (Convert.ToInt32(summary.FlightEndDate.Value.Subtract(summary.FlightStartDate.Value).TotalDays) + 1) - summary.FlightHiatusDays;
        }

        protected void AggregateBudgetAndGoalsInfo(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            // TODO: Remove this once full validation has been implemented
            // invalid nulls have been introduced during product evolution.
            // validate now to make sure cannot divide by zero
            if (plans.Any(p => p.DeliveryImpressions == null))
            {
                return;
            }

            summary.Budget = plans.Sum(p => p.Budget);
            summary.HouseholdImpressions = plans.Sum(p => p.HouseholdDeliveryImpressions);
            summary.HouseholdCPM = (plans.Sum(p => p.Budget) / Convert.ToDecimal(summary.HouseholdImpressions)) * 1000;
            summary.HouseholdRatingPoints = plans.Sum(p => p.HouseholdRatingPoints);
        }

        protected void AggregateCampaignStatus(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.CampaignStatus = plans
                .OrderByDescending(p => p.Status == PlanStatusEnum.Scenario)
                .ThenByDescending(p => p.Status == PlanStatusEnum.Canceled)
                .ThenByDescending(p => p.Status == PlanStatusEnum.Rejected)
                .ThenBy(p => p.Status)
                .Last().Status;
        }

        protected void AggregatePlansStatuses(List<PlanDto> plans, CampaignSummaryDto summary)
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

        protected void AggregateComponentsModifiedTime(List<PlanDto> plans, CampaignSummaryDto summary)
        {
            summary.ComponentsModified = plans.Max(p => p.ModifiedDate);
        }
    }
}