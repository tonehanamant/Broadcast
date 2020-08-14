namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResponseDto
    {
        public PlanBuyingJob Job { get; set; }
        public PlanBuyingResultDto Result { get; set; }
        
        /// <summary>
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsBuyingModelRunning { get; set; }
    }
}
