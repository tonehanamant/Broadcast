using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Extensions
{
    public static class DisplayDaypartExtensions
    {
        public static int GetTotalDurationInSeconds(this DisplayDaypart daypart)
        {
            var duration = _GetDurationInSeconds(daypart.StartTime, daypart.EndTime);

            return duration * daypart.Days.Count;
        }

        public static double GetDurationPerDayInHours(this DisplayDaypart daypart)
        {
            var duration = _GetDurationInSeconds(daypart.StartTime, daypart.EndTime);
            var hours = ((double)duration) / BroadcastConstants.OneHourInSeconds;

            return hours;
        }

        private static int _GetDurationInSeconds(int startTime, int endTime)
        {
            var duration = 0;

            if (startTime < endTime)
            {
                duration = endTime - startTime;
            }
            else if (startTime > endTime)
            {
                duration = BroadcastConstants.OneDayInSeconds - startTime + endTime;
            }

            // to include last second, e.g.
            // startTime = 4, EndTime = 6. Seconds 4,5,6 should be counted. 6 - 4 + 1 = 3
            return duration + 1;
        }
    }
}
