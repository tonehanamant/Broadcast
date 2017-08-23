using System;

namespace Services.Broadcast.Entities
{
    public class DisplayProposalVersion
    {
        public int Version { get; set; }
        public string Advertiser { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GuaranteedAudience { get; set; }
        public string Owner { get; set; }
        public DateTime DateModified { get; set; }
        public string Notes { get; set; }
        public ProposalEnums.ProposalStatusType Status { get; set; }

    }
}
