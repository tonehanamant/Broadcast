using System.Collections.Generic;

namespace Services.Broadcast.Entities.Buying
{
    public class PlanBuyingProgramDto
    {
        public int ManifestId { get; set; }
        public List<string> ProgramNames { get; set; }
        public PlanBuyingMarketDto PlanPricingMarket { get; set; }
        public DisplayScheduleStation Station { get; set; }
        public int SpotLength { get; set; }
        public decimal Rate { get; set; }
        public double DeliveryMultiplier { get; set; }
        public double GuaranteedImpressions { get; set; }
        public double ProjectedImpressions { get; set; }
    }
}
