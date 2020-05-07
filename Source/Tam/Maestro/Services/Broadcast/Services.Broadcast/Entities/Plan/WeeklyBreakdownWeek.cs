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
        public double WeeklyImpressions { get; set; }
        public double WeeklyImpressionsPercentage { get; set; }
        public double WeeklyRatings { get; set; }
        public decimal WeeklyBudget { get; set; }
        public int WeeklyAdu { get; set; }
        public int? SpotLengthId { get; set; }
        public int? DaypartCodeId { get; set; }
        public double? PercentageOfWeek { get; set; }
    }

    public class WeeklyBreakdownTotals
    {
        public decimal TotalBudget { get; set; }
        public double TotalImpressions { get; set; }
        public double TotalRatingPoints { get; set; }
        public double TotalImpressionsPercentage { get; set; }
        public int TotalActiveDays { get; set; }
    }

    public class WeeklyBreakdownByWeek
    {
        public int WeekNumber { get; set; }
        public int MediaWeekId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfActiveDays { get; set; }
        public string ActiveDays { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
    }
}
