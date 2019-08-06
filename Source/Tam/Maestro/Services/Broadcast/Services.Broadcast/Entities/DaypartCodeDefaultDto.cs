using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Default data for daypart codes.
    /// </summary>
    public class DaypartCodeDefaultDto : DaypartCodeDto
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