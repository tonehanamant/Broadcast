using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    ///  Dto for Plans Copy data.
    /// </summary>
    public class PlansCopyDto
    {
        public int SourcePlanId { get; set; }
        public string Name { get; set; }
        public string ProductMasterId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public double? Impressions { get; set; }
        public decimal? CPM { get; set; }
        public decimal? Budget { get; set; }
        public List<int> SpotLengths { get; set; }
        public PlanStatusEnum Status { get; set; }
        /// <summary>
        /// Gets or sets the version identifier.
        /// </summary>
        /// <value>
        /// The version identifier.
        /// </value>
        public int VersionId { get; set; }


        /// <summary>
        /// Specify whether Plan has saved as draft
        /// </summary>
        public bool? IsDraft { get; set; }
        /// <summary>
        /// Gets or sets the latest version id in the plan table that matches with version id
        /// </summary>
        public int LatestVersionId { get; set; }

    }
}
