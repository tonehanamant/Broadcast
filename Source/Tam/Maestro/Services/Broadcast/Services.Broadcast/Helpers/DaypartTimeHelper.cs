using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;

namespace Services.Broadcast.Helpers
{
    /// <summary>
    /// Helper operations for daypart times.
    /// </summary>
    public static class DaypartTimeHelper
    {
        /// <summary>
        /// Transforms the given candidates from their stored state for display.
        /// </summary>
        /// <param name="candidates">The candidates.</param>
        public static void AddOneSecondToEndTime<T>(List<T> candidates)
        {
            var incomingType = typeof(T);
            if (incomingType == typeof(DaypartCodeDefaultDto))
            {
                var items = candidates as List<DaypartCodeDefaultDto>;
                items.ForEach(c => c.DefaultEndTimeSeconds++);
                return;
            }

            if (incomingType == typeof(PlanDaypartDto))
            {
                var items = candidates as List<PlanDaypartDto>;
                items.ForEach(c => c.EndTimeSeconds++);
                return;
            }

            throw new InvalidOperationException("Invalid type provided in list.");
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
            if (incomingType == typeof(DaypartCodeDefaultDto))
            {
                var items = candidates as List<DaypartCodeDefaultDto>;
                items.ForEach(c => c.DefaultEndTimeSeconds--);
                return;
            }

            if (incomingType == typeof(PlanDaypartDto))
            {
                var items = candidates as List<PlanDaypartDto>;
                items.ForEach(c => c.EndTimeSeconds--);
                return;
            }

            throw new InvalidOperationException("Invalid type provided in list.");
        }
    }
}