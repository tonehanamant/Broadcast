using Services.Broadcast.Helpers;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ImpressionsDaypartResultForSingleBook
    {
        public List<StationImpressionsWithAudience> Impressions { get; set; }
        public List<MarketPlaybackType> UsedMarketPlaybackTypes { get; set; }

        internal ImpressionsDaypartResultForSingleBook MergeImpressions(
            ImpressionsDaypartResultForSingleBook otherImpressionsDaypartForSingleBook,
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
            
            foreach (var otherImpressions in otherImpressionsDaypartForSingleBook.Impressions)
            {
                var impressions = Impressions.Find(i => i.AudienceId == otherImpressions.AudienceId
                    && i.LegacyCallLetters.Equals(otherImpressions.LegacyCallLetters)
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
