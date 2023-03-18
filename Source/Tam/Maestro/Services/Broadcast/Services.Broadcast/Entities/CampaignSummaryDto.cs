using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// An aggregation job.
    /// </summary>
    public class CampaignSummaryDto
    {
        /// <summary>
        /// The processing status.
        /// </summary>
        public CampaignAggregationProcessingStatusEnum ProcessingStatus { get; set; }

        /// <summary>
        /// The processing error message.
        /// Contains the error message if the ProcessingStatus is Error.
        /// </summary>
        public string ProcessingErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the campaign identifier.
        /// </summary>
        public int CampaignId { get; set; }
        
        /// <summary>
        /// Marks when the job was queued.
        /// </summary>
        public DateTime QueuedAt { get; set; }

        /// <summary>
        /// The queued by.
        /// </summary>
        public string QueuedBy { get; set; }

        /// <summary>
        /// The flight start date.
        /// </summary>
        public DateTime? FlightStartDate { get; set; }

        /// <summary>
        /// The flight end date.
        /// </summary>
        public DateTime? FlightEndDate { get; set; }

        /// <summary>
        /// The flight hiatus day count.
        /// </summary>
        public int? FlightHiatusDays { get; set; }

        /// <summary>
        /// The flight active day count.
        /// </summary>
        public int? FlightActiveDays { get; set; }

        /// <summary>
        /// The budget.
        /// </summary>
        public decimal? Budget { get; set; }

        /// <summary>
        /// The household CPM.
        /// </summary>
        public decimal? HHCPM { get; set; }

        /// <summary>
        /// The household impressions.
        /// </summary>
        public double? HHImpressions { get; set; }

        /// <summary>
        /// The household rating points.
        /// </summary>
        public double? HHRatingPoints { get; set; }

        /// <summary>
        /// The household adu impressions.
        /// </summary>
        /// <value>
        /// The hh adu impressions.
        /// </value>
        public double? HHAduImpressions { get; set; }

        /// <summary>
        /// The campaign status.
        /// </summary>
        public PlanStatusEnum? CampaignStatus { get; set; }

        /// <summary>
        /// The count of plans with status Working
        /// </summary>
        public int? PlanStatusCountWorking { get; set; }

        /// <summary>
        /// The count of plans with status Reserved
        /// </summary>
        public int? PlanStatusCountReserved { get; set; }

        /// <summary>
        /// The count of plans with status Client Approval
        /// </summary>
        public int? PlanStatusCountClientApproval { get; set; }

        /// <summary>
        /// The count of plans with status Contracted
        /// </summary>
        public int? PlanStatusCountContracted { get; set; }

        /// <summary>
        /// The count of plans with status Live
        /// </summary>
        public int? PlanStatusCountLive { get; set; }

        /// <summary>
        /// The count of plans with status Complete
        /// </summary>
        public int? PlanStatusCountComplete { get; set; }

        /// <summary>
        /// The count of plans with status Complete
        /// </summary>
        public int? PlanStatusCountScenario { get; set; }

        /// <summary>
        /// The count of plans with status Complete
        /// </summary>
        public int? PlanStatusCountCanceled { get; set; }

        /// <summary>
        /// The count of plans with status Complete
        /// </summary>
        public int? PlanStatusCountRejected { get; set; }

        /// <summary>
        /// The last time the summary components were modified.
        /// </summary>
        public DateTime? ComponentsModified { get; set; }

        /// <summary>
        /// The last time the summary was aggregated.
        /// </summary>
        public DateTime? LastAggregated { get; set; }
    }
}