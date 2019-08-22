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
        private static readonly Dictionary<Type,string> _RegisteredTypesAndProperties = new Dictionary<Type, string>
        {
            {typeof(DaypartCodeDefaultDto), nameof(DaypartCodeDefaultDto.DefaultEndTimeSeconds)},
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
    }
}