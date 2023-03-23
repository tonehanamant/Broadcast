using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Default data for daypart defaults.
    /// </summary>
    public class StandardDaypartFullDto : StandardDaypartDto
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

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Monday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Tuesday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Wednesday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Thursday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Friday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Saturday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Sunday { get; set; }
    }
}