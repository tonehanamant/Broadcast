using System;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class SpotExceptionsRecommendedPlanDoneDecisionsDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the spot exceptions recommended plan details done identifier.
        /// </summary>
        /// <value>The spot exceptions recommended plan details done identifier.</value>
        public int SpotExceptionsRecommendedPlanDetailsDoneId { get; set; }

        /// <summary>
        /// Gets or sets the decided by.
        /// </summary>
        /// <value>The decided by.</value>
        public string DecidedBy { get; set; }

        /// <summary>
        /// Gets or sets the decided at.
        /// </summary>
        /// <value>The decided at.</value>
        public DateTime DecidedAt { get; set; }

        /// <summary>
        /// Gets or sets the synced by.
        /// </summary>
        /// <value>The synced by.</value>
        public string SyncedBy { get; set; }

        /// <summary>
        /// Gets or sets the synced at.
        /// </summary>
        /// <value>The synced at.</value>
        public DateTime? SyncedAt { get; set; }

        /// <summary>
        /// Gets or sets the SpotExceptionsRecommendedPlans Id 
        /// </summary>
        public int SpotExceptionsId { get; set; }
    }
}
