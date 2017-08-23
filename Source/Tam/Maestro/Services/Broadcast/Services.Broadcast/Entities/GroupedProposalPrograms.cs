using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class GroupedProposalPrograms
    {    
        public GroupedProposalPrograms()
        {
            DisplayProposalMarketPrograms = new List<GroupedDisplayProposalMarketPrograms>();
            TargetUnitTotals = new ProposalTotalFields();
            OverallTotals = new ProposalTotalFields();
        }
        public List<GroupedDisplayProposalMarketPrograms> DisplayProposalMarketPrograms; 
        public ProposalTotalFields TargetUnitTotals { get; set; }
        public ProposalTotalFields OverallTotals { get; set; }
    }
}
