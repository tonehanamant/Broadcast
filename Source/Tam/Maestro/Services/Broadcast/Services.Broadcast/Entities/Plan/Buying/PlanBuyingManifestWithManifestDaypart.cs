using static Services.Broadcast.Entities.Plan.CommonPricingEntities.BasePlanInventoryProgram;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingManifestWithManifestDaypart
    {
        public PlanBuyingInventoryProgram Manifest { get; set; }
        public ManifestDaypart ManifestDaypart { get; set; }
    }
}
