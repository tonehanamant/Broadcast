using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Flight information for a plan.
    /// </summary>
    public class PlanFlightDto
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Notes { get; set; }

        public List<DateTime> HiatusDays { get; set; } = new List<DateTime>();
    }
}