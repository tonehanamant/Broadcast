using System;
using Services.Broadcast.BusinessEngines;

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
        public double WeeklyAdu { get; set; }
        public double AduImpressions { get; set; }
        public int? SpotLengthId { get; set; }
        public int? SpotLengthDuration { get; set; }
        public int? DaypartCodeId { get; set; }
        public int PlanDaypartId { get; set; }
        public double? PercentageOfWeek { get; set; }
        public bool IsUpdated { get; set; }
        public double UnitImpressions { get; set; }
        public double WeeklyUnits { get; set; }
        // assumes that the WeeklyImpressions have been formatted for (000)
        public decimal WeeklyCpm => ProposalMath.CalculateCpm(WeeklyBudget, (WeeklyImpressions * 1000));
        public bool IsLocked { get; set; } = false;
        public int? DaypartOrganizationId { get; set; }
        public string CustomName { get; set; }
        public string DaypartOrganizationName { get; set; }
        public string DaypartUniquekey { get { return $"{DaypartCodeId}|{DaypartOrganizationId}|{CustomName?.ToLower()}"; } }
    }

    public class WeeklyBreakdownTotals
    {
        public decimal TotalBudget { get; set; }
        public double TotalImpressions { get; set; }
        public double TotalRatingPoints { get; set; }
        public double TotalImpressionsPercentage { get; set; }
        public int TotalActiveDays { get; set; }
        public double TotalUnits { get; set; }
        public double TotalAduImpressions { get; set; }
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
        public double Adu { get; set; }
        public double Units { get; set; }
        public bool IsLocked { get; set; }
        public int? SpotLengthId { get; set; }
        public int? DaypartCodeId { get; set; }
        public string CustomName { get; set; }
        public string DaypartOrganizationName { get; set; }
    }

    public class WeeklyBreakdownByWeekBySpotLength
    {
        public int WeekNumber { get; set; }
        public int MediaWeekId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfActiveDays { get; set; }
        public string ActiveDays { get; set; }
        public double Impressions { get; set; }
        public double RatingPoints { get; set; }
        public decimal Budget { get; set; }
        public int SpotLengthId { get; set; }
        public double Adu { get; set; }
        public double Units { get; set; }
        public bool IsLocked { get; set; }
    }

    public class WeeklyBreakdownByWeekByDaypart
    {
        public int WeekNumber { get; set; }
        public int MediaWeekId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfActiveDays { get; set; }
        public string ActiveDays { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
        public int DaypartCodeId { get; set; }
        public double Adu { get; set; }
        public double Units { get; set; }
        public bool IsLocked { get; set; }
        public int? DaypartOrganizationId { get; set; }
        public string CustomName { get; set; }
        public string DaypartOrganizationName { get; set; }
        public int PlanDaypartId { get; set; }
        public string DaypartUniquekey { get { return $"{DaypartCodeId}|{DaypartOrganizationId}|{CustomName?.ToLower()}"; } }
    }

    public class WeeklyBreakdownByStandardDaypart
    {
        public double Impressions { get; set; }
        public double AduImpressions { get; set; }
        public decimal Budget { get; set; }
        public int StandardDaypartId { get; set; }
    }
}
