using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class AffidavitMatchingDetail
    {
        public AffidavitFileDetail AffidavitDetail { get; set; }
        public bool Archived { get; set; }
        public List<AffidavitMatchingProposalWeek> ProposalDetailWeeks { get; set; } = new List<AffidavitMatchingProposalWeek>();
        public List<AffidavitFileDetailProblem> AffidavitDetailProblems { get; set; } = new List<AffidavitFileDetailProblem>();
        
    }
}
