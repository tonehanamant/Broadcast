
using System;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanDecisionDto
    {
        public int Id { get; set; }      
        public int SelectedDetailsId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        
    }
}