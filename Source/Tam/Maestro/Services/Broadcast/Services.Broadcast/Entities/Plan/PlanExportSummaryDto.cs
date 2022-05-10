using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanExportSummaryDto
    {
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>
        /// The plan identifier.
        /// </value>
        public int PlanId { get; set; }
        /// <summary>
        /// Gets or sets the plan name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the version identifier.
        /// </summary>
        /// <value>
        /// The version identifier.
        /// </value>
        public int VersionId { get; set; }
        /// <summary>
        /// Gets or sets the plan status
        /// </summary>
        public PlanStatusEnum Status { get; set; }
        /// <summary>
        /// Gets or sets the draft identifier.
        /// </summary>
        /// <value>
        /// The draft identifier.
        /// </value>
        public int? DraftId { get; set; }
        /// <summary>
        /// Specify whether Plan has saved as draft
        /// </summary>
        public bool? IsDraft { get; set; }

        /// <summary>
        /// Specify the version number of the plan
        /// </summary>
        public int? VersionNumber { get; set; }
        /// <summary>
        /// The quarters covered by the plan's flight.
        /// </summary>
        /// <value>
        /// The plan summary quarters.
        /// </value>
        public List<PlanSummaryQuarterDto> PlanSummaryQuarters { get; set; } = new List<PlanSummaryQuarterDto>();
    
    }
}
