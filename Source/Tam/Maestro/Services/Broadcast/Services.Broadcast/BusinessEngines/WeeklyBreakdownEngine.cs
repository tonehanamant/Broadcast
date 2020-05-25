using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IWeeklyBreakdownEngine : IApplicationService
    {
        List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);
        List<WeeklyBreakdownByWeekBySpotLength> GroupWeeklyBreakdownByWeekBySpotLength(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);
        Dictionary<int, int> GetWeekNumberByMediaWeekDictionary(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);

        /// <summary>
        /// Gets all the weekly breakdown combination of creative lengths and dayparts with the combined weight
        /// </summary>
        /// <param name="creativeLengths">List of creative lengths with the weight assigned</param>
        /// <param name="dayparts">List of dayparts with the weight assigned or not</param>
        /// <returns>List of WeeklyBreakdownCombination objects</returns>
        List<WeeklyBreakdownCombination> GetWeeklyBreakdownCombinations(List<CreativeLength> creativeLengths
            , List<PlanDaypartDto> dayparts);

        /// <summary>
        /// Takes a list of dayparts and calculates weights for those dayparts that does not have weight set
        /// Remaining weight is distributed evenly
        /// When weight can not be split evenly we split it into equal pieces and add what`s left to the first daypart that takes part in the distribution
        /// </summary>
        List<(int StadardDaypartId, double WeightingGoalPercent)> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts);
    }

    public class WeeklyBreakdownEngine : IWeeklyBreakdownEngine
    {
        public Dictionary<int, int> GetWeekNumberByMediaWeekDictionary(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            var weeklyBreakdownByWeek = GroupWeeklyBreakdownByWeek(weeklyBreakdown);

            return weeklyBreakdownByWeek
                .OrderBy(x => x.MediaWeekId)
                .Select((item, index) => new { item.MediaWeekId, weekNumber = index + 1 })
                .ToDictionary(x => x.MediaWeekId, x => x.weekNumber);
        }

        public List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            return weeklyBreakdown
                .GroupBy(x => x.MediaWeekId)
                .Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    var aduImpressions = allItems.Sum(x => x.AduImpressions);

                    return new WeeklyBreakdownByWeek
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        Impressions = allItems.Sum(x => x.WeeklyImpressions),
                        Budget = allItems.Sum(x => x.WeeklyBudget),
                        Adu = (int)(aduImpressions / BroadcastConstants.ImpressionsPerUnit)
                    };
                })
                .ToList();
        }

        public List<WeeklyBreakdownByWeekBySpotLength> GroupWeeklyBreakdownByWeekBySpotLength(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            return weeklyBreakdown
                .GroupBy(x => new { x.MediaWeekId, x.SpotLengthId } )
                .Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    var aduImpressions = allItems.Sum(x => x.AduImpressions);

                    return new WeeklyBreakdownByWeekBySpotLength
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        SpotLengthId = first.SpotLengthId.Value,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        Impressions = allItems.Sum(x => x.WeeklyImpressions),
                        Budget = allItems.Sum(x => x.WeeklyBudget),
                        Adu = (int)(aduImpressions / BroadcastConstants.ImpressionsPerUnit)
                    };
                })
                .ToList();
        }

        /// <inheritdoc />
        public List<WeeklyBreakdownCombination> GetWeeklyBreakdownCombinations(List<CreativeLength> creativeLengths
            , List<PlanDaypartDto> dayparts)
        {
            var standardDaypardWeightingGoals = GetStandardDaypardWeightingGoals(dayparts);
            var allSpotLengthIdAndStandardDaypartIdCombinations = creativeLengths.SelectMany(x => standardDaypardWeightingGoals, (a, b) => 
            new WeeklyBreakdownCombination
            {
                SpotLengthId = a.SpotLengthId,
                DaypartCodeId = b.StadardDaypartId,
                Weighting = GeneralMath.ConvertPercentageToFraction(a.Weight.Value) *  GeneralMath.ConvertPercentageToFraction(b.WeightingGoalPercent)
            }).ToList();
            return allSpotLengthIdAndStandardDaypartIdCombinations;
        }

        /// <inheritdoc />
        public List<(int StadardDaypartId, double WeightingGoalPercent)> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts)
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
                .Select(x => (
                    StadardDaypartId: x.DaypartCodeId,
                    WeightingGoalPercent: weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId]))
                .ToList();
        }
    }
}
