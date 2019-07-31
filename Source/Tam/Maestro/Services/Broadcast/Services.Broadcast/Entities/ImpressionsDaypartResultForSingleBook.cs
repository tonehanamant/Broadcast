using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ImpressionsDaypartResultForSingleBook
    {
        public List<StationImpressionsWithAudience> Impressions { get; set; }
        public List<MarketPlaybackType> UsedMarketPlaybackTypes { get; set; }
    }
}
