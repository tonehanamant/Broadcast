﻿using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Default data for daypart defaults.
    /// </summary>
    public class DaypartDefaultFullDto : DaypartDefaultDto
    {
        /// <summary>
        /// Gets or sets the daypart type.
        /// </summary>
        /// <value>
        /// The daypart type.
        /// </value>
        public DaypartTypeEnum DaypartType { get; set; }

        /// <summary>
        /// Gets or sets the default start time seconds from midnight ET.
        /// </summary>
        /// <value>
        /// The default start time seconds from midnight ET.
        /// </value>
        public int DefaultStartTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the default end time seconds from midnight ET.
        /// </summary>
        /// <value>
        /// The default end time seconds from midnight ET.
        /// </value>
        public int DefaultEndTimeSeconds { get; set; }
    }
}