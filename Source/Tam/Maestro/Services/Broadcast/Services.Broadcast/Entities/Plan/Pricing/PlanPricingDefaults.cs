using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingDefaults
    {
        public int UnitCap { get; set; }
        public List<PlanPricingInventorySourceDto> InventorySources { get; set; }
    }
}