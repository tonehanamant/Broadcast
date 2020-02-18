using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingDefaults
    {
        public int UnitCaps { get; set; }
        public UnitCapEnum UnitCapType { get; set; }
        public List<PlanPricingInventorySourceDto> InventorySourcePercentages { get; set; }
        public List<PlanPricingInventorySourceTypeDto> InventorySourceTypePercentages { get; set; }
        public double Margin { get; set; }
    }
}