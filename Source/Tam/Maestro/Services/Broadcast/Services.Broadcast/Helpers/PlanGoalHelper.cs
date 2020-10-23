using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Helpers
{
    /// <summary>
    /// Helper class for resolving the Plan Goals calculations.
    /// </summary>
    public static class PlanGoalHelper
    {
        /// <summary>
        /// Takes a list of dayparts and calculates weights for those dayparts that does not have weight set
        /// Remaining weight is distributed evenly
        /// When weight can not be split evenly we split it into equal pieces and add what`s left to the first daypart that takes part in the distribution
        /// </summary>
        public static List<StandardDaypartWeightingGoal> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts)
        {
            var weightingGoalPercentByStandardDaypartIdDictionary = new Dictionary<int, double>();

            var daypartsWithWeighting = dayparts.Where(x => x.WeightingGoalPercent.HasValue).ToList();
            var daypartsWithoutWeighting = dayparts.Where(x => !x.WeightingGoalPercent.HasValue).ToList();

            if (daypartsWithoutWeighting.Any())
            {
                var undistributedWeighing = 100 - daypartsWithWeighting.Sum(x => x.WeightingGoalPercent.Value);
                var undistributedWeighingPerDaypart = Math.Floor(undistributedWeighing / daypartsWithoutWeighting.Count);

                var remainingWeighing = undistributedWeighing - (undistributedWeighingPerDaypart * daypartsWithoutWeighting.Count);
                var firstDaypartWeighing = undistributedWeighingPerDaypart + remainingWeighing;
                var firstDaypart = daypartsWithoutWeighting.TakeOut(0);

                weightingGoalPercentByStandardDaypartIdDictionary[firstDaypart.DaypartCodeId] = firstDaypartWeighing;
                daypartsWithoutWeighting.ForEach(x => weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId] = undistributedWeighingPerDaypart);
            }

            daypartsWithWeighting.ForEach(x => weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId] = x.WeightingGoalPercent.Value);

            // to keep the original order
            return dayparts
                .Select(x => new StandardDaypartWeightingGoal
                {
                    StandardDaypartId = x.DaypartCodeId,
                    WeightingGoalPercent = weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId]
                })
                .ToList();
        }

        /// <summary>
        /// Gets all the weekly breakdown combination of creative lengths and dayparts with the combined weight
        /// </summary>
        /// <param name="creativeLengths">List of creative lengths with the weight assigned</param>
        /// <param name="dayparts">List of dayparts with the weight assigned or not</param>
        /// <returns>List of WeeklyBreakdownCombination objects</returns>
        public static List<WeeklyBreakdownCombination> GetWeeklyBreakdownCombinations(List<CreativeLength> creativeLengths
            , List<PlanDaypartDto> dayparts)
        {
            var standardDaypardWeightingGoals = GetStandardDaypardWeightingGoals(dayparts);
            var allSpotLengthIdAndStandardDaypartIdCombinations = creativeLengths.SelectMany(x => standardDaypardWeightingGoals, (a, b) =>
                new WeeklyBreakdownCombination
                {
                    SpotLengthId = a.SpotLengthId,
                    DaypartCodeId = b.StandardDaypartId,
                    Weighting = GeneralMath.ConvertPercentageToFraction(a.Weight.Value) * GeneralMath.ConvertPercentageToFraction(b.WeightingGoalPercent)
                }).ToList();
            return allSpotLengthIdAndStandardDaypartIdCombinations;
        }
    }
}