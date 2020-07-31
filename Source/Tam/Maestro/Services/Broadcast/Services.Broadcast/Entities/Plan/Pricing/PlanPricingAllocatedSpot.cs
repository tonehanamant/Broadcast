using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingAllocatedSpot
    {
        public int Id { get; set; }
        public int StationInventoryManifestId { get; set; }
        public MediaWeek InventoryMediaWeek { get; set; }
        public MediaWeek ContractMediaWeek { get; set; }
        public double Impressions30sec { get; set; }
        public DaypartDefaultDto StandardDaypart { get; set; }
        public List<SpotFrequency> SpotFrequencies { get;set; }

        public double TotalImpressions { get; set; }

        public decimal TotalCost
        {
            get
            {
                return SpotFrequencies.Sum(x => x.SpotCost * x.Spots);
            }
        }

        public decimal TotalCostWithMargin
        {
            get
            {
                return SpotFrequencies.Sum(x => x.SpotCostWithMargin * x.Spots);
            }
        }

        public int TotalSpots
        {
            get
            {
                return SpotFrequencies.Sum(x => x.Spots);
            }
        }

        public class SpotFrequency
        {
            public int SpotLengthId { get; set; }

            public decimal SpotCost { get; set; }

            public decimal SpotCostWithMargin { get; set; }

            public int Spots { get; set; }
        }
    }
}
