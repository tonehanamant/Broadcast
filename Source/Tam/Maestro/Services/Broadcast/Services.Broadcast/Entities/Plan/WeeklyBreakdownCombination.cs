using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Plan
{
    public class WeeklyBreakdownCombination
    {
        public int SpotLengthId { get; set; }
        public int DaypartCodeId { get; set; }
        public double Weighting { get; set; }
        public string DaypartOrganizationName { get; set; }
        public string CustomName { get; set; }
        public int? DaypartOrganizationId { get; set; }
    }
}
