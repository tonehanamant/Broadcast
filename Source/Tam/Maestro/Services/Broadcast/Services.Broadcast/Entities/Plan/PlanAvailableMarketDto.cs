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

        /// <summary>
        /// Gets or sets a value indicating whether the value in ShareOfVoicePercent was entered by the user or populated some other way.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user entered the value in ShareOfVoicePercent; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserShareOfVoicePercent { get; set; }
    }
}