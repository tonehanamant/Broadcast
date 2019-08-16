namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Describes the quarter covered by a plan's flight.
    /// </summary>
    public class PlanSummaryQuarterDto
    {
        /// <summary>
        /// A quarter covered by the plan's flight.
        /// </summary>
        /// <value>
        /// The quarter.
        /// </value>
        public int Quarter { get; set; }

        /// <summary>
        /// The related year for the Quarter.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public int Year { get; set; }
    }
}