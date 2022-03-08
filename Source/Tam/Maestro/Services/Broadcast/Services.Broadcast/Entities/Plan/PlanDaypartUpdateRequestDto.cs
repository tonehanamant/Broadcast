using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanDaypartUpdateRequestDto
    {
        public int PlanDaypartId { get; set; }
        public int DaypartTypeId { get; set; }
        public int DaypartCodeId { get; set; }
        public int? DaypartOrganizationId { get; set; }
        public string CustomName { get; set; }
        public List<WeeklyBreakdownWeek> Weeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public List<WeeklyBreakdownWeek> RawWeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public string DaypartUniquekey { get { return $"{DaypartCodeId}|{DaypartOrganizationId}|{CustomName?.ToLower()}"; } }
    }
}
