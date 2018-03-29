using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PostedContractedProposalsDto
    {
        public List<PostDto> Posts { get; set; }
        public int UnlinkedIscis { get; set; }
    }

    public class PostDto
    {
        public int ContractId { get; set; }
        public string ContractName { get; set; }
        public DateTime? UploadDate { get; set; }
        public int SpotsInSpec { get; set; }
        public int SpotsOutOfSpec { get; set; }
        public double? PrimaryAudienceImpressions { get; set; }
    }
}
