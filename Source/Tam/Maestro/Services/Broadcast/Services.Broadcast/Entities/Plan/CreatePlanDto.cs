using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    public class CreatePlanDto
    {
        public int CampaignId { get; set; }
        public string Name { get; set; }
        public int SpotLengthId { get; set; }
        public bool Equivalized { get; set; }
        public PlanStatusEnum Status { get; set; }
        public int ProductId { get; set; }
    }
}
