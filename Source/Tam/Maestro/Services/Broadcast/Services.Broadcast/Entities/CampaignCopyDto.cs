using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Dto for Campaign Copy data.
    /// </summary>
    public class CampaignCopyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AdvertiserMasterId { get; set; }
        public string AgencyMasterId { get; set; }
        public List<PlansCopyDto> Plans { get; set; }
    }
}
