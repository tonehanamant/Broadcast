using System;

namespace Services.Broadcast.Entities.Plan
{
    public class WeeklyBreakdownWeek
    {
        public int WeekNumber { get; set; }
        public int MediaWeekId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfActiveDays { get; set; }
        public string ActiveDays { get; set; }
        public double Impressions { get; set; }
        public double ShareOfVoice { get; set; }
    }
}
