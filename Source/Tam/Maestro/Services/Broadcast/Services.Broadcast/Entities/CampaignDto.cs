using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Dto for Campaign data.
    /// </summary>
    public class CampaignDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the advertiser identifier.
        /// The integer id used by the Aab Traffic Api.
        /// </summary>
        public int? AdvertiserId { get; set; }

        /// <summary>
        /// Gets or sets the advertiser master identifier.
        /// The guid id used by the Aab Api.
        /// </summary>
        public Guid? AdvertiserMasterId { get; set; }

        /// <summary>
        /// Gets or sets the agency identifier.
        /// The integer id used by the Aab Traffic Api.
        /// </summary>
        public int? AgencyId { get;set; }

        /// <summary>
        /// Gets or sets the agency master identifier.
        /// The guid id used by the Aab Api.
        /// </summary>
        public Guid? AgencyMasterId { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the modified date.
        /// </summary>
        /// <value>
        /// The modified date.
        /// </value>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the modified by.
        /// </summary>
        /// <value>
        /// The modified by.
        /// </value>
        public string ModifiedBy { get; set; }

        public DateTime? FlightStartDate { get; set; }

        public DateTime? FlightEndDate { get; set; }

        public int? FlightHiatusDays { get; set; }

        public int? FlightActiveDays { get; set; }

        public bool? HasHiatus { get; set; }

        public decimal? Budget { get; set; }

        public decimal? HHCPM { get; set; }

        public double? HHImpressions { get; set; }

        public double? HHRatingPoints { get; set; }

        public bool HasPlans { get; set; }

        public List<PlanSummaryDto> Plans { get; set; }

        public PlanStatusEnum? CampaignStatus { get; set; }

        public List<PlansStatusCountDto> PlanStatuses { get; set; }
    }
}
