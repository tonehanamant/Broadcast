using Newtonsoft.Json;
using Services.Broadcast.Entities.OpenMarketInventory;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideMarketDto : IInventoryMarket
    {
        public string MarketName { get; set; }
        public int MarketId { get; set; }
        public double MarketCoverage { get; set; }
        public int MarketRank { get; set; }
        public int TotalSpots { get; set; }
        public double TotalImpressions { get; set; }
        public decimal TotalCost { get; set; }
        public decimal MinCpm { get; set; }
        public decimal AvgCpm { get; set; }
        public decimal MaxCpm { get; set; }
        public decimal CPM { get; set; }
        public List<PricingGuideStationDto> Stations { get; set; } = new List<PricingGuideStationDto>();
        public double DisplayImpressions
        {
            get
            {
                return Stations.SelectMany(s => s.Programs).Sum(p => p.Spots * p.ImpressionsPerSpot);
            }
        }

        public double DisplayStationImpressions
        {
            get
            {
                return Stations.SelectMany(s => s.Programs).Sum(p => p.Spots * p.StationImpressionsPerSpot);
            }
        }

        [JsonIgnore]
        public bool HasEditedManuallySpots => Stations.Any(s => s.Programs.Any(p => p.SpotsEditedManually));
    }
}
