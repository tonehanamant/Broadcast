using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingAllocatedSpot
    {
        public int Id { get; set; }
        public int StationInventoryManifestId { get; set; }
        public MediaWeek InventoryMediaWeek { get; set; }
        public MediaWeek ContractMediaWeek { get; set; }
        public double Impressions { get; set; }
        public decimal Cost { get; set; }
        public int Spots { get; set; }
        public DaypartDefaultDto StandardDaypart { get; set; }
    }
}
