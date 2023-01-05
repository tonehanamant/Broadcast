using System;

namespace Services.Broadcast.Helpers
{
    /// <summary>
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Gets for display.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="dateType">Type of the date.</param>
        /// <returns></returns>
        public static string GetForDisplay(DateTime? date, string dateType)
        {
            switch (dateType)
            {
                case SpotExceptionsConstants.DateFormat:
                    return date?.ToString(SpotExceptionsConstants.DateFormat);

                case SpotExceptionsConstants.TimeFormat:
                    return date?.ToString(SpotExceptionsConstants.TimeFormat);

                case SpotExceptionsConstants.DateTimeFormat:
                    return date?.ToString(SpotExceptionsConstants.DateTimeFormat);

                default:
                    return date?.ToString();
            }
        }
    }
}
