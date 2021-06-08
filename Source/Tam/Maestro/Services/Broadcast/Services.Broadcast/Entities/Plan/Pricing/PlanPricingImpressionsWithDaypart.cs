using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingImpressionsWithDaypart
    {
        public int StandardDaypartId { get; set; }
        public double ProjectedImpressions { get; set; }
        public double HouseholdProjectedImpressions { get; set; }
    }
}
