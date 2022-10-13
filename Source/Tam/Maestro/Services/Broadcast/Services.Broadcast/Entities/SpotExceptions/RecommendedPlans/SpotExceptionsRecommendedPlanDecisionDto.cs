using System;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class SpotExceptionsRecommendedPlanDecisionDto
    {
        public int Id { get; set; }
        public int SpotExceptionsRecommendedPlanId { get; set; }
        public int SpotExceptionsRecommendedPlanDetailDoneId { get; set; }
        public string DecidedBy { get; set; }
        public DateTime DecidedAt { get; set; }
        public string SyncedBy { get; set; }
        public DateTime? SyncedAt { get; set; }
        public int SpotExceptionsId { get; set; }
    }
}