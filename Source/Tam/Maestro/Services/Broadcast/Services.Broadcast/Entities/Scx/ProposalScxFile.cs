using Services.Broadcast.Entities.DTO;
using System.IO;

namespace Services.Broadcast.Entities.Scx
{
    public class ProposalScxFile
    {
        public MemoryStream ScxStream { get; set; }
        public ProposalDetailDto ProposalDetailDto { get; set; }
    }
}
