using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Extensions
{
    public static class DisplayDaypartExtensions
    {
        private const int _OneDayInSeconds = 86400;

        public static int GetTotalTimeDuration(this DisplayDaypart daypart)
        {
            var duration = 0;

            if (daypart.StartTime < daypart.EndTime)
            {
                duration = daypart.EndTime - daypart.StartTime;
            }
            else if (daypart.StartTime > daypart.EndTime)
            {
                duration = _OneDayInSeconds - daypart.StartTime + daypart.EndTime;
            }

            return duration * daypart.Days.Count;
        }
    }
}
