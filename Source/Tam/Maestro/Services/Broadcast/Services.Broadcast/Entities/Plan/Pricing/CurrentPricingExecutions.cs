using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class CurrentPricingExecutions
    {
        public CurrentPricingExecutions()
        {
            Results = new List<CurrentPricingExecutionResultDto>();
        }
        public PlanPricingJob Job { get; set; }
        public List<CurrentPricingExecutionResultDto> Results { get; set; }

        /// <summary>
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsPricingModelRunning { get; set; }
    }
}
