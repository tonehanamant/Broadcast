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

        /// <summary>
        /// Gets or sets the flight days.
        /// </summary>
        /// <value>
        /// The flight days.
        /// </value>
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
        /// Gets or sets the creative lengths.
        /// </summary>
        /// <value>
        /// The creative lengths.
        /// </value>
        public List<CreativeLength> CreativeLengths { get; set; }

        /// <summary>
        /// Gets or sets the dayparts.
        /// </summary>
        /// <value>
        /// The dayparts.
        /// </value>
        public List<PlanDaypartDto> Dayparts { get; set; }

        /// <summary>
        /// Gets or sets the impressions per unit.
        /// </summary>
        /// <value>
        /// The impressions per unit.
        /// </value>
        public double ImpressionsPerUnit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is equivalized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if equivalized; otherwise, <c>false</c>.
        /// </value>
        public bool? Equivalized { get; set; }

        /// <summary>
        /// True if the request is for an ADU Only Context.
        /// </summary>
        public bool IsAduOnly { get; set; } = false;
    }
}
