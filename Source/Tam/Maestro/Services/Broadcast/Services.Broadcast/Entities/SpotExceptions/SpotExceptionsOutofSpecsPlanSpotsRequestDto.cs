using System;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutofSpecSpotsRequestDto
    {
        public int PlanId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
    }
}
