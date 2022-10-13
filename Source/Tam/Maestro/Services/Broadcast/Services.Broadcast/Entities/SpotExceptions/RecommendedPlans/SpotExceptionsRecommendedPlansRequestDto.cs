using System;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class SpotExceptionsRecommendedPlansRequestDto
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
    }
}
