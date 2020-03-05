using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingAllocatedSpot
    {
        public int Id { get; set; }
        public int StationInventoryManifestId { get; set; }
        public MediaWeek MediaWeek { get; set; }
        public double Impressions { get; set; }
        public decimal Cost { get; set; }
        public int Spots { get; set; }
        public DisplayDaypart Daypart { get; set; }
    }
}
