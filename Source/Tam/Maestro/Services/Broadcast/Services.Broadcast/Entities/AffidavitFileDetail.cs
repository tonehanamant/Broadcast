using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class AffidavitFileDetail
    {
        public long Id { get; set; }
        public int AffidavitFileId { get; set; }
        public string Station { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public DateTime AdjustedAirDate { get; set; }
        public int AirTime { get; set; }
        public int SpotLengthId { get; set; }
        public string Isci { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public string LeadinGenre { get; set; }
        public string LeadinProgramName { get; set; }
        public string LeadoutGenre { get; set; }
        public string LeadoutProgramName { get; set; }
        public List<AffidavitFileDetailAudience> AffidavitFileDetailAudiences { get; set; }
    }
}
