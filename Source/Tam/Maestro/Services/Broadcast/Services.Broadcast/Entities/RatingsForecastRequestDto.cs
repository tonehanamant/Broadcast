using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class RatingsForecastRequestDto
    {
        public short HutMediaMonth { get; set; }
        public short ShareMediaMonth { get; set; }
        public int AudiencId { get; set; }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; set; } = ProposalEnums.ProposalPlaybackType.LivePlus3;        

        public string StationLegacyCallLetters { get; set; }
        public int StartTimeSeconds { get; set; }
        public int EndTimeSeconds { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }
}
