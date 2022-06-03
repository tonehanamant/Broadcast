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
        public string SyncedBy { get; set; }
        public DateTime? SyncedAt { get; set; }
        public bool AcceptedAsInSpec { get; set; }
        /// <summary>
        /// Gets or sets the SpotExceptionsRecommendedPlans Id 
        /// </summary>
        public int SpotExceptionsId { get; set; }
    }
}