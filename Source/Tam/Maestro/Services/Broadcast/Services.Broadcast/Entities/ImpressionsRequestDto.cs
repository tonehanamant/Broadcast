using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class ImpressionsRequestDto
    {
        public ProposalEnums.ProposalPlaybackType? PlaybackType { get; set; }
        public int? ShareProjectionBookId { get; set; }
        public int? HutProjectionBookId { get; set; }
        public int? SingleProjectionBookId { get; set; }
        public bool Equivalized { get; set; }
        public int SpotLengthId { get; set; }
        public PostingTypeEnum PostType { get; set; }
    }
}
