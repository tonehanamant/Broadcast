using System;

namespace Services.Broadcast.Entities
{
    public class StationDetailPointInTime
    {
        public int Id { get; set; }
        public short Code { get; set; }
        public int TimeAired { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
    }
}