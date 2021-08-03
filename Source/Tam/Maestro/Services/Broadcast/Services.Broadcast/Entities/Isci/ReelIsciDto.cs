using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class ReelIsciDto
    {
        public ReelIsciDto()
        {
            ReelIsciAdvertiserNameReferences = new List<ReelIsciAdvertiserNameReferenceDto>();
        }
        public int Id { get; set; }
        public string Isci { get; set; }
        public int SpotLengthId { get; set; }
        public DateTime ActiveStartDate { get; set; }
        public DateTime ActiveEndDate { get; set; }
        public List<ReelIsciAdvertiserNameReferenceDto> ReelIsciAdvertiserNameReferences { get; set; }
        public DateTime IngestedAt { get; set; }
    }
}
