using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Weekly breakdown request.
    /// </summary>
    public class WeeklyBreakdownRequest
    {
        /// <summary>
        /// Gets or sets flight start date.
        /// </summary>
        public DateTime FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets flight end date.
        /// </summary>
        public DateTime FlightEndDate { get; set; }

        public List<int> FlightDays { get; set; }

        /// <summary>
        /// Gets or sets flight hiatus days.
        /// </summary>
        public List<DateTime> FlightHiatusDays { get; set; }

        /// <summary>
        /// Gets or sets delivery type.
        /// </summary>
        public PlanGoalBreakdownTypeEnum DeliveryType { get; set; }

        /// <summary>
        /// Gets or sets total impressions.
        /// </summary>
        public double TotalImpressions { get; set; }

        /// <summary>
        /// Gets or sets total ratings.
        /// </summary>
        public double TotalRatings { get; set; }

        /// <summary>
        /// Gets or sets total budget.
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// Gets or sets weekly breakdown calculation from.
        /// </summary>
        public WeeklyBreakdownCalculationFrom WeeklyBreakdownCalculationFrom { get; set; }

        /// <summary>
        /// Gets or sets Weeks.
        /// </summary>
        public List<WeeklyBreakdownWeek> Weeks { get; set; } = new List<WeeklyBreakdownWeek>();

        /// <summary>
        /// Gets or sets the Updated Week.
        /// </summary>
        public int UpdatedWeek { get; set; }

        public List<CreativeLength> CreativeLengths { get; set; }

        public List<PlanDaypartDto> Dayparts { get; set; }
    }
}
