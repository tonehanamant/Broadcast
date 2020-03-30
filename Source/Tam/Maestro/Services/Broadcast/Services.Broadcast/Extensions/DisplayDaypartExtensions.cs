using Services.Broadcast.Helpers;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Extensions
{
    public static class DisplayDaypartExtensions
    {
        public static int GetTotalDurationInSeconds(this DisplayDaypart daypart)
        {
            var duration = DaypartTimeHelper.GetTotalTimeInclusive(daypart.StartTime, daypart.EndTime);

            return duration * daypart.Days.Count;
        }

        public static double GetDurationPerDayInHours(this DisplayDaypart daypart)
        {
            var duration = DaypartTimeHelper.GetTotalTimeInclusive(daypart.StartTime, daypart.EndTime);
            var hours = ((double)duration) / BroadcastConstants.OneHourInSeconds;

            return hours;
        }
    }
}
