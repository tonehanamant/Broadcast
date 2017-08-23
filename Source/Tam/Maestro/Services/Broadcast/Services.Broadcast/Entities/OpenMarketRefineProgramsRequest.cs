namespace Services.Broadcast.Entities
{
    public class OpenMarketRefineProgramsRequest
    {
        public int ProposalDetailId { get; set; }
        public OpenMarketCriterion Criteria { get; set; }
        public bool IgnoreExistingAllocation { get; set; }
    }
}