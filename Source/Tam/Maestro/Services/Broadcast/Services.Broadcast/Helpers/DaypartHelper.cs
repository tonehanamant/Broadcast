using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Helpers
{
    public static class DaypartHelper
    {
        /// <summary>
        /// Convert a list of number based days to a daypart
        /// </summary>
        /// <param name="daypartDays">Each item can be a number 1-7 which represent M-Su days accordingly</param>
        /// <returns></returns>
        public static DisplayDaypart ConvertDaypartDaysToDaypart(IEnumerable<int> daypartDays)
        {
            var daypartDaysSet = new HashSet<int>(daypartDays);

            return ConvertDaypartDaysToDaypart(daypartDaysSet);
        }

        /// <summary>
        /// Convert a list of number based days to a daypart
        /// </summary>
        /// <param name="daypartDays">Each item can be a number 1-7 which represent M-Su days accordingly</param>
        /// <returns></returns>
        public static DisplayDaypart ConvertDaypartDaysToDaypart(HashSet<int> daypartDays)
        {
            return new DisplayDaypart
            {
                Monday = daypartDays.Contains(1),
                Tuesday = daypartDays.Contains(2),
                Wednesday = daypartDays.Contains(3),
                Thursday = daypartDays.Contains(4),
                Friday = daypartDays.Contains(5),
                Saturday = daypartDays.Contains(6),
                Sunday = daypartDays.Contains(7)
            };
        }
    }
}
