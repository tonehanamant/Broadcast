using System;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciSearchDto
    {
        public MediaMonthDto MediaMonth { get; set; }
        public DateTime? WeekStartDate { get; set; }
        public DateTime? WeekEndDate { get; set; }
        public bool UnmappedOnly { get; set; }
    }
}
