using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class ReelIsciDto
    {
        public int Id { get; set; }
        public string Isci { get; set; }
        public int SpotLengthId { get; set; }
        public DateTime ActiveStartDate { get; set; }
        public DateTime ActiveEndDate { get; set; }
        public List<ReelIsciAdvertiserNameReferenceDto> ReelIsciAdvertiserNameReferences { get; set; } = new List<ReelIsciAdvertiserNameReferenceDto>();
        public DateTime IngestedAt { get; set; }
    }
}
