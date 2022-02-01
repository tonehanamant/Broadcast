using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanDaypartGoalDto
    {
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public List<int> FlightDays { get; set; }
        public List<DateTime> FlightHiatusDays { get; set; } = new List<DateTime>();
        public int? TotalHiatusDays { get { return FlightHiatusDays.Count; } }
        public int? TotalActiveDays { get { return WeeklyBreakdownTotals.TotalActiveDays; } }
        public decimal? Budget { get; set; }
        public double? TargetImpressions { get; set; }
        public decimal? TargetCPM { get; set; }
        public double? TargetRatingPoints { get; set; }
        public decimal? TargetCPP { get; set; }
        public double TargetUniverse { get; set; }
        public double HHImpressions { get; set; }
        public decimal HHCPM { get; set; }
        public double HHRatingPoints { get; set; }
        public decimal HHCPP { get; set; }
        public double HHUniverse { get; set; }
        public PlanCurrenciesEnum Currency { get; set; }
        public double? CoverageGoalPercent { get; set; }
        public PlanGoalBreakdownTypeEnum GoalBreakdownType { get; set; }
        public bool IsAduEnabled { get; set; }
        public double ImpressionsPerUnit { get; set; }
        public List<WeeklyBreakdownWeek> WeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public List<WeeklyBreakdownWeek> RawWeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public WeeklyBreakdownTotals WeeklyBreakdownTotals { get; set; } = new WeeklyBreakdownTotals();
        public double AvailableMarketsSovTotal { get; set; }
        public int? BlackoutMarketCount { get; set; }
        public double? BlackoutMarketTotalUsCoveragePercent { get; set; }
        public int? AvailableMarketsWithSovCount { get; set; }
        public List<PlanBlackoutMarketDto> BlackoutMarkets { get; set; } = new List<PlanBlackoutMarketDto>();
        public List<PlanAvailableMarketDto> AvailableMarkets { get; set; } = new List<PlanAvailableMarketDto>();
    }
}
