using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class WeeklyBreakdownResponseDto
    {
        public List<WeeklyBreakdownWeek> Weeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public List<WeeklyBreakdownWeek> RawWeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public int TotalActiveDays { get; set; }
        public double TotalShareOfVoice { get; set; }
        public double TotalImpressions { get; set; }
        public double TotalRatingPoints { get; set; }
        public double TotalImpressionsPercentage { get; set; }
        public decimal TotalBudget { get; set; }
        public double TotalUnits { get; set; }
        public double TotalAduImpressions { get; set; }
    }
}
