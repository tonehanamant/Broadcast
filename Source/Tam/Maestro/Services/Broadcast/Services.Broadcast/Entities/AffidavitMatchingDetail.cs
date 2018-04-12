using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class AffidavitMatchingDetail
    {
        public int LineNumber { get; set; }
        public AffidavitSaveRequestDetail AffidavitDetail { get; set; }
        public List<AffidavitMatchingProposalWeek> ProposalDetailWeeks { get; set; }

        public List<AffidavitFileDetailProblem> AffidavitDetailProblems { get; set; }

        public AffidavitMatchingDetail()
        {
            ProposalDetailWeeks = new List<AffidavitMatchingProposalWeek>();
            AffidavitDetailProblems = new List<AffidavitFileDetailProblem>();
        }
    }
}
