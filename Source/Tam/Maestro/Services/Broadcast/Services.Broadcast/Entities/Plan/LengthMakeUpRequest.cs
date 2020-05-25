using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class LengthMakeUpRequest
    {
        public List<CreativeLength> CreativeLengths { get; set; }
        public List<WeeklyBreakdownWeek> Weeks { get; set; }
        public double TotalImpressions { get; set; }
    }
}
