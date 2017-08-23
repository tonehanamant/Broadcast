using Services.Broadcast.Entities;

namespace Services.Broadcast.Converters
{
    public class PlaybackTypeConverter
    {
        public static PlaybackType ProposalPlaybackTypeToForecastPlaybackType(ProposalEnums.ProposalPlaybackType? proposalPlaybackType)
        {
            switch (proposalPlaybackType)
            {
                case ProposalEnums.ProposalPlaybackType.Live:
                    return PlaybackType.O;
                case ProposalEnums.ProposalPlaybackType.LiveSameDay:
                    return PlaybackType.S;
                case ProposalEnums.ProposalPlaybackType.LivePlus1:
                    return PlaybackType.One;
                case ProposalEnums.ProposalPlaybackType.LivePlus3:
                    return PlaybackType.Three;
                case ProposalEnums.ProposalPlaybackType.LivePlus7:
                    return PlaybackType.Seven;
                default:
                    return PlaybackType.Three;
            }
        }
        public static ProposalEnums.ProposalPlaybackType ForecastPlaybackTypeToProposalPlaybackType(PlaybackType playbackType)
        {
            switch (playbackType)
            {
                case PlaybackType.O:
                    return ProposalEnums.ProposalPlaybackType.Live;
                case PlaybackType.S:
                    return ProposalEnums.ProposalPlaybackType.LiveSameDay;
                case PlaybackType.One:
                    return ProposalEnums.ProposalPlaybackType.LivePlus1 ;
                case PlaybackType.Three:
                    return ProposalEnums.ProposalPlaybackType.LivePlus3;
                case PlaybackType.Seven:
                    return ProposalEnums.ProposalPlaybackType.LivePlus7;
                default:
                    return ProposalEnums.ProposalPlaybackType.LivePlus3;
            }
        }
    }
}
