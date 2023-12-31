﻿using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Dto for saving a Campaign
    /// </summary>
    public class SaveCampaignDto
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
        /// Gets or sets the campaigns advertiser identifier.
        /// </summary>
        /// <value>
        /// The advertiser identifier.
        /// </value>
        public int? AdvertiserId { get; set; }

        public Guid? AdvertiserMasterId { get; set; }

        /// <summary>
        /// Gets or sets the campaigns agency identifier.
        /// </summary>
        /// <value>
        /// The agency identifier.
        /// </value>
        public int? AgencyId { get; set; }


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
        public string UnifiedId { get; set; }
        public int? MaxFluidityPercent { get; set; }
        public DateTime? UnifiedCampaignLastSentAt { get; set; }
        public DateTime? UnifiedCampaignLastReceivedAt { get; set; }
        /// <summary>
        /// Gets or sets the campaigns Account Executive.
        /// </summary>
        /// <value>
        /// The Account Executive.
        /// </value>
        public string AccountExecutive { get; set; }
        /// <summary>
        /// Gets or sets the campaigns Client Contact.
        /// </summary>
        /// <value>
        /// The Client Contact.
        /// </value>
        public string ClientContact { get; set; }
    }
}
