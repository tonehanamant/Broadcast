using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingDefaults
    {
        public int UnitCaps { get; set; }
        public List<PlanPricingInventorySourceDto> InventorySourcePercentages { get; set; }
    }
}