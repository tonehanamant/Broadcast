namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// The user defined daypart for the plan.
    /// </summary>
    public class PlanDaypartDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the daypart code identifier.
        /// </summary>
        /// <value>
        /// The daypart code identifier.
        /// </value>
        public int DaypartCodeId { get; set; }

        /// <summary>
        /// Gets or sets the start time seconds from midnight ET.
        /// </summary>
        /// <value>
        /// The start time seconds from midnight ET.
        /// </value>
        public int StartTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the end time seconds from midnight ET.
        /// </summary>
        /// <value>
        /// The end time seconds from midnight ET.
        /// </value>
        public int EndTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the weighting goal percent.
        /// </summary>
        /// <value>
        /// The weighting goal percent.
        /// </value>
        public double? WeightingGoalPercent { get; set; }
    }
}