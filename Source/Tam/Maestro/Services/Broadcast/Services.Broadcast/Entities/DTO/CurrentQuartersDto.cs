using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    /// <summary>
    /// Describes the list of quarters from "today".
    /// </summary>
    public class CurrentQuartersDto
    {
        /// <summary>
        /// The first start date oriented from "today".
        /// </summary>
        public DateTime FirstStartDate { get; set; }

        /// <summary>
        /// The list of quarters oriented from "today".
        /// </summary>
        public List<QuarterDetailDto> Quarters { get; set; } = new List<QuarterDetailDto>();
    }
}
