using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestAudience
    {
        public DisplayAudience Audience { get; set; }
        public bool IsReference { get; set; }
        public double? Impressions { get; set; }
        public double? Rating { get; set; }
        public decimal? CPM { get; set; }
        public double? Vpvh { get; set; }
        public ProposalPlaybackType? SharePlaybackType { get; set; }
        public ProposalPlaybackType? HutPlaybackType { get; set; }
    }
}
