using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class CurrentBuyingExecutions
    {
        public CurrentBuyingExecutions()
        {
            Results = new List<CurrentBuyingExecutionResultDto>();
        }
        public PlanBuyingJob Job { get; set; }
        public List<CurrentBuyingExecutionResultDto> Results { get; set; }

        /// <summary>
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsBuyingModelRunning { get; set; }
    }
}
