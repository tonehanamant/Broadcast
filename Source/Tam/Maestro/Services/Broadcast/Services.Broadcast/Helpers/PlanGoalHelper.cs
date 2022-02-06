using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Enums;
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
        private const int nonQualityCpmGoal = 1;

        /// <summary>
        /// Takes a list of dayparts and calculates weights for those dayparts that does not have weight set
        /// Remaining weight is distributed evenly
        /// When weight can not be split evenly we split it into equal pieces and add what`s left to the first daypart that takes part in the distribution
        /// </summary>
        public static List<StandardDaypartWeightingGoal> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts)
        {
            var weightingGoalPercentByStandardDaypartIdDictionary = new Dictionary<string, double>();

            var daypartsWithWeighting = dayparts.Where(x => x.WeightingGoalPercent.HasValue).ToList();
            var daypartsWithoutWeighting = dayparts.Where(x => !x.WeightingGoalPercent.HasValue).ToList();

            if (daypartsWithoutWeighting.Any())
            {
                var undistributedWeighing = 100 - daypartsWithWeighting.Sum(x => x.WeightingGoalPercent.Value);
                var undistributedWeighingPerDaypart = Math.Floor(undistributedWeighing / daypartsWithoutWeighting.Count);

                var remainingWeighing = undistributedWeighing - (undistributedWeighingPerDaypart * daypartsWithoutWeighting.Count);
                var firstDaypartWeighing = undistributedWeighingPerDaypart + remainingWeighing;
                var firstDaypart = daypartsWithoutWeighting.TakeOut(0);

                weightingGoalPercentByStandardDaypartIdDictionary[firstDaypart.DaypartUniquekey] = firstDaypartWeighing;
                daypartsWithoutWeighting.ForEach(x => weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartUniquekey] = undistributedWeighingPerDaypart);
            }

            daypartsWithWeighting.ForEach(x => weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartUniquekey] = x.WeightingGoalPercent.Value);

            // to keep the original order
            return dayparts
                .Select(x => new StandardDaypartWeightingGoal
                {
                    StandardDaypartId = x.DaypartCodeId,
                    DaypartOrganizationId = x.DaypartOrganizationId,
                    CustomName = x.CustomName,
                    DaypartOrganizationName = x.DaypartOrganizationName,
                    WeightingGoalPercent = weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartUniquekey]
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
                    CustomName=b.CustomName,
                    DaypartOrganizationName=b.DaypartOrganizationName,
                    DaypartOrganizationId=b.DaypartOrganizationId,                    
                    Weighting = GeneralMath.ConvertPercentageToFraction(a.Weight.Value) * GeneralMath.ConvertPercentageToFraction(b.WeightingGoalPercent)
                }).ToList();
            return allSpotLengthIdAndStandardDaypartIdCombinations;
        }

        /// <summary>Plans the calculate budget by mode to adjust impressions for floor and efficiency mode.</summary>
        /// <param name="weeklyBudget">The weekly budget.</param>
        /// <param name="impressionGoal">The impression goal.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="budgetCpmLever">The budget CPM lever.</param>
        public static PlanBudgetResponseByMode PlanCalculateBudgetByMode(decimal weeklyBudget, double impressionGoal, SpotAllocationModelMode spotAllocationModelMode,
            BudgetCpmLeverEnum budgetCpmLever)
        {
            var result = new PlanBudgetResponseByMode();
            var cpmGoal = ProposalMath.CalculateCpm(weeklyBudget, impressionGoal);

            if(spotAllocationModelMode == SpotAllocationModelMode.Quality)
            {
                result.CpmGoal = cpmGoal;
                result.ImpressionGoal = impressionGoal;
            }
            else
            {
                result.CpmGoal = nonQualityCpmGoal;
                result.ImpressionGoal = budgetCpmLever == BudgetCpmLeverEnum.Budget ? (double)cpmGoal * impressionGoal : impressionGoal;
            }
            return result;
        }

        public class PlanBudgetResponseByMode
        {
            public double ImpressionGoal { get; set; }
            public decimal CpmGoal { get; set; }
        }
    }
}