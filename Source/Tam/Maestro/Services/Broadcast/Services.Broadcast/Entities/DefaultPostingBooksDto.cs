namespace Services.Broadcast.Entities
{
    public class DefaultPostingBooksDto
    {
        public PostingBookResultDto DefaultShareBook { get; set; }
        public PostingBookResultDto DefaultHutBook { get; set; }
        public ProposalEnums.ProposalPlaybackType DefaultPlaybackType { get; set; }
    }
}
