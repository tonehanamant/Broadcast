using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Aggregates
{
    public class RatingsAggregate
    {
        private readonly Dictionary<int, ILookup<string, RatingsDetail>> _RatingsDetails;
        private readonly ILookup<int, audience_audiences> _RatingAudienceMappings;

        public RatingsAggregate(Dictionary<int, ILookup<string, RatingsDetail>> ratingsDetails, IEnumerable<audience_audiences> ratingAudienceMappings)
        {
            _RatingsDetails = ratingsDetails;
            _RatingAudienceMappings = ratingAudienceMappings.ToLookup(ram => ram.custom_audience_id);
        }

        public double GetDelivery(int audienceId, int timeAired, string stationCode, int weekPart)
        {
            var ratingsAudiences = _RatingAudienceMappings[audienceId].Select(y => y.rating_audience_id).Distinct().ToList();
            var targetDetails = _RatingsDetails[weekPart][stationCode].Where(x => ratingsAudiences.Contains(x.AudienceId) && x.StartTime <= timeAired && x.EndTime >= timeAired).ToList();
            return targetDetails.Sum(y => y.Viewers);
        }
    }

    public class RatingsDetail
    {
        public int AudienceId { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public double Viewers { get; set; }
    }
}
