using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.Entities
{
    public class StationImpressions
    {
        public int Id { get; set; }
        public string LegacyCallLetters { get; set; }
        public double Impressions { get; set; }
        public double Rating { get; set; }
        public ProposalPlaybackType SharePlaybackType { get; set; }
        public ProposalPlaybackType? HutPlaybackType { get; set; }
    }
}
