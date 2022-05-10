using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignExportDto
    {
        /// <summary>
        /// Gets or sets the CampaignId identifier.
        /// </summary>
        /// <value>
        /// The CampaignId.
        /// </value>
        public int Id { get; set; }
        /// <summary>
        /// Sepcifies whether campaign contains any plan
        /// </summary>
        public bool HasPlans { get; set; }
        /// <summary>
        /// Get or set the plan list
        /// </summary>
        public List<PlanExportSummaryDto> Plans { get; set; }
    }
}
