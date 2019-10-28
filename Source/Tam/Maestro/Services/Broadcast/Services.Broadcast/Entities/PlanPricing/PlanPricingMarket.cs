using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.PlanPricing
{
    public class PlanPricingMarket
    {
        public int MarketId { get; set; }
        public string MarketName { get; set; }
        public double MarketShareOfVoice { get; set; }
        public int MarketSegment { get; set; }
    }
}
