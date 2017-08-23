using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalQuarterDto
    {
        public int? Id { get; set; }
        public string QuarterText { get; set; }
        public byte Quarter { get; set; }
        public int Year { get; set; }
        public decimal Cpm { get; set; }
        public double ImpressionGoal { get; set; }
        public bool DistributeGoals { get; set; }
        public List<ProposalWeekDto> Weeks { get; set; }
    }
}
