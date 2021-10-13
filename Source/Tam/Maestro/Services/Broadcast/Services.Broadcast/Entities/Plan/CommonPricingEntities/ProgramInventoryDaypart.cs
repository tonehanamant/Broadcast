namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class ProgramInventoryDaypart
    {
        public PlanDaypartDto PlanDaypart { get; set; }

        public BasePlanInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
    }
}