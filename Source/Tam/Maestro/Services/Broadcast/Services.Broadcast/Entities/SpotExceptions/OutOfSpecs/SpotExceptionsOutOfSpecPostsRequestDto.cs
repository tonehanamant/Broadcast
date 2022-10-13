using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionsOutOfSpecPostsRequestDto
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
    }
}
