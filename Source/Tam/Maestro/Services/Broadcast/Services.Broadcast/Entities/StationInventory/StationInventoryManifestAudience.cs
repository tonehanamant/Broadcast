using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestAudience
    {
        public DisplayAudience Audience { get; set; }
        public bool IsReference { get; set; }
        public double? Impressions { get; set; }
        public double? Rating { get; set; }
        public decimal? CPM { get; set; }
    }
}
