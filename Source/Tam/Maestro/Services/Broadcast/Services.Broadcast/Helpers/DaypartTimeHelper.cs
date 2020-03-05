using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Services.Broadcast.Helpers
{
    /// <summary>
    /// Helper operations for daypart times.
    /// </summary>
    public static class DaypartTimeHelper
    {
        private static readonly Dictionary<Type,string> _RegisteredTypesAndProperties = new Dictionary<Type, string>
        {
            {typeof(DaypartDefaultFullDto), nameof(DaypartDefaultFullDto.DefaultEndTimeSeconds)},
            {typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds)}
        };

        /// <summary>
        /// Transforms the given candidates from their stored state for display.
        /// </summary>
        /// <param name="candidates">The candidates.</param>
        public static void AddOneSecondToEndTime<T>(List<T> candidates)
        {
            var incomingType = typeof(T);
            if (_RegisteredTypesAndProperties.ContainsKey(incomingType) == false)
            {
                throw new InvalidOperationException("Invalid type provided in list.");
            }

            var pi = incomingType.GetProperty(_RegisteredTypesAndProperties[incomingType]);
            candidates.ForEach(c => pi.SetValue(c, (int)pi.GetValue(c) + 1));
        }

        /// <summary>
        /// Subtracts the one second to end time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="candidates">The candidates.</param>
        /// <exception cref="System.InvalidOperationException">Invalid type for this operation.</exception>
        public static void SubtractOneSecondToEndTime<T>(List<T> candidates)
        {
            var incomingType = typeof(T);
            if (_RegisteredTypesAndProperties.ContainsKey(incomingType) == false)
            {
                throw new InvalidOperationException("Invalid type provided in list.");
            }

            var pi = incomingType.GetProperty(_RegisteredTypesAndProperties[incomingType]);
            candidates.ForEach(c => pi.SetValue(c, (int)pi.GetValue(c) - 1));
        }

        /// <summary>
        /// Gets the time format as "hh:mm:ss tt" from seconds.
        /// </summary>
        /// <param name="seconds">The seconds to format as time</param>
        /// <returns>String containing the time format</returns>
        public static string ConvertSecondsToFormattedTime(int seconds)
        {
            return DateTime.Today.Add(TimeSpan.FromSeconds(seconds))
                .ToString("hh:mmtt", new DateTimeFormatInfo { AMDesignator = "am", PMDesignator = "pm"});
        }

        /// <summary>
        /// Gets the seconds from the string time formatted as "hh:mmtt".
        /// </summary>
        /// <param name="formattedTime">The time in "hh:mmtt" format.</param>
        /// <returns>Integer value representing the time that has elapsed since midnight.</returns>
        public static int ConvertFormattedTimeToSeconds(string formattedTime)
        {
            DateTime.TryParseExact(formattedTime, "hh:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime effectiveDate);
            return Convert.ToInt32(effectiveDate.TimeOfDay.TotalSeconds);
        }

        public static int GetIntersectingTotalTime(TimeRange firstDaypart, TimeRange secondDaypart)
        {
            _AdjustTimeRange(firstDaypart);
            _AdjustTimeRange(secondDaypart);

            var ranges = GetIntersectingTimeRanges(firstDaypart, secondDaypart);

            return ranges.Sum(GetTotalTimeInclusive);
        }

        private static void _AdjustTimeRange(TimeRange range)
        {
            range.StartTime = _AdjustTime(range.StartTime);
            range.EndTime = _AdjustTime(range.EndTime);
        }

        /// <summary>
        /// Adjust time so that it becomes a value between 0 and OneDayInSeconds - 1 = 86399
        /// </summary>
        private static int _AdjustTime(int time)
        {
            if (time >= 0 && time <= BroadcastConstants.OneDayInSeconds - 1)
                return time;

            var absoluteValue = Math.Abs(time);

            if (time > 0)
            {
                var multiplierToAdjust = absoluteValue / BroadcastConstants.OneDayInSeconds;
                var result = time - (multiplierToAdjust * BroadcastConstants.OneDayInSeconds);
                return result;
            }
            else
            {
                if (absoluteValue % BroadcastConstants.OneDayInSeconds == 0)
                    return 0;

                var multiplierToAdjust = absoluteValue / BroadcastConstants.OneDayInSeconds + 1;
                var result = time + (multiplierToAdjust * BroadcastConstants.OneDayInSeconds);
                return result;
            }
        }

        public static List<TimeRange> GetIntersectingTimeRanges(TimeRange firstDaypart, TimeRange secondDaypart)
        {
            var firstDaypartTimeRange = new TimeRange();
            var secondDaypartTimeRange = new TimeRange();

            // let`s make the first daypart start time always start first. This simplifies the logic
            if (firstDaypart.StartTime < secondDaypart.StartTime)
            {
                firstDaypartTimeRange.StartTime = firstDaypart.StartTime;
                firstDaypartTimeRange.EndTime = firstDaypart.EndTime;
                secondDaypartTimeRange.StartTime = secondDaypart.StartTime;
                secondDaypartTimeRange.EndTime = secondDaypart.EndTime;
            }
            else
            {
                firstDaypartTimeRange.StartTime = secondDaypart.StartTime;
                firstDaypartTimeRange.EndTime = secondDaypart.EndTime;
                secondDaypartTimeRange.StartTime = firstDaypart.StartTime;
                secondDaypartTimeRange.EndTime = firstDaypart.EndTime;
            }

            return _GetIntersectingTimeRanges_WhenFirstDaypartGoesFirst(firstDaypartTimeRange, secondDaypartTimeRange);
        }
        
        private static List<TimeRange> _GetIntersectingTimeRanges_WhenFirstDaypartGoesFirst(
            TimeRange firstDaypart,
            TimeRange secondDaypart)
        {
            // at this point we know firstDaypart.StartTime starts earlier than secondDaypart.StartTime

            var result = new List<TimeRange>();
            var firstDaypartHasOvernight = HasOvernight(firstDaypart);
            var secondDaypartHasOvernight = HasOvernight(secondDaypart);

            // no overnights
            if (!firstDaypartHasOvernight && !secondDaypartHasOvernight)
            {
                if (secondDaypart.StartTime <= firstDaypart.EndTime)
                {
                    result.Add(new TimeRange
                    {
                        StartTime = secondDaypart.StartTime,
                        EndTime = Math.Min(firstDaypart.EndTime, secondDaypart.EndTime)
                    });
                }
            }

            // first daypart has overnight
            if (firstDaypartHasOvernight && !secondDaypartHasOvernight)
            {
                result.Add(new TimeRange
                {
                    StartTime = secondDaypart.StartTime,
                    EndTime = secondDaypart.EndTime
                });
            }

            // second daypart has overnight
            if (!firstDaypartHasOvernight && secondDaypartHasOvernight)
            {
                // secondDaypart intersects with right and left parts of firstDaypart
                if (secondDaypart.StartTime <= firstDaypart.EndTime &&
                    secondDaypart.EndTime >= firstDaypart.StartTime)
                {
                    // right part
                    result.Add(new TimeRange
                    {
                        StartTime = secondDaypart.StartTime,
                        EndTime = firstDaypart.EndTime
                    });

                    // left part
                    result.Add(new TimeRange
                    {
                        StartTime = firstDaypart.StartTime,
                        EndTime = Math.Min(firstDaypart.EndTime, secondDaypart.EndTime)
                    });
                }
                // secondDaypart intersects only with the right part of firstDaypart
                else if (secondDaypart.StartTime <= firstDaypart.EndTime)
                {
                    result.Add(new TimeRange
                    {
                        StartTime = secondDaypart.StartTime,
                        EndTime = firstDaypart.EndTime
                    });
                }
                // secondDaypart intersects only with the left part of firstDaypart
                else if (secondDaypart.EndTime >= firstDaypart.StartTime)
                {
                    result.Add(new TimeRange
                    {
                        StartTime = firstDaypart.StartTime,
                        EndTime = Math.Min(firstDaypart.EndTime, secondDaypart.EndTime)
                    });
                }
            }

            // both dayparts have overnight
            if (firstDaypartHasOvernight && secondDaypartHasOvernight)
            {
                result.Add(new TimeRange
                {
                    StartTime = secondDaypart.StartTime,
                    EndTime = Math.Min(firstDaypart.EndTime, secondDaypart.EndTime)
                });
            }

            return result;
        }

        public static bool HasOvernight(TimeRange timeRange)
        {
            return timeRange.EndTime < timeRange.StartTime;
        }

        public static int GetTotalTimeInclusive(TimeRange timeRange)
        {
            var result = -1;

            if (timeRange.StartTime <= timeRange.EndTime)
            {
                result = timeRange.EndTime - timeRange.StartTime;
            }
            else
            {
                result = BroadcastConstants.OneDayInSeconds - timeRange.StartTime + timeRange.EndTime;
            }

            // to make the count inclusive
            return result + 1;
        }
    }
}