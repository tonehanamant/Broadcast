using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.Entities
{
    public class MarketPlaybackType
    {
        public int MarketCode { get; set; }
        public ProposalPlaybackType PlaybackType { get; set; }
    }
}
