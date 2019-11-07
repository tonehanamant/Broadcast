using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanHistoryDto
    {
        /// <summary>
        /// Gets or sets the version identifier.
        /// </summary>
        /// <value>
        /// The version identifier.
        /// </value>
        public int VersionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the version.
        /// </summary>
        /// <value>
        /// The name of the version.
        /// </value>
        public string VersionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is draft.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is draft; otherwise, <c>false</c>.
        /// </value>
        public bool IsDraft { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public PlanStatusEnum Status { get; set; }

        /// <summary>
        /// Gets or sets the flight start date.
        /// </summary>
        /// <value>
        /// The flight start date.
        /// </value>
        public DateTime FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>
        /// The flight end date.
        /// </value>
        public DateTime FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the target audience identifier.
        /// </summary>
        /// <value>
        /// The target audience identifier.
        /// </value>
        public int TargetAudienceId { get; set; }

        /// <summary>
        /// Gets or sets the total dayparts.
        /// </summary>
        /// <value>
        /// The total dayparts.
        /// </value>
        public int TotalDayparts { get; set; }
        /// <summary>
        /// Gets or sets the budget.
        /// </summary>
        /// <value>
        /// The budget.
        /// </value>
        public decimal? Budget { get; set; }

        /// <summary>
        /// Gets or sets the delivery.
        /// </summary>
        /// <value>
        /// The delivery.
        /// </value>
        public double? DeliveryImpressions { get; set; }
        /// <summary>
        /// Gets or sets the CPM.
        /// </summary>
        /// <value>
        /// The CPM.
        /// </value>
        public decimal? CPM { get; set; }

        /// <summary>
        /// Gets or sets the modified by.
        /// </summary>
        /// <value>
        /// The modified by.
        /// </value>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the modified date.
        /// </summary>
        /// <value>
        /// The modified date.
        /// </value>
        public DateTime? ModifiedDate { get; set; }
    }
}
