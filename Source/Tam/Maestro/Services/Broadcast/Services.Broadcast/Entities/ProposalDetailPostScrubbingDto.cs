using System;

namespace Services.Broadcast.Entities
{
    public class ProposalDetailPostScrubbingDto
    {
        public DateTime TimeAired { get; set; }
        public int SpotLength { get; set; }
        public string ISCI { get; set; }
        public string ProgramName { get; set; }
        public string GenreName { get; set; }
        public string Affiliate { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
    }
}
