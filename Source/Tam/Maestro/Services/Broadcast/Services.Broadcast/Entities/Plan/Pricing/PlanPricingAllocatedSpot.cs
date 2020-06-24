using System;
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
        public decimal SpotCost { get; set; }
        public decimal SpotCostWithMargin { get; set; }
        public int Spots { get; set; }
        public DaypartDefaultDto StandardDaypart { get; set; }

        public decimal TotalCost
        {
            get
            {
                return SpotCost * Spots;
            }
        }

        public decimal TotalCostWithMargin
        {
            get
            {
                return SpotCostWithMargin * Spots;
            }
        }

        public double TotalImpressions
        {
            get
            {
                return Impressions * Spots;
            }
        }
    }
}
