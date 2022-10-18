﻿using System;

/// <summary></summary>
namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    /// <summary></summary>
    public class SpotExceptionsRecommendedPlanAdvertisersRequestDto
    {
        /// <summary>
        /// Gets or sets the week start date.
        /// </summary>
        /// <value>The week start date.</value>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// Gets or sets the week end date.
        /// </summary>
        /// <value>The week end date.</value>
        public DateTime WeekEndDate { get; set; }
    }
}
