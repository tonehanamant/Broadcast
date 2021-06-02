using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class CurrentBuyingExecution
    {
        public PlanBuyingJob Job { get; set; }
        public CurrentBuyingExecutionResultDto Result { get; set; }

        /// <summary>
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsBuyingModelRunning { get; set; }
    }

    public class CurrentBuyingExecutionResultDto
    {
        public decimal OptimalCpm { get; set; }
        public int? JobId { get; set; }
        public int? PlanVersionId { get; set; }
        public bool GoalFulfilledByProprietary { get; set; }
        public string Notes { get; set; }
        public bool HasResults { get; set; }
        public int CpmPercentage { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
        public PostingTypeEnum PostingType { get; set; }
    }
}
