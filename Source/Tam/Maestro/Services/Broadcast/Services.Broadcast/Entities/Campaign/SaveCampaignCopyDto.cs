using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Campaign
{
    /// <summary>
    /// Dto for copying a Campaign
    /// </summary>
    public class SaveCampaignCopyDto
    {
        public SaveCampaignCopyDto()
        {
            Plans = new List<SavePlansCopyDto>();
        }
        public string Name { get; set; }
        public Guid? AdvertiserMasterId { get; set; }
        public Guid? AgencyMasterId { get; set; }
        public List<SavePlansCopyDto> Plans { get; set; }
    }
}
