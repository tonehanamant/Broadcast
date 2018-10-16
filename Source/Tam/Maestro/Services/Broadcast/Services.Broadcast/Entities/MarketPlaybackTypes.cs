using System;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public struct MarketPlaybackTypes
    {
        public Int16 market_code { get; set; }

        private string _available_playback_type;
        public string available_playback_type
        {
            get { return _available_playback_type ;  }
            set
            {
                _available_playback_type = value;
                PlaybackTypeEnum playback;
                if (Enum.TryParse(_available_playback_type, true, out playback))
                {
                    ForecastPlaybackType = playback;
                    PlaybackType = PlaybackTypeConverter.ForecastPlaybackTypeToProposalPlaybackType(ForecastPlaybackType);
                }
                else
                {
                    throw new Exception("Could not read Forecast Playback type from DB");
                }
            }
        }
        public PlaybackTypeEnum ForecastPlaybackType { get; set; }

        public int MarketId { get { return market_code;} }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; private set; }

        public override string ToString()
        {
            return MarketId + " - " + PlaybackType;
        }
    }
}