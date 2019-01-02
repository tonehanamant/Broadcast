using System.Collections.Generic;

namespace Services.Broadcast.Entities.Nti
{
    public class NtiRatingDocument
    {
        public NtiRatingHeaderDto Header { get; set; }
        public List<NtiRatingCategory> Ratings { get; set; }
        public List<NtiProposalVersionDetailWeek> ProposalWeeks { get; set; }
    }
}
