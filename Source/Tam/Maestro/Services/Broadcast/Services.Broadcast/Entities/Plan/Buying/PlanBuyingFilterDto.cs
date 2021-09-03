using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingFilterDto
    {
        public List<string> RepFirmNames { get; set; }
        public List<string> OwnerNames { get; set; }
    }
}