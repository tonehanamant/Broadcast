using System;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanDecisionDto
    {
        public int Id { get; set; }
        public int SpotExceptionsRecommendedPlanId { get; set; }
        public int SpotExceptionsRecommendedPlanDetailId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}