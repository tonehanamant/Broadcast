namespace Services.Broadcast.Entities
{
    public class ProposalConstants
    {
        public const int UseShareBookOnlyId = -1;
        public const int ShareBookNotFoundId = -1;
        public const double ProposalDefaultMargin = 20;
        public const string HasInventorySelectedMessage = "Inventory has been reserved for this Proposal, changes may effect delivery and costs.";
        public const string ChangeProposalStatusReleaseInventoryMessage = "Changing proposal status from Agency On Hold to Proposed will release all allocated inventory";
    }
}
