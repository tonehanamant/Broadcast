using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class DefaultProjectionBooksDto
    {
        public PostingBookResultDto DefaultShareBook { get; set; }
        public PostingBookResultDto DefaultHutBook { get; set; }
        public ProposalEnums.ProposalPlaybackType DefaultPlaybackType { get; set; }
    }
}
