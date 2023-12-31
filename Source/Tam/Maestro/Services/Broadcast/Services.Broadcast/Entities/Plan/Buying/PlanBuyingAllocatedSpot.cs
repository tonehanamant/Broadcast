﻿using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingAllocatedSpot
    {
        public int Id { get; set; }
        public int StationInventoryManifestId { get; set; }
        public MediaWeek InventoryMediaWeek { get; set; }
        public MediaWeek ContractMediaWeek { get; set; }
        public double Impressions30sec { get; set; }
        public StandardDaypartDto StandardDaypart { get; set; }
        public List<SpotFrequency> SpotFrequencies { get;set; }

        public double TotalImpressions => SpotFrequencies.Sum(x => x.Impressions * x.Spots);

        public decimal TotalCost => SpotFrequencies.Sum(x => x.SpotCost * x.Spots);

        public decimal TotalCostWithMargin => SpotFrequencies.Sum(x => x.SpotCostWithMargin * x.Spots);

        public int TotalSpots => SpotFrequencies.Sum(x => x.Spots);
        public string RepFirm { get; set; }
        public string OwnerName { get; set; }
        public string LegacyCallLetters { get; set; }
    }
}
