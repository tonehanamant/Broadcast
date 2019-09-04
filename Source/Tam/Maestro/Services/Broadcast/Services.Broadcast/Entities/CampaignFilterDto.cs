using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class CampaignFilterDto
    {
        public QuarterDto Quarter { get; set; }

        public PlanStatusEnum? PlanStatus { get; set; }
    }
}