﻿using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class CurrentPricingExecution
    {
        public PlanPricingJob Job { get; set; }
        public CurrentPricingExecutionResultDto Result { get; set; }

        /// <summary>
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsPricingModelRunning { get; set; }
    }    

    public class CurrentPricingExecutionResultDto
    {
        public int Id { get; set; }
        public decimal OptimalCpm { get; set; }
        public int? JobId { get; set; }
        public int? PlanVersionId { get; set; }
        public bool GoalFulfilledByProprietary { get; set; }
        public string Notes { get; set; }
        public bool HasResults { get; set; }
        public int CpmPercentage { get; set; }
        public PostingTypeEnum PostingType { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
        public double CalculatedVpvh { get; set; }
        public decimal TotalBudget { get; set; }
        public double TotalImpressions { get; set; }
    }
}
