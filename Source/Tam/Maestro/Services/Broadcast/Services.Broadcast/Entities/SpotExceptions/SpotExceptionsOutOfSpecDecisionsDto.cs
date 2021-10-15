using System;
namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecDecisionsDto
    {
        public int Id { get; set; }
        public int SpotExceptionsRecommendedPlanId { get; set; }
        public bool AcceptedAsInSpec { get; set; }
        public string DecisionNotes { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
		
