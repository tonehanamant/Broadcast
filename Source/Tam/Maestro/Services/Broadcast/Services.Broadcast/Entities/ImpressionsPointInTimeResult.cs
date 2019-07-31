using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ImpressionsPointInTimeResult
    {
        public List<StationImpressionsWithAudience> Impressions { get; set; }
        public List<MarketPlaybackType> UsedMarketPlaybackTypes { get; set; }
    }
}
