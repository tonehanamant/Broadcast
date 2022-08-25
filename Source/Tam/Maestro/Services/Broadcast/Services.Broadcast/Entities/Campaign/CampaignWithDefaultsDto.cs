using System.Collections.Generic;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignWithDefaultsDto
    {
        public CampaignWithDefaultsDto()
        {
            this.SecondaryAudiences = new List<CampaignPlanSecondaryAudiencesDto>();
        }
        public List<CampaignPlanSecondaryAudiencesDto> SecondaryAudiences { get; set; }
        public int? MaxFluidityPercent { get; set; }
    }
}
