using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class AffidavitMatchingDetail
    {
        public AffidavitSaveRequestDetail AffidavitDetail { get; set; }
        public List<AffidavitMatchingProposalWeek> ProposalDetailWeeks { get; set; }

        public AffidavitMatchingDetail()
        {
            ProposalDetailWeeks = new List<AffidavitMatchingProposalWeek>();
        }
    }
}
