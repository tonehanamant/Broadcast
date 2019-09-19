using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanDefaultsDto
    {
        public int AudienceId { get; set; }

        public int SpotLengthId { get; set; }

        public bool Equivalized { get; set; }

        public bool EnableHiatus { get; set; }

        public AudienceTypeEnum AudienceType { get; set; }  

        public PostingTypeEnum PostingType { get; set; }
        
        public PlanStatusEnum PlanStatus { get; set; }

        public PlanCurrenciesEnum Currency { get; set; }

        public PlanGoalBreakdownTypeEnum PlanGoalBreakdownType { get; set; }

        public double CampaignGoalPercent { get; set; }

        public DaypartTypeEnum DaypartTypeEnum { get; set; }
    }
}
