namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingManifestWithManifestDaypart
    {
        public PlanPricingInventoryProgram Manifest { get; set; }
        public PlanPricingInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
    }
}
