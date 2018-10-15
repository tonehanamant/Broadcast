using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class RatingForecastResponse
    {
        public readonly short HutMediaMonthId;
        public readonly short ShareMediaMonthId;
        public readonly int MaestroAudienceId;
        public readonly PlaybackTypeEnum PlaybackType;
        public readonly List<ProgramRating> ProgramRatings;

        public RatingForecastResponse(RatingForecastRequest forecastRequest, List<ProgramRating> programRatings)
        {
            HutMediaMonthId = forecastRequest.HutMediaMonthId;
            ShareMediaMonthId = forecastRequest.ShareMediaMonthId;
            MaestroAudienceId = forecastRequest.MaestroAudienceId;
            PlaybackType = forecastRequest.MinPlaybackType;
            ProgramRatings = programRatings;
        }
    }
}