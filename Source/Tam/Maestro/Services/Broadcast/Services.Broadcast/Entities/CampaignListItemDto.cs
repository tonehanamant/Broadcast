﻿using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Dto for Campaign data.
    /// </summary>
    public class CampaignListItemDto
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

        public AdvertiserDto Advertiser { get; set; }

        public AgencyDto Agency { get; set; }

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
        public string UnifiedId { get; set; }
        public int? MaxFluidityPercent { get; set; }
        public DateTime? UnifiedCampaignLastSentAt { get; set; }
        public DateTime? UnifiedCampaignLastReceivedAt { get; set; }

    }
}
