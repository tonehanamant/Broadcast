namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// A market available to a plan.
    /// </summary>
    public class PlanAvailableMarketDto : PlanMarketDto
    {
        /// <summary>
        /// Gets or sets the percent of Share of Voice for the market for the plan.
        /// </summary>
        /// <value>
        /// The share of voice percent for the market for the plan.
        /// </value>
        public double? ShareOfVoicePercent { get; set; }
    }
}