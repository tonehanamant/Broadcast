using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class WeeklyBreakdownRequest
    {
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<int> FlightDays { get; set; }
        public List<DateTime> FlightHiatusDays { get; set; }
        public PlanGoalBreakdownTypeEnum DeliveryType { get; set; }
        public double TotalImpressions { get; set; }        
        public double TotalRatings { get; set; }
        public decimal TotalBudget { get; set; }
        public WeeklyBreakdownCalculationFrom WeeklyBreakdownCalculationFrom { get; set; }
        public List<WeeklyBreakdownWeek> Weeks { get; set; } = new List<WeeklyBreakdownWeek>();
        //add primary audience and calculate also for  HH
    }
}
