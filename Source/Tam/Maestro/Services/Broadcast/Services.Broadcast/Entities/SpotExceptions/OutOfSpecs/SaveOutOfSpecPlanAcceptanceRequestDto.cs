using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class SaveOutOfSpecPlanAcceptanceRequestDto
    {
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public List<int> PlanIds { get; set; }
        /// <summary>
        /// Filter out the result.
        /// </summary>
        /// <value>Filters.</value>
        public OutOfSpecPlansRequestDto Filters { get; set; }
    }
}
