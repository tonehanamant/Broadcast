namespace Services.Broadcast.Entities
{
    public class ProposalAudienceDto
    {
        public int? AudienceId { get; set; }
        public int? Rank { get; set; }

        public ProposalAudienceDto() { }
        public ProposalAudienceDto(int audienceId)
        {
            AudienceId = audienceId;
        }
    }
}
