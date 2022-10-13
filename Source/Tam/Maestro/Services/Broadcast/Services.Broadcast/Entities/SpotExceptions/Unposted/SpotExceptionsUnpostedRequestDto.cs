using System;

namespace Services.Broadcast.Entities.SpotExceptions.Unposted
{
    public class SpotExceptionsUnpostedRequestDto
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
    }
}
