using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanDaypartUpdateResponseDto
    {
        public List<WeeklyBreakdownWeek> Weeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public List<WeeklyBreakdownWeek> RawWeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
    }
}
