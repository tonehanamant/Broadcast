using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ImpressionsDaypartResultForTwoBooks
    {
        public List<StationImpressions> Impressions { get; set; }
        public List<MarketPlaybackType> UsedHutMarketPlaybackTypes { get; set; }
        public List<MarketPlaybackType> UsedShareMarketPlaybackTypes { get; set; }
    }
}
