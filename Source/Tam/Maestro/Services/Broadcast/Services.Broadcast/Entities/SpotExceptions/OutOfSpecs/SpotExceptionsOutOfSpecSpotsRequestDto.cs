using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class SpotExceptionsOutOfSpecSpotsRequestDto
    {
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int PlanId { get; set; }

        /// <summary>
        /// Gets or sets the week start date.</summary>
        /// <value>The week start date.
        /// </value>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// Gets or sets the week end date.</summary>
        /// <value>The week end date.
        /// </value>
        public DateTime WeekEndDate { get; set; }
    }
}
