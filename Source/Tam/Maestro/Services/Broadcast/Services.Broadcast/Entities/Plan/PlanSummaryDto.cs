using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Summary data for a plan.
    /// </summary>
    public class PlanSummaryDto
    {
        /// <summary>
        /// Gets or sets the processing status.
        /// </summary>
        /// <value>
        /// The processing status.
        /// </value>
        public PlanAggregationProcessingStatusEnum ProcessingStatus { get; set; }

        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>
        /// The plan identifier.
        /// </value>
        public int PlanId { get; set; }

        /// <summary>
        /// The total number of Hiatus days in the flight.
        /// </summary>
        /// <value>
        /// The total number of Hiatus days in the flight.
        /// </value>
        public int? TotalHiatusDays { get; set; }

        /// <summary>
        /// The total number of Active days in the flight.
        /// </summary>
        /// <value>
        /// The total number of Active days in the flight.
        /// </value>
        public int? TotalActiveDays { get; set; }

        /// <summary>
        /// Gets or sets the available market count.
        /// </summary>
        /// <value>
        /// The available market count.
        /// </value>
        public int? AvailableMarketCount { get; set; }

        /// <summary>
        /// Gets or sets the available market total us coverage percent.
        /// </summary>
        /// <value>
        /// The available market total us coverage percent.
        /// </value>
        public double? AvailableMarketTotalUsCoveragePercent { get; set; }

        /// <summary>
        /// Gets or sets the blackout market count.
        /// </summary>
        /// <value>
        /// The blackout market count.
        /// </value>
        public int? BlackoutMarketCount { get; set; }

        /// <summary>
        /// Gets or sets the blackout market total us coverage percent.
        /// </summary>
        /// <value>
        /// The blackout market total us coverage percent.
        /// </value>
        public double? BlackoutMarketTotalUsCoveragePercent { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the name of the audience.
        /// </summary>
        /// <value>
        /// The name of the audience.
        /// </value>
        public string AudienceName { get; set; }

        /// <summary>
        /// The quarters covered by the plan's flight.
        /// </summary>
        /// <value>
        /// The plan summary quarters.
        /// </value>
        public List<PlanSummaryQuarterDto> PlanSummaryQuarters { get; set; } = new List<PlanSummaryQuarterDto>();
    }
}