using Common.Services.ApplicationServices;
using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IWeeklyBreakdownEngine : IApplicationService
    {
        List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);
        Dictionary<int, int> GetWeekNumberByMediaWeekDictionary(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);
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

                    return new WeeklyBreakdownByWeek
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        Impressions = allItems.Sum(x => x.WeeklyImpressions),
                        Budget = allItems.Sum(x => x.WeeklyBudget)
                    };
                })
                .ToList();
        }
    }
}
