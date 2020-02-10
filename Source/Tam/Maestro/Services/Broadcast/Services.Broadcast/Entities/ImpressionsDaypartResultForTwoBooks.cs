using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ImpressionsDaypartResultForTwoBooks
    {
        public List<StationImpressionsWithAudience> Impressions { get; set; }
        public List<MarketPlaybackType> UsedHutMarketPlaybackTypes { get; set; }
        public List<MarketPlaybackType> UsedShareMarketPlaybackTypes { get; set; }

        internal ImpressionsDaypartResultForTwoBooks AddImpressions(ImpressionsDaypartResultForTwoBooks otherImpressionsDaypartForTwoBooks)
        {
            foreach(var otherImpressions in otherImpressionsDaypartForTwoBooks.Impressions)
            {
                var impressions = Impressions.Find(i => i.AudienceId == otherImpressions.AudienceId
                    && i.LegacyCallLetters.Equals(otherImpressions.LegacyCallLetters)
                    && i.HutPlaybackType == otherImpressions.HutPlaybackType
                    && i.SharePlaybackType == otherImpressions.SharePlaybackType);
                impressions.Impressions += otherImpressions.Impressions;
                impressions.Rating += otherImpressions.Rating;
            }
            return this;
        }
    }
}
