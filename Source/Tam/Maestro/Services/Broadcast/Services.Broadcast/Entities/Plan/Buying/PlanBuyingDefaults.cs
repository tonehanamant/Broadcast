using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingDefaults
    {
        public int UnitCaps { get; set; }
        public UnitCapEnum UnitCapType { get; set; }
        public List<PlanBuyingInventorySourceDto> InventorySourcePercentages { get; set; }
        public List<PlanBuyingInventorySourceTypeDto> InventorySourceTypePercentages { get; set; }
        public double Margin { get; set; }
        public MarketGroupEnum MarketGroup { get; set; }
    }
}