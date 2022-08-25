using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignPlanSecondaryAudiencesDto
    {
        public AudienceTypeEnum Type { get; set; }
        public int AudienceId { get; set; }
    }
}
