namespace Services.Broadcast.Entities
{
    public class EditProposalProgramSpotsRequest
    {
        public string CacheGuid { get; set; }
        public int ProposalProgramId { get; set; }
        public int Spots { get; set; }
    }
}
