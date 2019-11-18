using System.Collections.Generic;

namespace Services.Broadcast.Entities.PlanPricing
{
    public class PlanPricingInventoryProgramDto
    {
        public List<string> ProgramNames { get; set; }
        public PlanPricingMarketDto PlanPricingMarket { get; set; }
        public DisplayScheduleStation Station { get; set; }
        public int SpotLength { get; set; }
        public decimal Rate { get; set; }
        public double DeliveryMultiplier { get; set; }
        public double GuaranteedImpressions { get; set; }
        public double ProjectedImpressions { get; set; }
    }
}
