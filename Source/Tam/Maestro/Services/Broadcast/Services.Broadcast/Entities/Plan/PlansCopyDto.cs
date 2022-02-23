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

    }
}
