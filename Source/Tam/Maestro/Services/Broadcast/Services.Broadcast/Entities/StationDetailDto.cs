using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class StationDetailDto
    {
        public string StationName { get; set; }
        public string Affiliate { get; set; }
        public string Market { get; set; }
        public int StationCode { get; set; }
        public List<StationContact> Contacts { get; set; }
    }
}
