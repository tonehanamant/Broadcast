using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ImpressionsDaypartResultForSingleBook
    {
        public List<StationImpressionsWithAudience> Impressions { get; set; }
        public List<MarketPlaybackType> UsedMarketPlaybackTypes { get; set; }

        internal ImpressionsDaypartResultForSingleBook AddImpressions(ImpressionsDaypartResultForSingleBook otherImpressionsDaypartForSingleBook)
        {
            foreach (var otherImpressions in otherImpressionsDaypartForSingleBook.Impressions)
            {
                var impressions = Impressions.Find(i => i.AudienceId == otherImpressions.AudienceId
                    && i.LegacyCallLetters.Equals(otherImpressions.LegacyCallLetters)
                    && i.SharePlaybackType == otherImpressions.SharePlaybackType);
                impressions.Impressions += otherImpressions.Impressions;
                impressions.Rating += otherImpressions.Rating;
            }
            return this;
        }
    }
}
