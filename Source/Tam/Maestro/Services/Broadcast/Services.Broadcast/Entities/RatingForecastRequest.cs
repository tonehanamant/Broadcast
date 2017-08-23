using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class RatingForecastRequest
    {
        public readonly short HutMediaMonthId;
        public readonly short ShareMediaMonthId;
        public readonly int MaestroAudienceId;
        public readonly PlaybackType MinPlaybackType;
        public readonly List<Program> Programs;

        public RatingForecastRequest(short hutMonth, short shareMonth, int maestroAudienceId, PlaybackType minPlaybackType)
        {
            HutMediaMonthId = hutMonth;
            ShareMediaMonthId = shareMonth;
            MaestroAudienceId = maestroAudienceId;
            MinPlaybackType = minPlaybackType;
            Programs = new List<Program>();
        }

        public RatingForecastRequest(short hutMonth, short shareMonth, int maestroAudienceId, PlaybackType minPlaybackType, List<Program> programs) : this(hutMonth, shareMonth, maestroAudienceId, minPlaybackType)
        {
            Programs = programs;
        }

        public RatingForecastRequest(RatingForecastRequest request, List<Program> programs)
        {
            HutMediaMonthId = request.HutMediaMonthId;
            ShareMediaMonthId = request.ShareMediaMonthId;
            NielsenRatingAudienceIds = request.NielsenRatingAudienceIds;
            MinPlaybackType = request.MinPlaybackType;
            MaestroAudienceId = request.MaestroAudienceId;
            Programs = programs;
        }

        public IEnumerable<int> NielsenRatingAudienceIds { get; private set; }

        public void SetNielsenRatingAudienceIds(IEnumerable<int> nielsenRatingAudienceIds)
        {
            NielsenRatingAudienceIds = nielsenRatingAudienceIds;
        }
    }
}