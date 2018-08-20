using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class PostedContractedProposalsDto
    {
        public List<PostedContracts> Posts { get; set; }
        public int UnlinkedIscis { get; set; }
    }    
}
