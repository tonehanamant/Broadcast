using System;

namespace Services.Broadcast.Entities
{
    public class DateRange
    {
        public DateRange(DateTime? start, DateTime? end)
        {
            Start= start;
            End = end;
        }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public bool IsEmpty()
        {
            return !Start.HasValue && !End.HasValue;
        }
    }
}
