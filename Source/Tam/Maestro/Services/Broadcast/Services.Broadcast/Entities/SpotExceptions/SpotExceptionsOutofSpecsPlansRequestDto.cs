using System;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutofSpecsPlansRequestDto
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
    }
}