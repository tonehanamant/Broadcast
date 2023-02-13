using System;

namespace Services.Broadcast.Entities.SpotExceptions.Unposted
{
    /// <summary></summary>
    public class OutOfSpecUnpostedRequestDto
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
