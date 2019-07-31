using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Converters
{
    public class PlaybackTypeConverter
    {
        public static PlaybackTypeEnum ProposalPlaybackTypeToForecastPlaybackType(ProposalEnums.ProposalPlaybackType? proposalPlaybackType)
        {
            switch (proposalPlaybackType)
            {
                case ProposalEnums.ProposalPlaybackType.Live:
                    return PlaybackTypeEnum.O;
                case ProposalEnums.ProposalPlaybackType.LiveSameDay:
                    return PlaybackTypeEnum.S;
                case ProposalEnums.ProposalPlaybackType.LivePlus1:
                    return PlaybackTypeEnum.One;
                case ProposalEnums.ProposalPlaybackType.LivePlus3:
                    return PlaybackTypeEnum.Three;
                case ProposalEnums.ProposalPlaybackType.LivePlus7:
                    return PlaybackTypeEnum.Seven;
                default:
                    return PlaybackTypeEnum.Three;
            }
        }

        public static ProposalEnums.ProposalPlaybackType ForecastPlaybackTypeToProposalPlaybackType(PlaybackTypeEnum playbackType)
        {
            switch (playbackType)
            {
                case PlaybackTypeEnum.O:
                    return ProposalEnums.ProposalPlaybackType.Live;
                case PlaybackTypeEnum.S:
                    return ProposalEnums.ProposalPlaybackType.LiveSameDay;
                case PlaybackTypeEnum.One:
                    return ProposalEnums.ProposalPlaybackType.LivePlus1 ;
                case PlaybackTypeEnum.Three:
                    return ProposalEnums.ProposalPlaybackType.LivePlus3;
                case PlaybackTypeEnum.Seven:
                    return ProposalEnums.ProposalPlaybackType.LivePlus7;
                default:
                    return ProposalEnums.ProposalPlaybackType.LivePlus3;
            }
        }

        public static ProposalEnums.ProposalPlaybackType ForecastPlaybackTypeToProposalPlaybackType(string playbackType)
        {
            switch (playbackType)
            {
                case "O":
                    return ProposalEnums.ProposalPlaybackType.Live;
                case "S":
                    return ProposalEnums.ProposalPlaybackType.LiveSameDay;
                case "1":
                    return ProposalEnums.ProposalPlaybackType.LivePlus1;
                case "3":
                    return ProposalEnums.ProposalPlaybackType.LivePlus3;
                case "7":
                    return ProposalEnums.ProposalPlaybackType.LivePlus7;
                default:
                    throw new Exception($"Unknown forecast playback type: {playbackType}");
            }
        }
    }
}
