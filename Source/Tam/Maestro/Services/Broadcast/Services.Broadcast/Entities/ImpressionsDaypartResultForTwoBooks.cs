using Services.Broadcast.Helpers;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ImpressionsDaypartResultForTwoBooks
    {
        public List<StationImpressionsWithAudience> Impressions { get; set; }
        public List<MarketPlaybackType> UsedHutMarketPlaybackTypes { get; set; }
        public List<MarketPlaybackType> UsedShareMarketPlaybackTypes { get; set; }

        internal ImpressionsDaypartResultForTwoBooks MergeImpressions(
            ImpressionsDaypartResultForTwoBooks otherImpressionsDaypartForTwoBooks,
            ManifestDetailDaypart firstDaypart,
            ManifestDetailDaypart secondDaypart)
        {
            var fisrtDaypartTime = DaypartTimeHelper.GetTotalTimeInclusive(
                firstDaypart.DisplayDaypart.StartTime, 
                firstDaypart.DisplayDaypart.EndTime);

            var secondDaypartTime = DaypartTimeHelper.GetTotalTimeInclusive(
                secondDaypart.DisplayDaypart.StartTime,
                secondDaypart.DisplayDaypart.EndTime);

            var totalTime = fisrtDaypartTime + secondDaypartTime;

            var firstDaypartWeight = (double)fisrtDaypartTime / totalTime;
            var secondDaypartWeight = 1 - firstDaypartWeight;

            foreach (var otherImpressions in otherImpressionsDaypartForTwoBooks.Impressions)
            {
                var impressions = Impressions.Find(i => i.AudienceId == otherImpressions.AudienceId
                    && i.LegacyCallLetters.Equals(otherImpressions.LegacyCallLetters)
                    && i.HutPlaybackType == otherImpressions.HutPlaybackType
                    && i.SharePlaybackType == otherImpressions.SharePlaybackType);

                var averageImpressions = (impressions.Impressions * firstDaypartWeight) + (otherImpressions.Impressions * secondDaypartWeight);
                var avarageRating = (impressions.Rating * firstDaypartWeight) + (otherImpressions.Rating * secondDaypartWeight);

                impressions.Impressions = averageImpressions;
                impressions.Rating = avarageRating;
            }

            return this;
        }
    }
}
