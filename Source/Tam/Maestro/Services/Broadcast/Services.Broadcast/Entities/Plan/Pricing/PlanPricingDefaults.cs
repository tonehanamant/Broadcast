﻿using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingDefaults
    {
        public int UnitCaps { get; set; }
        public UnitCapEnum UnitCapsType { get; set; }
        public double Margin { get; set; }
        public MarketGroupEnum MarketGroup { get; set; }
    }
}