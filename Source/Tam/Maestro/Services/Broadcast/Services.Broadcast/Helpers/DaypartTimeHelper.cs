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
            { typeof(StandardDaypartFullDto), nameof(StandardDaypartFullDto.DefaultEndTimeSeconds) },
            { typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds) },
             { typeof(PlanCustomDaypartDto), nameof(PlanCustomDaypartDto.EndTimeSeconds) },
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

            var propertyInfo = incomingType.GetProperty(_RegisteredTypesAndProperties[incomingType]);

            foreach (var candidate in candidates)
            {
                var value = (int)propertyInfo.GetValue(candidate);
                var valueToSet = AdjustBoundaryValue(value + 1);

                propertyInfo.SetValue(candidate, valueToSet);
            }
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

            var propertyInfo = incomingType.GetProperty(_RegisteredTypesAndProperties[incomingType]);

            foreach (var candidate in candidates)
            {
                var value = (int)propertyInfo.GetValue(candidate);
                var valueToSet = AdjustBoundaryValue(value - 1);

                propertyInfo.SetValue(candidate, valueToSet);
            }
        }

        public static int AdjustBoundaryValue(int value)
        {
            switch (value)
            {
                case BroadcastConstants.OneDayInSeconds:
                    return 0;

                case -1:
                    return BroadcastConstants.OneDayInSeconds - 1;

                default:
                    return value;
            }
        }

        /// <summary>
        /// Converts seconds to a string with the given format
        /// </summary>
        /// <param name="seconds">Seconds to format as time</param>
        /// <param name="format">Time format</param>
        /// <returns>Seconds converted to the time format</returns>
        public static string ConvertSecondsToFormattedTime(int seconds, string format)
        {
            var time = DateTime.Today.Add(TimeSpan.FromSeconds(seconds));
            var result = time.ToString(format, new DateTimeFormatInfo { AMDesignator = "am", PMDesignator = "pm" });

            return result;
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
            var ranges = GetIntersectingTimeRangesWithAdjustment(firstDaypart, secondDaypart);

            return ranges.Sum(GetTotalTimeInclusive);
        }

        public static List<TimeRange> GetIntersectingTimeRangesWithAdjustment(TimeRange firstDaypart, TimeRange secondDaypart)
        {
            _AdjustTimeRange(firstDaypart);
            _AdjustTimeRange(secondDaypart);

            return GetIntersectingTimeRanges(firstDaypart, secondDaypart);
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
            return GetTotalTimeInclusive(timeRange.StartTime, timeRange.EndTime);
        }

        public static int GetTotalTimeInclusive(int startTime, int endTime)
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