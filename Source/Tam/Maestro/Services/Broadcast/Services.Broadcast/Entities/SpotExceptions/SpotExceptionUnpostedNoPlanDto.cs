using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionUnpostedNoPlanDto
    {
        public int Id { get; set; }
        public string HouseIsci { get; set; }
        public string ClientIsci { get; set; }
        public int Count { get; set; }
        public System.DateTime ProgramAirTime { get; set; }
        public long EstimateID { get; set; }
        public string IngestedBy { get; set; }
        public System.DateTime IngestedAt { get; set; }
        public int ClientSpotLengthId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedAt { get; set; }
    }
}
