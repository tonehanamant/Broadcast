using System;

namespace Services.Broadcast.Entities
{
    public class DateRange
    {
        public DateRange()
        { }

        public DateRange(DateTime? start, DateTime? end)
        {
            Start = start;
            End = end;
        }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public bool IsEmpty()
        {
            return !Start.HasValue && !End.HasValue;
        }

        public static DateRange ConvertToDateRange(Tuple<QuarterDetailDto, QuarterDetailDto> tuple)
        {
            if (tuple == null)
                return new DateRange(null, null);

            if (tuple.Item1 == null && tuple.Item2 == null)
                return new DateRange(null, null);

            if (tuple.Item1 != null && tuple.Item2 == null)
                return new DateRange(tuple.Item1.StartDate, tuple.Item1.EndDate);

            if (tuple.Item1 == null && tuple.Item2 != null)
                return new DateRange(tuple.Item2.StartDate, tuple.Item2.EndDate);

            return new DateRange(tuple.Item1.StartDate, tuple.Item2.EndDate);
        }
    }
}
