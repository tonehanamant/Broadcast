using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class WeeklyBreakdownResponseDto
    {
        public List<WeeklyBreakdownWeek> Weeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public int TotalActiveDays { get; set; }
        public double TotalShareOfVoice { get; set; }
    }
}
