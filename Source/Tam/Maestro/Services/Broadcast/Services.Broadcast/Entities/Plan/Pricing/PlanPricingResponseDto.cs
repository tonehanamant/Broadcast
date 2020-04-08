namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResponseDto
    {
        public PlanPricingJob Job { get; set; }
        public GetPlanPricingResultDto Result { get; set; }
        
        /// <summary>
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsPricingModelRunning { get; set; }
    }
}
